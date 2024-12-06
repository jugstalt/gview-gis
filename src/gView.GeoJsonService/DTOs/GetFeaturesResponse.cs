namespace gView.GeoJsonService.DTOs;

public class GetFeaturesResponse
{
    public string Type { get; set; } = "GetFeaturesResponse";
    public string Layer { get; set; } = string.Empty;
    public IEnumerable<Feature> Features { get; set; } = Array.Empty<Feature>();
    public int TotalFeatures { get; set; } // Total number of features available (ignoring Limit/Offset)
}



