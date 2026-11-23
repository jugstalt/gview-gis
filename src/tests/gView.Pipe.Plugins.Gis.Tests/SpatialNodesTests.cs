using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Nodes;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>Geometry, Spatial Aggregate and Spatial Join on GeoJSON items; no data source needed.</summary>
public class SpatialNodesTests
{
    static private JsonObject Feature(string geometry, string properties = "{}", int? srs = 31256)
        => new()
        {
            ["type"] = "Feature",
            ["geometry"] = JsonNode.Parse(geometry),
            ["properties"] = JsonNode.Parse(properties),
            ["srs"] = srs
        };

    static private JsonObject Square(double x, double y, double size, string properties = "{}")
        => Feature($$"""{"type":"Polygon","coordinates":[[[{{x}},{{y}}],[{{x + size}},{{y}}],[{{x + size}},{{y + size}}],[{{x}},{{y + size}}],[{{x}},{{y}}]]]}""", properties);

    static private Task<List<JsonObject>> GeometryAsync(object parameters, params JsonObject[] input)
        => FakeNodeContext.RunAsync(new GeometryNode(), parameters, new Dictionary<string, object>(), input);

    static private Task<List<JsonObject>> AggregateAsync(object parameters, params JsonObject[] input)
        => FakeNodeContext.RunAsync(new SpatialAggregateNode(), parameters, new Dictionary<string, object>(), input);

    static private Task<List<JsonObject>> JoinAsync(object parameters, JsonObject[] main, JsonObject[] overlay)
        => FakeNodeContext.RunAsync(new SpatialJoinNode(), parameters, new Dictionary<string, IReadOnlyList<JsonObject>>
        {
            [PipePorts.Main] = main,
            [SpatialJoinNode.OverlayPort] = overlay
        });

    static private double Area(JsonObject feature)
    {
        var area = 0.0;
        foreach (var ring in feature["geometry"]!["type"]!.GetValue<string>() == "Polygon"
                     ? feature["geometry"]!["coordinates"]!.AsArray().Take(1)
                     : feature["geometry"]!["coordinates"]!.AsArray().Select(p => p!.AsArray()[0]))
        {
            var points = ring!.AsArray().Select(p => (X: p![0]!.GetValue<double>(), Y: p[1]!.GetValue<double>())).ToList();
            area += Math.Abs(points.Zip(points.Skip(1), (a, b) => a.X * b.Y - b.X * a.Y).Sum()) / 2;
        }
        return area;
    }

    #region Geometry

    [Fact]
    public async Task Buffer_of_a_point_is_a_polygon_and_keeps_the_properties()
    {
        var result = Assert.Single(await GeometryAsync(new { operation = "buffer", distance = 10 }, Feature("""{"type":"Point","coordinates":[0,0]}""", """{"id":"S-1"}""")));

        Assert.Equal("Polygon", result["geometry"]!["type"]!.GetValue<string>());
        Assert.InRange(Area(result), 300, 315);   // ~ π · 10²
        Assert.Equal("S-1", result["properties"]!["id"]!.GetValue<string>());
        Assert.Equal(31256, result["srs"]!.GetValue<int>());
    }

    [Fact]
    public async Task Buffer_on_wgs84_is_refused()
    {
        var error = await Assert.ThrowsAsync<PipeException>(() => GeometryAsync(new { operation = "buffer", distance = 10 }, Feature("""{"type":"Point","coordinates":[15,47]}""", srs: 4326)));

        Assert.Contains("EPSG:4326", error.Message);
    }

    [Fact]
    public async Task Area_and_length_are_written_into_properties()
    {
        var area = Assert.Single(await GeometryAsync(new { operation = "area", property = "area_m2" }, Square(0, 0, 10)));
        var length = Assert.Single(await GeometryAsync(new { operation = "length" }, Feature("""{"type":"LineString","coordinates":[[0,0],[3,4]]}""")));

        Assert.Equal(100, area["properties"]!["area_m2"]!.GetValue<double>(), 6);
        Assert.Equal(5, length["properties"]!["length"]!.GetValue<double>(), 6);
        Assert.Equal("Polygon", area["geometry"]!["type"]!.GetValue<string>());
    }

    [Fact]
    public async Task Centroid_of_a_square_is_its_center()
    {
        var result = Assert.Single(await GeometryAsync(new { operation = "centroid" }, Square(0, 0, 10)));

        Assert.Equal("""{"type":"Point","coordinates":[5,5]}""", result["geometry"]!.ToJsonString());
    }

