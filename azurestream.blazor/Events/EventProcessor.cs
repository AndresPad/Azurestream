using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using azurestream.WebSite;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace azurestream.blazor.Events
{
    public class EventProcessor : IObservable<Event>, IObservable<EventProcessorInfo>, IAsyncDisposable
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        private readonly TimeSpan _maxWaitTime = TimeSpan.FromSeconds(30);
        private readonly int EventProcessorCreationMaximumRetries = 5;
        private readonly TimeSpan EventProcessorCreationRetryDelay = TimeSpan.FromSeconds(30);
        private int EventProcessorCreationRetryCount = 0;
        
        private readonly string _iotHubConnectionString = string.Empty;
        private readonly BlobContainerClient _checkpointStore;
        private readonly string _consumerGroup = string.Empty;
        private readonly CancellationTokenSource _cancellationSource;

        private EventProcessorClient _eventProcessorClient = null;

        private int _consecutiveErrorsHandlingEvents = 0;
        private DateTime _lastEventReceivedTimeStamp = DateTime.MinValue;
        private ConcurrentDictionary<string, int> _partitionTracking = new ConcurrentDictionary<string, int>();
        private List<IObserver<Event>> _eventObservers = new List<IObserver<Event>>();
        private List<IObserver<EventProcessorInfo>> _processorInfoObservers = new List<IObserver<EventProcessorInfo>>();
        

        public EventProcessor(string iotHubConnectionString, 
            string checkpointStoreConnectionString, 
            CancellationTokenSource cancellationTokenSource,
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName, 
            string checkpointContainer = "iot-hub-consumer-web-checkpointing")
        {

            _iotHubConnectionString = iotHubConnectionString;
            _consumerGroup = consumerGroup;

            if (cancellationTokenSource is null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }
            _cancellationSource = cancellationTokenSource;

            _checkpointStore = new BlobContainerClient(checkpointStoreConnectionString, checkpointContainer);
            _checkpointStore.CreateIfNotExists();

            _eventProcessorClient = CreateEventProcessorClientAsync(_iotHubConnectionString, _consumerGroup, _checkpointStore).Result;
            _eventProcessorClient.StartProcessingAsync(_cancellationSource.Token);
        }

        private async Task<EventProcessorClient> CreateEventProcessorClientAsync(string iotHubConnectionString, string consumerGroup, BlobContainerClient checkpointStore)
        {
            EventHubConnectionOptions connectionOptions = new EventHubConnectionOptions()
            {
                ConnectionIdleTimeout = TimeSpan.FromSeconds(30)
            };

            EventHubsRetryOptions retryOptions = new EventHubsRetryOptions()
            {
                Mode = EventHubsRetryMode.Exponential,
                Delay = TimeSpan.FromSeconds(10),
                MaximumDelay = TimeSpan.FromSeconds(180),
                MaximumRetries = 9,
                TryTimeout = TimeSpan.FromSeconds(15)
            };

            EventProcessorClientOptions clientOptions = new EventProcessorClientOptions()
            {
                MaximumWaitTime = _maxWaitTime,
                PrefetchCount = 250,
                CacheEventCount = 250,
                Identifier = $"{consumerGroup.Replace("$", "").ToLower()}-web-consumer",
                ConnectionOptions = connectionOptions,
                RetryOptions = retryOptions
            };

            EventProcessorClient? eventProcessor = null;

            //TODO: Is possible to obtain the EH Built-in endpoint conn string in other way (REST API, Management API...)??
            var iotHubConnStrWithDeviceId = !iotHubConnectionString.Contains(";DeviceId=") ? $"{iotHubConnectionString};DeviceId=NoMatterWhichDeviceIs" : iotHubConnectionString;
            string ehConnString = IotHubConnection.GetEventHubsConnectionStringAsync(iotHubConnStrWithDeviceId).Result;
            eventProcessor = new EventProcessorClient(checkpointStore, consumerGroup, ehConnString, clientOptions);
            eventProcessor.PartitionInitializingAsync += PartitionInitializingAsync;
            eventProcessor.PartitionClosingAsync += PartitionClosingAsync;
            eventProcessor.ProcessEventAsync += ProcessEventAsync;
            eventProcessor.ProcessErrorAsync += ProcessErrorAsync;

            return await Task.FromResult(eventProcessor);
        }

        private async Task ProcessEventAsync(ProcessEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (args.HasEvent)
                {
                    _lastEventReceivedTimeStamp = DateTime.UtcNow;
                    Event newEvent = new Event(args.Data);
                    
                    await Checkpoint(args);

                    BroadcastEvent(newEvent);
                }
                else
                {
                    if (DateTime.UtcNow - _lastEventReceivedTimeStamp > _maxWaitTime)
                    {
                        BroadcastProcessorInfo(new EventProcessorInfo()
                        {
                            Description = $"Processor is alive and waiting for events. Last event received at {_lastEventReceivedTimeStamp.ToString()}.",
                            Timestamp = DateTime.UtcNow,
                            Type = ProcessorInfoType.Heartbeat
                        });
                    }
                }
                _consecutiveErrorsHandlingEvents = 0; //We succeedded processing the event, reset the consecutive errors counter.
            }
            catch(Exception ex)
            {
                _consecutiveErrorsHandlingEvents++;
                if(_consecutiveErrorsHandlingEvents < 100)
                {
                    BroadcastProcessorInfo(new EventProcessorInfo()
                    {
                        Description = $"Exception handling new event. Error message: {ex.Message}",
                        Timestamp = DateTime.UtcNow,
                        Type = ProcessorInfoType.Warning,
                        Exception = ex
                    });
                }
                else
                {
                    BroadcastProcessorInfo(new EventProcessorInfo()
                    {
                        Description = $"The maximum number of consecutive errors handling events has been reached. Stopping the processing.",
                        Timestamp = DateTime.UtcNow,
                        Type = ProcessorInfoType.Error,
                        Exception = ex
                    });
                    _cancellationSource.Cancel();
                }
            }
            return;
        }

        private async Task Checkpoint(ProcessEventArgs args)
        {
            string partition = args.Partition.PartitionId;
            int eventsSinceLastCheckpoint = _partitionTracking.AddOrUpdate(key: partition, addValue: 1, updateValueFactory: (_, currentCount) => currentCount + 1);
            if (eventsSinceLastCheckpoint >= 50)
            {
                //Update the checkpoint every 50 delivered messages //TODO: Check logic for checkpointing
                await args.UpdateCheckpointAsync();
                _partitionTracking[partition] = 0;
            }
        }

        private async Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                // If out of memory, signal for cancellation. Consider other non-recoverable exceptions.
                if (args.Exception is OutOfMemoryException)
                {
                    BroadcastProcessorInfo(new EventProcessorInfo()
                    {
                        Description = $"The system is running out of memory. Unable to recover. The event processing is being cancelled.",
                        Timestamp = DateTime.UtcNow,
                        Type = ProcessorInfoType.Error,
                        Exception = args.Exception
                    });
                    _cancellationSource.Cancel();
                    //NOTIFY UPPER LAYERS TO ALLOW RECOVERY?
                    return;
                }

                //Network Connection Lost -> Consider not recoverable retry from UI, directly cancel
                if(args.Exception is SocketException)
                {
                    BroadcastProcessorInfo(new EventProcessorInfo()
                    {
                        Description = $"There is a network error during the processing of Operation '{args.Operation}' in PartitionId '{args.PartitionId}'. Unable to recover. The event processing is being cancelled.",
                        Timestamp = DateTime.UtcNow,
                        Type = ProcessorInfoType.Error,
                        Exception = args.Exception
                    });
                    _cancellationSource.Cancel();
                    return;
                    //NOTIFY UPPER LAYERS TO ALLOW RECOVERY?
                }

                //TODO: Advance scenario -> transient error or failure due to failover. Investigate further and review code.
                if ((args.Exception is EventHubsException && ((EventHubsException)args.Exception).IsTransient) ||
                    (args.Exception is EventHubsException && ((EventHubsException)args.Exception).Message.Contains("Namespace cannot be resolved")))

                {
                    await semaphoreSlim.WaitAsync(); //Ensure only one process is going to execute the reconnection
                    try
                    {
                        if (!_cancellationSource.IsCancellationRequested)
                        {
                            await _eventProcessorClient.StopProcessingAsync(_cancellationSource.Token);
                            _eventProcessorClient.PartitionInitializingAsync -= PartitionInitializingAsync;
                            _eventProcessorClient.PartitionClosingAsync -= PartitionClosingAsync;
                            _eventProcessorClient.ProcessEventAsync -= ProcessEventAsync;
                            _eventProcessorClient.ProcessErrorAsync -= ProcessErrorAsync;

                            BroadcastProcessorInfo(new EventProcessorInfo()
                            {
                                Description = $"Transient error for Operation '{args.Operation}' in PartitionId '{args.PartitionId}'. Reconnecting...",
                                Timestamp = DateTime.UtcNow,
                                Type = ProcessorInfoType.Error,
                                Exception = args.Exception
                            });
                            
                            _eventProcessorClient = await CreateEventProcessorClientAsync(_iotHubConnectionString, _consumerGroup, _checkpointStore);

                            await _eventProcessorClient.StartProcessingAsync(_cancellationSource.Token);
                        }
                        else
                        {
                            BroadcastProcessorInfo(new EventProcessorInfo()
                            {
                                Description = $"Transient error for Operation '{args.Operation}' in PartitionId '{args.PartitionId}'. Reconnecting process already initiated.",
                                Timestamp = DateTime.UtcNow,
                                Type = ProcessorInfoType.Error,
                                Exception = args.Exception
                            });
                        }
                    }
                    catch(Exception ex)
                    {
                        BroadcastProcessorInfo(new EventProcessorInfo()
                        {
                            Description = $"Unexpected exception reconnecting: { ex }.",
                            Timestamp = DateTime.UtcNow,
                            Type = ProcessorInfoType.Error,
                            Exception = ex
                        });
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }
                else if (!_eventProcessorClient.IsRunning && !_cancellationSource.IsCancellationRequested)
                {
                    //if the processor is not running, we re-start the process by stopping and starting again.
                    await _eventProcessorClient.StopProcessingAsync();
                    await _eventProcessorClient.StartProcessingAsync(_cancellationSource.Token);
                }
            }
            catch(Exception ex)
            {
                BroadcastProcessorInfo(new EventProcessorInfo()
                {
                    Description = $"Unexpected exception handling proccesor error: { ex }. Unable to recover. The event processing is being cancelled.",
                    Timestamp = DateTime.UtcNow,
                    Type = ProcessorInfoType.Error,
                    Exception = ex
                });
                //We cancel the process as we are suffering another exception just handling an error (probably we are unable to re-start the processor)
                _cancellationSource.Cancel();
            }
        }

        private Task PartitionInitializingAsync(PartitionInitializingEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                // If no checkpoint was found, start processing events enqueued now or in the future.

                EventPosition startPositionWhenNoCheckpoint =
                    EventPosition.FromEnqueuedTime(DateTimeOffset.UtcNow);
                args.DefaultStartingPosition = startPositionWhenNoCheckpoint;

                BroadcastProcessorInfo(new EventProcessorInfo()
                {
                    Description = $"Partition with Id={ args.PartitionId } is initializing. Starting processing new events from { startPositionWhenNoCheckpoint }.", 
                    Timestamp = DateTime.UtcNow,
                    Type = ProcessorInfoType.Information
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unexpected error in ProcessorErrorHandler. " + ex.ToString());
                _cancellationSource.Cancel();
            }

            return Task.CompletedTask;
        }

        private Task PartitionClosingAsync(PartitionClosingEventArgs args)
        {
            try
            {
                if (args.CancellationToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }

                string description = args.Reason switch
                {
                    ProcessingStoppedReason.OwnershipLost =>
                        "Another processor claimed ownership",

                    ProcessingStoppedReason.Shutdown =>
                        "The processor is shutting down",

                    _ => args.Reason.ToString()
                };

                BroadcastProcessorInfo(new EventProcessorInfo()
                {
                    Description = $"Partition with Id={ args.PartitionId } is closing. Reason: { description }.",
                    Timestamp = DateTime.UtcNow,
                    Type = ProcessorInfoType.Information
                });
            }
            catch(Exception ex)
            {
                BroadcastProcessorInfo(new EventProcessorInfo()
                {
                    Description = $"Unhandled exception handling partition closing for Partition with Id={ args.PartitionId } is closing. Error Message: { ex.Message }.",
                    Timestamp = DateTime.UtcNow,
                    Type = ProcessorInfoType.Error,
                    Exception = ex
                });
            }

            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await _eventProcessorClient.StopProcessingAsync();
            _eventProcessorClient.PartitionInitializingAsync -= PartitionInitializingAsync;
            _eventProcessorClient.PartitionClosingAsync -= PartitionClosingAsync;
            _eventProcessorClient.ProcessEventAsync -= ProcessEventAsync;
            _eventProcessorClient.ProcessErrorAsync -= ProcessErrorAsync;

            foreach (var observer in _eventObservers.ToArray())
            {
                if (observer != null)
                {
                    observer.OnCompleted();
                }
            }
            _eventObservers.Clear();

            foreach (var observer in _processorInfoObservers.ToArray())
            {
                if (observer != null)
                {
                    observer.OnCompleted();
                }
            }
            _processorInfoObservers.Clear();
        }

        public IDisposable Subscribe(IObserver<Event> observer)
        {
            if (!_eventObservers.Contains(observer))
                _eventObservers.Add(observer);

            return new Unsubscriber<Event>(_eventObservers, observer);
        }

        public IDisposable Subscribe(IObserver<EventProcessorInfo> observer)
        {
            if (!_processorInfoObservers.Contains(observer))
                _processorInfoObservers.Add(observer);

            return new Unsubscriber<EventProcessorInfo>(_processorInfoObservers, observer);
        }

        private void BroadcastEvent(Event data)
        {
            if (data != null)
            {
                foreach (var observer in _eventObservers)
                {
                    observer.OnNext(data);
                }
            }
        }

        private void BroadcastProcessorInfo(EventProcessorInfo data)
        {
            if (data != null)
            {
                foreach (var observer in _processorInfoObservers)
                {
                    observer.OnNext(data);
                }
            }
        }
    }
}
