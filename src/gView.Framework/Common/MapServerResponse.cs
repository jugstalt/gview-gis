using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.Common
{
    public class MapServerResponse
    {
        [JsonPropertyName("data")]
        public byte[] Data { get; set; }

        [JsonPropertyName("content-type")]
        public string ContentType { get; set; }

        public DateTime? Expires { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        static public MapServerResponse FromString(string json)
        {
            return JsonSerializer.Deserialize<MapServerResponse>(json);
        }
    }
}
