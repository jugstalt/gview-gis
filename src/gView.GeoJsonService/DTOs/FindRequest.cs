namespace gView.GeoJsonService.DTOs;

/// <summary>A text search over several layers and their text fields</summary>
public class FindRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.Find;

    public string SearchText { get; set; } = string.Empty;

    /// <summary>The fields to search (text fields only); null = all text fields of a layer</summary>
    public string[]? SearchFields { get; set; }

    /// <summary>The layer ids to search; null = all queryable layers</summary>
    public string[]? Layers { get; set; }

    /// <summary>true (default): the field contains the text; false: the field is the text. Case-insensitive.</summary>
    public bool Contains { get; set; } = true;

    public GeometryResult ReturnGeometry { get; set; } = GeometryResult.Geometry;
    public CoordinateReferenceSystem? OutCRS { get; set; }

    /// <summary>Max. features per layer (default and maximum: the max. record count of the service)</summary>
    public int? Limit { get; set; }

    public double? MaxAllowableOffset { get; set; }
    public int? GeometryPrecision { get; set; }
}

public class FindResponse : BaseResponse
{
    override public string Type { get; set; } = ResponseType.Find;

    /// <summary>One entry per layer with hits, top layer first</summary>
    public IEnumerable<IdentifyResult>? Results { get; set; }
}
