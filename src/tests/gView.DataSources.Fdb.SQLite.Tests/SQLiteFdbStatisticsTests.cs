using System.Data.SQLite;
using IoPath = System.IO.Path;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Data.Statistics;
using gView.Framework.Geometry;

namespace gView.DataSources.Fdb.SQLite.Tests;

/// <summary>
/// <see cref="IStatisticsTableClass"/> on a SQLite FDB: the SQL <c>GROUP BY</c> path has to return
/// the same as the in memory <see cref="StatisticsAggregator"/> (which is the reference implementation).
/// </summary>
public class SQLiteFdbStatisticsTests : IDisposable
{
    private readonly string _dbPath = IoPath.Combine(IoPath.GetTempPath(), $"gview_fdb_stat_{Guid.NewGuid():N}.sqlite");

    public void Dispose()
    {
        try
        {
            SQLiteConnection.ClearAllPools();
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
        }
        catch { }
    }

    private async Task<IFeatureClass> CreateFeatureClassAsync(GeometryStorageType storage)
    {
        var fdb = new SQLiteFDB();
        Assert.True(fdb.Create(_dbPath), "FDB Create failed: " + fdb.LastErrorMessage);
        Assert.True(await fdb.Open("Data Source=" + _dbPath));

        var sRef = SpatialReference.FromID("epsg:25832");
        int dsId = await fdb.CreateDataset("ds", sRef, new gViewSpatialIndexDef() { StorageType = storage, SpatialReference = sRef });
        Assert.True(dsId > 0, fdb.LastErrorMessage);

        var fields = new FieldCollection();
        fields.Add(new Field("COUNTRY", FieldType.String) { size = 10 });
        fields.Add(new Field("CITY", FieldType.String) { size = 30 });
        fields.Add(new Field("POP", FieldType.Double));
        fields.Add(new Field("RANK", FieldType.integer));

        Assert.True(await fdb.CreateFeatureClass("ds", "geo", new GeometryDef(GeometryType.Point), fields) > 0, fdb.LastErrorMessage);

        var dataset = await fdb.GetDataset("ds");
        var fc = (IFeatureClass)(await dataset.Element("geo")).Class;

        Feature F(double x, string? country, string city, double? pop, int rank)
        {
            var f = new Feature { Shape = new Point(x, x) };
            f.Fields.Add(new FieldValue("COUNTRY", country));
            f.Fields.Add(new FieldValue("CITY", city));
            f.Fields.Add(new FieldValue("POP", pop));
            f.Fields.Add(new FieldValue("RANK", rank));
            return f;
        }

        await fdb.Insert(fc, new List<IFeature>
        {
            F(100, "AT", "Vienna", 1900, 1),
            F(200, "AT", "Graz", 300, 2),
            F(300, "AT", "Linz", 200, 3),
            F(400, "DE", "Berlin", 3700, 1),
            F(500, "DE", "Bonn", null, 2),
            F(9000, null, "Nowhere", 10, 9),
        });

        return fc;
    }

    private static StatisticsQuery Query(string[] groupBy, params (StatisticType Type, string Field, FieldType FieldType, string Out)[] stats)
    {
        var q = new StatisticsQuery();
        foreach (var g in groupBy)
        {
            q.GroupByFields.Add(g);
            q.GroupByFieldTypes.Add(FieldType.String);
        }
        foreach (var s in stats)
        {
            q.Statistics.Add(new StatisticDefinition() { Type = s.Type, OnField = s.Field, OnFieldType = s.FieldType, OutName = s.Out });
        }
        return q;
    }

    private static readonly (StatisticType, string, FieldType, string)[] AllStatistics =
    {
        (StatisticType.Count, "POP", FieldType.Double, "cnt"),
        (StatisticType.Sum, "POP", FieldType.Double, "sum"),
        (StatisticType.Min, "POP", FieldType.Double, "min"),
        (StatisticType.Max, "CITY", FieldType.String, "maxcity"),
        (StatisticType.Avg, "RANK", FieldType.integer, "avgrank"),
        (StatisticType.StdDev, "POP", FieldType.Double, "std"),
        (StatisticType.Var, "POP", FieldType.Double, "var"),
    };

