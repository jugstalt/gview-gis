using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Pipe.Abstraction;
using Path = System.IO.Path;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>Write Features and Read Features against a gView FDB (SQLite) in a temporary folder.</summary>
public sealed class FeatureNodesTests : IAsyncLifetime
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gview-pipe-gis-tests", Guid.NewGuid().ToString("N"));
    private Dictionary<string, object> _credentials = new();

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_folder);
        new GisPluginStartup().ConfigureServices(null!);  // what the Pipe host does when it loads the plugin

        // a gView feature database in a SQLite file: it can insert, update and delete (shape files can't update)
        var file = Path.Combine(_folder, "grid.fdb");
        var database = new SQLiteFDB();
        Assert.True(database.Create(file), database.LastErrorMessage);
        Assert.True(await database.Open(file), database.LastErrorMessage);
        Assert.True(await database.CreateDataset("grid", null!) >= 0, database.LastErrorMessage);
        var fields = new FieldCollection();
        fields.Add(new Field("line_id", FieldType.String, 20));
        fields.Add(new Field("voltage", FieldType.integer));
        fields.Add(new Field("feeder", FieldType.String, 50));
        Assert.True(await database.CreateFeatureClass("grid", "lines", new GeometryDef(GeometryType.Polyline), fields) >= 0, database.LastErrorMessage);
        database.Dispose();

        _credentials["shapes"] = new GisDatasourceCredential { Type = GisDatasourceType.FdbSqlite, ConnectionString = $"Data Source={file};dsname=grid" };
    }

    public Task DisposeAsync()
    {
        try { Directory.Delete(_folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        return Task.CompletedTask;
    }

    static private JsonObject Line(string id, int voltage, params double[][] coordinates)
        => new()
        {
            ["type"] = "Feature",
            ["geometry"] = new JsonObject { ["type"] = "LineString", ["coordinates"] = new JsonArray(coordinates.Select(c => (JsonNode?)new JsonArray(c[0], c[1])).ToArray()) },
            ["properties"] = new JsonObject { ["line_id"] = id, ["voltage"] = voltage }
        };

    private Task<List<JsonObject>> ReadAsync(object parameters) => FakeNodeContext.RunAsync(new ReadFeaturesNode(), parameters, _credentials, new JsonObject());

    [Fact]
    public async Task Inserted_features_are_read_back_as_geojson_items()
    {
        var written = await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "shapes", featureClass = "lines", mode = "insert" }, _credentials,
            Line("L-1", 400, [0, 0], [10, 0]),
            Line("L-2", 400, [10, 0], [10, 5], [20, 5]));

        Assert.Equal(2, written.Count);

        var read = await ReadAsync(new { credentialId = "shapes", featureClass = "lines" });

        Assert.Equal(["L-1", "L-2"], read.Select(f => f["properties"]!["line_id"]!.GetValue<string>()).Order());
        var l2 = read.Single(f => f["properties"]!["line_id"]!.GetValue<string>() == "L-2");
        Assert.Equal("Feature", l2["type"]!.GetValue<string>());
        Assert.Equal("LineString", l2["geometry"]!["type"]!.GetValue<string>());
        Assert.Equal("[[10,0],[10,5],[20,5]]", l2["geometry"]!["coordinates"]!.ToJsonString());
        Assert.Equal(400, l2["properties"]!["voltage"]!.GetValue<long>());
        Assert.Equal(["feeder", "line_id", "voltage"], l2["properties"]!.AsObject().Select(p => p.Key).Order());   // no FDB_OID, FDB_SHAPE, FDB_NID
        Assert.True(l2["id"]!.GetValue<int>() > 0);
    }

    [Fact]
    public async Task Match_field_and_spatial_filter_select_features()
    {
        await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "shapes", featureClass = "lines", mode = "insert" }, _credentials,
            Line("L-1", 400, [0, 0], [10, 0]),
            Line("L-2", 230, [100, 100], [110, 100]));

        var byField = await ReadAsync(new { credentialId = "shapes", featureClass = "lines", matchField = "line_id", matchValue = "L-2" });
        var byArea = await ReadAsync(new
        {
            credentialId = "shapes", featureClass = "lines", spatialFilter = "intersects",
            filterGeometry = new { type = "Polygon", coordinates = new[] { new[] { new[] { -1.0, -1.0 }, new[] { 5.0, -1.0 }, new[] { 5.0, 1.0 }, new[] { -1.0, 1.0 }, new[] { -1.0, -1.0 } } } }
        });

        Assert.Equal("L-2", Assert.Single(byField)["properties"]!["line_id"]!.GetValue<string>());
        Assert.Equal("L-1", Assert.Single(byArea)["properties"]!["line_id"]!.GetValue<string>());
    }

    [Fact]
    public async Task Update_by_key_field_changes_only_the_given_properties()
    {
        await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "shapes", featureClass = "lines", mode = "insert" }, _credentials,
            Line("L-1", 400, [0, 0], [10, 0]));

        var updated = await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "shapes", featureClass = "lines", mode = "update", keyField = "line_id" }, _credentials,
            new JsonObject { ["type"] = "Feature", ["properties"] = new JsonObject { ["line_id"] = "L-1", ["feeder"] = "T-1#C-3" } });

        var line = Assert.Single(await ReadAsync(new { credentialId = "shapes", featureClass = "lines" }));
        Assert.Single(updated);
        Assert.Equal("T-1#C-3", line["properties"]!["feeder"]!.GetValue<string>());
        Assert.Equal(400, line["properties"]!["voltage"]!.GetValue<long>());
        Assert.Equal("[[0,0],[10,0]]", line["geometry"]!["coordinates"]!.ToJsonString());
    }

    [Fact]
    public async Task Update_of_a_missing_key_fails_and_upsert_inserts_it()
    {
        var item = new JsonObject { ["type"] = "Feature", ["properties"] = new JsonObject { ["line_id"] = "L-9", ["voltage"] = 230 } };

        await Assert.ThrowsAsync<PipeException>(() => FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "shapes", featureClass = "lines", mode = "update", keyField = "line_id" }, _credentials, item));
        await FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "shapes", featureClass = "lines", mode = "upsert", keyField = "line_id" }, _credentials, item);

        Assert.Equal("L-9", Assert.Single(await ReadAsync(new { credentialId = "shapes", featureClass = "lines" }))["properties"]!["line_id"]!.GetValue<string>());
    }

    [Fact]
    public async Task Key_values_are_quoted_not_inserted_as_sql()
    {
        await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "shapes", featureClass = "lines", mode = "insert" }, _credentials,
            Line("L-1", 400, [0, 0], [10, 0]));

        var read = await ReadAsync(new { credentialId = "shapes", featureClass = "lines", matchField = "line_id", matchValue = "x' OR '1'='1" });

        Assert.Empty(read);
    }

    [Fact]
    public async Task Unknown_feature_class_names_the_existing_ones()
    {
        var error = await Assert.ThrowsAsync<PipeException>(() => ReadAsync(new { credentialId = "shapes", featureClass = "nope" }));

        Assert.True(error.Message.Contains("lines"), error.Message);
    }
}
