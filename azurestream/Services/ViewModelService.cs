using apa.BOL.FishTank.ChartViewModels;
using apa.DAL.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace azurestream.Services
{
    //--------------------------------------------------------------------------------------------------------------
    public class ViewModelService : IViewModelService
    {
        private readonly ISensorRepository sensorDataService;
        private readonly IUrlHelper urlHelper;
        //----------------------------------------------------------------------------------------------------------
        public ViewModelService(ISensorRepository sensorDataService, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            this.sensorDataService = sensorDataService;
            urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        //----------------------------------------------------------------------------------------------------------
        public DashboardViewModel GetDashboardViewModel()
        {
            return new DashboardViewModel
            {
                LastFed = "unknown",

                WaterTemperatureTile = new SensorTileViewModel
                {
                    Title = "Water temperature",
                    Value = sensorDataService.GetWaterTemperatureFahrenheit().Value,
                    ColorCssClass = "bg-light",
                    IconCssClass = "fa-thermometer-empty",
                    Url = urlHelper.Action("GetWaterTemperatureChart", "FTHistory")
                },
                FishMotionTile = new SensorTileViewModel
                {
                    Title = "Fish motion",
                    Value = sensorDataService.GetFishMotionPercentage().Value,
                    ColorCssClass = "bg-success",
                    IconCssClass = "fa-circle-o-notch",
                    Url = urlHelper.Action("GetFishMotionPercentageChart", "FTHistory")
                },
                WaterOpacityTile = new SensorTileViewModel
                {
                    Title = "Water opacity",
                    Value = sensorDataService.GetWaterOpacityPercentage().Value,
                    ColorCssClass = "bg-warning",
                    IconCssClass = "fa-adjust",
                    Url = urlHelper.Action("GetWaterOpacityPercentageChart", "FTHistory")
                },
                LightIntensityTile = new SensorTileViewModel
                {
                    Title = "Light intensity",
                    Value = sensorDataService.GetLightIntensityLumens().Value,
                    ColorCssClass = "bg-danger",
                    IconCssClass = "fa-lightbulb-o",
                    Url = urlHelper.Action("GetLightIntensityLumensChart", "FTHistory")
                }
            };
        }
    }
}
