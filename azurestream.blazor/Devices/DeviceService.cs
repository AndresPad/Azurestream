using Azure.IoT.ModelsRepository;
using azurestream.blazor.Events;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Serialization;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Rest;
using System.Reactive.Linq;

namespace azurestream.blazor.Devices
{
    public class DeviceService : IDeviceService, IAsyncDisposable
    {
        public List<Device> Devices { get; } = new List<Device>();

        private readonly IConfiguration _configuration;
        private readonly IEventReaderService _eventReader;
        private readonly RegistryManager _registryManager;
        private readonly DigitalTwinClient _digitalTwinClient;
        private readonly ModelsRepositoryClient _modelsRepositoryClient;

        public DeviceService(IEventReaderService eventReader, IConfiguration confiuration)
        {
            _configuration = confiuration;
            _registryManager = RegistryManager.CreateFromConnectionString(_configuration.GetValue<string>("Iot:IotHub"));
            _digitalTwinClient = DigitalTwinClient.CreateFromConnectionString(_configuration.GetValue<string>("Iot:IotHub"));
            _modelsRepositoryClient = new ModelsRepositoryClient();
            _eventReader = eventReader;
            _eventReader.Events.Subscribe(UpsertOnlineDevices);
        }

        private void UpsertOnlineDevices(Event e)
        {
            int idx = Devices.FindIndex(d => d.DeviceId == e.DeviceId);
            if (idx != -1)
            {
                if (e.DataSchema != null)
                {
                    Devices[idx].ModelId = e.DataSchema;
                }

                if (e.Operation != null)
                {
                    Devices[idx].MessageSource = e.MessageSource;
                    Devices[idx].LastOperation = e.Operation;
                    Devices[idx].LastOperationTimestamp = e.EnqueuedTime;
                }

                if (e.MessageSource == "Telemetry")
                {                    
                    Devices[idx].LastTelemetryTimestamp = e.EnqueuedTime;
                }
            }
            else
            {
                var device = new Device()
                {
                    DeviceId = e.DeviceId,
                    ModelId = e.DataSchema,
                    MessageSource = e.Operation is not null ? e.MessageSource : null,
                    LastOperation = e.Operation is not null ? e.Operation : null,
                    LastTelemetryTimestamp = e.MessageSource == "Telemetry" ? e.EnqueuedTime : null,
                    LastOperationTimestamp = e.Operation is not null ? e.EnqueuedTime : null
                };
                Devices.Add(device);
            }
        }

        public async Task<Twin?> GetDeviceTwinAsync(string? deviceId)
        {
            try
            {
                var twin = await _registryManager.GetTwinAsync(deviceId);
                return twin;
            }
            catch
            {
                //TODO: Manage properly
                //Unable to resolve twin for deviceId
                return null;
            }
        }

        public async Task<BasicDigitalTwin?> GetDigitalTwinAsync(string? deviceId)
        {
            try
            {
                HttpOperationResponse<BasicDigitalTwin, DigitalTwinGetHeaders> getDigitalTwinResponse = await _digitalTwinClient.GetDigitalTwinAsync<BasicDigitalTwin>(deviceId);
                var digitalTwin = getDigitalTwinResponse.Body;
                return digitalTwin;
            }
            catch
            {
                //TODO: Manage properly
                //Unable to resolve twin for deviceId
                return null;
            }
        }

        public async Task<ModelResult?> ResolveModelAsync(string? dtmi)
        {
            try
            {
                var modelResult = await _modelsRepositoryClient.GetModelAsync(dtmi, ModelDependencyResolution.Enabled);
                return modelResult;
            }
            catch
            {
                //TODO: Manage properly (if 404 not found in the default / defined repository)
                //Unable to resolve modelId.
                return null;
            }
        }

        public ValueTask DisposeAsync()
        {
            _registryManager.Dispose();
            return _eventReader.DisposeAsync();
        }
    }
}
