namespace apa.BOL.CosmosDB
{
    //--------------------------------------------------------------------------------------------------------------
    public class Volcano
    {
        public string VolcanoName { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public Location Location { get; set; }
        public Measurements Measurements { get; set; }
        public int Elevation { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string LastKnownEruption { get; set; }
        public string id { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class Location
    {
        public string type { get; set; }
        public float[] coordinates { get; set; }
    }

    //--------------------------------------------------------------------------------------------------------------
    public class Measurements
    {
        public string CO2 { get; set; }
        public string H2S { get; set; }
        public string HCL { get; set; }
        public string HF { get; set; }
        public string SO2 { get; set; }
        public string NaOH { get; set; }
        public string SClratio { get; set; }
    }
}