namespace gView.GeoJsonService.DTOs;

public class GetServiceCapabilitiesRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetServiceCapabilities;

    public CoordinateReferenceSystem? CRS { get; set; }
}

