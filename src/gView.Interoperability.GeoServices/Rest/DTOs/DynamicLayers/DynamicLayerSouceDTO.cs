using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.DynamicLayers
{
    class DynamicLayerSouceDTO
    {
        public DynamicLayerSouceDTO()
        {
            this.Type = "mapLayer";
        }

        [JsonPropertyName("type")]
        public string Type { get; set; }


        [JsonPropertyName("mapLayerId")]
        public int MapLayerId { get; set; }
    }
}
