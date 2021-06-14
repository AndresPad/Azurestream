using apa.BOL.EventHubs;
using azurestream.eventhubs.Helpers;
using System;

namespace azurestream.eventhubs.Business
{
    //----------------------------------------------------------------------------------------------------------
    public class PollutionCollector : IPollutionCollector
    {
        // Collect pollution data
        //------------------------------------------------------------------------------------------------------
        public PollutionData Collect()
        {
            return new PollutionData() {
                ReadingId = Randomizer.Guid(),
                ReadingDateTime = DateTime.Now,
                LocationId = Randomizer.Number(1000, 2000),
                PollutionLevel = Randomizer.Number(1, 2)
            };
        }
    }
}