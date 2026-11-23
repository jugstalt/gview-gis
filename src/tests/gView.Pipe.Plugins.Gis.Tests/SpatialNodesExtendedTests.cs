using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Nodes;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>Offset curve, smooth and point at distance of Geometry; polygonize and Voronoi clip of Spatial Aggregate; cut and locate along line of Spatial Join.</summary>
public class SpatialNodesExtendedTests
{
    static private JsonObject Feature(string geometry, string properties = "{}", int? srs = 31256)
        => new()
        {
            ["type"] = "Feature",
            ["geometry"] = JsonNode.Parse(geometry),
            ["properties"] = JsonNode.Parse(properties),
            ["srs"] = srs
        };

    static private JsonObject Line(double x1, double y1, double x2, double y2, string properties = "{}", int? srs = 31256)
        => Feature($$"""{"type":"LineString","coordinates":[[{{x1}},{{y1}}],[{{x2}},{{y2}}]]}""", properties, srs);

    static private JsonObject Point(double x, double y, string properties = "{}")
        => Feature($$"""{"type":"Point","coordinates":[{{x}},{{y}}]}""", properties);

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

    static private double[][] Coordinates(JsonObject feature)
        => feature["geometry"]!["coordinates"]!.AsArray().Select(p => new[] { p![0]!.GetValue<double>(), p[1]!.GetValue<double>() }).ToArray();

    static private double Area(JsonObject feature)
    {
        var ring = feature["geometry"]!["coordinates"]![0]!.AsArray().Select(p => (X: p![0]!.GetValue<double>(), Y: p[1]!.GetValue<double>())).ToList();
        return Math.Abs(ring.Zip(ring.Skip(1), (a, b) => a.X * b.Y - b.X * a.Y).Sum()) / 2;
    }

    #region Geometry

    [Fact]
    public async Task Offset_curve_moves_a_line_to_the_left_or_right()
    {
        var left = Assert.Single(await GeometryAsync(new { operation = "offsetCurve", distance = 5 }, Line(0, 0, 100, 0)));
        var right = Assert.Single(await GeometryAsync(new { operation = "offsetCurve", distance = -5 }, Line(0, 0, 100, 0)));

        Assert.Equal("LineString", left["geometry"]!["type"]!.GetValue<string>());
        Assert.All(Coordinates(left), c => Assert.Equal(5, c[1], 6));
        Assert.All(Coordinates(right), c => Assert.Equal(-5, c[1], 6));
    }

    [Fact]
    public async Task Offset_curve_needs_lines_or_polygons_and_a_projected_srs()
    {
        await Assert.ThrowsAsync<PipeException>(() => GeometryAsync(new { operation = "offsetCurve", distance = 5 }, Point(0, 0)));
        await Assert.ThrowsAsync<PipeException>(() => GeometryAsync(new { operation = "offsetCurve", distance = 5 }, Line(15, 47, 16, 47, srs: 4326)));
    }

    [Fact]
    public async Task Smooth_adds_vertices_and_keeps_the_ends()
    {
        var item = Feature("""{"type":"LineString","coordinates":[[0,0],[10,10],[20,0]]}""");

        var smooth = Assert.Single(await GeometryAsync(new { operation = "smooth", iterations = 2 }, item));

        var coordinates = Coordinates(smooth);
        Assert.True(coordinates.Length > 3);
        Assert.Equal([0, 0], coordinates[0]);
        Assert.Equal([20, 0], coordinates[^1]);
        Assert.All(coordinates, c => Assert.InRange(c[1], 0, 10));   // the corner is cut, never overshot
    }

    [Fact]
    public async Task Point_at_distance_lies_on_the_line_and_beside_it()
    {
        var onLine = Assert.Single(await GeometryAsync(new { operation = "pointAtDistance", position = 30 }, Line(0, 0, 100, 0)));
        var fraction = Assert.Single(await GeometryAsync(new { operation = "pointAtDistance", position = 0.5, positionIsFraction = true }, Line(0, 0, 100, 0)));
        var beside = Assert.Single(await GeometryAsync(new { operation = "pointAtDistance", position = 30, sideOffset = 2 }, Line(0, 0, 100, 0)));

        Assert.Equal("Point", onLine["geometry"]!["type"]!.GetValue<string>());
        Assert.Equal("[30,0]", onLine["geometry"]!["coordinates"]!.ToJsonString());
        Assert.Equal("[50,0]", fraction["geometry"]!["coordinates"]!.ToJsonString());
        Assert.Equal("[30,2]", beside["geometry"]!["coordinates"]!.ToJsonString());
    }

