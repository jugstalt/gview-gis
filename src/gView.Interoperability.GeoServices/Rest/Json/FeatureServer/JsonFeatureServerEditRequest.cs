using gView.Interoperability.GeoServices.Rest.Json.Features;
using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.FeatureServer
{
    public class JsonFeatureServerEditRequest
    {
        [FormInput(FormInputAttribute.InputTypes.Hidden)]
        [JsonPropertyName("layerId")]
        public int LayerId { get; set; }
    }

    public class JsonFeatureServerUpdateRequest : JsonFeatureServerEditRequest
    {
        [FormInput(FormInputAttribute.InputTypes.TextBox10)]
        [JsonPropertyName("features")]
        public JsonFeature[] Features { get; set; }
    }

    public class JsonFeatureServerDeleteRequest : JsonFeatureServerEditRequest
    {
        [JsonPropertyName("objectIds")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ObjectIds { get; set; }
    }
}
