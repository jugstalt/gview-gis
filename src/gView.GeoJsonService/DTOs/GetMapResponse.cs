namespace gView.GeoJsonService.DTOs;

public class GetMapResponse
{
    public string Type { get; set; } = "GetMapResponse";
    public string? ImageUrl { get; set; }
    public string? ImageBase64 { get; set; }
    public double[] BBox { get; set; } = Array.Empty<double>();
    public int Width { get; set; }
    public int Height { get; set; }
    public double ScaleDenominator { get; set; }
    public double? Rotation { get; set; }
    public string ContentType { get; set; } = string.Empty;
}


