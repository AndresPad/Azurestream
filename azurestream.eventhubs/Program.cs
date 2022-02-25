using azurestream.eventhubs;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("-------------------------------------------------------------------------------------");
Console.WriteLine("Starting:" + System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString());
Console.WriteLine("-------------------------------------------------------------------------------------\n\n");

//------------------------------------------------------------------------------------------------------
//Event Hubs
//await SendSampleData.ExecuteAsync();
//DredgerEventCreator.Execute();
//await StormEventsData.ExecuteAsync();
await TaxiEventsData.ExecuteAsync();
//CreditCardEventCreator.Execute(args);
//PollutionDataCollector.Execute(args);

await Task.Delay(10);
Console.WriteLine("\n\n---------------------------------------");
Console.WriteLine("Ending Application");
Console.WriteLine("---------------------------------------");
Console.WriteLine("\n\nPress ENTER to exit");
Console.ReadLine();
