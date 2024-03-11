using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.DynamicLayers
{
    class DynamicLayer
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("source")]
        public DynamicLayerSouce Source { get; set; }

        [JsonPropertyName("definitionExpression")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string DefinitionExpression { get; set; }

        [JsonPropertyName("drawingInfo")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DynamicLayerDrawingInfo DrawingInfo { get; set; }
    }
}
