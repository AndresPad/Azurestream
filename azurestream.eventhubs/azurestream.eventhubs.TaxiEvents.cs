using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace azurestream.eventhubs
{
    //Ingest data from Event Hub into Azure Data Explorer
    //Taxi Cab Data Example
    //----------------------------------------------------------------------------------------------------------
    public class TaxiEventsData
    {
        private const string EventHubNamespaceCnx = "Endpoint=sb://YOUREVENTHUBNAMESPACE.servicebus.windows.net/;SharedAccessKeyName=YOURSHAREDACCESSPOLICY;SharedAccessKey=YOURACCESSKEY";
        private const string EventHubName = "databricks-eh";
        private static readonly string fileName = "NycTaxiStream.json";
        //------------------------------------------------------------------------------------------------------
        internal static async Task ExecuteAsync()
        {
            Console.WriteLine("Registering Event Hub Client...");
            Console.WriteLine("Ready to start sending messages to Event Hub:" + EventHubName);

            await EventHubIngestionAsync();

            Console.WriteLine("Sending Messages. Press enter key to stop worker.");
            Console.ReadLine();
        }

        //--------------------------------------------------------------------------------------------------------------
        public static async Task EventHubIngestionAsync()
        {
            //create an Event Hubs Producer client using the namespace connection string and the event hub name
            await using var producerClient = new EventHubProducerClient(EventHubNamespaceCnx, EventHubName);

            var taxiEvents = LoadEventsFromFile();

            if (taxiEvents == null || taxiEvents.Count == 0)
            {
                return;
            }

            CultureInfo culture = new CultureInfo("en-US");
            DateTime lastEventTime = DateTime.MinValue;
            bool isFirstEvent = true;
            int seconds = 0;
            int counter = 0;

            //List<StormEvent> TaxiEvents = JsonConvert.DeserializeObject<List<StormEvent>>(
            //   File.ReadAllText(@"NycTaxiStream.json"));

            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            foreach (dynamic te in taxiEvents)
            {
                // Pause and resume on Escape key
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Pausing the events. Press Esc key to continue");

                    while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                    {
                        // do nothing
                    }

                    Console.WriteLine("Resuming the events!");
                    Console.WriteLine();
                }

                try
                {
                    DateTime newPickupTime = Convert.ToDateTime(te.PickupTime.ToString(), culture);

                    if (isFirstEvent)
                    {
                        lastEventTime = newPickupTime;
                        isFirstEvent = false;

                        Console.Write($"Time: {lastEventTime} events - ");
                    }
                    else
                    {
                        seconds = Convert.ToInt32((newPickupTime - lastEventTime).TotalSeconds);
                        lastEventTime = newPickupTime;
                    }

                    if (seconds > 0)
                    {
                        Thread.Sleep(seconds * 1000);

                        Console.WriteLine();
                        Console.Write($"Time: {lastEventTime} events - ");

                        counter = 0;
                    }

                    counter += 1;

                    Console.Write(counter + " ");

                    //-----
                    if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Event {te}"))))
                    {
                        // if it is too large for the batch
                        throw new Exception($"Event {te} is too large for the batch and cannot be sent.");
                    }

                    EventData eventData = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(te)));
                    Console.WriteLine($"sending message..." + "\n" + JsonConvert.SerializeObject(te));

                    eventBatch.TryAdd(eventData);
                    await producerClient.SendAsync(eventBatch);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.WriteLine("Error sending events.");
                    Console.WriteLine($"Error: {exception.Message}");
                    Console.ResetColor();
                    break;
                }
            }

            //foreach (StormEvent se in StormEvents)
            //{
            //    try
            //    {
            //        EventData eventData = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(se)));
            //        Console.WriteLine($"sending message..." + "\n" + JsonConvert.SerializeObject(se));

            //        using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
            //        eventBatch.TryAdd(eventData);

            //        await producerClient.SendAsync(eventBatch);
            //    }
            //    catch (Exception exception)
            //    {
            //        Console.ForegroundColor = ConsoleColor.Red;
            //        Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
            //        Console.ResetColor();
            //    }
            //}
        }

        //--------------------------------------------------------------------------------------------------------------
        private static dynamic LoadEventsFromFile()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "SampleData", fileName);
            string fileData = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject(fileData);
        }

        //--------------------------------------------------------------------------------------------------------------
        //private static void SendEvent(dynamic taxiEvent)
        //{
        //    string serializedEvent = JsonConvert.SerializeObject(taxiEvent);
        //    var eventData = new EventData(Encoding.UTF8.GetBytes(serializedEvent));

        //    eventHubClient.SendAsync(eventData);
        //}
    }
}
