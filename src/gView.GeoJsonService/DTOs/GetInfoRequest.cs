namespace gView.Framework.GeoJsonService.DTOs;

public class GetInfoRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetInfo;
}