    [Fact]
    public async Task Point_at_distance_checks_its_input()
    {
        await Assert.ThrowsAsync<PipeException>(() => GeometryAsync(new { operation = "pointAtDistance", position = 1 }, Point(0, 0)));
        await Assert.ThrowsAsync<PipeException>(() => GeometryAsync(new { operation = "pointAtDistance", position = 1.5, positionIsFraction = true }, Line(0, 0, 10, 0)));
        await Assert.ThrowsAsync<PipeException>(() => GeometryAsync(new { operation = "pointAtDistance", position = 1 }, Line(15, 47, 16, 47, srs: 4326)));
    }

    #endregion

    #region Spatial Aggregate

    [Fact]
    public async Task Dissolve_aggregates_can_use_the_object_id_of_the_items()
    {
        var a = Square(0, 0, 10, """{"feeder":"A"}""");
        a["id"] = 7;
        var b = Square(10, 0, 10, """{"feeder":"A"}""");
        b["id"] = 3;

        var result = Assert.Single(await AggregateAsync(new
        {
            operation = "dissolve",
            groupBy = new[] { "feeder" },
            aggregates = new Dictionary<string, string> { ["n"] = "count", ["first_id"] = "min(id)", ["ids"] = "list(id)" }
        }, a, b));

        Assert.Equal(2, result["properties"]!["n"]!.GetValue<int>());
        Assert.Equal(3, result["properties"]!["first_id"]!.GetValue<double>());
        Assert.Equal("[7,3]", result["properties"]!["ids"]!.ToJsonString());
    }

