namespace gView.Framework.GeoJsonService.DTOs;

public class AttributeFilter
{
    public LogicOperator Logic { get; set; } = LogicOperator.AND; // Logical operator to combine conditions (e.g., "AND", "OR", "IN")
    public IEnumerable<FilterCondition> Conditions { get; set; } = Array.Empty<FilterCondition>();
    public IEnumerable<AttributeFilter>? NestedFilters { get; set; } // Optional nested filters for complex conditions
}



