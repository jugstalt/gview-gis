using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Exceptions;
using gView.Framework.Data.Statistics;

namespace gView.Framework.Data.Tests.Statistics;

public class StatisticsAggregatorTests
{
    // city, country, population
    private static readonly (string City, string? Country, double? Pop)[] Rows =
    {
        ("Vienna", "AT", 1900),
        ("Graz", "AT", 300),
        ("Linz", "AT", 200),
        ("Berlin", "DE", 3700),
        ("Bonn", "DE", null),
        ("Nowhere", null, 10),
    };

    private static StatisticsResult Run(StatisticsQuery query)
    {
        var aggregator = new StatisticsAggregator(query);
        foreach (var row in Rows)
        {
            var r = row;
            aggregator.Add(name => name switch
            {
                "CITY" => r.City,
                "COUNTRY" => r.Country,
                "POP" => r.Pop.HasValue ? r.Pop.Value : DBNull.Value,
                _ => throw new ArgumentException(name)
            });
        }
        return aggregator.GetResult();
    }

    private static StatisticDefinition Stat(StatisticType type, string outName, string field = "POP", FieldType fieldType = FieldType.Double)
        => new() { Type = type, OnField = field, OutName = outName, OnFieldType = fieldType };

    private static StatisticsQuery Query(params StatisticDefinition[] stats)
        => new() { Statistics = stats.ToList() };

    [Fact]
    public void NoGroupBy_ReturnsSingleRowWithAllStatistics()
    {
        var result = Run(Query(
            Stat(StatisticType.Count, "cnt"),
            Stat(StatisticType.Sum, "sum"),
            Stat(StatisticType.Min, "min"),
            Stat(StatisticType.Max, "max"),
            Stat(StatisticType.Avg, "avg")));

        var row = Assert.Single(result.Rows);
        Assert.Equal(5L, row["cnt"]);                  // NULL values are not counted
        Assert.Equal(6110.0, row["sum"]);
        Assert.Equal(10.0, row["min"]);
        Assert.Equal(3700.0, row["max"]);
        Assert.Equal(6110.0 / 5, row["avg"]);
    }

    [Fact]
    public void StdDevAndVar_UseSampleFormula()
    {
        var aggregator = new StatisticsAggregator(Query(
            Stat(StatisticType.Var, "var"), Stat(StatisticType.StdDev, "std")));
        foreach (var v in new double[] { 2, 4, 4, 4, 5, 5, 7, 9 })
        {
            aggregator.Add(_ => v);
        }

        var row = Assert.Single(aggregator.GetResult().Rows);
        Assert.Equal(32.0 / 7.0, (double)row["var"]!, 10);
        Assert.Equal(Math.Sqrt(32.0 / 7.0), (double)row["std"]!, 10);
    }

    [Fact]
    public void SingleValue_StdDevAndVar_AreNull()
    {
        var aggregator = new StatisticsAggregator(Query(Stat(StatisticType.Var, "var"), Stat(StatisticType.StdDev, "std")));
        aggregator.Add(_ => 5.0);

        var row = Assert.Single(aggregator.GetResult().Rows);
        Assert.Null(row["var"]);
        Assert.Null(row["std"]);
    }

    [Fact]
    public void NoGroupBy_NoRows_ReturnsOneRowWithCountZeroAndNulls()
    {
        var result = new StatisticsAggregator(Query(
            Stat(StatisticType.Count, "cnt"), Stat(StatisticType.Sum, "sum"), Stat(StatisticType.Max, "max"))).GetResult();

        var row = Assert.Single(result.Rows);
        Assert.Equal(0L, row["cnt"]);
        Assert.Null(row["sum"]);
        Assert.Null(row["max"]);
    }

    [Fact]
    public void GroupBy_NoRows_ReturnsNoRows()
    {
        var query = Query(Stat(StatisticType.Count, "cnt"));
        query.GroupByFields.Add("COUNTRY");

        Assert.Empty(new StatisticsAggregator(query).GetResult().Rows);
    }

