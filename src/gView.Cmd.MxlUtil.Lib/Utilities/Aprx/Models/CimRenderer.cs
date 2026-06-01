using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// Base class for all CIM renderer types.
/// Concrete type is identified by the <see cref="Type"/> property.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type",
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(CimSimpleRenderer), "CIMSimpleRenderer")]
[JsonDerivedType(typeof(CimUniqueValueRenderer), "CIMUniqueValueRenderer")]
[JsonDerivedType(typeof(CimClassBreaksRenderer), "CIMClassBreaksRenderer")]
internal class CimRenderer
{
    [JsonPropertyName("visualVariables")]
    public List<CimVisualVariable>? VisualVariables { get; set; }
}

/// <summary>
/// A renderer that draws all features with a single symbol.
/// </summary>
internal class CimSimpleRenderer : CimRenderer
{
    [JsonPropertyName("symbol")]
    public CimSymbolReference? Symbol { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// A renderer that maps distinct field values to individual symbols.
/// </summary>
internal class CimUniqueValueRenderer : CimRenderer
{
    [JsonPropertyName("fields")]
    public List<string>? Fields { get; set; }

    /// <summary>Default symbol used for values not covered by a class.</summary>
    [JsonPropertyName("defaultSymbol")]
    public CimSymbolReference? DefaultSymbol { get; set; }

    [JsonPropertyName("defaultLabel")]
    public string? DefaultLabel { get; set; }

    /// <summary>When false the default symbol should not be shown as "all other values".</summary>
    [JsonPropertyName("useDefaultSymbol")]
    public bool UseDefaultSymbol { get; set; }

    [JsonPropertyName("groups")]
    public List<CimUniqueValueGroup>? Groups { get; set; }
}

internal class CimUniqueValueGroup
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("classes")]
    public List<CimUniqueValueClass>? Classes { get; set; }
}

internal class CimUniqueValueClass
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Each value is a string that matches the renderer field(s).
    /// Multiple fields are separated by comma when field count > 1.
    /// </summary>
    [JsonPropertyName("values")]
    public List<CimUniqueValue>? Values { get; set; }

    [JsonPropertyName("symbol")]
    public CimSymbolReference? Symbol { get; set; }

    [JsonPropertyName("visible")]
    public bool Visible { get; set; } = true;
}

internal class CimUniqueValue
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Field value(s) for this class, one entry per renderer field.</summary>
    [JsonPropertyName("fieldValues")]
    public List<string>? FieldValues { get; set; }
}

/// <summary>
/// A renderer that classifies features by graduated colour or size.
/// </summary>
internal class CimClassBreaksRenderer : CimRenderer
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("classBreakType")]
    public string? ClassBreakType { get; set; }

    [JsonPropertyName("breaks")]
    public List<CimClassBreak>? Breaks { get; set; }

    [JsonPropertyName("defaultSymbol")]
    public CimSymbolReference? DefaultSymbol { get; set; }
}

internal class CimClassBreak
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("upperBound")]
    public double UpperBound { get; set; }

    [JsonPropertyName("symbol")]
    public CimSymbolReference? Symbol { get; set; }
}

/// <summary>Base for CIM visual variables (rotation, size, color, etc.).</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type",
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(CimRotationVisualVariable), "CIMRotationVisualVariable")]
internal class CimVisualVariable
{
}

/// <summary>
/// Rotation visual variable. Only the Z-axis rotation (<see cref="VisualVariableInfoZ"/>)
/// and <see cref="RotationTypeZ"/> are used by the converter.
/// </summary>
internal class CimRotationVisualVariable : CimVisualVariable
{
    [JsonPropertyName("visualVariableInfoZ")]
    public CimVisualVariableInfo? VisualVariableInfoZ { get; set; }

    /// <summary>"Arithmetic" or "Geographic".</summary>
    [JsonPropertyName("rotationTypeZ")]
    public string? RotationTypeZ { get; set; }
}

internal class CimVisualVariableInfo
{
    /// <summary>Expression string, e.g. "[FieldName]" or a more complex expression.</summary>
    [JsonPropertyName("expression")]
    public string? Expression { get; set; }

    [JsonPropertyName("visualVariableInfoType")]
    public string? VisualVariableInfoType { get; set; }
}
