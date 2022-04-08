using Newtonsoft.Json;

namespace gView.Framework.OGC.GeoJson
{
    public class GeoJsonFeatures
    {
        [JsonProperty("type")]
        public string Type => "FeatureCollection";

        [JsonProperty("features")]
        public GeoJsonFeature[] Features { get; set; }
    }
}
