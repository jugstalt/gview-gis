namespace gView.Framework.GeoJsonService.DTOs;
public class SupportedRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SupportedRequestProperties? Properties { get; set; }
}
