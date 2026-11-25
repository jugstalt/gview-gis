namespace gView.GeoJsonService.DTOs;

/// <summary>
/// The allowed values of a property: range (min, max), values (a list) or dictionary (label => value)
/// </summary>
public class PropertyDomain
{
    public string Type { get; set; } = "values";
    public double? Min { get; set; }
    public double? Max { get; set; }
    public IEnumerable<object?>? Values { get; set; }
    public IDictionary<string, object?>? Dictionary { get; set; }
}
