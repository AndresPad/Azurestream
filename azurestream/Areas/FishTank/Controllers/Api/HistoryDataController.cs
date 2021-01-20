using apa.BOL.FishTank;
using apa.DAL.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace apa.Areas.FishTank.Controllers.Api
{
    //--------------------------------------------------------------------------------------------------------------
    [AllowAnonymous]
    [Area("FishTank")]
    public class HistoryDataController: Controller
    {
        private readonly ISensorRepository sensorDataService;

        //----------------------------------------------------------------------------------------------------------
        public HistoryDataController(ISensorRepository sensorDataService)
        {
            this.sensorDataService = sensorDataService;
        }

        //-----------------------------------------------------------------------------------------------------------
        public IEnumerable<IntHistoryModel> GetWaterTemperatureHistory()
        {
            return (sensorDataService.GetWaterTemperatureFahrenheitHistory());
        }

        //-----------------------------------------------------------------------------------------------------------
        public IEnumerable<IntHistoryModel> GetFishMotionPercentageHistory()
        {
            return sensorDataService.GetFishMotionPercentageHistory();
        }

        //-----------------------------------------------------------------------------------------------------------
        public IEnumerable<IntHistoryModel> GetWaterOpacityPercentageHistory()
        {
            return sensorDataService.GetWaterOpacityPercentageHistory();
        }

        //-----------------------------------------------------------------------------------------------------------
        public IEnumerable<IntHistoryModel> GetLightIntensityLumensHistory()
        {
            return sensorDataService.GetLightIntensityLumensHistory();
        }
    }
}