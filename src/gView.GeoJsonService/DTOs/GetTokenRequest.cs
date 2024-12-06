namespace gView.GeoJsonService.DTOs;
public class GetTokenRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetToken;

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public int ExpireMinutes { get; set; } = 60;
}
