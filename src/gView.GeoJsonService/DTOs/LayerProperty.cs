namespace gView.Framework.GeoJsonService.DTOs;
public class LayerProperty
{
    public string Name { get; set; } = string.Empty;
    public string Aliasname { get; set; } = string.Empty;
    public PropertyType Type { get; set; } = PropertyType.String;
    public bool? IsPrimaryKey { get; set; } = null;
}
