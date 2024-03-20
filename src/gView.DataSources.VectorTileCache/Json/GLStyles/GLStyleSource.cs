using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.GLStyles;

public class GLStyleSource
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("tileSize")]
    public int? TileSize { get; set; }

    [JsonPropertyName("buffer")]
    public int? Buffer { get; set; }

    [JsonPropertyName("tiles")]
    public string[] Tiles { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("minzoom")]
    public int? MinZoom { get; set; }

    [JsonPropertyName("maxzoom")]
    public int? MaxZoom { get; set; }
}
