using Azure.IoT.ModelsRepository;
using Microsoft.Azure.Devices.Serialization;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace azurestream.blazor.Devices
{
    public static class Extensions
    {
        public static string SerializeTwin(this Twin twin)
        {
            
            return twin is not null ? JsonConvert.SerializeObject(twin, Formatting.Indented) : string.Empty;
        }

        public static string SerializeDigitalTwin(this BasicDigitalTwin digitalTwin)
        {

            return digitalTwin is not null ? JsonConvert.SerializeObject(digitalTwin, Formatting.Indented) : string.Empty;
        }

        public static string SerializeDigitalTwinModel(this ModelResult model, string dtmi)
        {
            StringBuilder sb = new StringBuilder();
            if (model.Content != null)
            {
                if(model.Content.ContainsKey(dtmi))
                {
                    var json = JsonConvert.DeserializeObject(model.Content[dtmi]); //TODO: Review, this is a trick to format
                    sb.AppendLine(JsonConvert.SerializeObject(json, Formatting.Indented));
                }
                
            }
            return sb.ToString();
        }
    }
}
