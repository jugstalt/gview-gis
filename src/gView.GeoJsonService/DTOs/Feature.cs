namespace gView.GeoJsonService.DTOs;

public class Feature
{
    public object? Oid { get; set; }
    public BBox? BBox { get; set; }
    public Geometry? Geometry { get; set; } = new Geometry(); // Keeping this Geometry property as it represents the geometry of the feature itself
    public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
}



