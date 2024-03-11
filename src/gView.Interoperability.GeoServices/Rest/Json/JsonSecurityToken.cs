using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonSecurityToken : JsonStopWatch
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("expires")]
        public long Expires { get; set; }

        [JsonPropertyName("ssl")]
        public bool Ssl => true;
    }
}
