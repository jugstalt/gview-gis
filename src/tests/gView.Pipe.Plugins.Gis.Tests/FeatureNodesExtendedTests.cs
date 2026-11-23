using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Nodes;
using Path = System.IO.Path;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>Order by, distinct, count and bounding box of Read Features; error output, field mapping and write fields of Write Features.</summary>
public sealed class FeatureNodesExtendedTests : IAsyncLifetime
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gview-pipe-gis-tests", Guid.NewGuid().ToString("N"));
    private readonly Dictionary<string, object> _credentials = new();

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_folder);
        new GisPluginStartup().ConfigureServices(null!);

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

        _credentials["test"] = new GisDatasourceCredential { Type = GisDatasourceType.FdbSqlite, ConnectionString = $"Data Source={file};dsname=grid", AllowSchemaChanges = true };
    }

    public Task DisposeAsync()
    {
        try { Directory.Delete(_folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        return Task.CompletedTask;
    }

    static private JsonObject Line(string id, int voltage, string feeder, double x)
        => new()
        {
            ["type"] = "Feature",
            ["geometry"] = new JsonObject
            {
                ["type"] = "LineString",
                ["coordinates"] = new JsonArray(new JsonArray(x, 0), new JsonArray(x + 10, 0))
            },
            ["properties"] = new JsonObject { ["line_id"] = id, ["voltage"] = voltage, ["feeder"] = feeder }
        };

    private Task<List<JsonObject>> InsertAsync(params JsonObject[] items)
        => FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "test", featureClass = "lines", mode = "insert" }, _credentials, items);

    private Task<List<JsonObject>> ReadAsync(object parameters) => FakeNodeContext.RunAsync(new ReadFeaturesNode(), parameters, _credentials, new JsonObject());

    private async Task SeedAsync()
        => await InsertAsync(Line("L-1", 400, "A", 0), Line("L-2", 110, "A", 100), Line("L-3", 230, "B", 200));

    #region Read Features

    [Fact]
    public async Task Order_by_sorts_and_offset_with_limit_pages()
    {
        await SeedAsync();

        var sorted = await ReadAsync(new { credentialId = "test", featureClass = "lines", orderBy = "voltage desc" });
        var page = await ReadAsync(new { credentialId = "test", featureClass = "lines", orderBy = "voltage desc", offset = 1, limit = 1 });

        Assert.Equal(["L-1", "L-3", "L-2"], sorted.Select(f => f["properties"]!["line_id"]!.GetValue<string>()));
        Assert.Equal("L-3", Assert.Single(page)["properties"]!["line_id"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("nofield")]
    [InlineData("voltage; drop table x")]
    [InlineData("voltage sideways")]
    public async Task Order_by_takes_only_fields_with_a_direction(string orderBy)
    {
        await SeedAsync();

        await Assert.ThrowsAsync<PipeException>(() => ReadAsync(new { credentialId = "test", featureClass = "lines", orderBy }));
    }

    [Fact]
    public async Task Distinct_values_gives_one_item_per_combination_without_geometry()
    {
        await SeedAsync();

        var feeders = await ReadAsync(new { credentialId = "test", featureClass = "lines", output = "distinctValues", fields = "feeder", orderBy = "feeder" });

        Assert.Equal(["A", "B"], feeders.Select(f => f["properties"]!["feeder"]!.GetValue<string>()));
        Assert.All(feeders, f => Assert.Null(f["geometry"]));
        Assert.All(feeders, f => Assert.Equal(["feeder"], f["properties"]!.AsObject().Select(p => p.Key)));
    }

    [Fact]
    public async Task Distinct_values_needs_fields()
    {
        await Assert.ThrowsAsync<PipeException>(() => ReadAsync(new { credentialId = "test", featureClass = "lines", output = "distinctValues" }));
    }

    [Fact]
    public async Task Count_gives_one_item_with_the_number_of_matching_features()
    {
        await SeedAsync();

        var all = Assert.Single(await ReadAsync(new { credentialId = "test", featureClass = "lines", output = "count" }));
        var feederA = Assert.Single(await ReadAsync(new { credentialId = "test", featureClass = "lines", output = "count", matchField = "feeder", matchValue = "A" }));

        Assert.Equal(3, all["count"]!.GetValue<int>());
        Assert.Equal(2, feederA["count"]!.GetValue<int>());
        Assert.Equal("lines", feederA["featureClass"]!.GetValue<string>());
    }

    [Fact]
    public async Task Filter_geometry_can_be_a_bounding_box()
    {
        await SeedAsync();

        var inside = await ReadAsync(new { credentialId = "test", featureClass = "lines", spatialFilter = "intersects", filterGeometry = new[] { -1.0, -1.0, 50.0, 1.0 } });

        Assert.Equal("L-1", Assert.Single(inside)["properties"]!["line_id"]!.GetValue<string>());
        await Assert.ThrowsAsync<PipeException>(() => ReadAsync(new { credentialId = "test", featureClass = "lines", spatialFilter = "intersects", filterGeometry = new[] { 1.0, 2.0, 3.0 } }));
    }

    #endregion

    #region Write Features

    [Fact]
    public async Task Skip_item_sends_items_that_can_not_be_written_to_the_error_output()
    {
        var noGeometry = JsonNode.Parse("""{"type":"Feature","properties":{"line_id":"L-9","voltage":110}}""")!.AsObject();

        var ports = await FakeNodeContext.RunPortsAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "insert", writeGeometry = "always", onError = "skipItem" }, _credentials,
            Line("L-1", 400, "A", 0), noGeometry);

        Assert.Equal("L-1", Assert.Single(ports[PipePorts.Main])["properties"]!["line_id"]!.GetValue<string>());
        var failed = Assert.Single(ports[PipePorts.Error]);
        Assert.Equal("L-9", failed["properties"]!["line_id"]!.GetValue<string>());
        Assert.Contains("no geometry", failed["error"]!["message"]!.GetValue<string>());
        Assert.Single(await ReadAsync(new { credentialId = "test", featureClass = "lines" }));
    }

    [Fact]
    public async Task Stop_is_the_default_and_fails_the_node()
    {
        var noGeometry = JsonNode.Parse("""{"type":"Feature","properties":{"line_id":"L-9"}}""")!.AsObject();

        await Assert.ThrowsAsync<PipeException>(() => FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "insert", writeGeometry = "always" }, _credentials, noGeometry));
    }

    [Fact]
    public void The_error_output_is_there_only_when_items_are_skipped()
    {
        var node = new WriteFeaturesNode();

        Assert.Equal(["main"], node.GetOutputs(new JsonObject()).Select(p => p.Name));
        Assert.Equal(["main"], node.GetOutputs(new JsonObject { ["onError"] = "stop" }).Select(p => p.Name));
        Assert.Equal(["main", "error"], node.GetOutputs(new JsonObject { ["onError"] = "skipItem" }).Select(p => p.Name));
    }

    [Fact]
    public async Task Field_mapping_renames_properties_on_the_way()
    {
        var item = JsonNode.Parse("""{"type":"Feature","geometry":{"type":"LineString","coordinates":[[0,0],[5,5]]},"properties":{"name":"L-7","volt":110,"feeder":"C"}}""")!.AsObject();

        await FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "insert", fieldMapping = new { line_id = "name", voltage = "volt" } }, _credentials, item);

        var line = Assert.Single(await ReadAsync(new { credentialId = "test", featureClass = "lines" }));
        Assert.Equal("L-7", line["properties"]!["line_id"]!.GetValue<string>());
        Assert.Equal(110, line["properties"]!["voltage"]!.GetValue<long>());
        Assert.Equal("C", line["properties"]!["feeder"]!.GetValue<string>());
    }

    [Fact]
    public async Task Field_mapping_to_a_missing_field_is_an_error()
    {
        await Assert.ThrowsAsync<PipeException>(() => FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "insert", fieldMapping = new { nofield = "name" } }, _credentials, Line("L-1", 1, "A", 0)));
    }

    [Fact]
    public async Task Unknown_properties_are_ignored_or_fail()
    {
        var item = Line("L-1", 400, "A", 0);
        item["properties"]!["colour"] = "red";

        var written = await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "test", featureClass = "lines", mode = "insert" }, _credentials, item);
        var error = await Assert.ThrowsAsync<PipeException>(() => FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "insert", unknownProperties = "fail" }, _credentials, item));

        Assert.Single(written);
        Assert.Contains("colour", error.Message);
    }

    [Fact]
    public async Task Write_fields_limit_what_an_update_changes()
    {
        await SeedAsync();
        var item = Line("L-1", 999, "Z", 500);

        await FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "update", keyField = "line_id", writeFields = "feeder", writeGeometry = "never" }, _credentials, item);

        var line = Assert.Single(await ReadAsync(new { credentialId = "test", featureClass = "lines", matchField = "line_id", matchValue = "L-1" }));
        Assert.Equal("Z", line["properties"]!["feeder"]!.GetValue<string>());
        Assert.Equal(400, line["properties"]!["voltage"]!.GetValue<long>());
        Assert.Equal("[[0,0],[10,0]]", line["geometry"]!["coordinates"]!.ToJsonString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("id")]
    [InlineData("FDB_OID")]
    public async Task The_object_id_of_the_item_is_the_key_when_the_key_field_is_empty_id_or_the_id_field(string keyField)
    {
        await SeedAsync();
        var read = await ReadAsync(new { credentialId = "test", featureClass = "lines", matchField = "line_id", matchValue = "L-2" });
        var item = Assert.Single(read);
        item["properties"]!["feeder"] = "Z";

        await FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "update", keyField, writeGeometry = "never" }, _credentials, item);

        var lines = await ReadAsync(new { credentialId = "test", featureClass = "lines" });
        Assert.Equal(["Z"], lines.Where(f => f["properties"]!["feeder"]!.GetValue<string>() == "Z").Select(f => f["properties"]!["feeder"]!.GetValue<string>()));
        Assert.Equal("L-2", lines.Single(f => f["properties"]!["feeder"]!.GetValue<string>() == "Z")["properties"]!["line_id"]!.GetValue<string>());
    }

    [Fact]
    public async Task Write_fields_with_a_missing_field_is_an_error()
    {
        await Assert.ThrowsAsync<PipeException>(() => FakeNodeContext.RunAsync(new WriteFeaturesNode(),
            new { credentialId = "test", featureClass = "lines", mode = "insert", writeFields = "nofield" }, _credentials, Line("L-1", 1, "A", 0)));
    }

    #endregion
}
