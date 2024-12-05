using System.Text.Json.Serialization;

namespace gView.GeoJsonService.DTOs;

public static class RequestProperties
{
    public const string GetMap = "map";
    public const string QueryFeatures = "query";
    public const string GetLegend = "legend";
    public const string EditFeatures = "features";
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "name")]
[JsonDerivedType(typeof(GetMapRequestProperties), RequestProperties.GetMap)]
[JsonDerivedType(typeof(GetFeaturesRequestProperties), RequestProperties.QueryFeatures)]
[JsonDerivedType(typeof(GetLegendRequestProperties), RequestProperties.GetLegend)]
[JsonDerivedType(typeof(EditFeaturesRequestProperties), RequestProperties.EditFeatures)]
abstract public class SupportedRequest
{
    abstract public string Name { get; }
    public string Url { get; set; } = string.Empty;
    public string[] HttpMethods { get; set; } = [];
}

public class GetMapRequestProperties : SupportedRequest
{
    override public string Name { get; } = RequestProperties.GetMap;
    public int MaxImageWidth { get; set; }
    public int MaxImageHeight { get; set; }
    public IEnumerable<string> SupportedFormats { get; set; } = Array.Empty<string>();
}

public class GetFeaturesRequestProperties : SupportedRequest
{
    override public string Name { get; } = RequestProperties.QueryFeatures;
    public int MaxFeaturesLimit { get; set; }
}

public class EditFeaturesRequestProperties : SupportedRequest
{
    override public string Name { get; } = RequestProperties.EditFeatures;
}

public class GetLegendRequestProperties : SupportedRequest
{
    override public string Name { get; } = RequestProperties.GetLegend;
    public int MaxFeaturesLimit { get; set; }
}

