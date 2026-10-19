using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Exceptions;
using gView.Framework.IO;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;

namespace gView.Interoperability.GeoServices.Tests.Extensions;

public class QueryCapabilitiesExtensionsTests
{
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
}
