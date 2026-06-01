using System.Text.Json.Serialization;

namespace gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

/// <summary>
/// An item inside GISProject.json that references a map or other project resource.
/// </summary>
internal class CimProjectItem
{
    /// <summary>CIM type, e.g. "CIMMapDocument" or "CIMProjectItem"</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Item kind for CIMProjectItem entries, e.g. "Map", "Layout", "Database".
    /// </summary>
    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; }

    /// <summary>Relative path inside the APRX ZIP, e.g. "Maps/Map.mapx"</summary>
    [JsonPropertyName("URI")]
    public string? Uri { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// CIMPATH reference used by ArcGIS Pro 3.x, e.g. "CIMPATH=Layer/Layer.json".
    /// Fallback when <see cref="Uri"/> is null.
    /// </summary>
    [JsonPropertyName("catalogPath")]
    public string? CatalogPath { get; set; }
}
