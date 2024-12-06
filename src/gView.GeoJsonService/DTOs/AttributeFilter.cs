namespace gView.GeoJsonService.DTOs;


public class AttributeFilter
{
    public string? WhereClause { get; set; }
    public IDictionary<string, object>? Parameters { get; set; }
}
