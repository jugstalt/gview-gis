using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Data.Filters;
using gView.Framework.Data.Statistics;

namespace gView.Framework.Data.Tests.Statistics;

public class StatisticsSqlTests
{
    private static StatisticsQuery Query()
    {
        var q = new StatisticsQuery();
        q.GroupByFields.Add("COUNTRY");
        q.Statistics.Add(new StatisticDefinition() { Type = StatisticType.Sum, OnField = "POP", OutName = "total", OnFieldType = FieldType.Double });
        q.Statistics.Add(new StatisticDefinition() { Type = StatisticType.Count, OnField = "POP", OutName = "cnt", OnFieldType = FieldType.Double });
        return q;
    }

    [Fact]
    public void BuildCommandText_SQLite_GroupByOrderAndPaging()
    {
        var q = Query();
        q.OrderBy.Add(new StatisticsOrderBy() { Name = "total", Descending = true });
        q.Limit = 10;
        q.Offset = 20;

        var sql = StatisticsSql.BuildCommandText(SqlDialect.SQLite, "FC_x", "KG='1'", q);

        Assert.Equal(
            "SELECT \"COUNTRY\" AS \"COUNTRY\", SUM((1.0 * \"POP\")) AS \"total\", COUNT(\"POP\") AS \"cnt\" FROM FC_x " +
            "WHERE KG='1' GROUP BY \"COUNTRY\" ORDER BY \"total\" DESC LIMIT 11 OFFSET 20", sql);
    }

    [Fact]
    public void BuildCommandText_NoOrder_SortsByGroupFields()
    {
        var sql = StatisticsSql.BuildCommandText(SqlDialect.SQLite, "FC_x", null, Query());

        Assert.EndsWith("GROUP BY \"COUNTRY\" ORDER BY \"COUNTRY\" ASC", sql);
        Assert.DoesNotContain("WHERE", sql);
        Assert.DoesNotContain("LIMIT", sql);
    }

    [Fact]
    public void BuildCommandText_PostgreSql_NullsOrderingAndCasts()
    {
        var q = Query();
        q.OrderBy.Add(new StatisticsOrderBy() { Name = "cnt", Descending = true });
        q.OrderBy.Add(new StatisticsOrderBy() { Name = "COUNTRY" });
        q.Statistics.Add(new StatisticDefinition() { Type = StatisticType.StdDev, OnField = "POP", OutName = "sd" });

        var sql = StatisticsSql.BuildCommandText(SqlDialect.PostgreSql, "\"FC_x\"", "", q);

        Assert.Contains("SUM(CAST(\"POP\" AS double precision))", sql);
        Assert.Contains("STDDEV_SAMP(CAST(\"POP\" AS double precision))", sql);
        Assert.EndsWith("ORDER BY \"cnt\" DESC NULLS LAST, \"COUNTRY\" ASC NULLS FIRST", sql);
    }

    [Fact]
    public void BuildCommandText_SqlServer_BracketsCastsAndOffsetFetch()
    {
        var q = Query();
        q.Limit = 5;
        q.Offset = 5;
        q.Statistics.Add(new StatisticDefinition() { Type = StatisticType.Var, OnField = "POP", OutName = "v" });

        var sql = StatisticsSql.BuildCommandText(SqlDialect.SqlServer, "[FC_x]", null, q);

        Assert.StartsWith("SELECT [COUNTRY] AS [COUNTRY], SUM(CAST([POP] AS float)) AS [total], COUNT_BIG([POP]) AS [cnt], VAR(CAST([POP] AS float)) AS [v] FROM [FC_x]", sql);
        Assert.EndsWith("GROUP BY [COUNTRY] ORDER BY [COUNTRY] ASC OFFSET 5 ROWS FETCH NEXT 6 ROWS ONLY", sql);
    }

    [Fact]
    public void BuildCommandText_NoGroupBy_NoGroupByOrPagingClause()
    {
        var q = Query();
        q.GroupByFields.Clear();
        q.Offset = 3;   // single row => paging is handled when reading the result

        var sql = StatisticsSql.BuildCommandText(SqlDialect.SqlServer, "[FC_x]", null, q);

        Assert.DoesNotContain("GROUP BY", sql);
        Assert.DoesNotContain("OFFSET", sql);
        Assert.DoesNotContain("ORDER BY", sql);
    }

    [Fact]
    public void BuildCommandText_QuotesIdentifiers()
    {
        var q = new StatisticsQuery();
        q.Statistics.Add(new StatisticDefinition() { Type = StatisticType.Min, OnField = "a\"b", OutName = "x" });

        var sql = StatisticsSql.BuildCommandText(SqlDialect.PostgreSql, "t", null, q);

        Assert.Contains("MIN(\"a\"\"b\")", sql);
    }

    [Fact]
    public void CanExecuteInSql_OnlyForPlainWhereFilters()
    {
        Assert.True(StatisticsSql.CanExecuteInSql(new QueryFilter() { WhereClause = "A=1" }));
        Assert.True(StatisticsSql.CanExecuteInSql(new SpatialFilter()));   // no geometry
        Assert.False(StatisticsSql.CanExecuteInSql(new SpatialFilter() { Geometry = new gView.Framework.Geometry.Envelope(0, 0, 1, 1) }));
        Assert.False(StatisticsSql.CanExecuteInSql(new RowIDFilter("FDB_OID", new List<int> { 1 })));
    }
}
