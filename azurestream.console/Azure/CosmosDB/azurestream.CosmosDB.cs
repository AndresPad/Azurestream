using Microsoft.Azure.Cosmos;
using System.Configuration;

namespace azurestream.console
{
    //----------------------------------------------------------------------------------------------------------
    public class CosmosDB
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndPointUri"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "ToDoList";
        private string containerId = "Items";
        //------------------------------------------------------------------------------------------------------
        internal static async Task Execute()
        {
            try
            {
                CosmosDB p = new CosmosDB();
                await p.GetStartedCosmosDemoAsync();
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        //----------------------------------------------------------------------------------------------------------
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        public async Task GetStartedCosmosDemoAsync()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this.ScaleContainerAsync();
            //await this.AddItemsToContainerAsync();
            //await this.QueryItemsAsync();
            //await this.ReplaceFamilyItemAsync();
            //await this.DeleteFamilyItemAsync();
            //await this.DeleteDatabaseAndCleanupAsync();
        }

        //----------------------------------------------------------------------------------------------------------
        /// Create the database if it does not exist
        private async Task CreateDatabaseAsync()
        {
            //1. Create a CosmosDB Database if doesn't exist
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        //----------------------------------------------------------------------------------------------------------
        /// Create the container if it does not exist. 
        /// Specifiy "/LastName" as the partition key since we're storing family information, to ensure good distribution of requests and storage.
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/LastName", 400);
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        //----------------------------------------------------------------------------------------------------------
        /// Scale the throughput provisioned on an existing Container.
        /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            int? throughput = await this.container.ReadThroughputAsync();
            if (throughput.HasValue)
            {
                Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                int newThroughput = throughput.Value + 100;
                // Update throughput
                await this.container.ReplaceThroughputAsync(newThroughput);
                Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
            }

        }

        //Read Documents from CosmosDB
        //----------------------------------------------------------------------------------------------------------


        //----------------------------------------------------------------------------------------------------------


        //----------------------------------------------------------------------------------------------------------

        //----------------------------------------------------------------------------------------------------------

        //----------------------------------------------------------------------------------------------------------

    }
}
