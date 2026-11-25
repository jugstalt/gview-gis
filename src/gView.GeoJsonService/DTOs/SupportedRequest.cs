using System.Text.Json.Serialization;

namespace gView.GeoJsonService.DTOs;

public static class RequestProperties
{
    public const string GetMap = "map";
    public const string QueryFeatures = "query";
    public const string GetLegend = "legend";
    public const string EditFeatures = "features";
    public const string GetLayers = "layers";
    public const string Identify = "identify";
    public const string ApplyEdits = "applyEdits";
}

//[JsonPolymorphic(TypeDiscriminatorPropertyName = "name")]
//[JsonDerivedType(typeof(GetMapRequestProperties), RequestProperties.GetMap)]
//[JsonDerivedType(typeof(GetFeaturesRequestProperties), RequestProperties.QueryFeatures)]
//[JsonDerivedType(typeof(GetLegendRequestProperties), RequestProperties.GetLegend)]
//[JsonDerivedType(typeof(EditFeaturesRequestProperties), RequestProperties.EditFeatures)]
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

public class GetLayersRequestProperties : SupportedRequest
{
    public GetLayersRequestProperties()
    {
        Name = RequestProperties.GetLayers;
    }
}

public class IdentifyRequestProperties : SupportedRequest
{
    public IdentifyRequestProperties()
    {
        Name = RequestProperties.Identify;
    }

    public int MaxFeaturesLimit { get; set; }
}

public class ApplyEditsRequestProperties : SupportedRequest
{
    public ApplyEditsRequestProperties()
    {
        Name = RequestProperties.ApplyEdits;
    }
}

/// <summary>A request a newer server offers that this client doesn't know (yet)</summary>
public class OtherRequestProperties : SupportedRequest
{
}

