using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Rest.Renderers.OtherRenderers
{
    class PictureFillSymbol
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("imageData")]
        public string ImageData { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("outline")]
        public SimpleLineSymbol Outline { get; set; }

        [JsonPropertyName("width")]
        public float Width { get; set; }

        [JsonPropertyName("height")]
        public float Height { get; set; }

        [JsonPropertyName("angle")]
        public float Angle { get; set; }

        [JsonPropertyName("xoffset")]
        public float Xoffset { get; set; }

        [JsonPropertyName("yoffset")]
        public float Yoffset { get; set; }

        [JsonPropertyName("xscale")]
        public float Xscale { get; set; }

        [JsonPropertyName("yscale")]
        public float Yscale { get; set; }
    }
}
