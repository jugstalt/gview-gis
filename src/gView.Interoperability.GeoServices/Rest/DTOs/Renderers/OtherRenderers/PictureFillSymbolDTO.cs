using gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs.Rest.Renderers.OtherRenderers
{
    class PictureFillSymbolDTO
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
        public SimpleLineSymbolDTO Outline { get; set; }

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
