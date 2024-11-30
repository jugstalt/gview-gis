namespace gView.Framework.GeoJsonService.DTOs;

public class GetInfoResponse
{
    public string Type { get; set; } = "GetInfoResponse";

    public Version Version { get; set; } = default!;
    public string GetTokenUrl { get; set; } = "";
}