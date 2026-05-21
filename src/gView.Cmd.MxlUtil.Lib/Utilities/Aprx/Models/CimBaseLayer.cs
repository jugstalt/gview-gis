using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// Base class for all CIM layer types.
/// The concrete type is selected via the JSON "type" discriminator
/// (e.g. "CIMFeatureLayer", "CIMGroupLayer").
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type",
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(CimFeatureLayer), "CIMFeatureLayer")]
[JsonDerivedType(typeof(CimGroupLayer), "CIMGroupLayer")]
internal class CimBaseLayer
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("uRI")]
    public string? Uri { get; set; }

    [JsonPropertyName("visibility")]
    public bool Visibility { get; set; } = true;

    [JsonPropertyName("showLegends")]
    public bool ShowLegends { get; set; } = true;
}
