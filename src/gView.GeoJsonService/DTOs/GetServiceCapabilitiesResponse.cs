namespace gView.GeoJsonService.DTOs;

public class GetServiceCapabilitiesResponse
{
    public string Type { get; set; } = "GetServiceCapabilitiesResponse";

    public string MapTitle { get; set; } = "";
    public string Description { get; set; } = "";
    public string Copyright { get; set; } = "";
    public IEnumerable<SupportedRequest> SupportedRequests { get; set; } = Array.Empty<SupportedRequest>();

    public CoordinateReferenceSystem? CRS { get; set; }
    public BBox? FullExtent { get; set; }
    public BBox? InitialExtent { get; set; }
    public string? Units { get; set; } = "";

    public IEnumerable<LayerInfo> Layers { get; set; } = Array.Empty<LayerInfo>();
}
