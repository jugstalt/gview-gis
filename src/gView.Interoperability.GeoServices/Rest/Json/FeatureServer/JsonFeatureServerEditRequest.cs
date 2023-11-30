using gView.Interoperability.GeoServices.Rest.Json.Features;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json.FeatureServer
{
    public class JsonFeatureServerEditRequest
    {
        [FormInput(FormInputAttribute.InputTypes.Hidden)]
        [JsonProperty(PropertyName = "layerId")]
        public int LayerId { get; set; }
    }

    public class JsonFeatureServerUpdateRequest : JsonFeatureServerEditRequest
    {
        [FormInput(FormInputAttribute.InputTypes.TextBox10)]
        [JsonProperty("features")]
        public JsonFeature[] Features { get; set; }
    }

    public class JsonFeatureServerDeleteRequest : JsonFeatureServerEditRequest
    {
        [JsonProperty("objectIds", NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectIds { get; set; }
    }
}
