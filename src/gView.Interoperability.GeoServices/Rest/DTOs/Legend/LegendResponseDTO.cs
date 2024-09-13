using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Legend
{
    public class LegendResponseDTO : JsonStopWatchDTO
    {
        [JsonPropertyName("layers")]
        public LayerDTO[] Layers { get; set; }
    }
}
