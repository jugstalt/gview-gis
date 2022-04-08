using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureCountResponse : JsonStopWatch
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
