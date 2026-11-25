namespace gView.GeoJsonService.DTOs;
public class LayerProperty
{
    public string Name { get; set; } = string.Empty;
    public string Aliasname { get; set; } = string.Empty;
    public PropertyType Type { get; set; } = PropertyType.String;
    public bool? IsPrimaryKey { get; set; } = null;

    // layers endpoint only
    public int? Length { get; set; }
    public bool? Nullable { get; set; }
    public bool? Editable { get; set; }
    public PropertyDomain? Domain { get; set; }
}
