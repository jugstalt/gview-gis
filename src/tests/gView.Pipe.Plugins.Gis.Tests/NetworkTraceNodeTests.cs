using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using gView.Pipe.Abstraction;
using SpatialNET;
using SpatialNET.Network;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>
/// Network Trace on a small built network: Trafo Knoten#2 with the connectors Anlagenverbinder#1 (to Knoten#4) and
/// Anlagenverbinder#2 (to Knoten#5), Trafo Knoten#3 with Anlagenverbinder#3 (to Knoten#8). Lines: Leitung#1 4-6,
/// Leitung#2 5-7, Leitung#4 in two parts 6-9-10 and, with the ring closed, Leitung#3 8-7.
/// </summary>
public sealed class NetworkTraceNodeTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-pipe-network-tests-" + Guid.NewGuid().ToString("N"));
    private const string CredentialId = "net";

    private const int Knoten = 0, Kabel = 1, Anlagenverbinder = 2, Leitung = 3;
    private const int Quelle = 10, Trafo = 12, Kasten = 13, MsKabel = 20, NsVerbinder = 22, NsLeitung = 23;

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    private void Build(bool closedRing = false, bool ringSwitchedOff = false)
    {
        var metadata = new NetworkMetadata
        {
            Name = "lv",
            Srs = "epsg:31256",
            Facets =
            [
                new NetworkFacetMetadata { Name = "Ebene", Values = new() { [1] = "Mittelspannung", [2] = "Niederspannung" } },
                new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Kabel", [2] = "Quelle", [3] = "Kasten", [4] = "Trafo", [5] = "Anlagenverbinder", [6] = "Leitung" } },
                new NetworkFacetMetadata { Name = "Status", Values = new() { [1] = "Betrieb" } },
            ],
            Types =
            [
                new NetworkTypeMetadata { Id = Quelle, Kind = ElementKind.Node, Facets = [1, 2, 1], Used = true },
                new NetworkTypeMetadata { Id = Trafo, Kind = ElementKind.Node, Facets = [1, 4, 1], Used = true },
                new NetworkTypeMetadata { Id = Kasten, Kind = ElementKind.Node, Facets = [2, 3, 1], Used = true },
                new NetworkTypeMetadata { Id = MsKabel, Kind = ElementKind.Edge, Facets = [1, 1, 1], Used = true },
                new NetworkTypeMetadata { Id = NsVerbinder, Kind = ElementKind.Edge, Facets = [2, 5, 1], Used = true },
                new NetworkTypeMetadata { Id = NsLeitung, Kind = ElementKind.Edge, Facets = [2, 6, 1], Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = Knoten, Layer = "Knoten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = Kabel, Layer = "Kabel", Kind = ElementKind.Edge },
                new NetworkFeatureClassMetadata { Index = Anlagenverbinder, Layer = "Anlagenverbinder", Kind = ElementKind.Edge },
                new NetworkFeatureClassMetadata { Index = Leitung, Layer = "Leitung", Kind = ElementKind.Edge },
            ],
            Tiers =
            [
                new NetworkTierConfig { Name = "Mittelspannung", Order = 1, Sources = "[*,Quelle,*]" },
                new NetworkTierConfig { Name = "Niederspannung", Order = 2, Sources = "[*,Trafo,*]" },
            ],
        };

        using var writer = new FileNetworkStore(_directory).CreateWriter();
        (int Oid, int Type)[] nodes = [(1, Quelle), (2, Trafo), (3, Trafo), (4, Kasten), (5, Kasten), (6, Kasten), (7, Kasten), (8, Kasten), (9, Kasten), (10, Kasten)];
        foreach (var (oid, type) in nodes)
        {
            var feature = new FeatureRef(Knoten, oid);
            writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(NetworkIds.NodeId(feature), type, new Coordinate(oid * 10, 0)), 1, false, feature));
        }

        void Edge(int featureClass, int oid, int from, int to, int type, double length, int part = 0, bool disabled = false)
        {
            var edge = new GraphEdge<long, long>(NetworkIds.Compose(featureClass, oid, part), NetworkIds.NodeId(new FeatureRef(Knoten, from)),
                NetworkIds.NodeId(new FeatureRef(Knoten, to)), type, length, EdgeDirection.Both, disabled, Array.Empty<EdgeWeight>());
            writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(edge, new FeatureRef(featureClass, oid), 0, length, 1, 1));
        }

        Edge(Kabel, 1, 1, 2, MsKabel, 100);
        Edge(Anlagenverbinder, 1, 2, 4, NsVerbinder, 1);
        Edge(Anlagenverbinder, 2, 2, 5, NsVerbinder, 2);
        Edge(Anlagenverbinder, 3, 3, 8, NsVerbinder, 1);
        Edge(Leitung, 1, 4, 6, NsLeitung, 10);
        Edge(Leitung, 2, 5, 7, NsLeitung, 5);
        Edge(Leitung, 4, 6, 9, NsLeitung, 3, part: 0);
        Edge(Leitung, 4, 9, 10, NsLeitung, 4, part: 1);
        if (closedRing || ringSwitchedOff)
        {
            Edge(Leitung, 3, 8, 7, NsLeitung, 5, disabled: ringSwitchedOff);
        }
        writer.Commit(metadata);
    }

    private Task<List<JsonObject>> RunAsync(object parameters, params JsonObject[] input)
        => FakeNodeContext.RunAsync(new NetworkTraceNode(), parameters,
            new Dictionary<string, object> { [CredentialId] = new GisNetworkCredential { Directory = _directory } }, input);

    static private JsonObject Item(object id) => new() { ["type"] = "Feature", ["id"] = JsonValue.Create(id), ["geometry"] = null, ["properties"] = new JsonObject() };

    static private JsonNode P(JsonObject item, string name) => item["properties"]![name]!;

    private static string Key(JsonObject item) => $"{P(item, "layer").GetValue<string>()}#{item["id"]!.GetValue<int>()}";

    private static Dictionary<string, JsonObject> ById(List<JsonObject> items) => items.ToDictionary(Key);

    [Fact]
    public async Task Feeders_label_every_line_with_transformer_and_connector()
    {
        Build();
        var items = ById(await RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[Mittelspannung,Trafo,*]", branches = "[Niederspannung,Anlagenverbinder,*]", tier = "Niederspannung" }));

        // the connectors themselves are no items unless Output types asks for them
        Assert.Equal(["Leitung#1", "Leitung#2", "Leitung#4"], items.Keys.Order());

        var line = items["Leitung#1"];
        Assert.Equal("Leitung", P(line, "layer").GetValue<string>());
        Assert.Equal(1, P(line, "oid").GetValue<int>());
        Assert.Equal("Knoten#2", P(line, "source").GetValue<string>());
        Assert.Equal(2, P(line, "sourceOid").GetValue<long>());
        Assert.Equal("Anlagenverbinder#1", P(line, "branch").GetValue<string>());
        Assert.Equal(1, P(line, "branchOid").GetValue<long>());
        Assert.Equal(1, P(line, "cost").GetValue<double>());
        Assert.Equal("Edge", P(line, "kind").GetValue<string>());
        Assert.Null(line["geometry"]);
        Assert.Equal(31256, line["srs"]!.GetValue<int>());

        Assert.Equal(2, P(items["Leitung#2"], "branchOid").GetValue<long>());
    }

    [Fact]
    public async Task Feeders_group_the_parts_of_a_line()
    {
        Build();
        var grouped = ById(await RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]", branches = "[*,Anlagenverbinder,*]", tier = "Niederspannung" }));
        var split = await RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]", branches = "[*,Anlagenverbinder,*]", tier = "Niederspannung", groupParts = false });

        Assert.Equal(2, P(grouped["Leitung#4"], "parts").GetValue<int>());
        Assert.Equal(7, P(grouped["Leitung#4"], "length").GetValue<double>());
        Assert.Equal(11, P(grouped["Leitung#4"], "cost").GetValue<double>());   // 1 (connector) + 10 (Leitung#1)

        var parts = split.Where(i => P(i, "oid").GetValue<int>() == 4).OrderBy(i => P(i, "part").GetValue<int>()).ToList();
        Assert.Equal([0, 1], parts.Select(p => P(p, "part").GetValue<int>()));
        Assert.Equal(4, parts[1]["id"]!.GetValue<int>());
    }

    [Fact]
    public async Task Feeders_mark_a_closed_ring_as_multiple()
    {
        Build(closedRing: true);
        var items = ById(await RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]", branches = "[*,Anlagenverbinder,*]", tier = "Niederspannung", outputTypes = "[*,Leitung,*]" }));

        var ring = items["Leitung#2"];
        Assert.Equal("Multiple", P(ring, "kind").GetValue<string>());
        Assert.Equal("Anlagenverbinder#2 Anlagenverbinder#3", P(ring, "branch").GetValue<string>());
        Assert.Equal(2, P(ring, "branchOid").GetValue<long>());   // the cheapest
        Assert.Equal("Edge", P(items["Leitung#1"], "kind").GetValue<string>());
    }

    [Fact]
    public async Task Feeders_take_the_sources_from_the_items()
    {
        Build();
        var items = await RunAsync(new { credentialId = CredentialId, operation = "feedersOfItems", node = "Knoten#3", branches = "[*,Anlagenverbinder,*]", tier = "Niederspannung", outputTypes = "[*,Anlagenverbinder,*]" }, Item(3));

        var connector = Assert.Single(items);
        Assert.Equal("Anlagenverbinder#3", Key(connector));
        Assert.Equal(3, P(connector, "sourceOid").GetValue<long>());
        Assert.Equal(0, P(connector, "cost").GetValue<double>());
    }

    // the fake context doesn't evaluate {{ }}: Node is given as it would be after evaluating Knoten#{{ $json.id }}
    private Task<List<JsonObject>> TraceAsync(string node, params JsonObject[] input)
        => RunAsync(new { credentialId = CredentialId, operation = "trace", node, tier = "Niederspannung", outputTypes = "[Niederspannung,Leitung,*]" }, input);

    [Fact]
    public async Task Trace_follows_the_network_from_the_node_of_the_item()
    {
        Build();
        var fromBox = await TraceAsync("Knoten#4", Item(4));
        var other = await TraceAsync("Knoten#5", Item(5));

        // from Knoten#4 the trace stops at the transformer (a source of the tier)
        Assert.Equal(["Leitung#1", "Leitung#4"], fromBox.Select(Key).Order());
        Assert.All(fromBox, i => Assert.Equal("Knoten#4", P(i, "start").GetValue<string>()));
        Assert.Equal(10, P(fromBox.Single(i => Key(i) == "Leitung#4"), "cost").GetValue<double>());
        Assert.Equal(["Leitung#2"], other.Select(Key));
    }

    [Fact]
    public async Task Trace_skips_items_whose_node_is_not_in_the_network()
    {
        Build();
        Assert.Empty(await TraceAsync("Knoten#99", Item(99)));
    }

    [Fact]
    public async Task Without_Node_the_input_items_are_an_error()
    {
        Build();
        var error = await Assert.ThrowsAsync<PipeException>(() => TraceAsync("", Item(4)));

        Assert.Contains("Set Node", error.Message);
    }

    [Fact]
    public async Task Feeders_add_colour_fields()
    {
        Build();
        var all = ById(await RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]", branches = "[*,Anlagenverbinder,*]", tier = "Niederspannung",
            colorFields = true, colors = 2, sourceColorField = "STAT_COLOR", branchColorField = "ABZW_COLOR" }));
        var only = await RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]", branches = "[*,Anlagenverbinder,*]", tier = "Niederspannung",
            colorFields = true, colors = 2, sourceColorField = "STAT_COLOR", branchColorField = "ABZW_COLOR", colorFieldsOnly = true });

        // Leitung#1: Trafo Knoten#2, Anlagenverbinder#1
        Assert.Equal(0, P(all["Leitung#1"], "STAT_COLOR").GetValue<long>());
        Assert.Equal(1, P(all["Leitung#1"], "ABZW_COLOR").GetValue<long>());
        Assert.Equal("Knoten#2", P(all["Leitung#1"], "source").GetValue<string>());

        var line = only.Single(i => i["id"]!.GetValue<int>() == 1);
        Assert.Equal(["ABZW_COLOR", "STAT_COLOR"], line["properties"]!.AsObject().Select(p => p.Key).Order());
    }

    [Fact]
    public async Task Open_lines_get_the_open_colour()
    {
        Build(ringSwitchedOff: true);
        object Parameters(bool openLines) => new
        {
            credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]", branches = "[*,Anlagenverbinder,*]", tier = "Niederspannung",
            outputTypes = "[*,Leitung,*]", openLines, colorFields = true, colors = 100, sourceColorField = "STAT_COLOR", branchColorField = "ABZW_COLOR"
        };

        Assert.DoesNotContain("Leitung#3", ById(await RunAsync(Parameters(openLines: false))).Keys);
        var items = ById(await RunAsync(Parameters(openLines: true)));

        var open = items["Leitung#3"];
        Assert.Equal("Open", P(open, "kind").GetValue<string>());
        Assert.Equal("Anlagenverbinder#3 Anlagenverbinder#2", P(open, "branch").GetValue<string>());
        Assert.Equal(998, P(open, "STAT_COLOR").GetValue<int>());
        Assert.Equal(998, P(open, "ABZW_COLOR").GetValue<int>());
        Assert.Equal(2, P(items["Leitung#2"], "ABZW_COLOR").GetValue<long>());
    }

    [Fact]
    public async Task Too_many_items_fail_instead_of_cutting()
    {
        Build();
        var error = await Assert.ThrowsAsync<PipeException>(() => RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]", tier = "Niederspannung", maxItems = 2 }));

        Assert.Contains("Max items", error.Message);
    }

    [Fact]
    public async Task A_folder_without_a_built_network_is_an_error()
    {
        var error = await Assert.ThrowsAsync<PipeException>(() => RunAsync(new { credentialId = CredentialId, operation = "feeders", sources = "[*,Trafo,*]" }));

        Assert.Contains("network.json", error.Message);
    }
}
