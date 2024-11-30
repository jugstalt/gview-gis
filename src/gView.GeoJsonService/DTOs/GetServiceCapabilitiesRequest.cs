namespace gView.Framework.GeoJsonService.DTOs;

public class GetServiceCapabilitiesRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetServiceCapabilities;
}

