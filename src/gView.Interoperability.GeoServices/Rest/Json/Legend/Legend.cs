using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Legend
{
    public class Legend
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("imageData")]
        public string ImageData { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("values")]
        public string[] Values { get; set; }
    }
}
