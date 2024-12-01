namespace gView.GeoJsonService.DTOs;

public class SpatialFilter
{
    public Geometry Geometry { get; set; } = new Geometry();
    public SpatialOperator Operator { get; set; } = SpatialOperator.Intersects; // Spatial relationship operator (e.g., "within", "intersects", "contains")
}



