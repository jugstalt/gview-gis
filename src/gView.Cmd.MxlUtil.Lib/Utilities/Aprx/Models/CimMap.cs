using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// CIM map definition (content of a *.mapx file or inline in a CIMMapDocument).
/// </summary>
internal class CimMap
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// CIMPATH references to layer definitions, e.g. "CIMPATH=Layers/Roads.lyrx".
    /// </summary>
    [JsonPropertyName("layers")]
    public List<string>? Layers { get; set; }

    [JsonPropertyName("spatialReference")]
    public CimSpatialReference? SpatialReference { get; set; }

    [JsonPropertyName("mapExtent")]
    public CimEnvelope? MapExtent { get; set; }

    [JsonPropertyName("defaultExtent")]
    public CimEnvelope? DefaultExtent { get; set; }

    /// <summary>
    /// Inline layer definitions – used when layer definitions are embedded
    /// directly inside the mapx file rather than stored in separate lyrx files.
    /// </summary>
    [JsonPropertyName("layerDefinitions")]
    public List<CimBaseLayer>? LayerDefinitions { get; set; }
}
