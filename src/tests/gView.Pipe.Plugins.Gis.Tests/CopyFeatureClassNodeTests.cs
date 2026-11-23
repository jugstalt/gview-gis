using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Credentials;
using gView.Pipe.Abstraction.Serialization;
using gView.Pipe.Abstraction.Storage;
using Microsoft.Extensions.DependencyInjection;
using Path = System.IO.Path;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>Copy Feature Class, extra and left out fields of Feature Class, and the choice list of feature classes.</summary>
public sealed class CopyFeatureClassNodeTests : IAsyncLifetime
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
        fields.Add(new Field("note", FieldType.String, 50));
        Assert.True(await database.CreateFeatureClass("grid", "lines", new GeometryDef(GeometryType.Polyline), fields) >= 0, database.LastErrorMessage);
        database.Dispose();

        var connectionString = $"Data Source={file};dsname=grid";
        _credentials["test"] = new GisDatasourceCredential { Type = GisDatasourceType.FdbSqlite, ConnectionString = connectionString, AllowSchemaChanges = true };
        _credentials["locked"] = new GisDatasourceCredential { Type = GisDatasourceType.FdbSqlite, ConnectionString = connectionString };

        await FakeNodeContext.RunAsync(new WriteFeaturesNode(), new { credentialId = "test", featureClass = "lines", mode = "insert" }, _credentials,
            Line("L-1", 400, 0), Line("L-2", 110, 100), Line("L-3", 230, 200));
    }

    public Task DisposeAsync()
    {
        try { Directory.Delete(_folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        return Task.CompletedTask;
    }

    static private JsonObject Line(string id, int voltage, double x)
        => new()
        {
            ["type"] = "Feature",
            ["geometry"] = new JsonObject { ["type"] = "LineString", ["coordinates"] = new JsonArray(new JsonArray(x, 0), new JsonArray(x + 10, 0)) },
            ["properties"] = new JsonObject { ["line_id"] = id, ["voltage"] = voltage, ["note"] = "n-" + id }
        };

    private Task<List<JsonObject>> CopyAsync(object parameters)
        => FakeNodeContext.RunAsync(new CopyFeatureClassNode(), parameters, _credentials, new JsonObject());

    private Task<List<JsonObject>> ReadAsync(string featureClass)
        => FakeNodeContext.RunAsync(new ReadFeaturesNode(), new { credentialId = "test", featureClass }, _credentials, new JsonObject());

    #region Copy Feature Class

    [Fact]
    public async Task Create_copies_schema_and_features()
    {
        var result = Assert.Single(await CopyAsync(new
        {
            sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "lines_copy", mode = "create"
        }));

        Assert.Equal(3, result["copied"]!.GetValue<int>());
        Assert.Equal("lines_copy", result["target"]!.GetValue<string>());
        var copy = await ReadAsync("lines_copy");
        Assert.Equal(["L-1", "L-2", "L-3"], copy.Select(f => f["properties"]!["line_id"]!.GetValue<string>()).Order());
        var l2 = copy.Single(f => f["properties"]!["line_id"]!.GetValue<string>() == "L-2");
        Assert.Equal(110, l2["properties"]!["voltage"]!.GetValue<long>());
        Assert.Equal("[[100,0],[110,0]]", l2["geometry"]!["coordinates"]!.ToJsonString());
    }

    [Fact]
    public async Task Create_stops_when_the_target_exists_and_append_adds_the_features()
    {
        var parameters = new { sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "lines_copy", mode = "create" };
        await CopyAsync(parameters);

        await Assert.ThrowsAsync<PipeException>(() => CopyAsync(parameters));
        await CopyAsync(new { sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "lines_copy", mode = "append" });

        Assert.Equal(6, (await ReadAsync("lines_copy")).Count);
    }

    [Fact]
    public async Task Replace_starts_the_target_anew()
    {
        var create = new { sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "lines_copy", mode = "create" };
        await CopyAsync(create);

        await CopyAsync(new { sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "lines_copy", mode = "replace" });

        Assert.Equal(3, (await ReadAsync("lines_copy")).Count);
    }

    [Fact]
    public async Task Where_and_spatial_filter_select_the_features_to_copy()
    {
        await CopyAsync(new
        {
            sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "high", mode = "create", where = "voltage > 200"
        });
        await CopyAsync(new
        {
            sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "west", mode = "create",
            spatialFilter = "intersects", filterGeometry = new[] { -1.0, -1.0, 50.0, 1.0 }
        });

        Assert.Equal(["L-1", "L-3"], (await ReadAsync("high")).Select(f => f["properties"]!["line_id"]!.GetValue<string>()).Order());
        Assert.Equal("L-1", Assert.Single(await ReadAsync("west"))["properties"]!["line_id"]!.GetValue<string>());
    }

    [Fact]
    public async Task Additional_and_excluded_fields_change_the_schema_of_the_copy()
    {
        await CopyAsync(new
        {
            sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "lines_copy", mode = "create",
            excludeFields = "note", additionalFields = new[] { new { name = "status", type = "text", size = 20 } }
        });

        var schema = Assert.Single(await FakeNodeContext.RunAsync(new FeatureClassNode(), new { credentialId = "test", operation = "describe", featureClass = "lines_copy" }, _credentials, new JsonObject()));
        Assert.Equal("line_id, voltage, status", string.Join(", ", schema["fields"]!.AsArray().Select(f => f!["name"]!.GetValue<string>())));
        var copy = await ReadAsync("lines_copy");
        Assert.Equal(3, copy.Count);
        Assert.All(copy, f => Assert.Null(f["properties"]!["status"]));
    }

    [Fact]
    public async Task The_target_needs_allow_schema_changes_and_is_not_the_source()
    {
        await Assert.ThrowsAsync<PipeException>(() => CopyAsync(new
        {
            sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "locked", targetFeatureClass = "lines_copy", mode = "create"
        }));
        await Assert.ThrowsAsync<PipeException>(() => CopyAsync(new
        {
            sourceCredentialId = "test", sourceFeatureClass = "lines", targetCredentialId = "test", targetFeatureClass = "lines", mode = "replace"
        }));
        Assert.Equal(3, (await ReadAsync("lines")).Count);   // the source is untouched
    }

    #endregion

    #region Feature Class: additional and excluded fields

    [Fact]
    public async Task Create_adds_and_leaves_out_fields_of_the_schema()
    {
        var schema = Assert.Single(await FakeNodeContext.RunAsync(new FeatureClassNode(), new { credentialId = "test", operation = "describe", featureClass = "lines" }, _credentials, new JsonObject()));

        var created = Assert.Single(await FakeNodeContext.RunAsync(new FeatureClassNode(),
            new
            {
                credentialId = "test", operation = "create", featureClass = "lines_ext", schema,
                excludeFields = "note", additionalFields = JsonNode.Parse("""[{"name":"status","type":"text","size":20},{"name":"checked","type":"date"}]""")
            }, _credentials, new JsonObject()));

        Assert.Equal(["line_id", "voltage", "status", "checked"], created["fields"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()));
    }

    [Fact]
    public void Additional_fields_must_be_new_and_excluded_fields_must_exist()
    {
        var schema = JsonNode.Parse("""{"name":"a","geometryType":"point","idField":"id","shapeField":"shape","fields":[{"name":"x","type":"text"}]}""")!.AsObject();

        Assert.Throws<PipeException>(() => GisSchema.Extend(schema, JsonNode.Parse("""[{"name":"X","type":"text"}]"""), ""));
        Assert.Throws<PipeException>(() => GisSchema.Extend(schema, JsonNode.Parse("""[{"name":"id","type":"integer"}]"""), ""));
        Assert.Throws<PipeException>(() => GisSchema.Extend(schema, null, "nofield"));
        Assert.Throws<PipeException>(() => GisSchema.Extend(schema, JsonNode.Parse("""[{"type":"text"}]"""), ""));
        // the schema itself stays as it is
        Assert.Equal(["x"], GisSchema.Extend(schema, null, "").AsObject()["fields"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()));
        Assert.Equal(["x", "y"], GisSchema.Extend(schema, JsonValue.Create("""[{"name":"y","type":"integer"}]"""), "")["fields"]!.AsArray().Select(f => f!["name"]!.GetValue<string>()));
        Assert.Single(schema["fields"]!.AsArray());
    }

    #endregion

    #region Choice list

    private sealed class FakeCredentialStore(Dictionary<string, object> credentials) : IPipeCredentialStore
    {
        public Task<IReadOnlyList<PipeCredentialInfo>> ListAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<PipeCredential?> GetAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult<PipeCredential?>(credentials.TryGetValue(id, out var data)
                ? new PipeCredential { Id = id, Name = id, Data = JsonSerializer.SerializeToNode(data, PipeJson.Options)!.AsObject() }
                : null);

        public Task SaveAsync(PipeCredential credential, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteAsync(string id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeServices(object store) : IServiceProvider
    {
        public object? GetService(Type serviceType) => serviceType == typeof(IPipeCredentialStore) ? store : null;
    }

    [Fact]
    public async Task The_editor_lists_the_feature_classes_of_the_selected_data_source()
    {
        var services = new FakeServices(new FakeCredentialStore(_credentials));

        var read = await new ReadFeaturesNode(services).GetOptionsAsync("featureClass", new JsonObject { ["credentialId"] = "test" }, false, CancellationToken.None);
        var write = await new WriteFeaturesNode(services).GetOptionsAsync("featureClass", new JsonObject { ["credentialId"] = "test" }, false, CancellationToken.None);
        var copy = await new CopyFeatureClassNode(services).GetOptionsAsync("sourceFeatureClass", new JsonObject { ["sourceCredentialId"] = "test" }, false, CancellationToken.None);

        Assert.Equal(["lines"], read.Select(o => o.Value));
        Assert.Equal(["lines"], write.Select(o => o.Value));
        Assert.Equal(["lines"], copy.Select(o => o.Value));
    }

    [Fact]
    public async Task The_host_creates_the_nodes_with_the_services_it_has()
    {
        // what the node registry does for the editor
        var services = new ServiceCollection().AddSingleton<IPipeCredentialStore>(new FakeCredentialStore(_credentials)).BuildServiceProvider();

        var node = (ReadFeaturesNode)ActivatorUtilities.CreateInstance(services, typeof(ReadFeaturesNode));

        Assert.Equal(["lines"], (await node.GetOptionsAsync("featureClass", new JsonObject { ["credentialId"] = "test" }, false, CancellationToken.None)).Select(o => o.Value));
        Assert.IsType<WriteFeaturesNode>(ActivatorUtilities.CreateInstance(services, typeof(WriteFeaturesNode)));
        Assert.IsType<CopyFeatureClassNode>(ActivatorUtilities.CreateInstance(services, typeof(CopyFeatureClassNode)));
    }

    [Fact]
    public async Task Without_a_usable_data_source_the_list_is_empty()
    {
        var services = new FakeServices(new FakeCredentialStore(_credentials));
        var node = new ReadFeaturesNode(services);

        Assert.Empty(await node.GetOptionsAsync("featureClass", new JsonObject(), false, CancellationToken.None));
        Assert.Empty(await node.GetOptionsAsync("featureClass", new JsonObject { ["credentialId"] = "unknown" }, false, CancellationToken.None));
        Assert.Empty(await node.GetOptionsAsync("featureClass", new JsonObject { ["credentialId"] = "{{ $json.credential }}" }, false, CancellationToken.None));
        Assert.Empty(await node.GetOptionsAsync("where", new JsonObject { ["credentialId"] = "test" }, false, CancellationToken.None));
        Assert.Empty(await new ReadFeaturesNode().GetOptionsAsync("featureClass", new JsonObject { ["credentialId"] = "test" }, false, CancellationToken.None));
    }

    #endregion
}
