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

    /// <summary>An extra filter per layer id (SQL whereClause or cql2), on top of the definition query of the layer</summary>
    public IDictionary<string, AttributeFilter>? LayerDefs { get; set; }

    /// <summary>Object ids to highlight per layer id, drawn on top of the map</summary>
    public IDictionary<string, int[]>? Highlight { get; set; }
}


