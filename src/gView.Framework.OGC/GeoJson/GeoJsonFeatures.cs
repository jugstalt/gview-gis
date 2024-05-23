using System.Text.Json.Serialization;

namespace gView.Framework.OGC.GeoJson
{
    public class GeoJsonFeatures
    {
        [JsonPropertyName("type")]
        public string Type => "FeatureCollection";

        [JsonPropertyName("features")]
        public GeoJsonFeature[] Features { get; set; }
    }
}
