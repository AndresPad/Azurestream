using apa.BOL.EventHubs;

namespace azurestream.eventhubs.Business
{
    //----------------------------------------------------------------------------------------------------------
    public interface IPollutionCollector
    {
        PollutionData Collect();
    }
}