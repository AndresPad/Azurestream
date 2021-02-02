using System;
using System.Threading.Tasks;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    class Program
    {
        //----------------------------------------------------------------------------------------------------------
        static async Task Main(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Starting:" + System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString());
            Console.WriteLine("-------------------------------------------------------------------------------------\n\n");


            //------------------------------------------------------------------------------------------------------
            //AI Cognative Services
            //SearchIndex.Execute();                                        //---Demo Azure Search
            //await AIKeyPhraseExtractionSample.Execute();
            //await AILanguageDetectionSample.Execute();
            //await AIRecognizeEntitiesSample.Execute();
            //await AISentimentAnalysisSample.Execute();


            //------------------------------------------------------------------------------------------------------
            //Cosmos DB
            await CosmosDBOptimizationDemo.Execute();                    
            //await CosmosDBVolcanoDemo.Execute();                         //Demo - Populate CosmosDB Container
            //await CosmosDBBulkDemo.Execute();                           //Demo - Optimize throughput when bulk importing data to Azure Cosmos DB SQL API account
            //await CosmosDBDemo1_CreatePartitionedContainer.Execute();


            Console.WriteLine("\n\n---------------------------------------");
            Console.WriteLine("Ending Application");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("\n\nPress ENTER to exit");
            Console.ReadLine();

        }
    }
}
