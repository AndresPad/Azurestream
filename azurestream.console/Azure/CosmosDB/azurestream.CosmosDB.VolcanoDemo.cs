using apa.BOL.CosmosDB;
using Bogus;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Configuration;

namespace azurestream.console
{
    //----------------------------------------------------------------------------------------------------------
    public class CosmosDBVolcanoDemo
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["accountEndpoint"];
        
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["accountKey"];

        // The name of the database and container we will create
        private static readonly string databaseName = "VolcanoList";
        private static readonly string containerName = "VolcanoItems";
        private static readonly string fileName = "VolcanoData.json";
        //------------------------------------------------------------------------------------------------------
        internal static async Task Execute()
        {
            try
            {
                Console.WriteLine("Initializing Cosmos DB Cnx");
                Console.WriteLine("--Database Name: " + databaseName);
                Console.WriteLine("--Container Name: " + containerName);

                var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBVolcanoQuickstart" });
                var db = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
                var container = await db.Database.CreateContainerIfNotExistsAsync(containerName, "/id", 10000);

                Console.WriteLine("Loading Volcano data from json file");
                var data = await GetJsonDataAsync();

                Console.WriteLine("Enriching volcano data with measurements");
                PopulateMeasurementData(ref data);

                Console.WriteLine("Bulk uploading data to Cosmos DB");
                await BulkUploadDataToCosmosDB(data, container.Container);

                Console.WriteLine("Delete Database");
                DatabaseResponse databaseResourceResponse = await db.Database.DeleteAsync();
                //Also valid: await cosmosClient.GetDatabase(databaseName).DeleteAsync();
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of Volcano demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        //----------------------------------------------------------------------------------------------------------
        private static async Task<List<Volcano>> GetJsonDataAsync()
        {
            var data = await File.ReadAllTextAsync(fileName);
            return JsonConvert.DeserializeObject<List<Volcano>>(data);
        }

        //----------------------------------------------------------------------------------------------------------
        private static void PopulateMeasurementData(ref List<Volcano> volcanoes)
        {
            foreach (var volcano in volcanoes)
            {
                var faker = new Faker<Measurements>()
                    .StrictMode(false)
                    .RuleFor(m => m.CO2, f => f.Random.Float(0.0f, 10.0f).ToString())
                    .RuleFor(m => m.H2S, f => f.Random.Int(100, 1000).ToString())
                    .RuleFor(m => m.HCL, f => f.Random.Float(0.1f, 5f).ToString())
                    .RuleFor(m => m.HF, f => f.Random.Int(1, 100).ToString())
                    .RuleFor(m => m.NaOH, f => f.Random.Float(1.0f, 9.0f).ToString())
                    .RuleFor(m => m.SClratio, f => f.Random.Float(0.1f, 10f).ToString())
                    .RuleFor(m => m.SO2, f => f.Random.Int(1000, 50000).ToString());

                volcano.Measurements = faker.Generate();
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
    }
}
