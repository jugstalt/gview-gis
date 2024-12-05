namespace gView.GeoJsonService.DTOs;
public class GetLegendRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetLegend;

    public int Width { get; set; } = 20;
    public int Height { get; set; } = 20;
    public int? Dpi { get; set; }
}

public class GetLegendResponse
{
    public string Type { get; set; } = "GetLegendResponse";

    public IEnumerable<LegendLayer>? Layers { get; set; }
}

public class LegendLayer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string LayerType { get; set; } = "";


    public double? MinScaleDenominator { get; set; }
    public double? MaxScaleDenominator { get; set; }

    public IEnumerable<LegendItem>? Items { get; set; }
}

public class LegendItem
{
    public string? Label { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImageContentType { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}