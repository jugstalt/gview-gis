using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json.Legend
{
    public class LegendResponse : JsonStopWatch
    {
        [JsonPropertyName("layers")]
        public Layer[] Layers { get; set; }
    }
}
