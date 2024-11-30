namespace gView.Framework.GeoJsonService.DTOs;

public class Geometry
{
    public GeometryType Type { get; set; } = GeometryType.Point; // Geometry type (e.g., "Polygon", "Point", "MultiPoint", etc.)
    public object Coordinates { get; set; } = Array.Empty<float[]>(); // Coordinates can be a variety of types depending on geometry
}