using azurestream.eventhubs.Business;
using azurestream.eventhubs.Models;
using Microsoft.Azure.EventHubs;
using System;
using System.Text;
using System.Threading.Tasks;

namespace azurestream.eventhubs
{
    //It generates random pollution data.
    //----------------------------------------------------------------------------------------------------------
    public class PollutionDataCollector
    {
        private static EventHubClient eventHubClient;
        private const string eventHubConnectionString = "Endpoint=sb://YOUREVENTHUBNAMESPACE.servicebus.windows.net/;SharedAccessKeyName=anomalies;SharedAccessKey=YOURACCESSTOKEN";
        private const string eventHubName = "YOUREVENTHUB";
        //------------------------------------------------------------------------------------------------------
        internal static void Execute(string[] args)
        {
            Console.WriteLine("Registering EventProcessor...");
            var collector = new PollutionCollector();
            Console.WriteLine("Ready to start collecting");
            MainAsync(args, collector).GetAwaiter().GetResult();

           
            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
        }

        static async Task MainAsync(string[] args, IPollutionCollector collector)
        {
            try
            {
                var connectionStringBuilder = new EventHubsConnectionStringBuilder(eventHubConnectionString)
                {
                    EntityPath = eventHubName
                };

                eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

                for (int i = 0; i <= 100000; i++)
                {
                    var dataSample = collector.Collect();
                    Console.WriteLine($"Sample {i} - ID: {dataSample.ReadingId} - Date: {dataSample.ReadingDateTime} - Location ID: {dataSample.LocationId} - Pollution Level: {dataSample.PollutionLevel}");
                    await SendMessagesToEventHub(dataSample);
                }

                await eventHubClient.CloseAsync();
                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {ex.Message}");
            }
        }

        private static async Task SendMessagesToEventHub(PollutionData data)
        {
            try
            {
                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(data.ToJSON())));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} > Exception: {ex.Message}");
            }
        }
    }
}
