using apa.BOL.EventHubs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using azurestream.eventhubs.Business;
using System;
using System.Text;
using System.Threading.Tasks;

namespace azurestream.eventhubs
{
    //Sample generates random pollution data.
    //----------------------------------------------------------------------------------------------------------
    public class PollutionDataCollector
    {
        private const string EventHubNamespaceCnx = "Endpoint=sb://YOUREVENTHUBNAMESPACE.servicebus.windows.net/;SharedAccessKeyName=YOURSHAREDACCESSPOLICY;SharedAccessKey=YOURACCESSKEY";
        private const string EventHubName = "anomaly-eh";
        //------------------------------------------------------------------------------------------------------
        internal static void Execute(string[] args)
        {
            Console.WriteLine("Registering Event Hub Client...");
            Console.WriteLine("Ready to start sending messages to Event Hub:" + EventHubName);
			
            MainAsync(args, new PollutionCollector()).GetAwaiter().GetResult();
           
            Console.WriteLine("Receiving Messages. Press enter key to stop worker.");
            Console.ReadLine();
        }

        //--------------------------------------------------------------------------------------------------------------
        static async Task MainAsync(string[] args, IPollutionCollector collector)
        {
            try
            {
                await using (var producerClient = new EventHubProducerClient(EventHubNamespaceCnx, EventHubName))
                {

                    for (int i = 0; i <= 100000; i++)
                    {
                        var dataSample = collector.Collect();
                        Console.WriteLine($"Sample {i} - ID: {dataSample.ReadingId} - Date: {dataSample.ReadingDateTime} - Location ID: {dataSample.LocationId} - Pollution Level: {dataSample.PollutionLevel}");
                        await SendMessagesToEventHub(dataSample, producerClient);
                    }                
                }

                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {ex.Message}");
            }
        }

		//--------------------------------------------------------------------------------------------------------------
        private static async Task SendMessagesToEventHub(PollutionData data, EventHubProducerClient producerClient)
        {
            try
            {
                // Create a batch of events 
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
                var eventData = new EventData(Encoding.UTF8.GetBytes(data.ToJSON()));

                eventBatch.TryAdd(eventData);

                // Use the producer client to send the batch of events to the event hub
                await producerClient.SendAsync(eventBatch);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {ex.Message}");
            }
        }
    }
}
