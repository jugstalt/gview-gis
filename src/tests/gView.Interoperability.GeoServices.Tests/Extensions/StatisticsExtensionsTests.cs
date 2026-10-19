using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Exceptions;
using gView.Framework.Data;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;

namespace gView.Interoperability.GeoServices.Tests.Extensions;

public class StatisticsExtensionsTests
{
    private class StubTableClass : ITableClass
    {
        private readonly FieldCollection _fields = new(new IField[]
        {
            new Field("OBJECTID", FieldType.ID),
            new Field("COUNTRY", FieldType.String),
            new Field("POP", FieldType.Double),
            new Field("FOUNDED", FieldType.Date),
        });

        public string Name => "stub";
        public string Aliasname => "stub";
        public IDataset Dataset => null!;
        public IFieldCollection Fields => _fields;
        public string IDFieldName => "OBJECTID";

        public IField FindField(string name)
            => _fields.ToEnumerable().FirstOrDefault(f => f.name.Equals(name, StringComparison.OrdinalIgnoreCase))!;

        public Task<ICursor> Search(IQueryFilter filter) => throw new NotSupportedException();
        public Task<ISelectionSet> Select(IQueryFilter filter) => throw new NotSupportedException();
    }

    private static readonly ITableClass Table = new StubTableClass();

    private static StatisticsQuery Parse(JsonQueryLayerDTO dto, int limit = 1000, int maxScan = 5000)
        => dto.ToStatisticsQuery(Table, limit, maxScan);

    [Fact]
    public void ToStatisticsQuery_ParsesStatisticsGroupByAndOrder()
    {
        var dto = new JsonQueryLayerDTO()
        {
            OutStatistics = """[{"statisticType":"SUM","onStatisticField":"pop","outStatisticFieldName":"total"},{"statisticType":"count","onStatisticField":"OBJECTID"}]""",
            GroupByFieldsForStatistics = "country",
            OrderByFields = "total DESC, COUNTRY",
            ResultOffset = 5
        };

        var query = Parse(dto, limit: 10, maxScan: 77);

        Assert.Equal(new[] { "COUNTRY" }, query.GroupByFields);
        Assert.Equal(new[] { FieldType.String }, query.GroupByFieldTypes);
        Assert.Equal(StatisticType.Sum, query.Statistics[0].Type);
        Assert.Equal("POP", query.Statistics[0].OnField);       // canonical field name
        Assert.Equal("total", query.Statistics[0].OutName);
        Assert.Equal("EXPR_2", query.Statistics[1].OutName);    // default name
        Assert.Equal(("total", true), (query.OrderBy[0].Name, query.OrderBy[0].Descending));
        Assert.Equal(("COUNTRY", false), (query.OrderBy[1].Name, query.OrderBy[1].Descending));
        Assert.Equal((5, 10, 77), (query.Offset, query.Limit, query.MaxScanRecords));
    }

    [Theory]
    [InlineData("stddev", StatisticType.StdDev)]
    [InlineData("var", StatisticType.Var)]
    [InlineData("avg", StatisticType.Avg)]
    [InlineData("MIN", StatisticType.Min)]
    [InlineData("Max", StatisticType.Max)]
    public void ToStatisticsQuery_StatisticTypes_AreCaseInsensitive(string type, StatisticType expected)
    {
        var dto = new JsonQueryLayerDTO() { OutStatistics = $$"""[{"statisticType":"{{type}}","onStatisticField":"POP","outStatisticFieldName":"x"}]""" };

        Assert.Equal(expected, Parse(dto).Statistics[0].Type);
    }

