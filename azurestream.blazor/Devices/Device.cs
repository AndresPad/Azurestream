using Microsoft.Azure.Devices.Shared;

namespace azurestream.blazor.Devices
{
    public class Device
    {
        public string DeviceId { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty; //TODO: SHould be ModelId ¿?
        public DateTimeOffset? LastTelemetryTimestamp { get; set; }
        public string? LastOperation { get; set; }
        public string? MessageSource { get; internal set; }
        public DateTimeOffset? LastOperationTimestamp { get; internal set; }
    }
}
