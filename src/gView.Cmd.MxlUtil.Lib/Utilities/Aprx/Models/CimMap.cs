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
    /// The map's reference scale (ArcGIS Pro: Map Properties -> General -> Reference Scale),
    /// in the map's ground units per page unit (e.g. 1000 for "1:1000"). Absent/null when the
    /// author never set one - ArcGIS Pro then keeps symbol/text sizes constant in page units
    /// regardless of the current map scale, instead of scaling them with ground distance.
    /// </summary>
    [JsonPropertyName("referenceScale")]
    public double? ReferenceScale { get; set; }

    /// <summary>
    /// Inline layer definitions – used when layer definitions are embedded
    /// directly inside the mapx file rather than stored in separate lyrx files.
    /// </summary>
    [JsonPropertyName("layerDefinitions")]
    public List<CimBaseLayer>? LayerDefinitions { get; set; }
}
