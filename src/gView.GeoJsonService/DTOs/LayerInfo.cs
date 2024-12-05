namespace gView.GeoJsonService.DTOs;
public class LayerInfo
{
    public string Id { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LayerType { get; set; } = "";  // FeatureLayer, RasterLayer, GroupLayer
    public bool? DefaultVisibility { get; set; }
    public string? GeometryType { get; set; }
    public double? MinScaleDenominator { get; set; }
    public double? MaxScaleDenominator { get; set; }
    public IEnumerable<LayerStyle>? Styles { get; set; }
    public IEnumerable<string>? SuportedOperations { get; set; }
    public IEnumerable<LayerProperty>? Properties { get; set; }
    
}
