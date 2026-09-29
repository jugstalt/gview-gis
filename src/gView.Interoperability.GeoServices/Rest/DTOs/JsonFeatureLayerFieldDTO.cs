using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonFeatureLayerFieldDTO : JsonFieldDTO
    {
        [JsonPropertyName("length")]
        public int Length { get; set; }
    }
}
