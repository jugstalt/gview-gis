using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Legend
{
    public class Layer
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
        public Legend[] Legend { get; set; }
    }
}
