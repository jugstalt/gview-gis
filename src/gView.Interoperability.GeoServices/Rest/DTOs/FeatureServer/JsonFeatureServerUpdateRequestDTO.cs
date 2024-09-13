using gView.Interoperability.GeoServices.Rest.DTOs.Features;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer
{
    public class JsonFeatureServerUpdateRequestDTO : JsonFeatureServerEditRequesDTO
    {
        [FormInput(FormInputAttribute.InputTypes.TextBox10)]
        [JsonPropertyName("features")]
        public JsonFeatureDTO[] Features { get; set; }
    }
}
