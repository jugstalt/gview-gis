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
    public void Build_TypeIsConstant_NumbersAreNumbers()
    {
        var api = GeoJsonServiceOpenApi.Build("https://server/x", "x", AccessTypes.Map);
        var getMap = api["components"]!["schemas"]!["GetMapRequest"]!["properties"]!;

        Assert.Equal("GetMap", getMap["type"]!["const"]!.GetValue<string>());
        Assert.Equal("integer", getMap["width"]!["type"]!.GetValue<string>());
        Assert.DoesNotContain("string", getMap["bbox"]!.ToJsonString());
    }

    [Fact]
    public void Build_ExamplesFromTheService()
    {
        var example = new ServiceExample("EPSG:31256", new gView.GeoJsonService.DTOs.BBox { MinX = 1, MinY = 2, MaxX = 3, MaxY = 4 }, ["0", "2"], ["2"]);
        var api = GeoJsonServiceOpenApi.Build("https://server/x", "x", AccessTypes.Map | AccessTypes.Query, example);

        var body = api["paths"]!["/map"]!["post"]!["requestBody"]!["content"]!["application/json"]!["example"]!;
        Assert.Equal("GetMap", body["type"]!.GetValue<string>());
        Assert.Equal(["0", "2"], body["layers"]!.AsArray().Select(l => l!.GetValue<string>()));
        Assert.Equal(3, body["bbox"]!["maxX"]!.GetValue<double>());
        Assert.Equal("EPSG:31256", body["crs"]!["properties"]!["name"]!.GetValue<string>());
        Assert.Equal(800, body["width"]!.GetValue<int>());

        string? Get(string path, string name) => api["paths"]![path]!["get"]!["parameters"]!.AsArray()
            .First(p => p!["name"]!.GetValue<string>() == name)!["example"]?.GetValue<string>();
        Assert.Equal("1,2,3,4", Get("/map", "bbox"));
        Assert.Equal("EPSG:31256", Get("/map", "crs"));
        Assert.Equal("0,2", Get("/map", "layers"));
        Assert.Equal("2", Get("/query/{layerId}", "layerId"));
        Assert.Equal("2,3", Get("/identify", "geometry"));

        // without a service: no examples
        Assert.Null(GeoJsonServiceOpenApi.Build("https://server/x", "x", AccessTypes.Map)["paths"]!["/map"]!["post"]!["requestBody"]!["content"]!["application/json"]!["example"]);
    }

    [Fact]
    public void Build_ApplyEditsExample_InsertsIntoTheEditLayer()
    {
        var example = new ServiceExample("EPSG:31256", new gView.GeoJsonService.DTOs.BBox { MinX = 0, MinY = 0, MaxX = 100, MaxY = 100 }, ["0"], ["0", "3"],
            new ServiceExample.EditLayer("3", gView.Framework.Core.Geometry.GeometryType.Polygon));
        var api = GeoJsonServiceOpenApi.Build("https://server/x", "x", AccessTypes.Edit, example);

        var body = api["paths"]!["/applyEdits"]!["post"]!["requestBody"]!["content"]!["application/json"]!["example"]!;
        Assert.Equal("ApplyEdits", body["type"]!.GetValue<string>());
        var edits = body["edits"]![0]!;
        Assert.Equal("3", edits["layerId"]!.GetValue<string>());
        var geometry = edits["adds"]![0]!["geometry"]!;
        Assert.Equal("Polygon", geometry["type"]!.GetValue<string>());
        Assert.Equal(5, geometry["coordinates"]![0]!.AsArray().Count);  // closed square around the center
        Assert.Equal(49.0, geometry["coordinates"]![0]![0]![0]!.GetValue<double>());
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
