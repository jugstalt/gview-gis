using System.Text.Json;
using gView.Interoperability.GeoServices.Rest.DTOs;

namespace gView.Interoperability.GeoServices.Tests.Rest;

public class JsonLayerDTOTests
{
    [Fact]
    public void Serialize_Default_StatisticsNotSupported_NoAdvancedQueryCapabilities()
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new JsonLayerDTO()));

        Assert.False(json.RootElement.GetProperty("supportsStatistics").GetBoolean());
        Assert.False(json.RootElement.TryGetProperty("advancedQueryCapabilities", out _));
    }

    [Fact]
    public void Serialize_StatisticsEnabled_WritesTopLevelAndNestedFlag()
    {
        var layer = new JsonLayerDTO()
        {
            SupportsStatistics = true,
            AdvancedQueryCapabilities = new JsonLayerDTO.AdvancedQueryCapabilitiesClass() { SupportsStatistics = true }
        };

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(layer));

        Assert.True(json.RootElement.GetProperty("supportsStatistics").GetBoolean());
        Assert.True(json.RootElement.GetProperty("advancedQueryCapabilities").GetProperty("supportsStatistics").GetBoolean());
    }

    [Fact]
    public void Serialize_FeatureServerLayer_HasAdvancedQueryCapabilities_StatisticsOffByDefault()
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new JsonFeatureServerLayerDTO()));

        Assert.True(json.RootElement.GetProperty("supportsAdvancedQueries").GetBoolean());
        Assert.False(json.RootElement.GetProperty("advancedQueryCapabilities").GetProperty("supportsStatistics").GetBoolean());
    }
}
