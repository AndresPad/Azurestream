// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


Console.WriteLine("-------------------------------------------------------------------------------------");
Console.WriteLine("Starting:" + System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString());
Console.WriteLine("-------------------------------------------------------------------------------------\n\n");

//------------------------------------------------------------------------------------------------------
//Azure Data Explorer (ADX)
//AzADX_GitHubToADX.Execute();

//------------------------------------------------------------------------------------------------------
//Azure IoT
//await SimulatedDeviceSample_RandomTelemetry.ExecuteAsync();
//await SimulatedDeviceSample_RandomVibration.ExecuteAsync(args);
//await SimulatedDeviceSample_RandomTemp.ExecuteAsync(args);
//await SimulatedDeviceSample2.ExecuteAsync(args);
//await DeviceProvisioningSample1.ExecuteAsync(args);
//await DeviceProvisioningSample2.ExecuteAsync();
//await DeviceProvisioningSample3.ExecuteAsync();

//------------------------------------------------------------------------------------------------------
//Messaging
//Event Hubs
//EventHubSender.Execute();
//await EventHubReceiver.Execute();

//------------------------------------------------------------------------------------------------------
//AI Cognative Services
//CognitiveSearch.Execute();                                        //---Demo Azure Search
//await AIKeyPhraseExtractionSample.Execute();
//await AILanguageDetectionSample.Execute();
//await AIRecognizeEntitiesSample.Execute();
//await AISentimentAnalysisSample.Execute();


//------------------------------------------------------------------------------------------------------
//Azure Storage
//Blob storage
//await AzStorage_Blobs.Execute();

//Storage Queues
//AzStorage_QueuesCreate.Execute();

//Storage Tables

//------------------------------------------------------------------------------------------------------
//Cloud Design Patterns
//FluentSdk.Execute();
//await RetryPattern.Execute();
//TransientErrors.Execute();


//------------------------------------------------------------------------------------------------------
//Cosmos DB
//await CosmosDBOptimizationDemo.Execute();                    
//await CosmosDBVolcanoDemo.Execute();                        //Demo - Populate CosmosDB Container
//await CosmosDBBulkDemo.Execute();                           //Demo - Optimize throughput when bulk importing data to Azure Cosmos DB SQL API account
//await CosmosDBDemo1_CreatePartitionedContainer.Execute();


await Task.Delay(10);
Console.WriteLine("\n\n---------------------------------------");
Console.WriteLine("Ending Application");
Console.WriteLine("---------------------------------------");
Console.WriteLine("\n\nPress ENTER to exit");
Console.ReadLine();
