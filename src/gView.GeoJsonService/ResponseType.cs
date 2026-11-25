namespace gView.GeoJsonService;

public class ResponseType
{
    public const string ServiceCapabilities = "GetServiceCapabilitiesResponse";
    public const string Map = "GetMapResponse";
    public const string Legend = "GetLegendResponse";
    public const string FeatureCollection = "FeatureCollection";
    public const string FeaturesCountOnly = "FeaturesCountOnlyResponse";
    public const string FeaturesIdsOnly = "FeaturesIdsOnlyResponse";
    public const string FeaturesDistinct = "FeaturesDistinct";
    public const string Error = "ErrorResponse";
    public const string Layers = "GetLayersResponse";
    public const string Identify = "IdentifyResponse";
    public const string FeaturesStatistics = "FeaturesStatisticsResponse";
    public const string EditFeaturesBatch = "ApplyEditsResponse";
}
