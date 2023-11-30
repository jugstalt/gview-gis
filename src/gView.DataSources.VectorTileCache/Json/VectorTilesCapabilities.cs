using Newtonsoft.Json;

namespace gView.DataSources.VectorTileCache.Json
{
    class VectorTilesCapabilities
    {
        [JsonProperty("sub_domains")]
        public object SubDomains { get; set; }  // String Array???

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("attribution")]
        public string Attribution { get; set; }

        [JsonProperty("scheme")]
        public string Scheme { get; set; }

        [JsonProperty("tiles")]
        public string[] Tiles { get; set; }

        [JsonProperty("minzoom")]
        public int MinZoom { get; set; }

        [JsonProperty("maxzoom")]
        public int MaxZoom { get; set; }

        [JsonProperty("bounds")]
        public double[] Bounds { get; set; }

        [JsonProperty("center")]
        public double[] Center { get; set; }

        [JsonProperty("vector_layers")]
        public VectorLayer[] VectorLayers { get; set; }
    }
}
