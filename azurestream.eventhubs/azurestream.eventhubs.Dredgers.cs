using apa.BOL.EventHubs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using azurestream.eventhubs.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace azurestream.eventhubs
{
    //Sample generates random dredger data.
    //https://docs.microsoft.com/en-us/azure/data-explorer/ingest-data-event-hub-overview?WT.mc_id=Portal-Microsoft_Azure_Kusto#set-events-routing
    //.create table Dredgers ingestion json mapping 'DredgerMapping' '[{"column":"DredgerId", "Properties": {"Path": "$.DredgerId"}}, {"column":"Amount", "Properties": {"Path":"$.Amount"}}, {"column":"Location", "Properties": {"Path":"$.Location"}}, {"column":"Timestamp", "Properties": {"Path":"$.Timestamp"}}, {"column":"LOAD_NUMBER", "Properties": {"Path":"$.LOAD_NUMBER"}}, {"column":"VESSEL_X", "Properties": {"Path":"$.VESSEL_X"}}]'
    //.create table Dredgers (DREDGE_ID: string , DREDGE_NAME: string, DATE_TIME: datetime, LOAD_NUMBER: int, VESSEL_X: decimal, VESSEL_Y: decimal, PORT_DRAG_X: decimal, PORT_DRAG_Y: decimal, PIQ: int) 
    //.create table Dredgers ingestion json mapping 'DredgerMapping' '[{"column":"DREDGE_ID", "Properties": {"Path": "$.DREDGE_ID"}}, {"column":"DREDGE_NAME", "Properties": {"Path":"$.DREDGE_NAME"}}, {"column":"DATE_TIME", "Properties": {"Path":"$.DATE_TIME"}}, {"column":"LOAD_NUMBER", "Properties": {"Path":"$.LOAD_NUMBER"}}, {"column":"VESSEL_X", "Properties": {"Path":"$.VESSEL_X"}}, {"column":"VESSEL_Y", "Properties": {"Path":"$.VESSEL_Y"}}, {"column":"PORT_DRAG_X", "Properties": {"Path":"$.PORT_DRAG_X"}}, {"column":"PORT_DRAG_Y", "Properties": {"Path":"$.PORT_DRAG_Y"}}, {"column":"PIQ", "Properties": {"Path":"$.PIQ"}}]'
    //.create table Dredgers ingestion json mapping 'DredgerMapping' '[{"column":"DREDGE_ID", "Properties": {"Path": "$.DREDGE_ID"}}, {"column":"DREDGE_NAME", "Properties": {"Path":"$.DREDGE_NAME"}}, {"column":"DATE_TIME", "Properties": {"Path":"$.DATE_TIME"}}, {"column":"LOAD_NUMBER", "Properties": {"Path":"$.LOAD_NUMBER"}}, {"column":"VESSEL_X", "Properties": {"Path":"$.VESSEL_X"}}, {"column":"VESSEL_Y", "Properties": {"Path":"$.VESSEL_Y"}}, {"column":"PORT_DRAG_X", "Properties": {"Path":"$.PORT_DRAG_X"}}, {"column":"PORT_DRAG_Y", "Properties": {"Path":"$.PORT_DRAG_Y"}},, {"column":"STBD_DRAG_X", "Properties": {"Path":"$.STBD_DRAG_X"}}, {"column":"STBD_DRAG_Y", "Properties": {"Path":"$.STBD_DRAG_Y"}}, {"column":"HULL_STATUS", "Properties": {"Path":"$.HULL_STATUS"}}, {"column":"VESSEL_COURSE", "Properties": {"Path":"$.VESSEL_COURSE"}}, {"column":"VESSEL_SPEED", "Properties": {"Path":"$.VESSEL_SPEED"}, {"column":"VESSEL_HEADING", "Properties": {"Path":"$.VESSEL_HEADING"}}, {"column":"TIDE", "Properties": {"Path":"$.TIDE"}}, {"column":"DRAFT_FORE", "Properties": {"Path":"$.DRAFT_FORE"}}, {"column":"DRAFT_AFT", "Properties": {"Path":"$.DRAFT_AFT"}}, {"column":"ULLAGE_FORE", "Properties": {"Path":"$.ULLAGE_FORE"}}, {"column":"ULLAGE_AFT", "Properties": {"Path":"$.ULLAGE_AFT"}}, {"column":"HOPPER_VOLUME", "Properties": {"Path":"$.HOPPER_VOLUME"}}, {"column":"DISPLACEMENT", "Properties": {"Path":"$.DISPLACEMENT"}}, {"column":"EMPTY_DISPLACEMENT", "Properties": {"Path":"$.EMPTY_DISPLACEMENT"}}, {"column":"DRAGHEAD_DEPTH_PORT", "Properties": {"Path":"$.DRAGHEAD_DEPTH_PORT"}}, {"column":"DRAGHEAD_DEPTH_STBD", "Properties": {"Path":"$.DRAGHEAD_DEPTH_STBD"}}, {"column":"PORT_DENSITY", "Properties": {"Path":"$.PORT_DENSITY"}}, {"column":"STBD_DENSITY", "Properties": {"Path":"$.STBD_DENSITY"}}, {"column":"PORT_VELOCITY", "Properties": {"Path":"$.PORT_VELOCITY"}}, {"column":"STBD_VELOCITY", "Properties": {"Path":"$.STBD_VELOCITY"}}, {"column":"PUMP_RPM_PORT", "Properties": {"Path":"$.PUMP_RPM_PORT"}}, {"column":"PUMP_RPM_STBD", "Properties": {"Path":"$.PUMP_RPM_STBD"}}, {"column":"MIN_PUMP_EFFORT_PORT", "Properties": {"Path":"$.MIN_PUMP_EFFORT_PORT"}}, {"column":"MIN_PUMP_EFFORT_STBD", "Properties": {"Path":"$.MIN_PUMP_EFFORT_STBD"}}, {"column":"PUMP_WATER_PORT", "Properties": {"Path":"$.PUMP_WATER_PORT"}}, {"column":"PUMP_WATER_STBD", "Properties": {"Path":"$.PUMP_WATER_STBD"}}, {"column":"PUMP_MATERIAL_PORT", "Properties": {"Path":"$.PUMP_MATERIAL_PORT"}}, {"column":"PUMP_MATERIAL_STBD", "Properties": {"Path":"$.PUMP_MATERIAL_STBD"}}, {"column":"PUMP_OUT_ON", "Properties": {"Path":"$.PUMP_OUT_ON"}}, {"column":"PIQ", "Properties": {"Path":"$.PIQ"}}]'
    //.create table Dredgers(DREDGE_ID: string , DREDGE_NAME: string, DATE_TIME: datetime, LOAD_NUMBER: int, VESSEL_X: decimal, VESSEL_Y: decimal, PORT_DRAG_X: decimal, PORT_DRAG_Y: decimal, STBD_DRAG_X: decimal, STBD_DRAG_Y: decimal, HULL_STATUS: string, VESSEL_COURSE: int, VESSEL_SPEED: decimal, VESSEL_HEADING: decimal, TIDE: decimal, DRAFT_FORE: decimal, DRAFT_AFT: decimal, ULLAGE_FORE: decimal, ULLAGE_AFT: decimal, HOPPER_VOLUME: decimal, DISPLACEMENT: decimal, EMPTY_DISPLACEMENT: decimal, DRAGHEAD_DEPTH_PORT: decimal, DRAGHEAD_DEPTH_STBD: decimal, PORT_DENSITY: decimal, STBD_DENSITY: decimal, PORT_VELOCITY: decimal, STBD_VELOCITY: decimal, PUMP_RPM_PORT: decimal, PUMP_RPM_STBD: decimal, MIN_PUMP_EFFORT_PORT: bool, MIN_PUMP_EFFORT_STBD: bool, PUMP_WATER_PORT: bool, PUMP_WATER_STBD: bool, PUMP_MATERIAL_PORT: bool, PUMP_MATERIAL_STBD: bool, PUMP_OUT_ON: bool, PIQ: int)
    //.show table Dredgers ingestion mappings
    //----------------------------------------------------------------------------------------------------------
    public class DredgerEventCreator
    {
        private const string EventHubNamespaceCnx = "Endpoint=sb://YOUREVENTHUBNAMESPACE.servicebus.windows.net/;SharedAccessKeyName=YOURSHAREDACCESSPOLICY;SharedAccessKey=YOURACCESSKEY";
        private const string EventHubName = "dredge-eh";
        private const string TransactionsDumpFile = "mocktransactions.csv";
        private static EventHubProducerClient producerClient;
        //------------------------------------------------------------------------------------------------------
        internal static void Execute()
        {
            Console.WriteLine("Registering Event Hub Client...");
            Console.WriteLine("Ready to start sending messages to Event Hub:" + EventHubName);

            MainAsync().GetAwaiter().GetResult();
      
            Console.WriteLine("Receiving Messages. Press enter key to stop worker.");
            Console.ReadLine();
        }

        //--------------------------------------------------------------------------------------------------------------
        static async Task MainAsync()
        {
            //create an Event Hubs Producer client using the namespace connection string and the event hub name
            producerClient = new EventHubProducerClient(EventHubNamespaceCnx, EventHubName);

            // send messages to the event hub
            await SendMessagesToEventHubAsync(10000);

            await producerClient.CloseAsync();

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        //--------------------------------------------------------------------------------------------------------------
        // Creates an Event Hub client and sends messages to the event hub.
        private static async Task SendMessagesToEventHubAsync(int numMessagesToSend)
        {
            var eg = new DredgerEventGenerator();

            IEnumerable<DredgerTelemetry> transactions = eg.GenerateEvents(numMessagesToSend);

            if (File.Exists(TransactionsDumpFile))
            {
                // exceptions not handled for brevity
                File.Delete(TransactionsDumpFile);
            }

            await File.AppendAllTextAsync(
                TransactionsDumpFile,
                //$"DREDGE_ID,DREDGE_NAME,DATE_TIME,LOAD_NUMBER,VESSEL_X,VESSEL_Y,PORT_DRAG_X,PORT_DRAG_Y,PIQ,Type{Environment.NewLine}");
                $"DREDGE_ID,DREDGE_NAME,DATE_TIME,LOAD_NUMBER,VESSEL_X,VESSEL_Y,PORT_DRAG_X,PORT_DRAG_Y,STBD_DRAG_X,STBD_DRAG_Y,HULL_STATUS,VESSEL_COURSE,VESSEL_SPEED,VESSEL_HEADING,TIDE,DRAFT_FORE,DRAFT_AFT,ULLAGE_FORE,ULLAGE_AFT,HOPPER_VOLUME,DISPLACEMENT,EMPTY_DISPLACEMENT,DRAGHEAD_DEPTH_PORT,DRAGHEAD_DEPTH_STBD,PORT_DENSITY,STBD_DENSITY,PORT_VELOCITY,STBD_VELOCITY,PUMP_RPM_PORT,PUMP_RPM_STBD,MIN_PUMP_EFFORT_PORT,MIN_PUMP_EFFORT_STBD,PUMP_WATER_PORT,PUMP_WATER_STBD,PUMP_MATERIAL_PORT,PUMP_MATERIAL_STBD,PUMP_OUT_ON,PIQ,Type{Environment.NewLine}");

        int numSuccessfulMessages = 0;
            try
            {
                // create a batch using the producer client
                using (EventDataBatch eventBatch = await producerClient.CreateBatchAsync())
                {
                    foreach (var t in transactions)
                    {
                        // we don't send the transaction type as part of the message.
                        // that is up to the downstream analytics to figure out!
                        // we just pretty print them here so they can easily be compared with the downstream
                        // analytics results.
                        var message = t.Data.ToJson();

                        if (t.Type == TelemetryType.Suspect)
                        {
                            var fc = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Yellow;

                            Console.WriteLine($"Suspect telemetry: {message}");

                            Console.ForegroundColor = fc; // reset to original
                        }
                        else
                        {
                            Console.WriteLine($"Regular telemetry: {message}");
                        }

                        //var line = $"{t.Data.DREDGE_ID},{t.Data.DREDGE_NAME},{t.Data.DATE_TIME.ToString("o")},{t.Data.LOAD_NUMBER},{t.Data.VESSEL_X},{t.Data.VESSEL_Y},{t.Data.PORT_DRAG_X},{t.Data.PORT_DRAG_Y},{t.Data.PIQ},{t.Type}{Environment.NewLine}";
                        var line = $"{t.Data.DREDGE_ID},{t.Data.DREDGE_NAME},{t.Data.DATE_TIME.ToString("o")},{t.Data.LOAD_NUMBER},{t.Data.VESSEL_X},{t.Data.VESSEL_Y},{t.Data.PORT_DRAG_X},{t.Data.PORT_DRAG_Y},{t.Data.STBD_DRAG_X},{t.Data.STBD_DRAG_Y},{t.Data.HULL_STATUS},{t.Data.VESSEL_COURSE},{t.Data.VESSEL_SPEED},{t.Data.VESSEL_HEADING},{t.Data.TIDE},{t.Data.DRAFT_FORE},{t.Data.DRAFT_AFT},{ t.Data.ULLAGE_FORE},{t.Data.ULLAGE_AFT},{t.Data.HOPPER_VOLUME},{t.Data.DISPLACEMENT},{t.Data.EMPTY_DISPLACEMENT},{t.Data.DRAGHEAD_DEPTH_PORT},{t.Data.DRAGHEAD_DEPTH_STBD},{t.Data.PORT_DENSITY},{t.Data.STBD_DENSITY},{t.Data.PORT_VELOCITY},{t.Data.STBD_VELOCITY},{t.Data.PUMP_RPM_PORT},{t.Data.PUMP_RPM_STBD},{t.Data.MIN_PUMP_EFFORT_PORT},{t.Data.MIN_PUMP_EFFORT_STBD},{t.Data.PUMP_WATER_PORT},{t.Data.PUMP_WATER_STBD},{t.Data.PUMP_MATERIAL_PORT},{t.Data.PUMP_MATERIAL_STBD},{t.Data.PUMP_OUT_ON},{t.Data.PIQ},{t.Type}{Environment.NewLine}";
 
                        File.AppendAllText(TransactionsDumpFile, line);

                        // add the message to the batch
                        eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));
                        numSuccessfulMessages++;
                    }
                    // send the batch of messages to the event hub using the producer object
                    await producerClient.SendAsync(eventBatch);
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Environment.NewLine}Exception: {ex.Message}");
            }
            Console.WriteLine();
            Console.WriteLine($"{numSuccessfulMessages} messages sent successfully.");
        }
    }
}
