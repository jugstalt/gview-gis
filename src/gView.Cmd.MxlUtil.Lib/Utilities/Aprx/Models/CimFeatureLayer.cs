using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// CIM feature layer definition (type = "CIMFeatureLayer").
/// </summary>
internal class CimFeatureLayer : CimBaseLayer
{
    /// <summary>
    /// SQL-style definition query / filter expression, e.g. "STATUS = 'Active'".
    /// </summary>
    [JsonPropertyName("definitionExpression")]
    public string? DefinitionExpression { get; set; }

    [JsonPropertyName("featureTable")]
    public CimFeatureTable? FeatureTable { get; set; }

    [JsonPropertyName("renderer")]
    public CimRenderer? Renderer { get; set; }

    [JsonPropertyName("labelClasses")]
    public List<CimLabelClass>? LabelClasses { get; set; }

    [JsonPropertyName("labelVisibility")]
    public bool LabelVisibility { get; set; }

    /// <summary>Minimum display scale (0 = no restriction).</summary>
    [JsonPropertyName("minScale")]
    public double MinScale { get; set; }

    /// <summary>Maximum display scale (0 = no restriction).</summary>
    [JsonPropertyName("maxScale")]
    public double MaxScale { get; set; }

    [JsonPropertyName("serviceLayerID")]
    public int ServiceLayerId { get; set; }

    /// <summary>
    /// Layer transparency, 0 (opaque, the default) - 100 (fully invisible). Set via ArcGIS
    /// Pro's Layer Properties -> Display -> Transparency slider - independent of, and
    /// multiplicative with, any transparency already baked into the layer's own symbol colors.
    /// </summary>
    [JsonPropertyName("transparency")]
    public double Transparency { get; set; }

    /// <summary>
    /// Whether this layer's symbols and labels scale with the map's reference scale - ArcGIS
    /// Pro's Layer Properties -> General -> "Scale symbols when a reference scale is set"
    /// checkbox. True (the CIM default) - symbols/labels keep a constant real-world size as the
    /// map is zoomed relative to the reference scale (classic cartographic behaviour). False -
    /// symbols/labels always render at a constant screen size, ignoring the reference scale.
    /// </summary>
    [JsonPropertyName("scaleSymbols")]
    public bool ScaleSymbols { get; set; } = true;
}
