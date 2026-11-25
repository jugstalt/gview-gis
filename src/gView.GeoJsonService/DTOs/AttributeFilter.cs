namespace gView.GeoJsonService.DTOs;


public class AttributeFilter
{
    public string? WhereClause { get; set; }
    public IDictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// A CQL2 filter: a string is CQL2-Text, an object CQL2-JSON. With a where clause, both must apply.
    /// </summary>
    public object? Cql2 { get; set; }
}
