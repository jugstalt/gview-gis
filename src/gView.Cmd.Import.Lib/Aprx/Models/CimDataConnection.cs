using System.Text.Json.Serialization;

namespace gView.Cmd.Import.Aprx.Models;

/// <summary>
/// Describes the data source (feature class) bound to a feature layer.
/// Corresponds to CIMFeatureTable in the CIM specification.
/// </summary>
internal class CimFeatureTable
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// SQL definition expression / filter applied to the feature class,
    /// e.g. "NIS1STAT &lt;&gt; 'Erfasst'". Takes priority over the layer-level
    /// <c>definitionExpression</c> when present.
    /// </summary>
    [JsonPropertyName("definitionExpression")]
    public string? DefinitionExpression { get; set; }

    [JsonPropertyName("dataConnection")]
    public CimDataConnection? DataConnection { get; set; }

    [JsonPropertyName("displayField")]
    public string? DisplayField { get; set; }

    [JsonPropertyName("fieldDescriptions")]
    public List<CimFieldDescription>? FieldDescriptions { get; set; }
}

/// <summary>
/// Data connection to the underlying dataset / feature class.
/// </summary>
internal class CimDataConnection
{
    /// <summary>e.g. "CIMStandardDataConnection", "CIMSqlQueryDataConnection"</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Workspace connection string, e.g.
    /// "DATABASE=C:\data\roads.gdb" or "SERVER=myserver;DATABASE=mydb".
    /// </summary>
    [JsonPropertyName("workspaceConnectionString")]
    public string? WorkspaceConnectionString { get; set; }

    /// <summary>Workspace factory type, e.g. "FileGDB", "SDE".</summary>
    [JsonPropertyName("workspaceFactory")]
    public string? WorkspaceFactory { get; set; }

    /// <summary>Name of the dataset / feature class.</summary>
    [JsonPropertyName("dataset")]
    public string? Dataset { get; set; }

    /// <summary>Dataset type, e.g. "esriDTFeatureClass", "esriDTTable".</summary>
    [JsonPropertyName("datasetType")]
    public string? DatasetType { get; set; }

    /// <summary>Optional SQL query used with CIMSqlQueryDataConnection.</summary>
    [JsonPropertyName("sqlQuery")]
    public string? SqlQuery { get; set; }

    /// <summary>Optional URL for web service connections.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Describes display properties of a single field within a feature table.
/// </summary>
internal class CimFieldDescription
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>The actual field name in the dataset.</summary>
    [JsonPropertyName("fieldName")]
    public string? FieldName { get; set; }

    /// <summary>Display alias shown in the UI (may differ from <see cref="FieldName"/>).</summary>
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }

    /// <summary>Whether the field is visible in attribute tables / pop-ups.</summary>
    [JsonPropertyName("visible")]
    public bool Visible { get; set; } = true;

    [JsonPropertyName("searchMode")]
    public string? SearchMode { get; set; }
}