    [Theory]
    [InlineData("""[{"statisticType":"median","onStatisticField":"POP"}]""")]                                  // unsupported type
    [InlineData("""[{"statisticType":"sum","onStatisticField":"NOPE"}]""")]                                    // unknown field
    [InlineData("""[{"statisticType":"sum","onStatisticField":"POP; DROP TABLE x"}]""")]                       // injection
    [InlineData("""[{"statisticType":"sum","onStatisticField":"UPPER(COUNTRY)"}]""")]                          // expression
    [InlineData("""[{"statisticType":"sum","onStatisticField":"COUNTRY"}]""")]                                 // non numeric
    [InlineData("""[{"statisticType":"avg","onStatisticField":"FOUNDED"}]""")]                                 // non numeric
    [InlineData("""[{"statisticType":"sum","onStatisticField":"POP","outStatisticFieldName":"a b"}]""")]       // invalid out name
    [InlineData("""[{"statisticType":"sum","onStatisticField":"POP","outStatisticFieldName":"a"},{"statisticType":"count","onStatisticField":"POP","outStatisticFieldName":"A"}]""")] // duplicate
    [InlineData("""[{"statisticType":"sum","onStatisticField":"POP","outStatisticFieldName":"COUNTRY"}]""")]   // clashes with group by
    [InlineData("[]")]
    [InlineData("not json")]
    public void ToStatisticsQuery_InvalidStatistics_Throws(string outStatistics)
    {
        var dto = new JsonQueryLayerDTO() { OutStatistics = outStatistics, GroupByFieldsForStatistics = "COUNTRY" };

        Assert.Throws<MapServerException>(() => Parse(dto));
    }

    [Theory]
    [InlineData("NOPE")]
    [InlineData("COUNTRY,COUNTRY")]
    [InlineData("COUNTRY; DROP TABLE x")]
    public void ToStatisticsQuery_InvalidGroupBy_Throws(string groupBy)
    {
        var dto = new JsonQueryLayerDTO()
        {
            OutStatistics = """[{"statisticType":"count","onStatisticField":"POP","outStatisticFieldName":"c"}]""",
            GroupByFieldsForStatistics = groupBy
        };

        Assert.Throws<MapServerException>(() => Parse(dto));
    }

    [Theory]
    [InlineData("POP")]                 // not a group by field / out name
    [InlineData("total SIDEWAYS")]
    [InlineData("total DESC, 1=1")]
    public void ToStatisticsQuery_InvalidOrderBy_Throws(string orderBy)
    {
        var dto = new JsonQueryLayerDTO()
        {
            OutStatistics = """[{"statisticType":"sum","onStatisticField":"POP","outStatisticFieldName":"total"}]""",
            GroupByFieldsForStatistics = "COUNTRY",
            OrderByFields = orderBy
        };

        Assert.Throws<MapServerException>(() => Parse(dto));
    }

    [Fact]
    public void IsStatisticsQuery_DetectsOutStatisticsAndGroupBy()
    {
        Assert.False(new JsonQueryLayerDTO().IsStatisticsQuery());
        Assert.True(new JsonQueryLayerDTO() { OutStatistics = "[]" }.IsStatisticsQuery());
        Assert.True(new JsonQueryLayerDTO() { GroupByFieldsForStatistics = "A" }.IsStatisticsQuery());
    }

    [Fact]
    public void CheckStatisticsParameters_GroupByWithoutOutStatistics_Throws()
    {
        Assert.Throws<MapServerException>(() =>
            new JsonQueryLayerDTO() { GroupByFieldsForStatistics = "COUNTRY" }.CheckStatisticsParameters());
    }

    [Fact]
    public void CheckStatisticsParameters_ConflictingReturnFlags_Throw()
    {
        const string stats = """[{"statisticType":"count","onStatisticField":"POP"}]""";

        Assert.Throws<MapServerException>(() => new JsonQueryLayerDTO() { OutStatistics = stats, ReturnIdsOnly = true }.CheckStatisticsParameters());
        Assert.Throws<MapServerException>(() => new JsonQueryLayerDTO() { OutStatistics = stats, ReturnCountOnly = true }.CheckStatisticsParameters());
        Assert.Throws<MapServerException>(() => new JsonQueryLayerDTO() { OutStatistics = stats, ReturnDistinctValues = true }.CheckStatisticsParameters());
        Assert.Throws<MapServerException>(() => new JsonQueryLayerDTO() { OutStatistics = stats, ReturnExtentOnly = true }.CheckStatisticsParameters());

        // returnGeometry / outFields are silently ignored
        new JsonQueryLayerDTO() { OutStatistics = stats, ReturnGeometry = true, OutFields = "*" }.CheckStatisticsParameters();
    }
}
