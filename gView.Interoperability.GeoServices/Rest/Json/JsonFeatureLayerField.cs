using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureLayerField : JsonField
    {
        [JsonProperty("editable")]
        public bool Editable { get; set; }

        [JsonProperty("nullable")]
        public bool Nullable { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }
    }
}
