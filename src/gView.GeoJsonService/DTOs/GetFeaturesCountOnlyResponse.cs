namespace gView.GeoJsonService.DTOs;

public class GetFeaturesCountOnlyResponse
{
    public int Count { get; set; }
}

public class GetFeaturesIdsOnlyResponse
{
    public string? ObjectIdFieldName { get; set; } 
    public IEnumerable<int>? ObjectIds { get; set; }    
}

public class GetFeaturesDistinctResponse
{
    public string? DistinctField { get; set; }
    public IEnumerable<object>? DistinctValues { get; set; }
}