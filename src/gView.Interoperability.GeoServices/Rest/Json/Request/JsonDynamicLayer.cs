using gView.Interoperability.GeoServices.Rest.Json.DynamicLayers;
using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json.Request
{
    public class JsonDynamicLayer
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "source")]
        public SourceClass Source { get; set; }

        [JsonProperty(PropertyName = "definitionExpression")]
        public string DefinitionExpression { get; set; }

        [JsonProperty(PropertyName = "drawingInfo")]
        public DynamicLayerDrawingInfo DrawingInfo { get; set; }

        #region Classes

        public class SourceClass
        {
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "mapLayerId")]
            public int MapLayerId { get; set; }
        }

        #endregion
    }
}
