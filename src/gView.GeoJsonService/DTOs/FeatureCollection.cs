namespace gView.GeoJsonService.DTOs;

public class FeatureCollection : BaseResponse
{
    override public string Type { get; set; } = ResponseType.FeatureCollection;

    public CoordinateReferenceSystem? CRS { get; set; }

    public IEnumerable<Feature>? Features { get; set; }

    /// <summary>
    /// Query (select): true if there are more features than returned (limit or the max. record count of the
    /// service), so the next page starts at offset + the number of features.
    /// </summary>
    public bool? ExceededLimit { get; set; }
}
