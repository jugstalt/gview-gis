namespace gView.Framework.GeoJsonService.DTOs;

public class GetMapResponse
{
    public string Type { get; set; } = "GetMapResponse";
    public string? ImageUrl { get; set; }
    public string? ImageBase64 { get; set; }
    public float[] BBox { get; set; } = Array.Empty<float>();
    public int Width { get; set; }
    public int Height { get; set; }
    public float ScaleDenominator { get; set; }
    public float? Rotation { get; set; }
    public string Format { get; set; } = string.Empty;
}


