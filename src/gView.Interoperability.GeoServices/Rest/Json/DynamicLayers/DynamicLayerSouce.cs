using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.DynamicLayers
{
    class DynamicLayerSouce
    {
        public DynamicLayerSouce()
        {
            this.Type = "mapLayer";
        }

        [JsonPropertyName("type")]
        public string Type { get; set; }


        [JsonPropertyName("mapLayerId")]
        public int MapLayerId { get; set; }
    }
}
