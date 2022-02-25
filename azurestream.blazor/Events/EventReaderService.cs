namespace azurestream.blazor.Events
{
    public class EventReaderService : IEventReaderService, IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        public IObservable<Event> Events { get; }
        public IObservable<EventProcessorInfo> ProcessorInfo { get; }

        EventProcessor _processor;


        public EventReaderService(IConfiguration configuration)
        {
            _configuration = configuration;

            //TODO: Managed Identity to retrieve the ConnectionString
            var iotHubServiceConnString = _configuration.GetValue<String>("Iot:IotHub");
            var storageAccountConnString = _configuration.GetValue<String>("Iot:StorageAccount");


            _processor = new EventProcessor(iotHubServiceConnString, storageAccountConnString, new CancellationTokenSource(), "webapp");

            Events = _processor;
            ProcessorInfo = _processor;
        }

        public ValueTask DisposeAsync()
        {
            return _processor.DisposeAsync();
        }

    }
}
