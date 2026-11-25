namespace gView.GeoJsonService.DTOs;

/// <summary>A statistic of the query command "statistics": count, sum, min, max, avg, stddev or var of a field</summary>
public class StatisticRequest
{
    public string Type { get; set; } = "count";
    public string Field { get; set; } = string.Empty;

    /// <summary>The name of the column in the result; default {type}_{field}</summary>
    public string? Alias { get; set; }
}

/// <summary>The result of the query command "statistics": a table, the group-by columns first</summary>
public class GetFeaturesStatisticsResponse : BaseResponse
{
    public override string Type { get; set; } = ResponseType.FeaturesStatistics;

    public IEnumerable<StatisticsField>? Fields { get; set; }

    /// <summary>One row per group (one row without groupBy): column name => value</summary>
    public IEnumerable<IDictionary<string, object?>>? Rows { get; set; }

    /// <summary>true if there are more rows than returned (limit)</summary>
    public bool ExceededLimit { get; set; }
}

public class StatisticsField
{
    public string Name { get; set; } = string.Empty;
    public PropertyType Type { get; set; } = PropertyType.String;
}
