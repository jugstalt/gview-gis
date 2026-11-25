namespace gView.GeoJsonService.DTOs;

public class GetLayersResponse : BaseResponse
{
    override public string Type { get; set; } = ResponseType.Layers;

    public IEnumerable<LayerDetail>? Layers { get; set; }
}

/// <summary>
/// A layer with everything a client needs to build forms and legends: the capabilities' layer info plus extent,
/// description, edit operations, the domains of the properties and the renderer
/// </summary>
public class LayerDetail : LayerInfo
{
    public string? Description { get; set; }
    public string? Copyright { get; set; }

    /// <summary>The extent of the data in <see cref="CRS"/></summary>
    public BBox? Extent { get; set; }
    public CoordinateReferenceSystem? CRS { get; set; }

    /// <summary>insert, update, delete: the edits the service allows for this layer</summary>
    public IEnumerable<string>? EditOperations { get; set; }

    public LayerRenderer? Renderer { get; set; }
}

/// <summary>
/// A simplified renderer: single (one symbol), categories (a symbol per value of <see cref="Field"/>),
/// classes (a symbol per range of <see cref="Field"/>) or other (only the legend images)
/// </summary>
public class LayerRenderer
{
    public string Type { get; set; } = "other";
    public string? Field { get; set; }
    public IEnumerable<RendererClass>? Classes { get; set; }

    /// <summary>The symbol of all other values (categories, classes)</summary>
    public RendererClass? Default { get; set; }
}

public class RendererClass
{
    public string? Label { get; set; }

    /// <summary>categories: the value</summary>
    public string? Value { get; set; }

    /// <summary>classes: the range</summary>
    public double? Min { get; set; }
    public double? Max { get; set; }

    public SymbolInfo? Symbol { get; set; }
}

/// <summary>
/// The main properties of a symbol: type point, line, fill, text or other; colors as [r, g, b, a] (0-255);
/// the legend image as PNG (base64) for every symbol, also the complex ones
/// </summary>
public class SymbolInfo
{
    public string Type { get; set; } = "other";
    public int[]? Color { get; set; }
    public int[]? OutlineColor { get; set; }

    /// <summary>line width, outline width (fill, point)</summary>
    public double? Width { get; set; }

    /// <summary>point size</summary>
    public double? Size { get; set; }

    public string? LegendImageBase64 { get; set; }
}
