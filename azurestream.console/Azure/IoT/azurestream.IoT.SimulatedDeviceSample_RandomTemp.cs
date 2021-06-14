// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples
//                  https://github.com/Azure-Samples/azure-iot-samples-csharp/blob/master/iot-hub/Quickstarts/SimulatedDevice/Program.cs

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    class SimulatedDeviceSample_RandomTemp
    {
        // The device connection string to authenticate the device with your IoT hub.
        private const string _primaryConnectionString = "HostName=YOURIOTHUB.azure-devices.net;DeviceId=YOURDEVICEID;SharedAccessKey=YOURACCESSKEY";
        private static DeviceClient _deviceClient;
        //--------------------------------------------------------------------------------------------------------------
        internal static async Task ExecuteAsync(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts - Simulated device. Ctrl-C to exit.\n");

            // Connect to the IoT hub using the MQTT protocol
            _deviceClient = DeviceClient.CreateFromConnectionString(_primaryConnectionString, TransportType.Mqtt);
            // Run the telemetry loop
            await SendDeviceToCloudMessagesAsync();

            _deviceClient.Dispose();
            Console.WriteLine("Random Temperature Device simulator finished.");
        }

        //--------------------------------------------------------------------------------------------------------------
        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync()
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            var rand = new Random();

            while (true)
            {
                //This generates random Temperature and Humidity values
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message
                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
				
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                //There's one little thing I added to the sample, and that is this part. 
                //It tells the client that we want to send the message as plain JSON. 
                //If we leave this out, the body of the message is encoded, and we wouldn't be able to read it without decoding it first.
                message.ContentType = "application/json";
                message.ContentEncoding = "utf-8";

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the tlemetry message
                await _deviceClient.SendEventAsync(message).ConfigureAwait(false);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }
    }
}
