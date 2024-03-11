using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json
{
    class VectorTilesCapabilities
    {
        [JsonPropertyName("sub_domains")]
        public object SubDomains { get; set; }  // String Array???

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("attribution")]
        public string Attribution { get; set; }

        [JsonPropertyName("scheme")]
        public string Scheme { get; set; }

        [JsonPropertyName("tiles")]
        public string[] Tiles { get; set; }

        [JsonPropertyName("minzoom")]
        public int MinZoom { get; set; }

        [JsonPropertyName("maxzoom")]
        public int MaxZoom { get; set; }

        [JsonPropertyName("bounds")]
        public double[] Bounds { get; set; }

        [JsonPropertyName("center")]
        public double[] Center { get; set; }

        [JsonPropertyName("vector_layers")]
        public VectorLayer[] VectorLayers { get; set; }
    }
}
