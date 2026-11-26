namespace gView.GeoJsonService.DTOs;

public class SpatialFilter
{
    public CoordinateReferenceSystem? CRS { get; set; }
    public Geometry? Geometry { get; set; }  // either Geometry or BBox
    [System.Text.Json.Serialization.JsonPropertyName("bbox")]
    public BBox? BBox { get; set; }

    public SpatialOperator Operator { get; set; } = SpatialOperator.Intersects; // Spatial relationship operator (e.g., "within", "intersects", "contains")
}



