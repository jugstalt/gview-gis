using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Cmd.Import.Aprx.Models;

/// <summary>
/// A reference to a CIM symbol, optionally with overrides.
/// </summary>
internal class CimSymbolReference
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("symbol")]
    public CimSymbol? Symbol { get; set; }
}

/// <summary>
/// Base class for all CIM symbol types.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type",
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(CimPointSymbol), "CIMPointSymbol")]
[JsonDerivedType(typeof(CimLineSymbol), "CIMLineSymbol")]
[JsonDerivedType(typeof(CimPolygonSymbol), "CIMPolygonSymbol")]
[JsonDerivedType(typeof(CimTextSymbol), "CIMTextSymbol")]
internal class CimSymbol
{
    [JsonPropertyName("symbolLayers")]
    public List<CimSymbolLayer>? SymbolLayers { get; set; }
}

internal class CimPointSymbol : CimSymbol { }
internal class CimLineSymbol : CimSymbol { }
internal class CimPolygonSymbol : CimSymbol { }

/// <summary>
/// Base for individual layers inside a multi-layer CIM symbol.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type",
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(CimSolidFill), "CIMSolidFill")]
[JsonDerivedType(typeof(CimHatchFill), "CIMHatchFill")]
[JsonDerivedType(typeof(CimSolidStroke), "CIMSolidStroke")]
[JsonDerivedType(typeof(CimCharacterMarker), "CIMCharacterMarker")]
[JsonDerivedType(typeof(CimVectorMarker), "CIMVectorMarker")]
[JsonDerivedType(typeof(CimPictureMarker), "CIMPictureMarker")]
[JsonDerivedType(typeof(CimPictureFill), "CIMPictureFill")]
internal class CimSymbolLayer
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = true;
}

internal class CimSolidFill : CimSymbolLayer
{
    [JsonPropertyName("color")]
    public CimColor? Color { get; set; }
}

/// <summary>
/// Hatch fill layer: uses a <c>lineSymbol</c> drawn at a given <c>rotation</c>
/// with a given line <c>separation</c>.
/// </summary>
internal class CimHatchFill : CimSymbolLayer
{
    /// <summary>The line symbol used to draw the hatch lines.</summary>
    [JsonPropertyName("lineSymbol")]
    public CimLineSymbol? LineSymbol { get; set; }

    /// <summary>Rotation of the hatch lines in degrees (0 = horizontal).</summary>
    [JsonPropertyName("rotation")]
    public double Rotation { get; set; }

    /// <summary>Distance between hatch lines in points.</summary>
    [JsonPropertyName("separation")]
    public double Separation { get; set; }
}

internal class CimSolidStroke : CimSymbolLayer
{
    [JsonPropertyName("color")]
    public CimColor? Color { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("lineStyle3D")]
    public string? LineStyle3D { get; set; }

    /// <summary>
    /// Geometric effects applied to the stroke (e.g. CIMGeometricEffectDashes for dash patterns).
    /// </summary>
    [JsonPropertyName("effects")]
    public List<CimGeometricEffect>? Effects { get; set; }
}

/// <summary>Base for CIM geometric effects. Unknown subtypes fall back to <see cref="CimUnknownGeometricEffect"/>.</summary>
[JsonConverter(typeof(CimGeometricEffectConverter))]
internal class CimGeometricEffect
{
}

/// <summary>Represents any geometric effect type not explicitly modelled — allows graceful fallback.</summary>
internal class CimUnknownGeometricEffect : CimGeometricEffect
{
    public string TypeName { get; init; } = string.Empty;
}

/// <summary>
/// Dash-pattern effect. <c>DashTemplate</c> is an alternating array of
/// dash-length / gap-length values (in points).
/// </summary>
internal class CimGeometricEffectDashes : CimGeometricEffect
{
    [JsonPropertyName("dashTemplate")]
    public List<double>? DashTemplate { get; set; }
}

/// <summary>
/// Tolerant converter for <see cref="CimGeometricEffect"/>.
/// Known types are fully deserialized; unknown types become <see cref="CimUnknownGeometricEffect"/>.
/// </summary>
internal sealed class CimGeometricEffectConverter : JsonConverter<CimGeometricEffect>
{
    public override CimGeometricEffect? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var typeName = root.TryGetProperty("type", out var typeProp)
            ? typeProp.GetString() ?? string.Empty
            : string.Empty;

        if (typeName == "CIMGeometricEffectDashes")
        {
            return JsonSerializer.Deserialize<CimGeometricEffectDashes>(root.GetRawText(), options);
        }

        // All other effect types: record the type name for warning purposes, ignore properties
        return new CimUnknownGeometricEffect { TypeName = typeName };
    }

    public override void Write(Utf8JsonWriter writer, CimGeometricEffect value, JsonSerializerOptions options)
        => throw new NotSupportedException();
}

internal class CimCharacterMarker : CimSymbolLayer
{
    [JsonPropertyName("size")]
    public double Size { get; set; }

    [JsonPropertyName("color")]
    public CimColor? Color { get; set; }

    [JsonPropertyName("characterIndex")]
    public int CharacterIndex { get; set; }

    [JsonPropertyName("fontFamilyName")]
    public string FontFamilyName { get; set; } = "Arial";

    [JsonPropertyName("rotation")]
    public double Rotation { get; set; }

    /// <summary>
    /// Nested symbol (usually CIMPolygonSymbol) that carries the fill color
    /// of the character marker when no top-level color is set.
    /// </summary>
    [JsonPropertyName("symbol")]
    public CimSymbol? Symbol { get; set; }
}

internal class CimVectorMarker : CimSymbolLayer
{
    [JsonPropertyName("size")]
    public double Size { get; set; }

    [JsonPropertyName("colorLocked")]
    public bool ColorLocked { get; set; }

    [JsonPropertyName("anchorPointUnits")]
    public string? AnchorPointUnits { get; set; }

    [JsonPropertyName("markerGraphics")]
    public List<CimMarkerGraphic>? MarkerGraphics { get; set; }
}

internal class CimMarkerGraphic
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("symbol")]
    public CimSymbol? Symbol { get; set; }
}

internal class CimPictureMarker : CimSymbolLayer
{
    [JsonPropertyName("size")]
    public double Size { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Picture fill layer. Not supported by gView — deserialized to allow
/// graceful fallback to <see cref="SimpleFillSymbol"/>.
/// </summary>
internal class CimPictureFill : CimSymbolLayer
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
