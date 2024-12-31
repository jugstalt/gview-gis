namespace gView.GeoJsonService.DTOs;

public class GetMapRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetMap;
    public IEnumerable<string> Layers { get; set; } = Array.Empty<string>();
    public BBox BBox { get; set; } = default!;
    public CoordinateReferenceSystem? CRS { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = string.Empty;
    public bool Transparent { get; set; } = false;
    public double? Rotation { get; set; }
    public float? Dpi { get; set; }
    public MapReponseFormat ResponseFormat { get; set; } = MapReponseFormat.Url;
}


