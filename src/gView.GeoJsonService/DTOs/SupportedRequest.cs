using System.Text.Json.Serialization;

namespace gView.GeoJsonService.DTOs;

public static class RequestProperties
{
    public const string GetMap = "map";
    public const string QueryFeatures = "query";
    public const string GetLegend = "legend";
    public const string EditFeatures = "features";
}

abstract public class SupportedRequest
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = string.Empty;
    public string[] HttpMethods { get; set; } = [];
}

public class GetMapRequestProperties : SupportedRequest
{
    public GetMapRequestProperties()
    {
        Name = RequestProperties.GetMap;
    }

    public int MaxImageWidth { get; set; }
    public int MaxImageHeight { get; set; }
    public IEnumerable<string> SupportedFormats { get; set; } = Array.Empty<string>();
}

public class GetFeaturesRequestProperties : SupportedRequest
{
    public GetFeaturesRequestProperties()
    {
        Name = RequestProperties.QueryFeatures;
    }
    
    public int MaxFeaturesLimit { get; set; }
}

public class EditFeaturesRequestProperties : SupportedRequest
{
    public EditFeaturesRequestProperties()
    {
        Name = RequestProperties.EditFeatures;
    }
}

public class GetLegendRequestProperties : SupportedRequest
{
    public GetLegendRequestProperties()
    {
        Name = RequestProperties.GetLegend;
    }
}

