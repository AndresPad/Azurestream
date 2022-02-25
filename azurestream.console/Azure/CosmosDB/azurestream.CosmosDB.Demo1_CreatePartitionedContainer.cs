using apa.BOL.CosmosDB;
using Microsoft.Azure.Cosmos;
using System.Configuration;

namespace azurestream.console
{
    //https://github.com/CosmosDB/labs/blob/master/dotnet/labs/01-creating_partitioned_collection.md
    //----------------------------------------------------------------------------------------------------------
    public class CosmosDBDemo1_CreatePartitionedContainer
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["accountEndpoint"];

        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = ConfigurationManager.AppSettings["accountKey"];

        // The name of the database and container we will create
        private static readonly string databaseName = "EntertainmentDatabase";
        private static readonly string containerName = "DefaultCollection";
        //------------------------------------------------------------------------------------------------------
        internal static async Task Execute()
        {
            try
            {
                using CosmosClient client = new CosmosClient(EndpointUri, PrimaryKey);
                DatabaseResponse databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseName);
                Database targetDatabase = databaseResponse.Database;
                await Console.Out.WriteLineAsync($"Database Id:\t{targetDatabase.Id}");


                ContainerResponse response = await targetDatabase.CreateContainerIfNotExistsAsync(containerName, "/id");
                Container defaultContainer = response.Container;
                await Console.Out.WriteLineAsync($"Default Container Id:\t{defaultContainer.Id}");

                IndexingPolicy indexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true,
                    IncludedPaths =
                    {
                        new IncludedPath
                        {
                            Path = "/*"
                        }
                    }
                };

                ContainerProperties containerProperties = new ContainerProperties("CustomCollection", $"/{nameof(IInteraction.type)}")
                {
                    IndexingPolicy = indexingPolicy,
                };
                var containerResponse = await targetDatabase.CreateContainerIfNotExistsAsync(containerProperties, 10000);
                var customContainer = containerResponse.Container;
                //var customContainer = targetDatabase.GetContainer("CustomCollection");
                await Console.Out.WriteLineAsync($"Custom Container Id:\t{customContainer.Id}");

                //For the next few instructions, we will use the Bogus library to create test data. 
                //This library allows you to create a collection of objects with fake data set on each object's property.
                //As a reminder, the Bogus library generates a set of test data.In this example, you are creating 500 items using the Bogus 
                //library and the rules listed above. The GenerateLazy method tells the Bogus library to prepare for a request of 500 items by 
                //returning a variable of type IEnumerable.Since LINQ uses deferred execution by default, the items aren't actually created until the collection is iterated.
               var foodInteractions = new Bogus.Faker<PurchaseFoodOrBeverage>()
                   .RuleFor(i => i.id, (fake) => Guid.NewGuid().ToString())
                   .RuleFor(i => i.type, (fake) => nameof(PurchaseFoodOrBeverage))
                   .RuleFor(i => i.unitPrice, (fake) => Math.Round(fake.Random.Decimal(1.99m, 15.99m), 2))
                   .RuleFor(i => i.quantity, (fake) => fake.Random.Number(1, 5))
                   .RuleFor(i => i.totalPrice, (fake, user) => Math.Round(user.unitPrice * user.quantity, 2))
                   .GenerateLazy(500);

                foreach (var interaction in foodInteractions)
                {
                    //The CreateItemAsync method of the CosmosItems class takes in an object that you would like to serialize into JSON and store 
                    //as a document within the specified container. The id property, which here we've assigned to a unique Guid on each object, 
                    //is a special required value in Cosmos DB that is used for indexing and must be unique for every item in a container.
                    ItemResponse<PurchaseFoodOrBeverage> result = await customContainer.CreateItemAsync(interaction);
                    await Console.Out.WriteLineAsync($"Item Created\t{result.Resource.id}");
                }

                var tvInteractions = new Bogus.Faker<WatchLiveTelevisionChannel>()
                    .RuleFor(i => i.id, (fake) => Guid.NewGuid().ToString())
                    .RuleFor(i => i.type, (fake) => nameof(WatchLiveTelevisionChannel))
                    .RuleFor(i => i.minutesViewed, (fake) => fake.Random.Number(1, 45))
                    .RuleFor(i => i.channelName, (fake) => fake.PickRandom(new List<string> { "NEWS-6", "DRAMA-15", "ACTION-12", "DOCUMENTARY-4", "SPORTS-8" }))
                    .GenerateLazy(500);

                foreach (var interaction in tvInteractions)
                {
                    ItemResponse<WatchLiveTelevisionChannel> result = await customContainer.CreateItemAsync(interaction);
                    await Console.Out.WriteLineAsync($"Item Created\t{result.Resource.id}");
                }

                var mapInteractions = new Bogus.Faker<ViewMap>()
                    .RuleFor(i => i.id, (fake) => Guid.NewGuid().ToString())
                    .RuleFor(i => i.type, (fake) => nameof(ViewMap))
                    .RuleFor(i => i.minutesViewed, (fake) => fake.Random.Number(1, 45))
                    .GenerateLazy(500);

                foreach (var interaction in mapInteractions)
                {
                    ItemResponse<ViewMap> result = await customContainer.CreateItemAsync(interaction);
                    await Console.Out.WriteLineAsync($"Item Created\t{result.Resource.id}");
                }

                FeedIterator<GeneralInteraction> query = customContainer.GetItemQueryIterator<GeneralInteraction>("SELECT * FROM c");
                while (query.HasMoreResults)
                {
                    foreach (GeneralInteraction interaction in await query.ReadNextAsync())
                    {
                        Console.Out.WriteLine($"[{interaction.type}]\t{interaction.id}");
                    }
                }
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("End of Creating Partition demo, press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}