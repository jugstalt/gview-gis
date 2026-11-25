namespace gView.GeoJsonService.DTOs;

public class IdentifyResponse : BaseResponse
{
    override public string Type { get; set; } = ResponseType.Identify;

    /// <summary>One entry per layer with hits, top layer first</summary>
    public IEnumerable<IdentifyResult>? Results { get; set; }
}

public class IdentifyResult
{
    public string LayerId { get; set; } = string.Empty;
    public string LayerName { get; set; } = string.Empty;
    public FeatureCollection? FeatureCollection { get; set; }
}
