using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// A label class associated with a feature layer.
/// </summary>
internal class CimLabelClass
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Label expression, e.g. a field name like "STRNAME" or an Arcade / VBScript expression.
    /// </summary>
    [JsonPropertyName("expression")]
    public string? Expression { get; set; }

    [JsonPropertyName("expressionEngine")]
    public string? ExpressionEngine { get; set; }

    [JsonPropertyName("fieldNames")]
    public List<string>? FieldNames { get; set; }

    [JsonPropertyName("textSymbol")]
    public CimSymbolReference? TextSymbol { get; set; }

    /// <summary>SQL where clause to restrict labeling, e.g. "STATUS = 'Active'".</summary>
    [JsonPropertyName("whereClause")]
    public string? WhereClause { get; set; }

    [JsonPropertyName("visibility")]
    public bool Visibility { get; set; } = true;

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("minimumScale")]
    public double MinimumScale { get; set; }

    [JsonPropertyName("maximumScale")]
    public double MaximumScale { get; set; }
}
