using Newtonsoft.Json;

namespace JsonPlayground.Models.NGeoJson;

public class NGeoJsonFeatures
{
    [JsonProperty("type")]
    public string Type => "FeatureCollection";

    [JsonProperty("features")]
    public NGeoJsonFeature[] Features { get; set; }
}
