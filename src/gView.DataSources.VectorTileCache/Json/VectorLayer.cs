using Newtonsoft.Json;
using System.Collections.Generic;

namespace gView.DataSources.VectorTileCache.Json
{
    class VectorLayer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("minzoom")]
        public int MinZoom { get; set; }

        [JsonProperty("maxzoom")]
        public int MaxZoom { get; set; }

        [JsonProperty("fields")]
        public Dictionary<string, string> Fields { get; set; }
    }
}
