namespace azurestream.blazor.Events
{
    public enum ProcessorInfoType
    {
        Information,
        Heartbeat,
        Warning,
        Error
    }
    public class EventProcessorInfo
    {
        public DateTimeOffset Timestamp { get; set; } = default(DateTimeOffset);
        public ProcessorInfoType Type { get; set; } = ProcessorInfoType.Information;
        public string Description { get; set; } = string.Empty;
        public Exception? Exception { get; set; } = null;

    }
}