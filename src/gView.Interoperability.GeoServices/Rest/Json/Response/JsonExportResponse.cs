using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Response
{
    public class JsonExportResponse : JsonStopWatch
    {
        [JsonPropertyName("href")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Href { get; set; }

        [JsonPropertyName("imageData")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ImageData { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("extent")]
        public JsonExtent Extent { get; set; }

        [JsonPropertyName("scale")]
        public double Scale { get; set; }
    }
}
