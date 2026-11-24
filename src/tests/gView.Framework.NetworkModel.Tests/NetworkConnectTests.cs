using Xunit;
using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using gView.Framework.NetworkModel.Types;
using SpatialNET;
using SpatialNET.Imp;
using SpatialNET.Network;
using SpatialNET.Network.Building;

namespace gView.Framework.NetworkModel.Tests;

/// <summary>
/// Separate tiers and connect rules on build (SpatialNET ADR 0055): an NS joint on a vertex of an MS cable, a transformer
/// between MS and NS, and the ends of an MS and an NS cable at one place.
/// </summary>
public class NetworkConnectTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-connect-tests-" + Guid.NewGuid().ToString("N"));

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

    private static List<FacetConfig> Facets() =>
    [
        new FacetConfig { Name = "Ebene", Values = new() { [1] = "MS", [2] = "NS" } },
        new FacetConfig { Name = "Objektart" },
    ];

    private static NetworkConfig Config(bool separate = true, params ConnectRuleConfig[] rules) => new()
    {
        Tolerance = 0.01,
        Facets = Facets(),
        SeparateTiers = separate,
        ConnectRules = rules.ToList(),
        Tiers = [new NetworkTierConfig { Name = "NS", Sources = "[MS,Trafo]" }],
    };

    private static IPolyline2D Line(params (double X, double Y)[] points) =>
        ImpGeometryFactory.Instance.CreatePolyline([points.Select(p => new Coordinate(p.X, p.Y)).ToArray()], null);

    /// <summary>
    /// Knoten#1 NS joint at 50,0 on Leitung#1 (MS, 0,0 - 50,0 - 100,0); Knoten#2 MS transformer at 100,0;
    /// Leitung#2 NS from the joint, Leitung#3 NS from the transformer, Leitung#4 NS from 0,0 where Leitung#1 ends.
    /// </summary>
    private BuildResult Build(NetworkConfig config)
    {
        var catalog = new TypeCatalog(config.Facets, null);
        int msLine = catalog.Resolve(ElementKind.Edge, ["MS", "Leitung"]);
        int nsLine = catalog.Resolve(ElementKind.Edge, ["NS", "Leitung"]);
        int nsJoint = catalog.Resolve(ElementKind.Node, ["NS", "Muffe"]);
        int trafo = catalog.Resolve(ElementKind.Node, ["MS", "Trafo"]);

        NetworkConnectRules? rules = null;
        var options = NetworkConnectRules.Attach(new NetworkBuildOptions { Tolerance = config.Tolerance }, config, () => rules);
        var builder = new NetworkBuilder<long, long, FeatureRef>(options, new NetworkIds(config.Tolerance));
        void Node(long oid, double x, double y, int type)
        {
            var source = new FeatureRef(NodeClass, oid);
            builder.AddNode(source, NetworkIds.NodeId(source), new Coordinate(x, y), type);
        }

        Node(1, 50, 0, nsJoint);
        Node(2, 100, 0, trafo);
        builder.AddLine(new FeatureRef(EdgeClass, 1), Line((0, 0), (50, 0), (100, 0)), msLine);
        builder.AddLine(new FeatureRef(EdgeClass, 2), Line((50, 0), (50, 40)), nsLine);
        builder.AddLine(new FeatureRef(EdgeClass, 3), Line((100, 0), (150, 0)), nsLine);
        builder.AddLine(new FeatureRef(EdgeClass, 4), Line((0, 0), (0, 40)), nsLine);

        if (NetworkConnectRules.Needed(config))
        {
            rules = NetworkConnectRules.Create(config, catalog);
        }
        var metadata = new NetworkMetadata
        {
            Name = "connect",
            Tolerance = config.Tolerance,
            Facets = catalog.FacetMetadata().ToList(),
            Types = catalog.TypeMetadata().ToList(),
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = NodeClass, Layer = "Knoten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = EdgeClass, Layer = "Leitung", Kind = ElementKind.Edge },
            ],
            Tiers = config.Tiers,
        };
        var registry = catalog.ToRegistry();
        config.ResolveTiers(registry);
        rules?.Resolve(config, registry);

        using var writer = new FileNetworkStore(_directory).CreateWriter();
        var result = builder.Build(writer);
        writer.Commit(metadata);
        return result;
    }

    private NetworkQueryService Service() => new(new FileNetworkGraph(_directory));

    private static HashSet<string> Labels(NetworkQueryResult result) => result.Nodes.Select(n => n.Label).ToHashSet();

    [Fact]
    public void WithoutSeparateTiers_TheJointSplitsTheMsCable()
    {
        Build(Config(separate: false));
        var trace = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#2", Tier = "MS" });
        Assert.Contains("Knoten#1", Labels(trace));
    }

    [Fact]
    public void SeparateTiers_TheJointOfNsDoesNotSplitTheMsCable_AndTheTransformerJoinsBoth()
    {
        Build(Config());

        var ms = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#2", Tier = "MS" });
        Assert.DoesNotContain("Knoten#1", Labels(ms));
        Assert.Single(ms.Edges); // Leitung#1 is one edge

        // NS from the joint: the NS line to the transformer ends there as at a source of NS
        var ns = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#2", Tier = "NS" });
        Assert.Contains(ns.Edges, e => e.Layer == "Leitung" && e.Oid == 3);

        var types = new FileNetworkGraph(_directory).Metadata.Types.Where(t => t.Helper).ToList();
        Assert.Equal(["MS (untyped)", "NS (untyped)"], types.Select(t => t.Name).Order());
    }

    [Fact]
    public void SeparateTiers_TwoHelperNodesAtOnePlace_TheTierChoosesOne()
    {
        Build(Config());
        var service = Service();

        var ex = Assert.Throws<ArgumentException>(() => service.Run("info", new NetworkQueryRequest { Node = "@0,0" }));
        Assert.Contains("2 nodes at one place", ex.Message);

        var ns = service.Run("info", new NetworkQueryRequest { Node = "@0,0", Tier = "NS" });
        Assert.Contains(ns.Messages, m => m.Contains("NS (untyped)"));
        var ms = service.Run("info", new NetworkQueryRequest { Node = "@0,0", Tier = "MS" });
        Assert.Contains(ms.Messages, m => m.Contains("MS (untyped)"));
    }

    [Fact]
    public void ConnectRule_Report_ListsTheRefusedSplit()
    {
        var result = Build(Config(true, new ConnectRuleConfig { At = ConnectAt.Inside, Nodes = "[NS,*]", Edges = "[MS,*]", Action = ConnectAction.Report }));
        var refused = Assert.Single(result.Diagnostics, d => d.Kind == BuildDiagnosticKind.SplitRefused);
        Assert.Equal([new FeatureRef(NodeClass, 1), new FeatureRef(EdgeClass, 1)], refused.Sources!.Cast<FeatureRef>());
    }

    [Fact]
    public void ConnectRule_SwitchedOff_IsSkipped_AsIfDeleted()
    {
        var result = Build(Config(true, new ConnectRuleConfig { At = ConnectAt.Inside, Nodes = "[NS,*]", Edges = "[MS,*]", Action = ConnectAction.Report, Enabled = false }));
        Assert.DoesNotContain(result.Diagnostics, d => d.Kind == BuildDiagnosticKind.SplitRefused);
    }

    [Fact]
    public void ConnectRule_SwitchedOff_MayBeUnfinished_AndIsNotNeeded()
    {
        var config = new NetworkConfig { ConnectRules = [new ConnectRuleConfig { Nodes = "[NS,Gibtsnicht]", Edges = "", Enabled = false }] };
        Assert.False(NetworkConnectRules.Needed(config));
        Build(Config(true, new ConnectRuleConfig { Nodes = "[NS,Gibtsnicht]", Edges = "", Enabled = false }));
    }

    [Fact]
    public void ConnectRule_WithAName_IsNamedInErrors()
    {
        var ex = Assert.Throws<InvalidDataException>(() => Build(Config(true, new ConnectRuleConfig { Name = "LS auf Muffe", Nodes = "[NS,Gibtsnicht]", Edges = "*" })));
        Assert.Contains("Connect rule 1 'LS auf Muffe'", ex.Message);
    }

    [Fact]
    public void ConnectRule_DisplayName_IsTheNameOrTheRuleInShort()
    {
        Assert.Equal("inside: [NS,*] -> [MS,*]", new ConnectRuleConfig { At = ConnectAt.Inside, Nodes = "[NS,*]", Edges = "[MS,*]" }.DisplayName);
        Assert.Equal("LS auf Muffe", new ConnectRuleConfig { Name = " LS auf Muffe ", Nodes = "*", Edges = "*" }.DisplayName);
    }

    [Fact]
    public void ConnectRule_WinsOverTheTiers()
    {
        // the joint may split MS cables again
        Build(Config(true, new ConnectRuleConfig { At = ConnectAt.Any, Nodes = "[NS,Muffe]", Edges = "[MS,*]", Action = ConnectAction.Connect }));
        var ms = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#2", Tier = "MS" });
        Assert.Contains("Knoten#1", Labels(ms));
    }

    [Fact]
    public void ConnectRule_WithAnUnknownType_IsAnError()
    {
        var ex = Assert.Throws<InvalidDataException>(() => Build(Config(true, new ConnectRuleConfig { Nodes = "[NS,Gibtsnicht]", Edges = "*" })));
        Assert.Contains("Connect rule 1", ex.Message);
    }

    [Fact]
    public void ConnectRules_Decide_ByRulesThenSourcesThenTiers()
    {
        var config = Config(true, new ConnectRuleConfig { At = ConnectAt.Inside, Nodes = "untyped & [NS,*]", Edges = "[NS,*]", Action = ConnectAction.Report });
        var catalog = new TypeCatalog(config.Facets, null);
        int msLine = catalog.Resolve(ElementKind.Edge, ["MS", "Leitung"]);
        int nsLine = catalog.Resolve(ElementKind.Edge, ["NS", "Leitung"]);
        int nsJoint = catalog.Resolve(ElementKind.Node, ["NS", "Muffe"]);
        int trafo = catalog.Resolve(ElementKind.Node, ["MS", "Trafo"]);
        var rules = NetworkConnectRules.Create(config, catalog);
        rules.Resolve(config, catalog.ToRegistry());
        int nsHelper = rules.HelperNodeType(nsLine);

        Assert.NotEqual(0, nsHelper);
        Assert.NotEqual(nsHelper, rules.HelperNodeType(msLine));
        Assert.Equal(ConnectionDecision.Report, rules.Split(nsHelper, nsLine));      // the rule: T junctions of NS
        Assert.Equal(ConnectionDecision.Connect, rules.Connect(nsHelper, nsLine));   // the rule is for the inside only
        Assert.Equal(ConnectionDecision.Connect, rules.Connect(trafo, nsLine));      // a source of NS
        Assert.Equal(ConnectionDecision.Ignore, rules.Split(trafo, nsLine));         // sources only at line ends
        Assert.Equal(ConnectionDecision.Ignore, rules.Connect(nsJoint, msLine));     // another tier
        Assert.Equal(rules.Partition(trafo), rules.Partition(catalog.Resolve(ElementKind.Node, ["MS", "Muffe"])));
        Assert.NotEqual(rules.Partition(trafo), rules.Partition(nsJoint));
        Assert.Equal(ConnectionDecision.Connect, rules.Connect(catalog.Resolve(ElementKind.Node, ["", "Marker"]), msLine)); // no tier
    }

    [Fact]
    public void NodesOnLine_Ignore_NoNodeSplitsALine_EvenWithoutSeparateTiers()
    {
        var config = Config(separate: false);
        config.NodesOnLine = ConnectAction.Ignore;
        Build(config);
        var trace = Service().Run("trace", new NetworkQueryRequest { Start = "Knoten#2" });
        Assert.DoesNotContain("Knoten#1", Labels(trace));
    }

    [Fact]
    public void SplitDefaults_ComeAfterRulesAndTiers()
    {
        // only switches split, T junctions are reported, other tiers stay silent
        var config = Config(true, new ConnectRuleConfig { At = ConnectAt.Inside, Nodes = "[NS,Trenner]", Edges = "[NS,*]", Action = ConnectAction.Connect });
        config.NodesOnLine = ConnectAction.Ignore;
        config.LineEndsOnLine = ConnectAction.Report;
        var catalog = new TypeCatalog(config.Facets, null);
        int msLine = catalog.Resolve(ElementKind.Edge, ["MS", "Leitung"]);
        int nsLine = catalog.Resolve(ElementKind.Edge, ["NS", "Leitung"]);
        int nsJoint = catalog.Resolve(ElementKind.Node, ["NS", "Muffe"]);
        int nsSwitch = catalog.Resolve(ElementKind.Node, ["NS", "Trenner"]);
        catalog.Resolve(ElementKind.Node, ["MS", "Trafo"]); // the sources of NS
        var rules = NetworkConnectRules.Create(config, catalog);
        rules.Resolve(config, catalog.ToRegistry());

        Assert.Equal(ConnectionDecision.Connect, rules.Split(nsSwitch, nsLine));                      // the rule
        Assert.Equal(ConnectionDecision.Ignore, rules.Split(nsJoint, nsLine));                        // nodes on a line
        Assert.Equal(ConnectionDecision.Report, rules.Split(rules.HelperNodeType(nsLine), nsLine));   // T junction
        Assert.Equal(ConnectionDecision.Ignore, rules.Split(rules.HelperNodeType(msLine), nsLine));   // another tier
        Assert.Equal(ConnectionDecision.Connect, rules.Connect(nsJoint, nsLine));                     // ends are not affected

        // without separate tiers the helper nodes are untyped (0)
        config.SeparateTiers = false;
        rules = NetworkConnectRules.Create(config, catalog);
        rules.Resolve(config, catalog.ToRegistry());
        Assert.Equal(ConnectionDecision.Report, rules.Split(0, msLine));
        Assert.True(NetworkConnectRules.Needed(new NetworkConfig { LineEndsOnLine = ConnectAction.Ignore }));
        Assert.False(NetworkConnectRules.Needed(new NetworkConfig()));
    }

    [Fact]
    public void HelperIds_DependOnTheType_AndUntypedOnesStayAsBefore()
    {
        var at = new Coordinate(5, 5);
        Assert.Equal(new NetworkIds(0.01).NewHelperNode(at, 0), new NetworkIds(0.01).NewHelperNode(at, 0));
        Assert.NotEqual(new NetworkIds(0.01).NewHelperNode(at, 0), new NetworkIds(0.01).NewHelperNode(at, 7));
        Assert.Equal(new NetworkIds(0.01).NewHelperNode(at, 7), new NetworkIds(0.01).NewHelperNode(at, 7));
    }
}
