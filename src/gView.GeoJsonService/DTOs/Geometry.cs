namespace gView.GeoJsonService.DTOs;

public class Geometry
{
    public GeometryType Type { get; set; } 
    public object? Coordinates { get; set; } 
}