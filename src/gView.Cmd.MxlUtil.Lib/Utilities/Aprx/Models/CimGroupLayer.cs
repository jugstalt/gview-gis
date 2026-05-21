using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// CIM group layer definition (type = "CIMGroupLayer").
/// </summary>
internal class CimGroupLayer : CimBaseLayer
{
    /// <summary>
    /// CIMPATH references or inline layer definitions for child layers.
    /// </summary>
    [JsonPropertyName("layers")]
    public List<string>? Layers { get; set; }

    [JsonPropertyName("layerDefinitions")]
    public List<CimBaseLayer>? LayerDefinitions { get; set; }

    /// <summary>Minimum display scale (0 = no restriction).</summary>
    [JsonPropertyName("minScale")]
    public double MinScale { get; set; }

    /// <summary>Maximum display scale (0 = no restriction).</summary>
    [JsonPropertyName("maxScale")]
    public double MaxScale { get; set; }

    [JsonPropertyName("serviceLayerID")]
    public int ServiceLayerId { get; set; }
}
