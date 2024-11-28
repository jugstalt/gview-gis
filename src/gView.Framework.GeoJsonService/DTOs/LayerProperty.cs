namespace gView.Framework.GeoJsonService.DTOs;
public class LayerProperty
{
    public string Name { get; set; } = string.Empty;
    public Enums Type { get; set; } = Enums.String;
    public bool IsPrimaryKey { get; set; }
}
