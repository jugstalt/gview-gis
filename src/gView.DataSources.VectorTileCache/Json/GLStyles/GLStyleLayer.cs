using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.GLStyles;

public class GLStyleLayer
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("source-layer")]
    public string SourceLayerId { get; set; }

    [JsonPropertyName("minzoom")]
    public float? MinZoom { get; set; }

    [JsonPropertyName("maxzoom")]
    public float? MaxZoom { get; set; }

    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Filter { get; set; }

    [JsonPropertyName("layout")]
    public GLStyleLayerLayout Layout { get; set;}

    [JsonPropertyName("paint")]
    public GLStyleLayerPaint Paint { get; set; }
}
