using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// Wrapper document that contains one or more map definitions (*.mapx inside the APRX).
/// </summary>
internal class CimMapDocument
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("build")]
    public string? Build { get; set; }

    /// <summary>
    /// Inline map definitions. In ArcGIS Pro 3.x the map is usually stored
    /// directly in this array rather than referenced externally.
    /// </summary>
    [JsonPropertyName("layerDefinitions")]
    public List<CimMap>? LayerDefinitions { get; set; }

    /// <summary>
    /// ArcGIS Pro 2.x style: a single map definition at root level.
    /// When present, <see cref="LayerDefinitions"/> is usually null.
    /// </summary>
    [JsonPropertyName("map")]
    public CimMap? Map { get; set; }
}
