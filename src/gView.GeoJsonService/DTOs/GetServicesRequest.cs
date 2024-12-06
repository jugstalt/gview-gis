namespace gView.GeoJsonService.DTOs;

public class GetServicesRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetServices;

    public string? Folder { get; set; }
}
