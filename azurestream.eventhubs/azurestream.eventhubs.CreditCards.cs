using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace azurestream.eventhubs
{
    //----------------------------------------------------------------------------------------------------------
    public class CreditCardEventCreator
    {
        private const string EventHubNamespaceConnectionString = "Endpoint=sb://YOUREVENTHUBNAMESPACE.servicebus.windows.net/;SharedAccessKeyName=anomalies;SharedAccessKey=YOURTOKEN";
        private const string EventHubName = "YOUREVENTHUB";
        private const string TransactionsDumpFile = "mocktransactions.csv";
        private static EventHubProducerClient producerClient;
        //------------------------------------------------------------------------------------------------------
        internal static void Execute(string[] args)
        {
            Console.WriteLine("Ready to start collecting");

            MainAsync(args).GetAwaiter().GetResult();
      
            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
        }

        //--------------------------------------------------------------------------------------------------------------
        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Hello Azure EventHubs!");

            // create an Event Hubs Producer client using the namespace connection string and the event hub name
            producerClient = new EventHubProducerClient(EventHubNamespaceConnectionString, EventHubName);

            // send messages to the event hub
            await SendMessagesToEventHubAsync(10000);

            await producerClient.CloseAsync();

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        //--------------------------------------------------------------------------------------------------------------
        // Creates an Event Hub client and sends messages to the event hub.
        private static async Task SendMessagesToEventHubAsync(int numMessagesToSend)
        {
            var eg = new EventGenerator();

            IEnumerable<Transaction> transactions = eg.GenerateEvents(numMessagesToSend);

            if (File.Exists(TransactionsDumpFile))
            {
                // exceptions not handled for brevity
                File.Delete(TransactionsDumpFile);
            }

            await File.AppendAllTextAsync(
                TransactionsDumpFile,
                $"CreditCardId,Timestamp,Location,Amount,Type{Environment.NewLine}");

            int numSuccessfulMessages = 0;
            try
            {
                // create a batch using the producer client
                using (EventDataBatch eventBatch = await producerClient.CreateBatchAsync())
                {
                    foreach (var t in transactions)
                    {
                        // we don't send the transaction type as part of the message.
                        // that is up to the downstream analytics to figure out!
                        // we just pretty print them here so they can easily be compared with the downstream
                        // analytics results.
                        var message = t.Data.ToJson();

                        if (t.Type == TransactionType.Suspect)
                        {
                            var fc = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Yellow;

                            Console.WriteLine($"Suspect transaction: {message}");

                            Console.ForegroundColor = fc; // reset to original
                        }
                        else
                        {
                            Console.WriteLine($"Regular transaction: {message}");
                        }

                        var line = $"{t.Data.CreditCardId},{t.Data.Timestamp.ToString("o")},{t.Data.Location},{t.Data.Amount},{t.Type}{Environment.NewLine}";

                        File.AppendAllText(TransactionsDumpFile, line);

                        // add the message to the batch
                        eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));
                        numSuccessfulMessages++;
                    }
                    // send the batch of messages to the event hub using the producer object
                    await producerClient.SendAsync(eventBatch);
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Environment.NewLine}Exception: {ex.Message}");
            }
            Console.WriteLine();
            Console.WriteLine($"{numSuccessfulMessages} messages sent successfully.");
        }
    }
}
