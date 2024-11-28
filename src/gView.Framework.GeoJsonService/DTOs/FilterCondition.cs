namespace gView.Framework.GeoJsonService.DTOs;

public class FilterCondition
{
    public string Property { get; set; } = string.Empty;
    public ComparisonOperator Operator { get; set; } = ComparisonOperator.Equals; // Comparison operator (e.g., "equals", "greater_than", "less_than", etc.)
    public object Value { get; set; } = new object();
}



