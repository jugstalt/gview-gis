using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureLayerField : JsonField
    {
        [JsonPropertyName("editable")]
        public bool Editable { get; set; }

        [JsonPropertyName("nullable")]
        public bool Nullable { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }
    }
}
