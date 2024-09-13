using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer
{
    public class JsonFeatureServerEditRequesDTO
    {
        [FormInput(FormInputAttribute.InputTypes.Hidden)]
        [JsonPropertyName("layerId")]
        public int LayerId { get; set; }
    }
}