    [Fact]
    public async Task Polygonize_builds_polygons_from_lines_that_enclose_an_area()
    {
        var result = await AggregateAsync(new { operation = "polygonize" },
            Line(0, 0, 10, 0), Line(10, 0, 10, 10), Line(10, 10, 0, 10), Line(0, 10, 0, 0),
            Line(0, 5, 10, 5),      // splits the square in two
            Line(20, 20, 30, 30));  // encloses nothing

        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Equal("Polygon", f["geometry"]!["type"]!.GetValue<string>()));
        Assert.All(result, f => Assert.Equal(50, Area(f), 6));
        Assert.All(result, f => Assert.Equal(31256, f["srs"]!.GetValue<int>()));
    }

    [Fact]
    public async Task Polygonize_of_lines_that_enclose_nothing_gives_nothing()
    {
        Assert.Empty(await AggregateAsync(new { operation = "polygonize" }, Line(0, 0, 10, 0), Line(10, 0, 10, 10)));
    }

    [Fact]
    public async Task Voronoi_cells_are_cut_to_the_clip_geometry()
    {
        var points = new[] { Point(10, 10, """{"id":"A"}"""), Point(90, 10, """{"id":"B"}"""), Point(50, 80, """{"id":"C"}""") };

        var clipped = await AggregateAsync(new { operation = "voronoi", clipGeometry = new[] { 0.0, 0.0, 100.0, 100.0 } }, points);
        var polygon = await AggregateAsync(new
        {
            operation = "voronoi",
            clipGeometry = new { type = "Polygon", coordinates = new[] { new[] { new[] { 0.0, 0.0 }, new[] { 100.0, 0.0 }, new[] { 100.0, 100.0 }, new[] { 0.0, 100.0 }, new[] { 0.0, 0.0 } } } }
        }, points);

        Assert.Equal(3, clipped.Count);
        Assert.Equal(10000, clipped.Sum(Area), 3);   // the cells tile the clip box
        Assert.Equal(["A", "B", "C"], clipped.Select(c => c["properties"]!["id"]!.GetValue<string>()).Order());
        Assert.Equal(10000, polygon.Sum(Area), 3);
    }

    [Fact]
    public async Task Voronoi_clip_must_be_a_polygon_in_the_srs_of_the_items()
    {
        var points = new[] { Point(10, 10), Point(90, 10) };

        await Assert.ThrowsAsync<PipeException>(() => AggregateAsync(new { operation = "voronoi", clipGeometry = new[] { 0.0, 0.0, 100.0, 100.0 }, clipSrs = 4326 }, points));
        await Assert.ThrowsAsync<PipeException>(() => AggregateAsync(new { operation = "voronoi", clipGeometry = new { type = "Point", coordinates = new[] { 1.0, 2.0 } } }, points));
        await Assert.ThrowsAsync<PipeException>(() => AggregateAsync(new { operation = "voronoi", clipGeometry = new[] { 1.0, 2.0 } }, points));
    }

    #endregion

    #region Spatial Join

    [Fact]
    public async Task Cut_splits_polygons_and_lines_along_the_overlay_lines()
    {
        var polygons = await JoinAsync(new { operation = "cut" },
            [Square(0, 0, 10, """{"parcel":"P-1"}""")],
            [Line(5, -5, 5, 15)]);
        var lines = await JoinAsync(new { operation = "cut" },
            [Line(0, 0, 10, 0, """{"id":"L-1"}""")],
            [Line(5, -5, 5, 5)]);

        Assert.Equal(2, polygons.Count);
        Assert.All(polygons, f => Assert.Equal(50, Area(f), 6));
        Assert.All(polygons, f => Assert.Equal("P-1", f["properties"]!["parcel"]!.GetValue<string>()));
        Assert.Equal(2, lines.Count);
        Assert.All(lines, f => Assert.Equal("L-1", f["properties"]!["id"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Cut_without_cutting_lines_keeps_the_main_features()
    {
        var result = await JoinAsync(new { operation = "cut" }, [Square(0, 0, 10)], []);

        Assert.Equal(50 * 2, Area(Assert.Single(result)));
    }

    [Fact]
    public async Task Locate_along_line_tells_where_a_point_lies_on_the_nearest_line()
    {
        var result = Assert.Single(await JoinAsync(new { operation = "locateAlongLine", prefix = "line_" },
            [Point(30, 4, """{"id":"S-1"}""")],
            [Line(0, 0, 100, 0, """{"name":"road"}"""), Line(0, 50, 100, 50, """{"name":"far"}""")]));

        var properties = result["properties"]!;
        Assert.Equal("S-1", properties["id"]!.GetValue<string>());
        Assert.Equal(30, properties["line_position"]!.GetValue<double>(), 6);
        Assert.Equal(0.3, properties["line_fraction"]!.GetValue<double>(), 6);
        Assert.Equal(4, properties["line_distance"]!.GetValue<double>(), 6);
        Assert.Equal(4, Math.Abs(properties["line_offset"]!.GetValue<double>()), 6);
        Assert.Equal("road", properties["line_name"]!.GetValue<string>());
        Assert.Equal("[30,4]", result["geometry"]!["coordinates"]!.ToJsonString());   // the point stays
    }

    [Fact]
    public async Task Locate_along_line_can_snap_the_point_and_respects_max_distance()
    {
        var snapped = Assert.Single(await JoinAsync(new { operation = "locateAlongLine", pointOnLine = true },
            [Point(30, 4)], [Line(0, 0, 100, 0)]));
        var tooFar = await JoinAsync(new { operation = "locateAlongLine", locateMaxDistance = 2, keepUnmatched = false },
            [Point(30, 4)], [Line(0, 0, 100, 0)]);
        var kept = Assert.Single(await JoinAsync(new { operation = "locateAlongLine", locateMaxDistance = 2 },
            [Point(30, 4)], [Line(0, 0, 100, 0)]));

        Assert.Equal("[30,0]", snapped["geometry"]!["coordinates"]!.ToJsonString());
        Assert.Empty(tooFar);
        Assert.Null(kept["properties"]!["join_position"]);
    }

    [Fact]
    public async Task Locate_along_line_needs_points_and_a_projected_srs()
    {
        await Assert.ThrowsAsync<PipeException>(() => JoinAsync(new { operation = "locateAlongLine" }, [Line(0, 0, 1, 1)], [Line(0, 0, 100, 0)]));
        await Assert.ThrowsAsync<PipeException>(() => JoinAsync(new { operation = "locateAlongLine" },
            [Feature("""{"type":"Point","coordinates":[15,47]}""", srs: 4326)], [Line(15, 47, 16, 47, srs: 4326)]));
    }

    #endregion
}
