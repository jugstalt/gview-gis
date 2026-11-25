namespace gView.GeoJsonService.DTOs;

public class Feature
{
    public object? Oid { get; set; }

    /// <summary>applyEdits: the version (entity tag) the feature must still have to be changed</summary>
    public string? Version { get; set; }
    public BBox? BBox { get; set; }
    public Geometry? Geometry { get; set; } // Keeping this Geometry property as it represents the geometry of the feature itself
    public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
}



