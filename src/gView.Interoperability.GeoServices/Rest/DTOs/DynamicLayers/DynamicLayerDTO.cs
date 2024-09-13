using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.DynamicLayers
{
    class DynamicLayerDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("source")]
        public DynamicLayerSouceDTO Source { get; set; }

        [JsonPropertyName("definitionExpression")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DefinitionExpression { get; set; }

        [JsonPropertyName("drawingInfo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DynamicLayerDrawingInfoDTO DrawingInfo { get; set; }
    }
}
