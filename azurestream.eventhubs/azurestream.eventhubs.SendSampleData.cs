using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azurestream.eventhubs
{
    //Ingest data from Event Hub into Azure Data Explorer
    //https://docs.microsoft.com/en-us/azure/data-explorer/ingest-data-event-hub#connect-to-the-event-hub
    //----------------------------------------------------------------------------------------------------------
    public class SendSampleData
    {
        private const string EventHubNamespaceCnx = "Endpoint=sb://YOUREVENTHUBNAMESPACE.servicebus.windows.net/;SharedAccessKeyName=YOURSHAREDACCESSPOLICY;SharedAccessKey=YOURACCESSKEY";
        private const string EventHubName = "test-eh";
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
            int counter = 0;
            for (int i = 0; i < 100; i++)
            {
                int recordsPerMessage = 3;
                try
                {
                    var records = Enumerable
                        .Range(0, recordsPerMessage)
                        .Select(recordNumber => $"{{\"timeStamp\": \"{DateTime.UtcNow.AddSeconds(100 * counter)}\", \"name\": \"{$"name {counter}"}\", \"metric\": {counter + recordNumber}, \"source\": \"EventHubMessage\"}}");

                    string recordString = string.Join(Environment.NewLine, records);

                    EventData eventData = new EventData(Encoding.UTF8.GetBytes(recordString));
                    Console.WriteLine($"sending message {counter}");
                    // Optional "dynamic routing" properties for the database, table, and mapping you created. 
                    eventData.Properties.Add("Table", "TestTable");
                    eventData.Properties.Add("IngestionMappingReference", "TestMapping");
                    eventData.Properties.Add("Format", "json");

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

                counter += recordsPerMessage;
            }
        }
    }
}
