using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Data;
using gView.Framework.IO;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;

namespace gView.Interoperability.GeoServices.Tests.Extensions;

public class JsonQueryLayerDTOExtensionsTests
{
    #region Query capabilities

    private const int LayerId = 3;

    [Fact]
    public void Default_AllRequestsAllowed()
    {
        var properties = new MapServiceProperties();
        var query = new JsonQueryLayerDTO()
        {
            OutStatistics = "[]",
            OrderByFields = "A",
            ResultOffset = 10,
            ReturnDistinctValues = true,
            ReturnExtentOnly = true
        };

        query.CheckQueryCapabilities(properties, LayerId);
    }

    [Theory]
    [InlineData(RestCapability.Statistics)]
    [InlineData(RestCapability.Pagination)]
    [InlineData(RestCapability.OrderBy)]
    [InlineData(RestCapability.Distinct)]
    [InlineData(RestCapability.ReturningQueryExtent)]
    public void ServiceOff_RejectsRequestedParameter(RestCapability capability)
    {
        var properties = new MapServiceProperties();
        properties.SetCapability(capability, CapabilitySwitch.Off);

        Assert.Throws<MapServerException>(() => Query(capability).CheckQueryCapabilities(properties, LayerId));
    }

    [Fact]
    public void ServiceOff_NotRequested_IsAllowed()
    {
        var properties = new MapServiceProperties();
        properties.SetCapability(RestCapability.Statistics, CapabilitySwitch.Off);

        new JsonQueryLayerDTO() { Where = "1=1" }.CheckQueryCapabilities(properties, LayerId);
    }

    [Fact]
    public void LayerOn_OverridesServiceOff()
    {
        var properties = new MapServiceProperties();
        properties.SetCapability(RestCapability.OrderBy, CapabilitySwitch.Off);
        properties.SetLayerCapability(LayerId, RestCapability.OrderBy, CapabilitySwitch.On);

        Query(RestCapability.OrderBy).CheckQueryCapabilities(properties, LayerId);
        Assert.Throws<MapServerException>(() => Query(RestCapability.OrderBy).CheckQueryCapabilities(properties, LayerId + 1));
    }

    [Fact]
    public void LayerOff_OverridesServiceOn()
    {
        var properties = new MapServiceProperties();
        properties.SetCapability(RestCapability.Distinct, CapabilitySwitch.On);
        properties.SetLayerCapability(LayerId, RestCapability.Distinct, CapabilitySwitch.Off);

        Assert.Throws<MapServerException>(() => Query(RestCapability.Distinct).CheckQueryCapabilities(properties, LayerId));
        Query(RestCapability.Distinct).CheckQueryCapabilities(properties, LayerId + 1);
    }

    [Theory]
    [InlineData(CapabilitySwitch.Auto, CapabilitySwitch.Auto, true, true)]
    [InlineData(CapabilitySwitch.Auto, CapabilitySwitch.Auto, false, false)]
    [InlineData(CapabilitySwitch.On, CapabilitySwitch.Auto, false, true)]
    [InlineData(CapabilitySwitch.Off, CapabilitySwitch.Auto, true, false)]
    [InlineData(CapabilitySwitch.Off, CapabilitySwitch.On, false, true)]
    [InlineData(CapabilitySwitch.On, CapabilitySwitch.Off, true, false)]
    public void IsCapabilityEnabled_ResolvesLayerBeforeServiceBeforeAuto(
        CapabilitySwitch service, CapabilitySwitch layer, bool auto, bool expected)
    {
        var properties = new MapServiceProperties();
        properties.SetCapability(RestCapability.Statistics, service);
        properties.SetLayerCapability(LayerId, RestCapability.Statistics, layer);

        Assert.Equal(expected, properties.IsCapabilityEnabled(RestCapability.Statistics, LayerId, auto));
    }

    [Fact]
    public void PersistAndClone_RoundTrip()
    {
        var properties = new MapServiceProperties() { MaxRecordCount = 500 };
        properties.SetCapability(RestCapability.DynamicLayers, CapabilitySwitch.Off);
        properties.SetLayerCapability(LayerId, RestCapability.Statistics, CapabilitySwitch.Off);
        properties.SetLayerCapability(LayerId, RestCapability.Pagination, CapabilitySwitch.On);
        properties.SetLayerCapability(7, RestCapability.Distinct, CapabilitySwitch.Off);

        var xml = new XmlStream("root");
        properties.Save(xml);
        using var ms = new MemoryStream();
        xml.WriteStream(ms);
        ms.Position = 0;

        var readXml = new XmlStream("root");
        Assert.True(readXml.ReadStream(ms));
        var loaded = new MapServiceProperties();
        loaded.Load(readXml);

        foreach (var candidate in new[] { loaded, (MapServiceProperties)properties.Clone() })
        {
            Assert.Equal(500, candidate.MaxRecordCount);
            Assert.Equal(CapabilitySwitch.Off, candidate.GetCapability(RestCapability.DynamicLayers));
            Assert.Equal(CapabilitySwitch.Off, candidate.GetLayerCapability(LayerId, RestCapability.Statistics));
            Assert.Equal(CapabilitySwitch.On, candidate.GetLayerCapability(LayerId, RestCapability.Pagination));
            Assert.Equal(CapabilitySwitch.Off, candidate.GetLayerCapability(7, RestCapability.Distinct));
            Assert.Equal(CapabilitySwitch.Auto, candidate.GetLayerCapability(7, RestCapability.Statistics));
            Assert.Equal(CapabilitySwitch.Auto, candidate.GetCapability(RestCapability.Statistics));
        }
    }

    [Fact]
    public void Load_WithoutCapabilityKeys_IsAuto()
    {
        var xml = new XmlStream("root");
        xml.Save("MapService_MaxRecordCount", 100);

        var loaded = new MapServiceProperties();
        loaded.Load(xml);

        Assert.Equal(100, loaded.MaxRecordCount);
        Assert.Equal(CapabilitySwitch.Auto, loaded.GetCapability(RestCapability.Statistics));
        Assert.Equal(CapabilitySwitch.Auto, loaded.GetLayerCapability(LayerId, RestCapability.Statistics));
    }

    private static JsonQueryLayerDTO Query(RestCapability capability) => capability switch
    {
        RestCapability.Statistics => new JsonQueryLayerDTO() { OutStatistics = "[]" },
        RestCapability.Pagination => new JsonQueryLayerDTO() { ResultRecordCount = 5 },
        RestCapability.OrderBy => new JsonQueryLayerDTO() { OrderByFields = "A" },
        RestCapability.Distinct => new JsonQueryLayerDTO() { ReturnDistinctValues = true },
        RestCapability.ReturningQueryExtent => new JsonQueryLayerDTO() { ReturnExtentOnly = true },
        _ => throw new NotSupportedException()
    };

    #endregion

    #region Statistics

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

    #endregion
}
