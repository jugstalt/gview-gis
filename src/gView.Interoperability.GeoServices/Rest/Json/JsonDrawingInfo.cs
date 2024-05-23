using gView.Interoperability.GeoServices.Rest.Json.DynamicLayers;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonDrawingInfo
    {
        [JsonPropertyName("renderer")]
        public JsonRenderer Renderer
        {
            get; set;
        }

        [JsonPropertyName("transparency")]
        public double Transparency { get; set; }

        [JsonPropertyName("labelingInfo")]
        public LabelingInfo[] LabelingInfo { get; set; }
    }
}
