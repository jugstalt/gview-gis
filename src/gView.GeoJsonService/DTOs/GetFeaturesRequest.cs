namespace gView.GeoJsonService.DTOs;

public class GetFeaturesRequest : BaseRequest
{
    override public string Type { get; set; } = RequestTypes.GetFeatures;

    public QueryCommand Command { get; set; } = QueryCommand.Select;
    public string[] OutFields { get; set; } = Array.Empty<string>();
    public GeometryResult ReturnGeometry { get; set; } = GeometryResult.None;
    public CoordinateReferenceSystem? OutCRS { get; set; } 
    public string[]? OrderByFields { get; set; }

    public string[]? ObjectIds { get; set; }

    public SpatialFilter? SpatialFilter { get; set; } // Optional spatial filter object
    public AttributeFilter? Filter { get; set; } // Optional attribute filter

    public int? Limit { get; set; } // Optional limit for the number of features to return
    public int? Offset { get; set; } // Optional offset for pagination

    // command "statistics"

    /// <summary>The statistics (command statistics)</summary>
    public IEnumerable<StatisticRequest>? Statistics { get; set; }

    /// <summary>The fields to group by (command statistics); none = one row</summary>
    public string[]? GroupBy { get; set; }

    /// <summary>A CQL2 filter on the result rows (command statistics), on group-by fields and statistic names</summary>
    public object? Having { get; set; }
}



