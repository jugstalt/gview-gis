namespace gView.GeoJsonService.DTOs;

public class GetServicesResponse
{
    public string Type { get; set; } = "GetServicesResponse";

    public IEnumerable<string> Folders { get; set; } = Array.Empty<string>();
    public IEnumerable<string> Services { get; set; } = Array.Empty<string>();
}