    private static void AssertSameRows(StatisticsResult expected, StatisticsResult actual)
    {
        Assert.Equal(expected.Columns.Select(c => (c.Name, c.Type)), actual.Columns.Select(c => (c.Name, c.Type)));
        Assert.Equal(expected.ExceededLimit, actual.ExceededLimit);
        Assert.Equal(expected.Rows.Count, actual.Rows.Count);

        for (int i = 0; i < expected.Rows.Count; i++)
        {
            foreach (var column in expected.Columns)
            {
                var e = expected.Rows[i][column.Name];
                var a = actual.Rows[i][column.Name];

                if (e is double de && a is double da)
                {
                    Assert.Equal(de, da, 6);
                }
                else
                {
                    Assert.Equal(e, a);
                }
            }
        }
    }

    [Theory]
    [InlineData(GeometryStorageType.Classic)]
    [InlineData(GeometryStorageType.GeoPackage)]
    public async Task ExecuteStatistics_Grouped_SqlEqualsInMemoryAggregation(GeometryStorageType storage)
    {
        var fc = await CreateFeatureClassAsync(storage);
        var table = Assert.IsAssignableFrom<IStatisticsTableClass>(fc);

        var expected = await StatisticsAggregator.ExecuteAsync(fc, new QueryFilter(), Query(new[] { "COUNTRY" }, AllStatistics));
        var actual = await table.ExecuteStatistics(new QueryFilter(), Query(new[] { "COUNTRY" }, AllStatistics));

        Assert.Equal(3, actual.Rows.Count);
        AssertSameRows(expected, actual);
    }

    [Fact]
    public async Task ExecuteStatistics_NoGroupBy_WithWhere_SqlEqualsInMemoryAggregation()
    {
        var fc = await CreateFeatureClassAsync(GeometryStorageType.Classic);
        var table = (IStatisticsTableClass)fc;

        var expected = await StatisticsAggregator.ExecuteAsync(fc, new QueryFilter() { WhereClause = "COUNTRY='AT'" }, Query(Array.Empty<string>(), AllStatistics));
        var actual = await table.ExecuteStatistics(new QueryFilter() { WhereClause = "COUNTRY='AT'" }, Query(Array.Empty<string>(), AllStatistics));

        AssertSameRows(expected, actual);
        Assert.Equal(3L, actual.Rows[0]["cnt"]);
        Assert.Equal(2400.0, (double)actual.Rows[0]["sum"], 6);
    }

    [Fact]
    public async Task ExecuteStatistics_NoMatchingRows_ReturnsOneRowWithCountZero()
    {
        var fc = await CreateFeatureClassAsync(GeometryStorageType.Classic);

        var result = await ((IStatisticsTableClass)fc).ExecuteStatistics(
            new QueryFilter() { WhereClause = "COUNTRY='XX'" }, Query(Array.Empty<string>(), AllStatistics));

        var row = Assert.Single(result.Rows);
        Assert.Equal(0L, row["cnt"]);
        Assert.Null(row["sum"]);
        Assert.Null(row["std"]);
    }

    [Fact]
    public async Task ExecuteStatistics_OrderByDescending_OffsetAndLimit_SqlEqualsInMemoryAggregation()
    {
        var fc = await CreateFeatureClassAsync(GeometryStorageType.Classic);
        var table = (IStatisticsTableClass)fc;

        StatisticsQuery Q(int offset)
        {
            var q = Query(new[] { "COUNTRY" }, (StatisticType.Sum, "POP", FieldType.Double, "total"));
            q.OrderBy.Add(new StatisticsOrderBy() { Name = "total", Descending = true });
            q.Limit = 2;
            q.Offset = offset;
            return q;
        }

        foreach (var offset in new[] { 0, 1, 2 })
        {
            var expected = await StatisticsAggregator.ExecuteAsync(fc, new QueryFilter(), Q(offset));
            var actual = await table.ExecuteStatistics(new QueryFilter(), Q(offset));

            AssertSameRows(expected, actual);
        }

        Assert.True((await table.ExecuteStatistics(new QueryFilter(), Q(0))).ExceededLimit);
        Assert.False((await table.ExecuteStatistics(new QueryFilter(), Q(1))).ExceededLimit);
    }

    [Fact]
    public async Task ExecuteStatistics_SpatialFilter_FallsBackToInMemoryAggregation()
    {
        var fc = await CreateFeatureClassAsync(GeometryStorageType.Classic);

        var filter = new SpatialFilter
        {
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            Geometry = new Envelope(0, 0, 600, 600),   // everything but "Nowhere"
        };
        Assert.False(StatisticsSql.CanExecuteInSql(filter));

        var result = await ((IStatisticsTableClass)fc).ExecuteStatistics(
            filter, Query(Array.Empty<string>(), (StatisticType.Count, "CITY", FieldType.String, "cnt")));

        Assert.Equal(5L, Assert.Single(result.Rows)["cnt"]);
    }
}
