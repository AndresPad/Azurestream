using System;

namespace azurestream.eventhubs
{
    //--------------------------------------------------------------------------------------------------------------
    class Program
    {
        //----------------------------------------------------------------------------------------------------------
        internal static void Main(string[] args)
        {
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Starting:" + System.Reflection.Assembly.GetExecutingAssembly().GetName().ToString());
            Console.WriteLine("-------------------------------------------------------------------------------------\n\n");

            //------------------------------------------------------------------------------------------------------
            //Event Hubs
            string[] args1 = { "Hello World", "Joe", "Bloggs" };
            CreditCardEventCreator.Execute(args1);
            PollutionDataCollector.Execute(args1);

            Console.WriteLine("\n\n---------------------------------------");
            Console.WriteLine("Ending Application");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("\n\nPress ENTER to exit");
            Console.ReadLine();
        }
    }
}
