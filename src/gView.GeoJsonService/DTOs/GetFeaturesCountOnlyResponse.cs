namespace gView.GeoJsonService.DTOs;

public class GetFeaturesCountOnlyResponse : BaseResponse
{
    public override string Type { get; set; } = ResponseType.FeaturesCountOnly;

    public int Count { get; set; }
}

public class GetFeaturesIdsOnlyResponse : BaseResponse
{
    public override string Type { get; set; } = ResponseType.FeaturesIdsOnly;

    public string? ObjectIdFieldName { get; set; } 
    public IEnumerable<int>? ObjectIds { get; set; }    
}

public class GetFeaturesDistinctResponse : BaseResponse
{
    public override string Type { get; set; } = ResponseType.FeaturesDistinct;

    public string? DistinctField { get; set; }
    public IEnumerable<object>? DistinctValues { get; set; }
}