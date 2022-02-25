using apa.BOL.IoT;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    class SimulatedDeviceSample_RandomTelemetry

    {
        // The device connection string to authenticate the device with your IoT hub.
        private const string _primaryConnectionString = "HostName=YOURIOTHUB.azure-devices.net;DeviceId=YOURDEVICEID;SharedAccessKey=YOURACCESSKEY";
        private static DeviceClient _deviceClient;
        //--------------------------------------------------------------------------------------------------------------
        internal static async Task ExecuteAsync()
        {
            Console.WriteLine("IoT Hub - Simulated Telemetry device. Ctrl-C to exit.\n");

            // Connect to the IoT hub using the MQTT protocol
            _deviceClient = DeviceClient.CreateFromConnectionString(_primaryConnectionString, TransportType.Mqtt);
            await _deviceClient.OpenAsync();

            Console.WriteLine("Device is connected!");

            //var twinProperties = new TwinCollection();
            //twinProperties["connection.type"] = "wi-fi";
            //twinProperties["connectionStrength"] = "full";

            //await _deviceClient.UpdateReportedPropertiesAsync(twinProperties);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendMessagesAsync(cts.Token);

            _deviceClient.Dispose();
            Console.WriteLine("Random Telemetry Device simulator finished.");
        }

        //--------------------------------------------------------------------------------------------------------------
        // Async method to send simulated telemetry
        private static async Task SendMessagesAsync(CancellationToken cancellationToken)
        {
            var count = 1;
            while (!cancellationToken.IsCancellationRequested)
            {
                var telemetry = new IoTTelemetry
                {
                    Message = "Sending complex object...",
                    StatusCode = count++
                };

                var telemetryJson = JsonConvert.SerializeObject(telemetry);
                var telemetryMessage = new Message(Encoding.ASCII.GetBytes(telemetryJson))
                {

                    //There's one little thing I added to the sample, and that is this part. 
                    //It tells the client that we want to send the message as plain JSON. 
                    //If we leave this out, the body of the message is encoded, and we wouldn't be able to read it without decoding it first.
                    ContentType = "application/json",
                    ContentEncoding = "utf-8"
                };

                await _deviceClient.SendEventAsync(telemetryMessage);
                Console.WriteLine($"{DateTime.Now} > Sending message: {telemetryMessage}");
                Thread.Sleep(2000);
                //await Task.Delay(2000);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
