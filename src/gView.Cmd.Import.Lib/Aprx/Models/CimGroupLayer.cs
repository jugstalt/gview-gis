using System.Text.Json.Serialization;

namespace gView.Cmd.Import.Aprx.Models;

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
}
