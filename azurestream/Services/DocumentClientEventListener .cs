using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics.Tracing;
using System.Threading;

namespace azurestream.Services
{
    public class DocumentClientEventListener : EventListener
    {
        private bool _initialised;
        public static AsyncLocal<RequestTelemetry> Request { get; }
            = new AsyncLocal<RequestTelemetry>();

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (!_initialised && eventSource.Name == "DocumentDBClient")
            {
                this.EnableEvents(eventSource, EventLevel.Verbose, (EventKeywords)1);
                _initialised = true;
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData == null || eventData.Payload == null || eventData.EventSource?.Name != "DocumentDBClient")
            {
                return;
            }

            // const int cosmosDBRequestEventId = 1;
            const int cosmosDBResponseEventId = 2;
            if (eventData.EventId == cosmosDBResponseEventId)
            {
                OnCosmosDBResponseEvent(eventData, Request!.Value);
            }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            static void OnCosmosDBResponseEvent(EventWrittenEventArgs eventData, RequestTelemetry? requestTelemetry)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            {
                if (requestTelemetry == null)
                {
                    return;
                }

                if (eventData?.Payload?.Count != 30)
                {
                    return;
                }

                if (eventData.Payload[22] is string requestChargeAsString
                    && double.TryParse(requestChargeAsString, out double requestCharge))
                {
                    const string key = "CosmosDBTotalRequestCharge";
                    var metrics = requestTelemetry.Metrics;
                    if (metrics.ContainsKey(key))
                    {
                        metrics[key] += requestCharge;
                    }
                    else
                    {
                        metrics[key] = requestCharge;
                    }
                }
            }
        }
    }
}
