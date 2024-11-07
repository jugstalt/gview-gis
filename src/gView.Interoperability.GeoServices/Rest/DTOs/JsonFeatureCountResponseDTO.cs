using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonFeatureCountResponseDTO : JsonStopWatchDTO
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
