namespace gView.Framework.GeoJsonService.DTOs;
public abstract class SupportedRequestProperties
{
    // Common properties for all requests can be added here if needed
}

public class GetMapRequestProperties : SupportedRequestProperties
{
    public int MaxImageWidth { get; set; }
    public int MaxImageHeight { get; set; }
    public IEnumerable<string> SupportedFormats { get; set; } = Array.Empty<string>();
}

public class GetFeaturesRequestProperties : SupportedRequestProperties
{
    public int MaxFeaturesLimit { get; set; }
}