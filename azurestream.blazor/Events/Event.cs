using Azure.Messaging.EventHubs;
using System.Runtime.InteropServices;

namespace azurestream.blazor.Events
{
    public class Event
    {
        public string? DeviceId { get; set; } = string.Empty;
        public string? ModuleId { get; set; } = null;
        public DateTimeOffset EnqueuedTime { get; set; } = DateTimeOffset.MinValue;
        public string? Body { get; set; } = string.Empty;
        public string? HubName { get; set; } = string.Empty;
        public string? Operation { get; set; } = string.Empty;
        public string? AuthMethod { get; set; } = null;
        public string? MessageSource { get; set; } = null;
        public string? DataSchema { get; set; } = null;
        public string? Component { get; set; } = null;
        public long? SequenceNumber { get; set; } = null;
        public long? Offset { get; set; } = null;

        

        public Event()
        {
        }

        public Event(EventData eventData)
        {
            if(eventData != null)
            {
                Body = eventData.EventBody.ToString();
                EnqueuedTime = eventData.EnqueuedTime;
                Offset = eventData.Offset;
                if (eventData.Properties != null)
                {
                    eventData.Properties.TryGetValue("hubName", out var hubName);
                    HubName = hubName?.ToString();

                    eventData.Properties.TryGetValue("opType", out var opType);
                    Operation = opType?.ToString();
                }

                if (eventData.SystemProperties != null)
                {
                    eventData.SystemProperties.TryGetValue("iothub-connection-device-id", out var deviceId);
                    DeviceId = deviceId?.ToString();

                    eventData.SystemProperties.TryGetValue("iothub-connection-module-id", out var moduleId);
                    ModuleId = moduleId?.ToString();

                    eventData.SystemProperties.TryGetValue("dt-dataschema", out var dataSchema);
                    DataSchema = dataSchema?.ToString();

                    eventData.SystemProperties.TryGetValue("dt-subject", out var component);
                    Component = component?.ToString();

                    eventData.SystemProperties.TryGetValue("iothub-connection-auth-method", out var authMethod);
                    AuthMethod = authMethod?.ToString();

                    eventData.SystemProperties.TryGetValue("iothub-message-source", out var messageSource);
                    MessageSource = messageSource?.ToString();

                    eventData.SystemProperties.TryGetValue("x-opt-sequence-number", out var sequenceNumberStr);
                    long.TryParse(sequenceNumberStr?.ToString(), out var sequenceNumber);
                    SequenceNumber = sequenceNumber;
                }
            }    
        }
    }
}

