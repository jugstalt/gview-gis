using gView.Interoperability.GeoServices.Rest.DTOs.DynamicLayers;
using gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonDrawingInfoDTO
    {
        [JsonPropertyName("renderer")]
        public JsonRendererDTO Renderer
        {
            get; set;
        }

        [JsonPropertyName("transparency")]
        public double Transparency { get; set; }

        [JsonPropertyName("labelingInfo")]
        public LabelingInfoDTO[] LabelingInfo { get; set; }
    }
}
