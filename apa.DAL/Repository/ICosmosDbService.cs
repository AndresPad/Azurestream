using apa.BOL.CosmosDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace apa.DAL.Repository
{
    //--------------------------------------------------------------------------------------------------------------
    public interface ICosmosDbService
    {
        Task<IEnumerable<Item>> GetItemsAsync(string query);
        Task<Item> GetItemAsync(string id);
        Task AddItemAsync(Item item);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(string id);  
    }
}
