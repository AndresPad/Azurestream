using System;
using System.Threading.Tasks;

namespace azurestream.eventhubs
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
            //Event Hubs
            //await SendSampleData.ExecuteAsync();
            //DredgerEventCreator.Execute();
            //await StormEventsData.ExecuteAsync();
            //CreditCardEventCreator.Execute(args1);
            //PollutionDataCollector.Execute(args1);

            await Task.Delay(10);
            Console.WriteLine("\n\n---------------------------------------");
            Console.WriteLine("Ending Application");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("\n\nPress ENTER to exit");
            Console.ReadLine();
        }
    }
}
