using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json.Legend
{
    public class LegendResponse : JsonStopWatch
    {
        [JsonProperty("layers")]
        public Layer[] Layers { get; set; }
    }
}
