using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureCountResponse : JsonStopWatch
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
