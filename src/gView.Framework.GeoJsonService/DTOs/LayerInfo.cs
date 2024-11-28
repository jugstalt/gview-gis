namespace gView.Framework.GeoJsonService.DTOs;
public class LayerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float? MinScaleDenominator { get; set; }
    public float? MaxScaleDenominator { get; set; }
    public IEnumerable<LayerStyle> Styles { get; set; } = Array.Empty<LayerStyle>();
    public IEnumerable<LayerProperty> Properties { get; set; } = Array.Empty<LayerProperty>();
    public string GeometryType { get; set; } = string.Empty;
}
