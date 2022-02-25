using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace azurestream.console
{
    //--------------------------------------------------------------------------------------------------------------
    public class DeviceProvisioningSample2
    {
        private const string dpsGlobalDeviceEndpoint = "azurestream.azure-devices-provisioning.net";
        private const string dpsIdScope = "YOURDPSIDSCOPE";

        // TODO: set your certificates info here:
        private const string deviceCertificatePassword = @"YOURCERTIFICATEPASSWORD";
        private const string device1CertificateFileName = @"C:\Git\AZURESTREAM\azurestream.console\Azure\IoT\Certificates\DemoDevice1.pfx";
        private const string device2CertificateFileName = @"C:\Git\AZURESTREAM\azurestream.console\Azure\IoT\Certificates\DemoDevice2.pfx";

        private static readonly ConsoleColor defaultConsoleForegroundColor = Console.ForegroundColor;
        //----------------------------------------------------------------------------------------------------------
        internal static async Task ExecuteAsync()
        {

            var device1RegistrationId = $"cert-device-1";
            var device2RegistrationId = $"cert-device-2";
            var device1Certificate = LoadProvisioningCertificate(device1CertificateFileName, deviceCertificatePassword);
            var device2Certificate = LoadProvisioningCertificate(device2CertificateFileName, deviceCertificatePassword);

            // try to register devices
            await TryRegisterDevice(device1RegistrationId, device1Certificate);
            await TryRegisterDevice(device2RegistrationId, device2Certificate);

            // try to send device data to IoT Hub
            await TrySendDeviceData(device1RegistrationId, device1Certificate);
            await TrySendDeviceData(device2RegistrationId, device2Certificate);

            ConsoleWriteLine("*** Press ENTER to quit ***");
            Console.ReadLine();

        }

        //----------------------------------------------------------------------------------------------------------
        private static async Task TryRegisterDevice(string deviceRegistrationId, X509Certificate2 deviceCertificate)
        {
            ConsoleWriteLine($"[{deviceRegistrationId}] Will attempt to REGISTER");

            try
            {
                using var securityProvider = new SecurityProviderX509Certificate(deviceCertificate);
                using var transportHandler = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly);

                // set up provisioning client for given device
                var provisioningDeviceClient = ProvisioningDeviceClient.Create(
                  globalDeviceEndpoint: dpsGlobalDeviceEndpoint,
                  idScope: dpsIdScope,
                  securityProvider: securityProvider,
                  transport: transportHandler);

                // register device
                var deviceRegistrationResult = await provisioningDeviceClient.RegisterAsync();

                ConsoleWriteLine($"[{deviceRegistrationId}] Device registration result: {deviceRegistrationResult.Status}");

                if (string.IsNullOrEmpty(deviceRegistrationResult.AssignedHub))
                {
                    ConsoleWriteLine($"[{deviceRegistrationId}] * ERROR * registration failed", ConsoleColor.Red);
                }
                else
                {
                    // registration OK

                    ConsoleWriteLine($"[{deviceRegistrationId}] Assigned to hub '{deviceRegistrationResult.AssignedHub}'", ConsoleColor.Green);

                    // save hub info for device to file
                    var deviceConnectionInfoFileName = GetDeviceConnectionInfoFileName(deviceRegistrationId);
                    var deviceConnectionInfoJson = JsonConvert.SerializeObject(
                      new DeviceConnectionInfo
                      {
                          AssignedHub = deviceRegistrationResult.AssignedHub,
                          DeviceId = deviceRegistrationResult.DeviceId
                      }
                    );
                    File.WriteAllText(deviceConnectionInfoFileName, deviceConnectionInfoJson);
                }
            }
            catch (Exception ex)
            {
                ConsoleWriteLine($"[{deviceRegistrationId}] * ERROR * {ex.Message}", ConsoleColor.Red);
            }

            ConsoleWriteLine();
        }

        //----------------------------------------------------------------------------------------------------------
        private static async Task TrySendDeviceData(string deviceRegistrationId, X509Certificate2 deviceCertificate)
        {
            ConsoleWriteLine($"[{deviceRegistrationId}] Will attempt to SEND DATA");

            try
            {
                // get hub info for device from file
                var deviceConnectionInfoFileName = GetDeviceConnectionInfoFileName(deviceRegistrationId);
                if (!File.Exists(deviceConnectionInfoFileName))
                {
                    ConsoleWriteLine($"[{deviceRegistrationId}] * ERROR * device connection info file not found, cannot send data", ConsoleColor.Red);
                    Console.WriteLine();
                    return;
                }
                var deviceConnectionInfoJson = File.ReadAllText(deviceConnectionInfoFileName);
                var deviceConnectionInfo = JsonConvert.DeserializeObject<DeviceConnectionInfo>(deviceConnectionInfoJson);

                // set up device client to assigned hub 

                var deviceAuthentication = new DeviceAuthenticationWithX509Certificate(
                  deviceId: deviceConnectionInfo.DeviceId,
                  certificate: deviceCertificate);

                using var deviceClient = DeviceClient.Create(
                  hostname: deviceConnectionInfo.AssignedHub,
                  authenticationMethod: deviceAuthentication,
                  transportType: TransportType.Amqp_Tcp_Only);

                // create message

                var payload = new
                {
                    deviceRegistrationId,
                    message = $"Message sent @ {DateTime.UtcNow}"
                };
                var bodyJson = JsonConvert.SerializeObject(payload);
                var message = new Message(Encoding.UTF8.GetBytes(bodyJson))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8"
                };

                // send message

                ConsoleWriteLine($"[{deviceRegistrationId}] Will send message: {bodyJson}");

                await deviceClient.SendEventAsync(message);

                ConsoleWriteLine($"[{deviceRegistrationId}] Message sent successfully: {bodyJson}", ConsoleColor.Green);
            }
            catch (Microsoft.Azure.Devices.Client.Exceptions.UnauthorizedException)
            {
                ConsoleWriteLine($"[{deviceRegistrationId}] * ERROR * Unauthorized", ConsoleColor.Red);
            }
            catch (Microsoft.Azure.Devices.Client.Exceptions.DeviceNotFoundException)
            {
                ConsoleWriteLine($"[{deviceRegistrationId}] * ERROR * Device is disabled or doesn't exist in IoT Hub", ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                ConsoleWriteLine($"[{deviceRegistrationId}] * ERROR * {ex.Message}", ConsoleColor.Red);
            }

            ConsoleWriteLine();
        }

        //----------------------------------------------------------------------------------------------------------
        private static string GetDeviceConnectionInfoFileName(string deviceRegistrationId)
        {
            return Path.Combine(Environment.CurrentDirectory, deviceRegistrationId + ".json");
        }

        //----------------------------------------------------------------------------------------------------------
        private static X509Certificate2 LoadProvisioningCertificate(string certificateFileName, string certificatePassword)
        {
          // FROM: https://github.com/Azure-Samples/azure-iot-samples-csharp/blob/master/provisioning/Samples/device/X509Sample/ProvisioningDeviceClientSample.cs

          var certificateCollection = new X509Certificate2Collection();

          certificateCollection.Import(
            certificateFileName,
            certificatePassword,
            X509KeyStorageFlags.UserKeySet);

          X509Certificate2 certificate = null;

          // find certificate with private key
          foreach (X509Certificate2 element in certificateCollection)
          {
            ConsoleWriteLine($"Found certificate: {element?.Thumbprint} {element?.Subject}; PrivateKey: {element?.HasPrivateKey}");

            if (certificate == null && element.HasPrivateKey)
            {
              certificate = element;
            }
            else
            {
              element.Dispose();
            }
          }

          if (certificate == null)
          {
            throw new FileNotFoundException($"{certificateFileName} did not contain any certificate with a private key.");
          }

          ConsoleWriteLine($"Using certificate {certificate.Thumbprint} {certificate.Subject}", ConsoleColor.Green);
          ConsoleWriteLine();

          return certificate;
        }

        //----------------------------------------------------------------------------------------------------------
        private static void ConsoleWriteLine(string message = null, ConsoleColor? foregroundColor = null)
        {
            Console.ForegroundColor = foregroundColor ?? defaultConsoleForegroundColor;
            Console.WriteLine(message);
        }

        //----------------------------------------------------------------------------------------------------------
        private class DeviceConnectionInfo
        {
            public string AssignedHub { get; set; }
            public string DeviceId { get; set; }
        }

    }
}
