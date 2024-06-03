using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Renderers.OtherRenderers
{
    class PictureMarkerSymbol
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("imageData")]
        public string ImageData { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("angle")]
        public float Angle { get; set; }

        [JsonPropertyName("xoffset")]
        public float Xoffset { get; set; }

        [JsonPropertyName("yoffset")]
        public float Yoffset { get; set; }
    }
}
