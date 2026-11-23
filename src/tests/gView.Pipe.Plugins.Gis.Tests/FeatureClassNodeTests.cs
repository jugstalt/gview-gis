using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Pipe.Abstraction;
using System.Text.Json.Nodes;
using Path = System.IO.Path;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>The Feature Class node against a gView FDB (SQLite) in a temporary folder.</summary>
public sealed class FeatureClassNodeTests : IAsyncLifetime
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
        Assert.True(await database.CreateFeatureClass("grid", "lines", new GeometryDef(GeometryType.Polyline), fields) >= 0, database.LastErrorMessage);
        database.Dispose();

        var connectionString = $"Data Source={file};dsname=grid";
        _credentials["test"] = new GisDatasourceCredential { Type = GisDatasourceType.FdbSqlite, ConnectionString = connectionString, AllowSchemaChanges = true };
        _credentials["locked"] = new GisDatasourceCredential { Type = GisDatasourceType.FdbSqlite, ConnectionString = connectionString };
    }

    public Task DisposeAsync()
    {
        try { Directory.Delete(_folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        return Task.CompletedTask;
    }

    private Task<List<JsonObject>> RunAsync(object parameters, params JsonObject[] input)
        => FakeNodeContext.RunAsync(new FeatureClassNode(), parameters, _credentials, input.Length > 0 ? input : [new JsonObject()]);

    private async Task<JsonObject> DescribeAsync(string name)
        => Assert.Single(await RunAsync(new { credentialId = "test", operation = "describe", featureClass = name }));

    static private string FieldsOf(JsonObject schema)
        => string.Join(", ", schema["fields"]!.AsArray().Select(f => $"{f!["name"]}:{f["type"]}"));

    [Fact]
    public async Task Describe_returns_geometry_type_and_fields()
    {
        var schema = await DescribeAsync("lines");

        Assert.Equal("lines", schema["name"]!.GetValue<string>());
        Assert.Equal("polyline", schema["geometryType"]!.GetValue<string>());
        Assert.Equal("line_id:text, voltage:integer", FieldsOf(schema));
        Assert.Equal(20, schema["fields"]![0]!["size"]!.GetValue<int>());
    }

    [Fact]
    public async Task Describe_then_create_copies_the_schema_under_a_new_name()
    {
        var source = await DescribeAsync("lines");

        var created = Assert.Single(await RunAsync(new { credentialId = "test", operation = "create", featureClass = "lines_copy", schema = source }));

        Assert.Equal("lines_copy", created["name"]!.GetValue<string>());
        Assert.Equal("polyline", created["geometryType"]!.GetValue<string>());
        Assert.Equal(FieldsOf(source), FieldsOf(created));
        Assert.Equal(["lines", "lines_copy"], (await RunAsync(new { credentialId = "test", operation = "list" })).Select(f => f["name"]!.GetValue<string>()));

        // the copy takes features
        await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "test", featureClass = "lines_copy", mode = "insert" }, _credentials,
            JsonNode.Parse("""{"type":"Feature","geometry":{"type":"LineString","coordinates":[[0,0],[1,1]]},"properties":{"line_id":"L-1","voltage":400}}""")!.AsObject());
        Assert.Single(await FakeNodeContext.RunAsync(new ReadFeaturesNode(), new { credentialId = "test", featureClass = "lines_copy" }, _credentials, new JsonObject()));
    }

    [Fact]
    public async Task Create_without_schema_takes_the_input_item()
    {
        var source = await DescribeAsync("lines");

        var created = Assert.Single(await RunAsync(new { credentialId = "test", operation = "create", featureClass = "lines_copy" }, source));

        Assert.Equal(FieldsOf(source), FieldsOf(created));
    }

    [Fact]
    public async Task Create_of_an_existing_class_fails_or_replaces_and_create_if_not_exists_keeps_it()
    {
        var schema = new JsonObject
        {
            ["geometryType"] = "point",
            ["fields"] = new JsonArray(new JsonObject { ["name"] = "name", ["type"] = "text" })
        };

        await Assert.ThrowsAsync<PipeException>(() => RunAsync(new { credentialId = "test", operation = "create", featureClass = "lines", schema }));
        var kept = Assert.Single(await RunAsync(new { credentialId = "test", operation = "createIfNotExists", featureClass = "lines", schema }));
        var replaced = Assert.Single(await RunAsync(new { credentialId = "test", operation = "create", featureClass = "lines", schema, ifExists = "replace" }));

        Assert.Equal("polyline", kept["geometryType"]!.GetValue<string>());
        Assert.Equal("point", replaced["geometryType"]!.GetValue<string>());
        Assert.Equal("name:text", FieldsOf(replaced));
        Assert.Equal(GisSchema.DefaultTextSize, replaced["fields"]![0]!["size"]!.GetValue<int>());
    }

    [Fact]
    public async Task Truncate_deletes_all_features_and_delete_the_class()
    {
        await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "test", featureClass = "lines", mode = "insert" }, _credentials,
            JsonNode.Parse("""{"type":"Feature","geometry":{"type":"LineString","coordinates":[[0,0],[1,1]]},"properties":{"line_id":"L-1"}}""")!.AsObject(),
            JsonNode.Parse("""{"type":"Feature","geometry":{"type":"LineString","coordinates":[[1,1],[2,2]]},"properties":{"line_id":"L-2"}}""")!.AsObject());

        var truncated = Assert.Single(await RunAsync(new { credentialId = "test", operation = "truncate", featureClass = "lines" }));
        var read = await FakeNodeContext.RunAsync(new ReadFeaturesNode(), new { credentialId = "test", featureClass = "lines" }, _credentials, new JsonObject());
        var deleted = Assert.Single(await RunAsync(new { credentialId = "test", operation = "delete", featureClass = "lines" }));

        Assert.Equal(2, truncated["deletedFeatures"]!.GetValue<int>());
        Assert.Empty(read);
        Assert.True(deleted["deleted"]!.GetValue<bool>());
        Assert.Empty(await RunAsync(new { credentialId = "test", operation = "list" }));
    }

    [Theory]
    [InlineData("create")]
    [InlineData("truncate")]
    [InlineData("delete")]
    public async Task Schema_changes_need_the_switch_of_the_credential(string operation)
    {
        var schema = new JsonObject { ["geometryType"] = "point", ["fields"] = new JsonArray() };

        var error = await Assert.ThrowsAsync<PipeException>(() => RunAsync(new { credentialId = "locked", operation, featureClass = operation == "create" ? "new_points" : "lines", schema }));

        Assert.Contains("Allow schema changes", error.Message);
        Assert.Equal(["lines"], (await RunAsync(new { credentialId = "test", operation = "list" })).Select(f => f["name"]!.GetValue<string>()));
    }

    [Fact]
    public async Task Describe_and_list_need_no_switch()
    {
        Assert.Single(await RunAsync(new { credentialId = "locked", operation = "list" }));
        Assert.Equal("lines", Assert.Single(await RunAsync(new { credentialId = "locked", operation = "describe", featureClass = "lines" }))["name"]!.GetValue<string>());
    }

    [Fact]
    public async Task Unknown_field_types_are_refused()
    {
        var schema = new JsonObject { ["geometryType"] = "point", ["fields"] = new JsonArray(new JsonObject { ["name"] = "x", ["type"] = "money" }) };

        var error = await Assert.ThrowsAsync<PipeException>(() => RunAsync(new { credentialId = "test", operation = "create", featureClass = "p", schema }));

        Assert.Contains("money", error.Message);
    }
}
