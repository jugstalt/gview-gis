namespace gView.GeoJsonService.DTOs;

public class GetMapRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetMap;
    public IEnumerable<string> Layers { get; set; } = Array.Empty<string>();
    public double[] BBox { get; set; } = Array.Empty<double>();
    public CoordinateReferenceSystem? CRS { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool Transparent { get; set; } = false;
    public double? Rotation { get; set; }
    public int? Dpi { get; set; }
    public string ResponseFormat { get; set; } = "link";
}


