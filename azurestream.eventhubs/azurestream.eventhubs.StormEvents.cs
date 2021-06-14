using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using azurestream.eventhubs.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azurestream.eventhubs
{
    //Ingest data from Event Hub into Azure Data Explorer
    //https://docs.microsoft.com/en-us/azure/data-explorer/ingest-data-event-hub
    //----------------------------------------------------------------------------------------------------------
    public class StormEventsData
    {
        private const string EventHubNamespaceCnx = "Endpoint=sb://YOUREVENTHUBNAMESPACE.servicebus.windows.net/;SharedAccessKeyName=YOURSHAREDACCESSPOLICY;SharedAccessKey=YOURACCESSKEY";
        private const string EventHubName = "adx-eh";
        //------------------------------------------------------------------------------------------------------
        internal static async Task ExecuteAsync()
        {
            Console.WriteLine("Registering Event Hub Client...");
            Console.WriteLine("Ready to start sending messages to Event Hub:" + EventHubName);

            await EventHubIngestionAsync();

            Console.WriteLine("Receiving Messages. Press enter key to stop worker.");
            Console.ReadLine();
        }

        //--------------------------------------------------------------------------------------------------------------
        public static async Task EventHubIngestionAsync()
        {
            //create an Event Hubs Producer client using the namespace connection string and the event hub name
            await using var producerClient = new EventHubProducerClient(EventHubNamespaceCnx, EventHubName);

            List<StormEvent> StormEvents = JsonConvert.DeserializeObject<List<StormEvent>>(
               File.ReadAllText(@"StormEvents.json")
           );

            foreach (StormEvent se in StormEvents)
            {
                try
                {
                    EventData eventData = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(se)));
                    Console.WriteLine($"sending message..." + "\n" + JsonConvert.SerializeObject(se));

                    using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
                    eventBatch.TryAdd(eventData);

                    await producerClient.SendAsync(eventBatch);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }
            }
        }
    }
}