    [Fact]
    public async Task Empty_results_are_dropped_unless_kept()
    {
        var thin = Square(0, 0, 1);

        Assert.Empty(await GeometryAsync(new { operation = "buffer", distance = -5 }, thin));
        Assert.Null(Assert.Single(await GeometryAsync(new { operation = "buffer", distance = -5, keepEmpty = true }, thin))["geometry"]);
    }

    #endregion

    #region Aggregate

    [Fact]
    public async Task Dissolve_groups_by_field_with_aggregates()
    {
        var result = await AggregateAsync(new
        {
            operation = "dissolve",
            groupBy = new[] { "feeder" },
            aggregates = new Dictionary<string, string> { ["lines"] = "count", ["total"] = "sum(length)", ["ids"] = "list(id)" }
        },
            Square(0, 0, 10, """{"feeder":"A","length":10,"id":1}"""),
            Square(10, 0, 10, """{"feeder":"A","length":5,"id":2}"""),
            Square(50, 0, 10, """{"feeder":"B","length":7,"id":3}"""));

        var a = result.Single(f => f["properties"]!["feeder"]!.GetValue<string>() == "A");
        Assert.Equal(2, result.Count);
        Assert.Equal(2, a["properties"]!["lines"]!.GetValue<int>());
        Assert.Equal(15, a["properties"]!["total"]!.GetValue<double>());
        Assert.Equal("[1,2]", a["properties"]!["ids"]!.ToJsonString());
        Assert.Equal("Polygon", a["geometry"]!["type"]!.GetValue<string>());   // the two squares touch: one polygon
        Assert.Equal(200, Area(a), 6);
    }

    [Fact]
    public async Task Dissolve_without_group_merges_everything()
    {
        var result = Assert.Single(await AggregateAsync(new { operation = "dissolve" }, Square(0, 0, 10), Square(5, 0, 10)));

        Assert.Equal(150, Area(result), 6);
    }

    [Fact]
    public async Task Unknown_aggregate_functions_are_refused()
        => await Assert.ThrowsAsync<PipeException>(() => AggregateAsync(new { operation = "dissolve", aggregates = new Dictionary<string, string> { ["x"] = "median(a)" } }, Square(0, 0, 1)));

