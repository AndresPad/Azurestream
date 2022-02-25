namespace azurestream.blazor.Events
{
    public interface IEventReaderService : IAsyncDisposable
    {
        IObservable<Event> Events { get; }
        IObservable<EventProcessorInfo> ProcessorInfo { get; }
    }
}