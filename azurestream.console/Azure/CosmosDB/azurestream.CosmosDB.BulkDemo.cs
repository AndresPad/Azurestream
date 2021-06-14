using apa.BOL.CosmosDB;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace azurestream.console
{
    //https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-sql-api-dotnet-bulk-import
    //https://github.com/Azure-Samples/cosmos-dotnet-bulk-import-throughput-optimizer/blob/main/src/Program.cs
    //----------------------------------------------------------------------------------------------------------
    public class CosmosDBBulkDemo
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["accountEndpoint"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["accountKey"];

        // The name of the database and container we will create
        private static readonly string databaseName = "Bulk-Demo";
        private static readonly string containerName = "Items";
        private const int ItemsToInsert = 300000;
        //------------------------------------------------------------------------------------------------------
        internal static async Task Execute()
        {
            try
            {
                Console.WriteLine("Initializing Cosmos DB Cnx");
                Console.WriteLine("--Database Name: " + databaseName);
                Console.WriteLine("--Container Name: " + containerName);

                //CreateClient
                var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { AllowBulkExecution = true });

                //Create with a throughput of 50000 RU/s
                //Indexing Policy to exclude all attributes to maximize RU/s usage
                Console.WriteLine("\n\nCreating a 50000 RU/s container");

                //Initialize
                Database db = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

                await db.DefineContainer(containerName, "/pk")
                        .WithIndexingPolicy()
                        .WithIndexingMode(IndexingMode.Consistent)
                        .WithIncludedPaths()
                            .Attach()
                        .WithExcludedPaths()
                            .Path("/*")
                            .Attach()
                        .Attach()
                        .CreateIfNotExistsAsync(50000);

                // Prepare items for insertion
                Console.WriteLine($"\n\nPreparing {ItemsToInsert} items to insert...");

                // Operations
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (BulkItem item in GetItemsToInsert())
                {
                    MemoryStream stream = new MemoryStream();
                    await System.Text.Json.JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.pk), stream);
                }

                // Create the list of Tasks
                Console.WriteLine($"\n\nStarting Bulk Inserts...");
                Stopwatch stopwatch = Stopwatch.StartNew();

                //ConcurrentTasks
                Container container = db.GetContainer(containerName);
                List<Task> tasks = new List<Task>(ItemsToInsert);

                foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
                {
                    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
                        .ContinueWith((Task<ResponseMessage> task) =>
                        {
                            using ResponseMessage response = task.Result;
                            if (!response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                            }
                        }));
                }

                await Task.WhenAll(tasks);
                stopwatch.Stop();

                Console.WriteLine($"\n\nFinished in writing {ItemsToInsert} items in {stopwatch.Elapsed}.");

                Console.WriteLine("Delete Database");
                DatabaseResponse databaseResourceResponse = await db.DeleteAsync();
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of Bulk Demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        //----------------------------------------------------------------------------------------------------------
        private static async Task BulkUploadDataToCosmosDB(List<Volcano> data, Container container)
        {
            if (data == null || data.Count == 0)
            {
                return;
            }

            List<Task> tasks = new List<Task>(data.Count);
            foreach (var volcano in data)
            {
                tasks.Add(container.CreateItemAsync(volcano));
            }
            await Task.WhenAll(tasks);
        }

        //----------------------------------------------------------------------------------------------------------
        private static IReadOnlyCollection<BulkItem> GetItemsToInsert()
        {
            return new Bogus.Faker<BulkItem>()
            .StrictMode(true)
            //Generate item
            .RuleFor(o => o.id, f => Guid.NewGuid().ToString()) //id
            .RuleFor(o => o.username, f => f.Internet.UserName())
            .RuleFor(o => o.pk, (f, o) => o.id) //partitionkey
            .Generate(ItemsToInsert);
        }
    }

    //----------------------------------------------------------------------------------------------------------
    public class BulkItem
    {
        public string id { get; set; }
        public string pk { get; set; }

        public string username { get; set; }
    }
}