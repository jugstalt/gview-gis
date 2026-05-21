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
}
