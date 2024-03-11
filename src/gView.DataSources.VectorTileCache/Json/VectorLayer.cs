using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json
{
    class VectorLayer
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("minzoom")]
        public int MinZoom { get; set; }

        [JsonPropertyName("maxzoom")]
        public int MaxZoom { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, string> Fields { get; set; }
    }
}
