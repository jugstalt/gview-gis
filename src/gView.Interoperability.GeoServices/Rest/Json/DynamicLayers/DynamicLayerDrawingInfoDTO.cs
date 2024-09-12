using gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.DynamicLayers
{
    public class DynamicLayerDrawingInfoDTO
    {
        [JsonPropertyName("renderer")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonRendererDTO Renderer { get; set; }

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
        public LabelingInfoDTO[] LabelingInfo { get; set; }
    }
}
