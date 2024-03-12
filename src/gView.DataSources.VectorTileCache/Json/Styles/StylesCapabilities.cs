using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.Styles;

public class StylesCapabilities
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("center")]
    public double[] Center { get; set; }

    [JsonPropertyName("zoom")]
    public double Zoom { get; set; }

    [JsonPropertyName("sprite")]
    public string Sprite { get; set; }

    [JsonPropertyName("glyphs")]
    public string Glyphs { get; set; }

    [JsonPropertyName("filter")]
    public JsonArray Filter { get; set; }

    [JsonPropertyName("sources")]
    public Dictionary<string, StyleSource> Sources { get; set; }

    [JsonPropertyName("layers")]
    public StyleLayer[] Layers { get; set; }
}
