namespace gView.GeoJsonService.DTOs;

public class Geometry
{
    public GeometryType Type { get; set; } // Geometry type (e.g., "Polygon", "Point", "MultiPoint", etc.)
    public object? Coordinates { get; set; } // Coordinates can be a variety of types depending on geometry
}