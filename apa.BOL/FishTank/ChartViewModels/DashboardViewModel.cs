using System.ComponentModel.DataAnnotations;

namespace apa.BOL.FishTank.ChartViewModels
{
    //--------------------------------------------------------------------------------------------------------------
    public class DashboardViewModel
    {
        public SensorTileViewModel WaterTemperatureTile { get; set; }
        public SensorTileViewModel FishMotionTile { get; set; }
        public SensorTileViewModel WaterOpacityTile { get; set; }
        public SensorTileViewModel LightIntensityTile { get; set; }

        [Display(Name = "Please enter the food amount:")]
        public int FoodAmount { get; set; }

        [Display(Name = "Last feeding was at: ")]
        public string LastFed { get; set; }
    }
}
