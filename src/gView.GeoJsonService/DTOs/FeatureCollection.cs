namespace gView.GeoJsonService.DTOs;

public class FeatureCollection : BaseResponse
{
    override public string Type { get; set; } = ResponseType.FeatureCollection;

    public CoordinateReferenceSystem? CRS { get; set; }

    public IEnumerable<Feature>? Features { get; set; }
}
