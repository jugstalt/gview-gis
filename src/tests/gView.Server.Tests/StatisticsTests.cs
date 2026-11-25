using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Exceptions;
using gView.Framework.Data;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.Extensions.Cql2;
using gView.Server.EndPoints.GeoJsonService;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using Microsoft.AspNetCore.Http;

namespace gView.Server.Tests;

/// <summary>The query command statistics of the GeoJson services: request checks, having, the rights of the urls</summary>
public class StatisticsTests
{
    private sealed class FakeTable : ITableClass
    {
        private readonly FieldCollection _fields = new();

        public FakeTable()
        {
            _fields.Add(new Field("OBJECTID", FieldType.ID));
            _fields.Add(new Field("LAND", FieldType.String));
            _fields.Add(new Field("EW", FieldType.integer));
        }

        public string Name => "fake";
        public string Aliasname => "fake";
        public IDataset Dataset => null!;
        public IFieldCollection Fields => _fields;
        public IField FindField(string name) => _fields.FindField(name);
        public string IDFieldName => "OBJECTID";
        public Task<ICursor> Search(Framework.Core.Data.Filters.IQueryFilter filter) => throw new NotImplementedException();
        public Task<ISelectionSet> Select(Framework.Core.Data.Filters.IQueryFilter filter) => throw new NotImplementedException();
        public Task<int> ExecuteCount(Framework.Core.Data.Filters.IQueryFilter filter) => throw new NotImplementedException();
    }

    private static GetFeaturesRequest Request(params StatisticRequest[] statistics)
        => new() { Command = QueryCommand.Statistics, Statistics = statistics, GroupBy = ["land"], OrderByFields = ["n DESC"] };

    [Fact]
    public void ToStatisticsQuery_NamesTypesAndOrder()
    {
        var query = Request(new StatisticRequest { Type = "count", Field = "OBJECTID", Alias = "n" }, new StatisticRequest { Type = "AVG", Field = "ew" })
            .ToStatisticsQuery(new FakeTable(), limit: 10, withHaving: false);

        Assert.Equal(["LAND"], query.GroupByFields);
        Assert.Equal(["n", "avg_EW"], query.Statistics.Select(s => s.OutName));
        Assert.Equal(StatisticType.Avg, query.Statistics[1].Type);
        Assert.Equal(("n", true), (query.OrderBy[0].Name, query.OrderBy[0].Descending));
        Assert.Equal(10, query.Limit);
    }

    [Fact]
    public void ToStatisticsQuery_WithHaving_ReadsAllRows()
    {
        var query = Request(new StatisticRequest { Type = "count", Field = "OBJECTID", Alias = "n" }).ToStatisticsQuery(new FakeTable(), 10, withHaving: true);

        Assert.Equal((0, 0), (query.Offset, query.Limit));
    }

    [Theory]
    [InlineData("sum", "LAND")]       // not numeric
    [InlineData("median", "EW")]      // unknown type
    [InlineData("count", "NOPE")]     // unknown field
    public void ToStatisticsQuery_Invalid_IsBadRequest(string type, string field)
        => Assert.Throws<MapServerException>(() => Request(new StatisticRequest { Type = type, Field = field }).ToStatisticsQuery(new FakeTable(), 10, false));

    [Fact]
    public void ToStatisticsResponse_HavingThenOffsetAndLimit()
    {
        var result = new StatisticsResult();
        result.Columns.Add(new StatisticsColumn { Name = "LAND", Type = FieldType.String });
        result.Columns.Add(new StatisticsColumn { Name = "n", Type = FieldType.integer });
        foreach (var (land, n) in new[] { ("A", 30), ("B", 20), ("C", 5), ("D", 25) })
        {
            result.Rows.Add(new Dictionary<string, object> { ["LAND"] = land, ["n"] = n });
        }

        var response = result.ToStatisticsResponse(CqlTextParser.Parse("n > 10"), offset: 1, limit: 1);

        Assert.Equal(["B"], response.Rows!.Select(r => r["LAND"]));
        Assert.True(response.ExceededLimit);
        Assert.Equal(PropertyType.Integer, response.Fields!.Last().Type);
    }

    [Fact]
    public void QueryParameters_Statistics()
    {
        var request = new QueryCollection(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
            "?command=statistics&statistics=count:OBJECTID:n,avg:EW&groupBy=LAND&having=n>2")).ToGetFeaturesRequest();

        Assert.Equal(QueryCommand.Statistics, request.Command);
        Assert.Equal(["count:OBJECTID:n", "avg:EW:"], request.Statistics!.Select(s => $"{s.Type}:{s.Field}:{s.Alias}"));
        Assert.Equal(["LAND"], request.GroupBy!);
        Assert.Equal("n>2", request.Having);
    }

    [Theory]
    [InlineData("/geojsonservice/v1/services/sqlite/net/query/0", "query")]
    [InlineData("/geojsonservice/v1/services/net/features/12", "features")]
    [InlineData("/geojsonservice/v1/services/sqlite/net/map", "map")]
    [InlineData("/geojsonservice/v1/services/sqlite/net/applyEdits", "applyedits")]
    [InlineData("/geojsonservice/v1/services/sqlite/net/layers/3", "layers")]
    public void RequestMethod_IsTheOperation_NotTheLayerId(string path, string method)
        => Assert.Equal(method, TestableEndpoint.Method(path));

    private sealed class TestableEndpoint : BaseApiEndpoint
    {
        private TestableEndpoint() : base("x", () => 0) { }
        public static string Method(string path) => RequestMethod(path);
    }
}
