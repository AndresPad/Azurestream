using azurestream.eventhubs.Models;

namespace azurestream.eventhubs.Business
{
    public interface IPollutionCollector
    {
        PollutionData Collect();
    }
}