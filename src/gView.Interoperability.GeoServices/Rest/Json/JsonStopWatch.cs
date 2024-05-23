using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonStopWatch : JsonError
    {
        [JsonPropertyName("_duration_ms")]
        public double DurationMilliseconds { get; set; }

        [JsonPropertyName("_idle_ms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string,double> IdleMilliseconds { get; set; }

        [JsonPropertyName("_size_bytes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? SizeBytes { get; set; }
    }
}