    [Fact]
    public void GroupBy_GroupsPerValue_NullIsItsOwnGroup_SortedByGroupFieldByDefault()
    {
        var query = Query(Stat(StatisticType.Sum, "total"), Stat(StatisticType.Count, "cnt"));
        query.GroupByFields.Add("COUNTRY");

        var result = Run(query);

        Assert.Equal(3, result.Rows.Count);
        Assert.Null(result.Rows[0]["COUNTRY"]);       // NULL first
        Assert.Equal(10.0, result.Rows[0]["total"]);
        Assert.Equal("AT", result.Rows[1]["COUNTRY"]);
        Assert.Equal(2400.0, result.Rows[1]["total"]);
        Assert.Equal(3L, result.Rows[1]["cnt"]);
        Assert.Equal("DE", result.Rows[2]["COUNTRY"]);
        Assert.Equal(3700.0, result.Rows[2]["total"]);
        Assert.Equal(1L, result.Rows[2]["cnt"]);      // Bonn has NULL population
    }

    [Fact]
    public void GroupBy_MultipleFields()
    {
        var query = Query(Stat(StatisticType.Count, "cnt", "CITY", FieldType.String));
        query.GroupByFields.Add("COUNTRY");
        query.GroupByFields.Add("CITY");

        Assert.Equal(Rows.Length, Run(query).Rows.Count);
    }

    [Fact]
    public void OrderByStatisticDescending_Offset_Limit_SetsExceededLimit()
    {
        var query = Query(Stat(StatisticType.Sum, "total"));
        query.GroupByFields.Add("COUNTRY");
        query.OrderBy.Add(new StatisticsOrderBy { Name = "total", Descending = true });
        query.Offset = 0;
        query.Limit = 2;

        var result = Run(query);

        Assert.Equal(2, result.Rows.Count);
        Assert.True(result.ExceededLimit);
        Assert.Equal("DE", result.Rows[0]["COUNTRY"]);
        Assert.Equal("AT", result.Rows[1]["COUNTRY"]);

        query.Offset = 2;
        result = Run(query);
        Assert.Single(result.Rows);
        Assert.False(result.ExceededLimit);
        Assert.Null(result.Rows[0]["COUNTRY"]);
    }

    [Fact]
    public void MinMax_KeepFieldTypeAndWorkForStrings()
    {
        var result = Run(Query(
            Stat(StatisticType.Min, "first", "CITY", FieldType.String),
            Stat(StatisticType.Max, "last", "CITY", FieldType.String)));

        var row = Assert.Single(result.Rows);
        Assert.Equal("Berlin", row["first"]);
        Assert.Equal("Vienna", row["last"]);
        Assert.Equal(FieldType.String, result.Columns.Single(c => c.Name == "first").Type);
    }

    [Fact]
    public void Columns_GroupFieldsFirst_ThenStatistics_WithOutputTypes()
    {
        var query = Query(Stat(StatisticType.Count, "cnt"), Stat(StatisticType.Avg, "avg"));
        query.GroupByFields.Add("COUNTRY");
        query.GroupByFieldTypes.Add(FieldType.String);

        var columns = Run(query).Columns;

        Assert.Equal(new[] { "COUNTRY", "cnt", "avg" }, columns.Select(c => c.Name));
        Assert.Equal(new[] { FieldType.String, FieldType.integer, FieldType.Double }, columns.Select(c => c.Type));
    }

    [Fact]
    public void MaxScanRecords_Exceeded_Throws()
    {
        var query = Query(Stat(StatisticType.Count, "cnt"));
        query.MaxScanRecords = Rows.Length - 1;

        Assert.Throws<MapServerException>(() => Run(query));
    }

    [Fact]
    public void MaxScanRecords_Equal_DoesNotThrow()
    {
        var query = Query(Stat(StatisticType.Count, "cnt"));
        query.MaxScanRecords = Rows.Length;

        Assert.Equal(5L, Run(query).Rows[0]["cnt"]);   // 6 scanned, 5 non-null
    }
}
