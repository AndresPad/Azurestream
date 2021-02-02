using apa.BOL.CosmosDB;
using apa.DAL.Repository;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azurestream.Services
{
    //------------------------------------------------------------------------------------------------------------------
    public class XCosmosDbService : ICosmosDbService
    {
        private readonly Container _container;
        //--------------------------------------------------------------------------------------------------------------
        public XCosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task AddItemAsync(Item item)
        {
            // Measure the performance (Request Units) of writes
            ItemResponse<Item> response = await _container.CreateItemAsync<Item>(item, new PartitionKey(item.Id));
            Console.WriteLine("Insert of item consumed {0} request units", response.RequestCharge);
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<Item>(id, new PartitionKey(id));
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<Item> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<Item> response = await this._container.ReadItemAsync<Item>(id, new PartitionKey(id));

                //https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips-dotnet-sdk-v3-sql
                //There's a mechanism for logging additional diagnostics information and troubleshooting latency issues, as shown in the following sample. 
                //You can log the diagnostics string for requests that have a higher read latency. 
                response.Diagnostics.ToString();
                return response;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task<IEnumerable<Item>> GetItemsAsync(string queryString)
        {
            //Measure the performance (Request Units) of queries
            FeedIterator<Item> queryable = _container.GetItemQueryIterator<Item>(new QueryDefinition(queryString));
            List<Item> results = new List<Item>();
            while (queryable.HasMoreResults)
            {
                FeedResponse<Item> queryResponse = await queryable.ReadNextAsync();
                Console.WriteLine("Query batch consumed {0} request units", queryResponse.RequestCharge);

                results.AddRange(queryResponse.ToList());
            }

            return results;
        }

        //--------------------------------------------------------------------------------------------------------------
        public async Task UpdateItemAsync(Item item)
        {
            await this._container.UpsertItemAsync<Item>(item, new PartitionKey(item.Id));
        }
    }
}