using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.DynamicLayers
{
    public class DynamicLayerDrawingInfo
    {
        [JsonPropertyName("renderer")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonRenderer Renderer { get; set; }

        [JsonPropertyName("transparency")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Transparency { get; set; }

        [JsonPropertyName("scaleSymbols")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ScaleSymbols { get; set; }

        [JsonPropertyName("showLabels")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ShowLabels { get; set; }

        [JsonPropertyName("labelingInfo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LabelingInfo[] LabelingInfo { get; set; }
    }
}
