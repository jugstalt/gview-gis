namespace gView.GeoJsonService.DTOs;
public class GetTokenRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetToken;

    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int ExpireMinutes { get; set; } = 60;
}
