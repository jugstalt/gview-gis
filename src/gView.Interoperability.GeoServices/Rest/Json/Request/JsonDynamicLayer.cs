using gView.Interoperability.GeoServices.Rest.Json.DynamicLayers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Request
{
    public class JsonDynamicLayer
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("source")]
        public SourceClass Source { get; set; }

        [JsonPropertyName("definitionExpression")]
        public string DefinitionExpression { get; set; }

        [JsonPropertyName("drawingInfo")]
        public DynamicLayerDrawingInfo DrawingInfo { get; set; }

        #region Classes

        public class SourceClass
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("mapLayerId")]
            public int MapLayerId { get; set; }
        }

        #endregion
    }
}
