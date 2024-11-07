using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Legend
{
    public class LayerDTO
    {
        [JsonPropertyName("layerId")]
        public int LayerId { get; set; }

        [JsonPropertyName("layerName")]
        public string LayerName { get; set; }

        [JsonPropertyName("layerType")]
        public string LayerType { get; set; }

        [JsonPropertyName("minScale")]
        public int MinScale { get; set; }

        [JsonPropertyName("maxScale")]
        public int MaxScale { get; set; }

        [JsonPropertyName("legend")]
        public LegendDTO[] Legend { get; set; }
    }
}
