using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    class SimulatedDeviceSample_RandomSpeed
    {
        // The device connection string to authenticate the device with your IoT hub.
        private static string _primaryConnectionString = "HostName=YOURIOTHUB.azure-devices.net;DeviceId=YOURDEVICEID;SharedAccessKey=YOURACCESSKEY";
        private static DeviceClient _deviceClient;
        //--------------------------------------------------------------------------------------------------------------
        internal static async Task ExecuteAsync(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts - Simulated device. Ctrl-C to exit.\n");

            // Connect to the IoT hub using the MQTT protocol
            _deviceClient = DeviceClient.CreateFromConnectionString(_primaryConnectionString, TransportType.Mqtt);

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
            Console.WriteLine("Random Speed Device simulator finished.");
        }

        //--------------------------------------------------------------------------------------------------------------
        // Async method to send simulated telemetry
        private static async Task SendMessagesAsync(CancellationToken cancellationToken)
        {
            // Initial telemetry values
            int speed = 0;
            var rand = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                //This generates random speed values
                int currentSpeed = speed + rand.Next(1, 60);

                string messageBody = JsonSerializer.Serialize( new
                {
                    vehicleRegistration = "RJ69 XRT",
                    vehiclespeed = currentSpeed

                });
					
                //There's one little thing I added to the sample, and that is this part. 
                //It tells the client that we want to send the message as plain JSON. 
                //If we leave this out, the body of the message is encoded, and we wouldn't be able to read it without decoding it first.
                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

                await Task.Delay(1000);
            }
        }
    }
}
