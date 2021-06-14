using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace apa.BOL.EventHubs
{
    //--------------------------------------------------------------------------------------------------------------
    [Serializable]
    public class PollutionData
    {
        public string ReadingId { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter), "yyyy-MM-dd HH:mm:ss")]
        public DateTime ReadingDateTime { get; set; }

        public int LocationId { get; set; }

        public int PollutionLevel { get; set; }

        public string ToJSON()
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { ContractResolver = contractResolver, Formatting = Formatting.Indented });
        }

    }

    //--------------------------------------------------------------------------------------------------------------
    class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter(string format)
        {
            base.DateTimeFormat = format;
        }   
    }
}