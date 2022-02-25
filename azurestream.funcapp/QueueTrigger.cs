using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace azurestream.funcapp
{
    //--------------------------------------------------------------------------------------------------------------
    public class QueueTrigger
    {
        //----------------------------------------------------------------------------------------------------------
        [FunctionName("QueueTrigger")]
        public void Run([QueueTrigger("orders", Connection = "QueueCnx")]Order myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            log.LogInformation($"Received an order: Order {myQueueItem.OrderId}, Product {myQueueItem.ProductId}, Email {myQueueItem.Email}, Price {myQueueItem.Price}");
        }
    }
}