    [Fact]
    public async Task Voronoi_cells_keep_the_properties_of_their_point()
    {
        var result = await AggregateAsync(new { operation = "voronoi" },
            Feature("""{"type":"Point","coordinates":[0,0]}""", """{"station":"T-1"}"""),
            Feature("""{"type":"Point","coordinates":[100,0]}""", """{"station":"T-2"}"""),
            Feature("""{"type":"Point","coordinates":[50,80]}""", """{"station":"T-3"}"""));

        Assert.Equal(["T-1", "T-2", "T-3"], result.Select(f => f["properties"]!["station"]!.GetValue<string>()).Order());
        Assert.All(result, f => Assert.Equal("Polygon", f["geometry"]!["type"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Convex_hull_and_delaunay_of_points()
    {
        var points = new[] { Feature("""{"type":"Point","coordinates":[0,0]}"""), Feature("""{"type":"Point","coordinates":[10,0]}"""), Feature("""{"type":"Point","coordinates":[10,10]}"""), Feature("""{"type":"Point","coordinates":[0,10]}"""), Feature("""{"type":"Point","coordinates":[5,5]}""") };

        var hull = Assert.Single(await AggregateAsync(new { operation = "convexHull" }, points));
        var triangles = await AggregateAsync(new { operation = "delaunay" }, points);

        Assert.Equal(100, Area(hull), 6);
        Assert.Equal(4, triangles.Count);
    }

    [Fact]
    public async Task Line_merge_joins_lines_meeting_at_their_ends()
    {
        var result = Assert.Single(await AggregateAsync(new { operation = "lineMerge" },
            Feature("""{"type":"LineString","coordinates":[[0,0],[10,0]]}"""),
            Feature("""{"type":"LineString","coordinates":[[10,0],[10,5]]}""")));

        Assert.Equal("LineString", result["geometry"]!["type"]!.GetValue<string>());
        Assert.Equal(3, result["geometry"]!["coordinates"]!.AsArray().Count);
    }

    [Fact]
    public async Task Different_srs_are_refused()
        => await Assert.ThrowsAsync<PipeException>(() => AggregateAsync(new { operation = "unionAll" },
            Feature("""{"type":"Point","coordinates":[0,0]}""", srs: 31256), Feature("""{"type":"Point","coordinates":[0,0]}""", srs: 3857)));

    #endregion

    #region Join

    [Fact]
    public async Task Join_adds_the_properties_of_the_containing_feature()
    {
        var result = await JoinAsync(new { operation = "join", relation = "within" },
            [Feature("""{"type":"Point","coordinates":[5,5]}""", """{"station":"T-1"}"""), Feature("""{"type":"Point","coordinates":[50,50]}""", """{"station":"T-2"}""")],
            [Square(0, 0, 10, """{"district":"North"}""")]);

        Assert.Equal(2, result.Count);   // unmatched stays
        Assert.Equal("North", result[0]["properties"]!["join_district"]!.GetValue<string>());
        Assert.Null(result[1]["properties"]!["join_district"]);
    }

    [Fact]
    public async Task Join_nearest_respects_max_distance()
    {
        var result = await JoinAsync(new { operation = "join", relation = "nearest", maxDistance = 20, keepUnmatched = false },
            [Feature("""{"type":"Point","coordinates":[0,0]}""", """{"name":"a"}"""), Feature("""{"type":"Point","coordinates":[100,0]}""", """{"name":"b"}""")],
            [Feature("""{"type":"Point","coordinates":[10,0]}""", """{"pole":"P-1"}"""), Feature("""{"type":"Point","coordinates":[15,0]}""", """{"pole":"P-2"}""")]);

        var joined = Assert.Single(result);
        Assert.Equal("a", joined["properties"]!["name"]!.GetValue<string>());
        Assert.Equal("P-1", joined["properties"]!["join_pole"]!.GetValue<string>());
    }

    [Fact]
    public async Task Clip_and_difference_split_lines_at_the_overlay()
    {
        JsonObject[] lines = [Feature("""{"type":"LineString","coordinates":[[-5,5],[5,5]]}""", """{"line_id":"L-1"}""")];
        JsonObject[] district = [Square(0, 0, 10)];

        var clipped = Assert.Single(await JoinAsync(new { operation = "clip" }, lines, district));
        var outside = Assert.Single(await JoinAsync(new { operation = "difference" }, lines, district));

        Assert.Equal("[[0,5],[5,5]]", clipped["geometry"]!["coordinates"]!.ToJsonString());
        Assert.Equal("[[-5,5],[0,5]]", outside["geometry"]!["coordinates"]!.ToJsonString());
        Assert.Equal("L-1", clipped["properties"]!["line_id"]!.GetValue<string>());
    }

    [Fact]
    public async Task Intersect_gives_one_feature_per_overlapping_pair()
    {
        var result = await JoinAsync(new { operation = "intersect" },
            [Square(0, 0, 10, """{"zone":"Z"}""")],
            [Square(5, 0, 10, """{"parcel":"1"}"""), Square(-5, 0, 10, """{"parcel":"2"}"""), Square(50, 50, 1, """{"parcel":"3"}""")]);

        Assert.Equal(["1", "2"], result.Select(f => f["properties"]!["join_parcel"]!.GetValue<string>()).Order());
        Assert.All(result, f => Assert.Equal(50, Area(f), 6));
    }

    [Fact]
    public async Task Join_refuses_different_srs()
        => await Assert.ThrowsAsync<PipeException>(() => JoinAsync(new { operation = "join" },
            [Feature("""{"type":"Point","coordinates":[0,0]}""", srs: 31256)],
            [Feature("""{"type":"Point","coordinates":[0,0]}""", srs: 3857)]));

    #endregion

    [Fact]
    public async Task Read_features_format_passes_through_the_spatial_nodes()
    {
        // polygon with hole and multipolygon, as Read Features writes them (GisGeoJson)
        var holed = Feature("""{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]],[[2,2],[2,4],[4,4],[4,2],[2,2]]]}""");
        var multi = Feature("""{"type":"MultiPolygon","coordinates":[[[[0,0],[10,0],[10,10],[0,10],[0,0]]],[[[20,0],[30,0],[30,10],[20,10],[20,0]]]]}""");

        var holedArea = Assert.Single(await GeometryAsync(new { operation = "area" }, holed));
        var multiArea = Assert.Single(await GeometryAsync(new { operation = "area" }, multi));
        var valid = Assert.Single(await GeometryAsync(new { operation = "makeValid" }, holed));

        Assert.Equal(96, holedArea["properties"]!["area"]!.GetValue<double>(), 6);
        Assert.Equal(200, multiArea["properties"]!["area"]!.GetValue<double>(), 6);
        // and back into gView geometry as Write Features reads it
        var polygon = (gView.Framework.Core.Geometry.IPolygon)GisGeoJson.ToGeometry(valid["geometry"])!;
        Assert.Equal(1, polygon.OuterRingCount);
        Assert.Equal(1, polygon.InnerRingCount);
    }
}
