namespace gView.Framework.GeoJsonService.DTOs;

public class GetMapRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetMap;
    public IEnumerable<string> Layers { get; set; } = Array.Empty<string>();
    public float[] BBox { get; set; } = Array.Empty<float>();
    public string CRS { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool Transparent { get; set; } = false;
    public float? Rotation { get; set; }
    public int? Dpi { get; set; }
}


