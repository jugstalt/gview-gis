using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json.DynamicLayers
{
    class DynamicLayer
    {
        public int id { get; set; }
        public DynamicLayerSouce source { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string definitionExpression { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DynamicLayerDrawingInfo drawingInfo { get; set; }
    }
}
