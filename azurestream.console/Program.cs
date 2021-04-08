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
            //Azure IoT
            //await SimulatedDeviceSample1.ExecuteAsync(args);
            //await SimulatedDeviceSample2.ExecuteAsync(args);
            //await DeviceProvisioningSample1.ExecuteAsync(args);
            //await DeviceProvisioningSample2.ExecuteAsync();
            //await DeviceProvisioningSample3.ExecuteAsync();

            //------------------------------------------------------------------------------------------------------
            //AI Cognative Services
            //CognitiveSearch.Execute();                                        //---Demo Azure Search
            //await AIKeyPhraseExtractionSample.Execute();
            //await AILanguageDetectionSample.Execute();
            //await AIRecognizeEntitiesSample.Execute();
            //await AISentimentAnalysisSample.Execute();


            //------------------------------------------------------------------------------------------------------
            //Cosmos DB
            //await CosmosDBOptimizationDemo.Execute();                    
            //await CosmosDBVolcanoDemo.Execute();                         //Demo - Populate CosmosDB Container
            //await CosmosDBBulkDemo.Execute();                           //Demo - Optimize throughput when bulk importing data to Azure Cosmos DB SQL API account
            //await CosmosDBDemo1_CreatePartitionedContainer.Execute();

            //------------------------------------------------------------------------------------------------------

            await Task.Delay(10);
            Console.WriteLine("\n\n---------------------------------------");
            Console.WriteLine("Ending Application");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("\n\nPress ENTER to exit");
            Console.ReadLine();

        }
    }
}
