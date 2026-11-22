using Xunit;
using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using SpatialNET;
using SpatialNET.Network;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Tests;

/// <summary>Tiers (Ebenen): Quelle#1 -MS- Kasten#2 -MS- Trafo#3 -NS- Kasten#4 -NS- Kunde#5.</summary>
public class NetworkTierTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-tier-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        foreach (var dir in new[] { _directory, _directory + ".old", _directory + ".building" })
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    private const int NodeClass = 0, EdgeClass = 1;
    private const int MsKabel = 20, NsKabel = 21;

    private static List<NetworkTierConfig> Tiers() =>
    [
        new() { Name = "MS", Order = 1, Sources = "[*,Quelle]", Sinks = "[*,Trafo]" },
        new() { Name = "NS", Order = 2, Sources = "[*,Trafo]", Sinks = "[*,Kunde]" },
    ];

    private void Build(List<NetworkTierConfig>? tiers = null, List<NetworkRuleConfig>? rules = null)
    {
        var metadata = new NetworkMetadata
        {
            Name = "tiers",
            Facets =
            [
                new NetworkFacetMetadata { Name = "Ebene", Values = new() { [1] = "MS", [2] = "NS" } },
                new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Kabel", [2] = "Quelle", [3] = "Kasten", [4] = "Trafo", [5] = "Kunde" } },
            ],
            Types =
            [
                new NetworkTypeMetadata { Id = 10, Kind = ElementKind.Node, Facets = [1, 2], Used = true },
                new NetworkTypeMetadata { Id = 11, Kind = ElementKind.Node, Facets = [1, 3], Used = true },
                new NetworkTypeMetadata { Id = 12, Kind = ElementKind.Node, Facets = [1, 4], Used = true },
                new NetworkTypeMetadata { Id = 13, Kind = ElementKind.Node, Facets = [2, 3], Used = true },
                new NetworkTypeMetadata { Id = 14, Kind = ElementKind.Node, Facets = [2, 5], Used = true },
                new NetworkTypeMetadata { Id = MsKabel, Kind = ElementKind.Edge, Facets = [1, 1], Used = true },
                new NetworkTypeMetadata { Id = NsKabel, Kind = ElementKind.Edge, Facets = [2, 1], Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = NodeClass, Layer = "Knoten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = EdgeClass, Layer = "Kabel", Kind = ElementKind.Edge },
            ],
            SupplySources = "[*,Quelle]",
            Customers = "[*,Kunde]",
            Tiers = tiers ?? Tiers(),
            Rules = rules ?? [],
        };

        using var writer = new FileNetworkStore(_directory).CreateWriter();
        int[] nodeTypes = [10, 11, 12, 13, 14];
        for (int i = 0; i < nodeTypes.Length; i++)
        {
            var feature = new FeatureRef(NodeClass, i + 1);
            writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(NetworkIds.NodeId(feature), nodeTypes[i], new Coordinate(i * 10, 0)), 1, false, feature));
        }
        for (int i = 0; i < 4; i++)
        {
            var feature = new FeatureRef(EdgeClass, i + 1);
            var edge = new GraphEdge<long, long>(NetworkIds.Compose(EdgeClass, i + 1), NetworkIds.NodeId(new FeatureRef(NodeClass, i + 1)),
                NetworkIds.NodeId(new FeatureRef(NodeClass, i + 2)), i < 2 ? MsKabel : NsKabel, 10, EdgeDirection.Both, false, Array.Empty<EdgeWeight>());
            writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(edge, feature, 0, 10, 1, 1));
        }
        writer.Commit(metadata);
    }

    private NetworkQueryService Service() => new(new FileNetworkGraph(_directory));

    private static HashSet<string> Labels(NetworkQueryResult result) => result.Nodes.Select(n => n.Label).ToHashSet();

    [Fact]
    public void Trace_WithTier_StaysInTheTier()
    {
        Build();
        var all = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3" });
        Assert.Equal(5, Labels(all).Count);

        var ns = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "NS" });
        Assert.Equal(["Knoten#3", "Knoten#4", "Knoten#5"], Labels(ns).Order());
        Assert.All(ns.Edges, e => Assert.Equal("Kabel", e.Layer));
        Assert.Equal(2, ns.Edges.Count);
    }

    [Fact]
    public void Trace_WithTier_StopsAtTheSourcesOfTheTier_UnlessTurnedOff()
    {
        Build();
        // from Kasten#2 (MS) towards the Quelle#1 and the Trafo#3: only the Quelle is a source of MS
        var stopping = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "MS" });
        // Trafo#3 is the start; Quelle#1 is a source and stops the trace, the start is never a stop
        Assert.Equal("Source", stopping.Nodes.Single(n => n.Label == "Knoten#1").Kind);

        var through = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "MS", StopAtSources = false });
        Assert.Equal("Reached", through.Nodes.Single(n => n.Label == "Knoten#1").Kind);
    }

    [Fact]
    public void Trace_WithATierWithoutSources_DoesNotStopAtTheGlobalSupplySources()
    {
        Build([new NetworkTierConfig { Name = "MS", Order = 1 }]);
        // the global supply source [*,Quelle] (Knoten#1) is no stop of a tier that names no sources
        var result = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "MS" });
        Assert.Equal("Reached", result.Nodes.Single(n => n.Label == "Knoten#1").Kind);
    }

    [Fact]
    public void Supply_UsesSourcesOfTheTopAndSinksOfTheBottomTier()
    {
        Build();
        // NS: Trafo -> Kunde
        var ns = Service().Run("supply", new NetworkQueryRequest { Tier = "NS" });
        var customer = Assert.Single(ns.Nodes);
        Assert.Equal("Knoten#5", customer.Label);
        Assert.Equal("Supplied", customer.Kind);
        Assert.Equal("Knoten#3", customer.Source);

        // MS: Quelle -> Trafo (the tier's sinks replace the customers)
        var ms = Service().Run("supply", new NetworkQueryRequest { Tier = "MS" });
        Assert.Equal("Knoten#3", Assert.Single(ms.Nodes).Label);

        // both: sources of MS (top), sinks of NS (bottom)
        var both = Service().Run("supply", new NetworkQueryRequest { Tier = "NS,MS" });
        var node = Assert.Single(both.Nodes);
        Assert.Equal("Knoten#5", node.Label);
        Assert.Equal("Knoten#1", node.Source);
    }

    [Fact]
    public void Supply_ExplicitSourcesWin_AndATierWithoutSourcesFallsBackToTheGlobalOnes()
    {
        Build([new NetworkTierConfig { Name = "NS", Order = 2 }]);
        // no sources/sinks in the tier: the global [*,Quelle] / [*,Kunde]; the Quelle is outside the tier's edges
        var global = Service().Run("supply", new NetworkQueryRequest { Tier = "NS" });
        Assert.Equal("NotSupplied", Assert.Single(global.Nodes).Kind);

        var explicitSources = Service().Run("supply", new NetworkQueryRequest { Tier = "NS", Sources = "Knoten#3" });
        Assert.Equal("Supplied", Assert.Single(explicitSources.Nodes).Kind);
    }

    [Fact]
    public void TierAndFilter_AreIntersected_AndTheBlockOfTheTierIsAdded()
    {
        Build([new NetworkTierConfig { Name = "NS", Order = 2, Block = "Knoten#4" }]);
        var blocked = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "NS", StopAtSources = false });
        Assert.Equal("Blocked", blocked.Nodes.Single(n => n.Label == "Knoten#4").Kind);
        Assert.DoesNotContain("Knoten#5", Labels(blocked));

        Build([new NetworkTierConfig { Name = "NS", Order = 2 }]);
        var noCommon = Assert.Throws<ArgumentException>(() => Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "NS", Filter = "MS.*" }));
        Assert.Contains("no edge type in common", noCommon.Message);
    }

    [Fact]
    public void UnknownTier_IsAnError()
    {
        Build();
        var ex = Assert.Throws<ArgumentException>(() => Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "HS" }));
        Assert.Contains("MS, NS", ex.Message);
    }

    [Fact]
    public void Trace_ListsNodesOfRareTypesWithLocation()
    {
        Build();
        var result = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3" });
        // every type has one node here: each is listed with its label and where it lies
        Assert.Contains(result.Messages, m => m.Contains("Knoten#4  at 30, 0"));
    }

    [Fact]
    public void Trace_ANodeThatIsBlockedAnyway_IsNotReportedAsSource()
    {
        Build([new NetworkTierConfig { Name = "MS", Order = 1, Sources = "[*,Quelle]" }]);
        var stopped = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "MS" });
        Assert.Equal("Source", stopped.Nodes.Single(n => n.Label == "Knoten#1").Kind);

        // the same node is also blocked by the request: it is a block first
        var blocked = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "MS", Block = "Knoten#1" });
        Assert.Equal("Blocked", blocked.Nodes.Single(n => n.Label == "Knoten#1").Kind);
    }

    [Fact]
    public void Validate_RuleTierAndQueryTierWithoutCommonEdges_SkipsTheRuleAndSaysSo()
    {
        var rule = new NetworkRuleConfig { Id = "R", Subject = RuleSubject.Node, SubjectTypes = "[*,Kasten]", CountTypes = "[*,Kabel]", Min = 3, Tier = "NS" };
        Build(rules: [rule]);
        var result = Service().Run("validate", new NetworkQueryRequest { Tier = "MS" });
        Assert.Empty(result.Violations);
        Assert.Empty(result.Rules);
        Assert.Contains(result.Messages, m => m.Contains("Rule 'R' skipped"));
    }

    [Fact]
    public void RestrictToTier_ReturnsNullWhenTheTierHasNoneOfTheCountedEdges()
    {
        Build();
        var graph = new FileNetworkGraph(_directory);
        var rule = new NetworkRuleConfig { Id = "R", Subject = RuleSubject.Edge, SubjectTypes = "NS.*", CountTypes = "MS.*", Max = 1 };
        var resolved = rule.Resolve(graph.Types);
        var ms = graph.Tiers.Single(t => t.Name == "MS");
        var ns = graph.Tiers.Single(t => t.Name == "NS");
        Assert.Null(rule.RestrictToTier(resolved, ms)); // subject NS.* has no MS edge
        Assert.Null(rule.RestrictToTier(resolved, ns)); // counts MS.*: no NS edge
    }

    [Fact]
    public void RefreshMetadata_RejectsRulesWithInvalidPatterns_AndLeavesTheFileAlone()
    {
        Build(tiers: []);
        var path = Path.Combine(_directory, FileNetworkStore.MetadataFile);
        var before = File.ReadAllText(path);
        var config = new NetworkConfig
        {
            Tiers = Tiers(),
            Rules = [new NetworkRuleConfig { Id = "Kaputt", Subject = RuleSubject.Node, SubjectTypes = "[*,Gibtsnicht]", CountTypes = "[*,Kabel]", Min = 1 }]
        };
        var ex = Assert.Throws<InvalidDataException>(() => NetworkMetadataRefresh.Refresh(config, _directory));
        Assert.Contains("Kaputt", ex.Message);
        Assert.Equal(before, File.ReadAllText(path));

        config.Rules[0].SubjectTypes = "[*,Kasten]";
        config.Rules[0].Tier = "Unbekannt";
        Assert.Throws<InvalidDataException>(() => NetworkMetadataRefresh.Refresh(config, _directory));
    }

    [Fact]
    public void Validate_RuleTier_AndQueryTier_RestrictTheRule()
    {
        // every Kasten needs at least 3 edges: both Kasten fail on the whole network
        var rule = new NetworkRuleConfig { Id = "R", Subject = RuleSubject.Node, SubjectTypes = "[*,Kasten]", CountTypes = "[*,Kabel]", Min = 3 };
        Build(rules: [rule]);
        var whole = Service().Run("validate", new NetworkQueryRequest());
        Assert.Equal(2, whole.Violations.Count);

        // asked for the MS tier: only Kasten#2 is in it
        var queryTier = Service().Run("validate", new NetworkQueryRequest { Tier = "MS" });
        var violation = Assert.Single(queryTier.Violations);
        Assert.Equal("Knoten#2", violation.NodeLabel);
        Assert.Equal(1, queryTier.Rules.Single().Checked);

        // the rule itself belongs to the NS tier
        rule.Tier = "NS";
        Build(rules: [rule]);
        var ruleTier = Service().Run("validate", new NetworkQueryRequest());
        Assert.Equal("Knoten#4", Assert.Single(ruleTier.Violations).NodeLabel);
    }

    [Fact]
    public void Validate_EdgeRuleOfATier_OnlyLooksAtTheEdgesOfTheTier()
    {
        var rule = new NetworkRuleConfig
        {
            Id = "E", Subject = RuleSubject.Edge, SubjectTypes = "*", CountTypes = "*", Min = 5, Tier = "NS"
        };
        Build(rules: [rule]);
        var result = Service().Run("validate", new NetworkQueryRequest());
        Assert.Equal(2, result.Rules.Single().Checked); // the two NS cables
    }

    [Fact]
    public void RefreshMetadata_WritesTiersWithoutTouchingTheShards_AndChecksThePatterns()
    {
        Build(tiers: []);
        var stamp = File.GetLastWriteTimeUtc(Path.Combine(_directory, "shards", "1.bin"));
        File.SetLastWriteTimeUtc(Path.Combine(_directory, FileNetworkStore.MetadataFile), DateTime.UtcNow.AddMinutes(-5));

        var cache = new NetworkGraphCache();
        var before = cache.Get(_directory);
        Assert.Empty(before.Metadata.Tiers);

        var config = new NetworkConfig { SupplySources = "[*,Trafo]", Customers = "[*,Kunde]", Tiers = Tiers() };
        NetworkMetadataRefresh.Refresh(config, _directory);

        var after = cache.Get(_directory);
        Assert.NotSame(before, after);
        Assert.Equal(["MS", "NS"], after.Metadata.Tiers.Select(t => t.Name));
        Assert.Equal("[*,Trafo]", after.Metadata.SupplySources);
        Assert.Equal(4, after.Metadata.EdgeCount);
        Assert.Equal(stamp, File.GetLastWriteTimeUtc(Path.Combine(_directory, "shards", "1.bin")));

        // a tier is a value of the first facet
        var invalid = new NetworkConfig { Tiers = [new NetworkTierConfig { Name = "X" }] };
        var ex = Assert.Throws<InvalidDataException>(() => NetworkMetadataRefresh.Refresh(invalid, _directory));
        Assert.Contains("Tier 'X'", ex.Message);
    }

    [Fact]
    public void Tiers_AreTheValuesOfTheFirstFacet_EntriesOnlyAddSettings()
    {
        // no entries: MS and NS come from the facet, ranked by their number
        Build(tiers: []);
        var graph = new FileNetworkGraph(_directory);
        Assert.Equal(["MS", "NS"], graph.Tiers.Select(t => t.Name));
        Assert.Equal([1, 2], graph.Tiers.Select(t => t.Rank));
        var ns = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#3", Tier = "NS" });
        Assert.Equal(["Knoten#3", "Knoten#4", "Knoten#5"], Labels(ns).Order());

        // an entry gives NS the order 0: it comes first
        Build([new NetworkTierConfig { Name = "ns", Order = 0, Sinks = "[*,Kunde]" }]);
        graph = new FileNetworkGraph(_directory);
        Assert.Equal(["ns", "MS"], graph.Tiers.Select(t => t.Name));
        Assert.Equal([13, 14], graph.Tiers[0].NodeTypes.TypeIds);
    }

    [Fact]
    public void Config_RoundTripsTiers_AndRejectsUnknownRuleTiersAndDuplicateNames()
    {
        var config = new NetworkConfig { Tiers = Tiers(), Rules = [new NetworkRuleConfig { Id = "r", Tier = "MS" }] };
        var back = JsonSerializer.Deserialize<NetworkConfig>(config.ToJson(), NetworkConfig.JsonOptions)!;
        Assert.Equal(["MS", "NS"], back.Tiers.Select(t => t.Name));
        Assert.Equal("[*,Trafo]", back.Tiers[1].Sources);
        Assert.Equal("MS", back.Rules[0].Tier);
        back.ValidateTiersAndRules();

        // unknown tiers of rules need the types of a network
        Build();
        var types = new FileNetworkGraph(_directory).Types;
        Assert.Equal(["MS", "NS"], back.ResolveTiers(types).Select(t => t.Name));
        back.Rules[0].Tier = "HS";
        Assert.Throws<InvalidDataException>(() => back.ResolveTiers(types));
        back.Rules[0].Tier = null;
        back.Tiers.Add(new NetworkTierConfig { Name = "ms" });
        Assert.Throws<InvalidDataException>(() => back.ValidateTiersAndRules());
    }
}
