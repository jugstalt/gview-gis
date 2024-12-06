namespace gView.GeoJsonService.DTOs;
public class EditFeaturesRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.EditFeatures;

    public CoordinateReferenceSystem? CRS { get; set; }

    public IEnumerable<Feature>? Features { get; set; }

    public IEnumerable<object>? ObjectIds { get; set; }
}
