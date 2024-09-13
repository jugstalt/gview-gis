using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.DTOs
{
    public class JsonSecurityTokenDTO : JsonStopWatchDTO
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("expires")]
        public long Expires { get; set; }

        [JsonPropertyName("ssl")]
        public bool Ssl => true;
    }
}
