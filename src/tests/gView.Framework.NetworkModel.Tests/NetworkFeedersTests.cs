using Xunit;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using SpatialNET;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Tests;

/// <summary>
/// feeders: Quelle#1 -MS- Trafo#2 with the connectors Verbinder#1 (to Kasten#4) and Verbinder#2 (to Kasten#5);
/// Trafo#3 with Verbinder#3 (to Kasten#8). Low voltage lines: Leitung#1 Kasten#4 - Kasten#6,
/// Leitung#2 Kasten#5 - Kasten#7, Leitung#3 Kasten#8 - Kasten#7 (a closed ring between both transformers).
/// </summary>
public class NetworkFeedersTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-feeders-tests-" + Guid.NewGuid().ToString("N"));

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

    private const int Knoten = 0, Kabel = 1, Verbinder = 2, Leitung = 3;
    private const int QuelleType = 10, TrafoType = 12, KastenType = 13, MsKabel = 20, NsVerbinder = 22, NsLeitung = 23;

    private void Build(bool closedRing = true, bool ringSwitchedOff = false)
    {
        var metadata = new NetworkMetadata
        {
            Name = "feeders",
            Facets =
            [
                new NetworkFacetMetadata { Name = "Ebene", Values = new() { [1] = "MS", [2] = "NS" } },
                new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Kabel", [2] = "Quelle", [3] = "Kasten", [4] = "Trafo", [5] = "Verbinder", [6] = "Leitung" } },
            ],
            Types =
            [
                new NetworkTypeMetadata { Id = QuelleType, Kind = ElementKind.Node, Facets = [1, 2], Used = true },
                new NetworkTypeMetadata { Id = TrafoType, Kind = ElementKind.Node, Facets = [1, 4], Used = true },
                new NetworkTypeMetadata { Id = KastenType, Kind = ElementKind.Node, Facets = [2, 3], Used = true },
                new NetworkTypeMetadata { Id = MsKabel, Kind = ElementKind.Edge, Facets = [1, 1], Used = true },
                new NetworkTypeMetadata { Id = NsVerbinder, Kind = ElementKind.Edge, Facets = [2, 5], Used = true },
                new NetworkTypeMetadata { Id = NsLeitung, Kind = ElementKind.Edge, Facets = [2, 6], Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = Knoten, Layer = "Knoten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = Kabel, Layer = "Kabel", Kind = ElementKind.Edge },
                new NetworkFeatureClassMetadata { Index = Verbinder, Layer = "Verbinder", Kind = ElementKind.Edge },
                new NetworkFeatureClassMetadata { Index = Leitung, Layer = "Leitung", Kind = ElementKind.Edge },
            ],
            SupplySources = "[*,Quelle]",
            Tiers =
            [
                new NetworkTierConfig { Name = "MS", Order = 1, Sources = "[*,Quelle]", Sinks = "[*,Trafo]" },
                new NetworkTierConfig { Name = "NS", Order = 2, Sources = "[*,Trafo]" },
            ],
        };

        using var writer = new FileNetworkStore(_directory).CreateWriter();
        (int Oid, int Type)[] nodes = [(1, QuelleType), (2, TrafoType), (3, TrafoType), (4, KastenType), (5, KastenType), (6, KastenType), (7, KastenType), (8, KastenType)];
        foreach (var (oid, type) in nodes)
        {
            var feature = new FeatureRef(Knoten, oid);
            writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(NetworkIds.NodeId(feature), type, new Coordinate(oid * 10, 0)), 1, false, feature));
        }

        void Edge(int featureClass, int oid, int from, int to, int type, double length, bool disabled = false)
        {
            var feature = new FeatureRef(featureClass, oid);
            var edge = new GraphEdge<long, long>(NetworkIds.Compose(featureClass, oid), NetworkIds.NodeId(new FeatureRef(Knoten, from)),
                NetworkIds.NodeId(new FeatureRef(Knoten, to)), type, length, EdgeDirection.Both, disabled, Array.Empty<EdgeWeight>());
            writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(edge, feature, 0, length, 1, 1));
        }

        Edge(Kabel, 1, 1, 2, MsKabel, 100);
        Edge(Verbinder, 1, 2, 4, NsVerbinder, 1);
        Edge(Verbinder, 2, 2, 5, NsVerbinder, 2);
        Edge(Verbinder, 3, 3, 8, NsVerbinder, 1);
        Edge(Leitung, 1, 4, 6, NsLeitung, 10);
        Edge(Leitung, 2, 5, 7, NsLeitung, 5);
        if (closedRing)
        {
            Edge(Leitung, 3, 8, 7, NsLeitung, 5, disabled: ringSwitchedOff);
        }
        writer.Commit(metadata);
    }

    private NetworkQueryResult Feeders(NetworkQueryRequest request)
        => new NetworkQueryService(new FileNetworkGraph(_directory)).Run("feeders", request);

    [Fact]
    public void Feeders_LabelEveryEdgeWithItsSourceAndBranch()
    {
        Build(closedRing: false);
        var result = Feeders(new NetworkQueryRequest { Tier = "NS", Branches = "[NS,Verbinder]" });

        Assert.True(result.Success);
        var edges = result.Edges.ToDictionary(e => e.Label);
        Assert.Equal(["Leitung#1/0", "Leitung#2/0", "Verbinder#1/0", "Verbinder#2/0", "Verbinder#3/0"], edges.Keys.Order());
        Assert.All(edges.Values, e => Assert.Equal("Edge", e.Kind));

        Assert.Equal("Knoten#2", edges["Leitung#1/0"].Source);
        Assert.Equal("Verbinder#1", edges["Leitung#1/0"].Branch);
        Assert.Equal(1, edges["Leitung#1/0"].Cost);

        // the transformer stops the search: Leitung#2 hangs on Verbinder#2 only, not on Verbinder#1
        Assert.Equal("Verbinder#2", edges["Leitung#2/0"].Branch);
        Assert.Equal(2, edges["Leitung#2/0"].Cost);

        Assert.Equal("Verbinder#1", edges["Verbinder#1/0"].Branch);
        Assert.Equal(0, edges["Verbinder#1/0"].Cost);
        Assert.Equal("Knoten#3", edges["Verbinder#3/0"].Source);

        Assert.Equal(["Knoten#2", "Knoten#3"], result.Nodes.Where(n => n.Kind == "Source").Select(n => n.Label).Order());
    }

    [Fact]
    public void Feeders_WithoutTier_AnyEdgeOfTheBranchTypeStartsABranch()
    {
        Build(closedRing: false);
        var result = Feeders(new NetworkQueryRequest { Sources = "[*,Trafo]", Branches = "[*,Verbinder]" });

        // the medium voltage cable at Trafo#2 is no branch and the search never gets past the transformer to it
        Assert.DoesNotContain(result.Edges, e => e.Layer == "Kabel");
        Assert.Equal("Verbinder#1", result.Edges.Single(e => e.Label == "Leitung#1/0").Branch);
    }

    [Fact]
    public void Feeders_AClosedRingIsMultiple()
    {
        Build();
        var result = Feeders(new NetworkQueryRequest { Tier = "NS", Branches = "[NS,Verbinder]" });

        var ring = result.Edges.Single(e => e.Label == "Leitung#2/0");
        Assert.Equal("Multiple", ring.Kind);
        Assert.Equal("Knoten#2 Knoten#3", ring.Source);
        Assert.Equal("Verbinder#2 Verbinder#3", ring.Branch);
        Assert.Equal(2, ring.Cost);
        Assert.Equal("Multiple", result.Edges.Single(e => e.Label == "Leitung#3/0").Kind);
        Assert.Equal("Edge", result.Edges.Single(e => e.Label == "Leitung#1/0").Kind);
        Assert.Contains(result.Messages, m => m.Contains("reached from more than one branch: 2"));
    }

    [Fact]
    public void Feeders_ASwitchedOffRingLineIsOpen_WhenAsked()
    {
        Build(ringSwitchedOff: true);
        var without = Feeders(new NetworkQueryRequest { Tier = "NS", Branches = "[NS,Verbinder]" });
        var with = Feeders(new NetworkQueryRequest { Tier = "NS", Branches = "[NS,Verbinder]", OpenEdges = true });

        Assert.DoesNotContain(without.Edges, e => e.Label == "Leitung#3/0");
        var open = with.Edges.Single(e => e.Label == "Leitung#3/0");
        Assert.Equal("Open", open.Kind);
        // both ends were reached: Kasten#7 from Verbinder#2 (cost 2 + 5), Kasten#8 from Verbinder#3 (cost 1 + 0)
        Assert.Equal("Verbinder#3 Verbinder#2", open.Branch);
        Assert.Equal("Knoten#3 Knoten#2", open.Source);
        Assert.Equal(1, open.Cost);
        // the ring is open: Leitung#2 hangs on Verbinder#2 only
        Assert.Equal("Edge", with.Edges.Single(e => e.Label == "Leitung#2/0").Kind);
        // the medium voltage cable at the transformer is no open edge: the transformer is a stop, not gone through
        Assert.DoesNotContain(with.Edges, e => e.Layer == "Kabel");
        Assert.Contains(with.Messages, m => m.Contains("open: 1"));
    }

    [Fact]
    public void Feeders_ADisabledConnectorIsNoBranch()
    {
        Build(closedRing: false);
        var result = Feeders(new NetworkQueryRequest { Tier = "NS", Branches = "[NS,Verbinder]", Disable = "Verbinder#2" });

        Assert.DoesNotContain(result.Edges, e => e.Label is "Verbinder#2/0" or "Leitung#2/0");
    }
}
