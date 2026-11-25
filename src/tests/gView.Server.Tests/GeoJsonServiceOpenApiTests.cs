using gView.Framework.Core.MapServer;
using gView.Server.EndPoints.GeoJsonService;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using Microsoft.AspNetCore.Http;

namespace gView.Server.Tests;

public class GeoJsonServiceOpenApiTests
{
    [Fact]
    public void Build_OnlyTheOperationsOfTheRights()
    {
        var mapOnly = GeoJsonServiceOpenApi.Build("https://server/geojsonservice/v1/services/net", "net", AccessTypes.Map);
        var all = GeoJsonServiceOpenApi.Build("https://server/geojsonservice/v1/services/net", "net", AccessTypes.Map | AccessTypes.Query | AccessTypes.Edit);

        Assert.Equal("3.1.0", mapOnly["openapi"]!.GetValue<string>());
        Assert.Contains("/map", mapOnly["paths"]!.AsObject().Select(p => p.Key));
        Assert.DoesNotContain("/query/{layerId}", mapOnly["paths"]!.AsObject().Select(p => p.Key));
        Assert.DoesNotContain("/applyEdits", mapOnly["paths"]!.AsObject().Select(p => p.Key));
        Assert.Contains("/applyEdits", all["paths"]!.AsObject().Select(p => p.Key));
        Assert.Contains("/find", all["paths"]!.AsObject().Select(p => p.Key));
    }

    [Fact]
    public void Build_SchemasFromTheDtos()
    {
        var api = GeoJsonServiceOpenApi.Build("https://server/x", "x", AccessTypes.Map | AccessTypes.Query);
        var schemas = api["components"]!["schemas"]!.AsObject();

        Assert.True(schemas.ContainsKey("GetMapRequest"));
        Assert.True(schemas["GetMapRequest"]!["properties"]!.AsObject().ContainsKey("layerDefs"));
        Assert.True(schemas.ContainsKey("IdentifyResponse"));
        Assert.Equal("#/components/schemas/GetMapRequest",
            api["paths"]!["/map"]!["post"]!["requestBody"]!["content"]!["application/json"]!["schema"]!["$ref"]!.GetValue<string>());
    }

    [Fact]
    public void Build_AllRefsResolve()
    {
        var api = GeoJsonServiceOpenApi.Build("https://server/x", "x", AccessTypes.Map | AccessTypes.Query | AccessTypes.Edit);

        IEnumerable<string> Refs(System.Text.Json.Nodes.JsonNode? node) => node switch
        {
            System.Text.Json.Nodes.JsonObject obj => obj.SelectMany(p => p.Key == "$ref" && p.Value is System.Text.Json.Nodes.JsonValue v
                ? [v.GetValue<string>()]
                : Refs(p.Value)),
            System.Text.Json.Nodes.JsonArray array => array.SelectMany(Refs),
            _ => []
        };

        var refs = Refs(api).Distinct().ToArray();
        Assert.Contains(refs, r => r.StartsWith("#/components/schemas/") && r.Contains("/properties/"));
        Assert.All(refs, reference =>
        {
            System.Text.Json.Nodes.JsonNode? target = api;
            foreach (var segment in reference.TrimStart('#', '/').Split('/'))
            {
                target = target?[segment.Replace("~1", "/").Replace("~0", "~")];
            }
            Assert.True(target is not null, $"unresolved {reference}");
        });
    }

    [Fact]
    public void FindParameters()
    {
        var request = new QueryCollection(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
            "?searchText=Wien&searchFields=NAME,LAND&layers=0,1&contains=false&returnGeometry=centroid&limit=5&geometryPrecision=2&maxAllowableOffset=10")).ToFindRequest();

        Assert.Equal("Wien", request.SearchText);
        Assert.Equal(["NAME", "LAND"], request.SearchFields!);
        Assert.Equal(["0", "1"], request.Layers!);
        Assert.False(request.Contains);
        Assert.Equal(gView.GeoJsonService.DTOs.GeometryResult.Centroid, request.ReturnGeometry);
        Assert.Equal((5, 2, 10.0), (request.Limit, request.GeometryPrecision, request.MaxAllowableOffset));
    }
}
