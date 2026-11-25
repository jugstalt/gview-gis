namespace gView.GeoJsonService.DTOs;

/// <summary>
/// Adds, updates and deletes of several layers in one request: all or nothing, so all layers must be in one database
/// </summary>
public class ApplyEditsRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.ApplyEdits;

    /// <summary>The CRS of the geometries</summary>
    public CoordinateReferenceSystem? CRS { get; set; }

    public IEnumerable<LayerEdits>? Edits { get; set; }
}

public class LayerEdits
{
    public string LayerId { get; set; } = string.Empty;

    /// <summary>New features (without oid)</summary>
    public IEnumerable<Feature>? Adds { get; set; }

    /// <summary>Changed features (with oid; with version, only if the feature still has it)</summary>
    public IEnumerable<Feature>? Updates { get; set; }

    /// <summary>Features to delete: an object id, or { "oid": 1, "version": "..." } to delete only if unchanged</summary>
    public IEnumerable<object>? Deletes { get; set; }
}

public class ApplyEditsResponse : BaseResponse
{
    override public string Type { get; set; } = ResponseType.EditFeaturesBatch;

    public bool Succeeded { get; set; } = true;

    public IEnumerable<LayerEditsResult>? Results { get; set; }
}

public class LayerEditsResult
{
    public string LayerId { get; set; } = string.Empty;

    /// <summary>The new features: the new object id (0 if the database can't report it) and version</summary>
    public IEnumerable<EditedFeature>? Adds { get; set; }

    /// <summary>The changed features with their new version</summary>
    public IEnumerable<EditedFeature>? Updates { get; set; }

    public IEnumerable<int>? Deletes { get; set; }
}

public class EditedFeature
{
    public int Oid { get; set; }

    /// <summary>The version (entity tag) after the edit, null if it can't be computed</summary>
    public string? Version { get; set; }
}
