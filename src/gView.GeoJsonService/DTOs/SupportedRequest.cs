using System.Text.Json.Serialization;

namespace gView.Framework.GeoJsonService.DTOs;


[JsonPolymorphic(TypeDiscriminatorPropertyName = "name")]
[JsonDerivedType(typeof(GetMapRequestProperties), "GetMap")]
[JsonDerivedType(typeof(GetFeaturesRequestProperties), "GetFeatures")]
[JsonDerivedType(typeof(GetLegendRequestProperties), "GetLegend")]
abstract public class SupportedRequest
{
    abstract public string Name { get; }
    public string Url { get; set; } = string.Empty;
    public string[] HttpMethods { get; set; } = [];
}

public class GetMapRequestProperties : SupportedRequest
{
    override public string Name { get; } = "GetMap";
    public int MaxImageWidth { get; set; }
    public int MaxImageHeight { get; set; }
    public IEnumerable<string> SupportedFormats { get; set; } = Array.Empty<string>();
}

public class GetFeaturesRequestProperties : SupportedRequest
{
    override public string Name { get; } = "GetFeatures";
    public int MaxFeaturesLimit { get; set; }
}
public class GetLegendRequestProperties : SupportedRequest
{
    override public string Name { get; } = "GetLegend";
    public int MaxFeaturesLimit { get; set; }
}

