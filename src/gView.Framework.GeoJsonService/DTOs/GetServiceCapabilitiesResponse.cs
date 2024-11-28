namespace gView.Framework.GeoJsonService.DTOs;
public class GetServiceCapabilitiesResponse
{
    public string Type { get; set; } = "GetServiceCapabilitiesResponse";
    public IEnumerable<SupportedRequest> SupportedRequests { get; set; } = Array.Empty<SupportedRequest>();
    public IEnumerable<LayerInfo> Layers { get; set; } = Array.Empty<LayerInfo>();
}
