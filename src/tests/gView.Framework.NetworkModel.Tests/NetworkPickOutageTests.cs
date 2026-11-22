using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using SpatialNET;
using SpatialNET.Network;
using Xunit;

namespace gView.Framework.NetworkModel.Tests;

/// <summary>
/// Edge ends (helper nodes) as start nodes (@x,y, nearest), edges switched off (disable) and the outage of a branch.
/// The network is a station with two branches:
/// <code>
///                         Kunde#4 (0,15)
///                            | Leitung#22
///                         end (0,5)  --(Leitung#23, mesh, optional)--  Kasten#2
///                            | Verbinder#11
/// Trafo#1 (0,0) --Verbinder#10-- end (5,0) --Leitung#20-- Kasten#2 (15,0) --Leitung#21-- Kunde#3 (25,0)
/// </code>
/// Kunde#5 is drawn on Kasten#2 (merged point at 15.004,0).
/// </summary>
public class NetworkPickOutageTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-pick-tests-" + Guid.NewGuid().ToString("N"));

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

    private const int TrafoFc = 0, KastenFc = 1, KundeFc = 2, VerbinderFc = 3, LeitungFc = 4;
    private const int TrafoType = 1, KastenType = 2, KundeType = 3, VerbinderType = 10, LeitungType = 11;
    private const double Tolerance = 0.01;

    private long _end1, _end2;

    private void Build(bool mesh = false)
    {
        var metadata = new NetworkMetadata
        {
            Name = "station",
            Tolerance = Tolerance,
            SupplySources = "[Trafo]",
            Customers = "[Kunde]",
            Facets = [new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Trafo", [2] = "Kasten", [3] = "Kunde", [4] = "Verbinder", [5] = "Leitung" } }],
            Types =
            [
                new NetworkTypeMetadata { Id = TrafoType, Kind = ElementKind.Node, Facets = [1], Used = true },
                new NetworkTypeMetadata { Id = KastenType, Kind = ElementKind.Node, Facets = [2], Used = true },
                new NetworkTypeMetadata { Id = KundeType, Kind = ElementKind.Node, Facets = [3], Used = true },
                new NetworkTypeMetadata { Id = VerbinderType, Kind = ElementKind.Edge, Facets = [4], Used = true },
                new NetworkTypeMetadata { Id = LeitungType, Kind = ElementKind.Edge, Facets = [5], Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = TrafoFc, Layer = "NS/Trafo", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = KastenFc, Layer = "NS/Kasten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = KundeFc, Layer = "NS/Kunde", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = VerbinderFc, Layer = "NS/Verbinder", Kind = ElementKind.Edge },
                new NetworkFeatureClassMetadata { Index = LeitungFc, Layer = "NS/Leitung", Kind = ElementKind.Edge },
            ],
        };

        var ids = new NetworkIds(Tolerance);
        long trafo = Node(TrafoFc, 1), kasten = Node(KastenFc, 2), kunde3 = Node(KundeFc, 3), kunde4 = Node(KundeFc, 4);
        _end1 = ids.NewHelperNode(new Coordinate(5, 0), 0);
        _end2 = ids.NewHelperNode(new Coordinate(0, 5), 0);

        using var writer = new FileNetworkStore(_directory).CreateWriter();
        writer.WriteNode(Real(trafo, TrafoType, 0, 0, TrafoFc, 1));
        writer.WriteNode(Real(kasten, KastenType, 15, 0, KastenFc, 2) with
        {
            Merged = [new CoincidentNode<long, FeatureRef>(Node(KundeFc, 5), KundeType, new Coordinate(15.004, 0), new FeatureRef(KundeFc, 5))]
        });
        writer.WriteNode(Real(kunde3, KundeType, 25, 0, KundeFc, 3));
        writer.WriteNode(Real(kunde4, KundeType, 0, 15, KundeFc, 4));
        writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(_end1, 0, new Coordinate(5, 0)), 1, true, default));
        writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(_end2, 0, new Coordinate(0, 5)), 1, true, default));

        Edge(writer, VerbinderFc, 10, trafo, _end1, VerbinderType, 5);
        Edge(writer, LeitungFc, 20, _end1, kasten, LeitungType, 10);
        Edge(writer, LeitungFc, 21, kasten, kunde3, LeitungType, 10);
        Edge(writer, VerbinderFc, 11, trafo, _end2, VerbinderType, 5);
        Edge(writer, LeitungFc, 22, _end2, kunde4, LeitungType, 10);
        if (mesh)
        {
            Edge(writer, LeitungFc, 23, _end2, kasten, LeitungType, 16);
        }
        writer.Commit(metadata);

        static long Node(int fc, long oid) => NetworkIds.NodeId(new FeatureRef(fc, oid));

        static BuiltNode<long, FeatureRef> Real(long id, int type, double x, double y, int fc, long oid)
            => new(new GraphNode<long>(id, type, new Coordinate(x, y)), 1, false, new FeatureRef(fc, oid));

        static void Edge(INetworkStoreWriter writer, int fc, long oid, long from, long to, int type, double length)
        {
            var edge = new GraphEdge<long, long>(NetworkIds.Compose(fc, oid), from, to, type, length, EdgeDirection.Both, false, Array.Empty<EdgeWeight>());
            writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(edge, new FeatureRef(fc, oid), 0, length, 1, 1));
        }
    }

    private NetworkQueryService Service() => new(new FileNetworkGraph(_directory));

    [Fact]
    public void Location_ResolvesToTheEdgeEnd()
    {
        Build();
        var graph = new FileNetworkGraph(_directory);

        Assert.Equal([_end1], graph.ResolveNodes("@5.05,0.02"));
        Assert.Equal("end of Verbinder#10, Leitung#20", graph.NodeLabel(_end1));
        // the comma of the location does not split the list
        Assert.Equal([_end1, NetworkIds.NodeId(new FeatureRef(TrafoFc, 1))], graph.ResolveNodes("@5,0, Trafo#1"));
        Assert.Throws<ArgumentException>(() => graph.ResolveNodes("@50,50"));
        Assert.Equal([_end1], graph.ResolveNodes("@7,0~3"));
    }

    [Fact]
    public void Location_OnAMergedPoint_ResolvesToItsNode()
    {
        Build();
        var graph = new FileNetworkGraph(_directory);

        Assert.Equal([NetworkIds.NodeId(new FeatureRef(KastenFc, 2))], graph.ResolveNodes("@15.004,0~0.001"));
    }

    [Fact]
    public void Nearest_ListsEachNodeOnce_NearestFirst()
    {
        Build();

        var result = Service().Run("nearest", new NetworkQueryRequest { X = 1, Y = 0.5, Radius = 10, Count = 3 });

        Assert.True(result.Success);
        Assert.Equal(["NS/Trafo#1", "end of Verbinder#10, Leitung#20", "end of Verbinder#11, Leitung#22"], result.Nodes.Select(n => n.Label));
    }

    [Fact]
    public void Trace_FromAnEdgeEnd_WithoutTheConnectors_StaysOnTheBranch()
    {
        Build();

        var result = Service().Run("trace", new NetworkQueryRequest { Start = "@5,0", Filter = "![Verbinder]" });

        var labels = result.Nodes.Where(n => n.AliasOf.Length == 0).Select(n => n.Label).ToList();
        Assert.Contains("NS/Kasten#2", labels);
        Assert.Contains("NS/Kunde#3", labels);
        Assert.DoesNotContain("NS/Trafo#1", labels);
        Assert.DoesNotContain("NS/Kunde#4", labels);
    }

    [Fact]
    public void EdgeReferences_FindAllPartsOrOne()
    {
        Build();
        var graph = new FileNetworkGraph(_directory);

        Assert.Equal([NetworkIds.Compose(LeitungFc, 20)], graph.ResolveEdges("Leitung#20"));
        Assert.Equal([NetworkIds.Compose(LeitungFc, 20)], graph.ResolveEdges("NS/Leitung#20/0"));
        Assert.Throws<ArgumentException>(() => graph.ResolveEdges("Leitung#99"));
        Assert.Throws<ArgumentException>(() => graph.ResolveEdges("Leitung#20/1"));
    }

    [Fact]
    public void Disable_CutsTheSearch()
    {
        Build();

        var result = Service().Run("trace", new NetworkQueryRequest { Start = "Trafo#1", Disable = "Leitung#20" });

        Assert.DoesNotContain(result.Nodes, n => n.Label == "NS/Kunde#3");
        Assert.Contains(result.Nodes, n => n.Label == "NS/Kunde#4");
    }

    [Fact]
    public void Outage_ReportsTheCustomersBehindTheBranch()
    {
        Build();

        var result = Service().Run("outage", new NetworkQueryRequest { Disable = "Leitung#20" });

        Assert.Equal(["NS/Kasten#2", "NS/Kunde#3"], result.Nodes.Where(n => n.AliasOf.Length == 0 && n.Kind == "Affected").Select(n => n.Label).Order());
        // the customer drawn on the box is affected too
        Assert.Contains(result.Nodes, n => n.Label == "NS/Kunde#5" && n.AliasOf == "NS/Kasten#2");
        Assert.Equal("Disabled", Assert.Single(result.Edges, e => e.Oid == 20).Kind);
        Assert.Equal("Dead", Assert.Single(result.Edges, e => e.Oid == 21).Kind);
        Assert.DoesNotContain(result.Edges, e => e.Oid == 22);
    }

    [Fact]
    public void Outage_InAMesh_KeepsTheCustomersFedFromTheOtherSide()
    {
        Build(mesh: true);

        var result = Service().Run("outage", new NetworkQueryRequest { Disable = "Leitung#20", Sinks = "[Kunde]" });

        Assert.DoesNotContain(result.Nodes, n => n.Kind == "Affected");
        Assert.Contains(result.Messages, m => m.Contains("still supplied") && m.TrimEnd().EndsWith('3'));
    }

    [Fact]
    public void Outage_OfABoxConnector_ReportsOnlyTheHouseBehindIt()
    {
        // Trafo#1 -V#10- end(5,0) -L#20- end(15,-1) -KV#34- Kasten#2 (15,0) -KV#28- end(15,1) -HA#40- Kunde#6
        //                                                    Kasten#2 -KV#33- end(16,0) -L#21- Kunde#3
        var ids = new NetworkIds(Tolerance);
        long trafo = NetworkIds.NodeId(new FeatureRef(TrafoFc, 1)), kasten = NetworkIds.NodeId(new FeatureRef(KastenFc, 2));
        long kunde3 = NetworkIds.NodeId(new FeatureRef(KundeFc, 3)), kunde6 = NetworkIds.NodeId(new FeatureRef(KundeFc, 6));
        long e1 = ids.NewHelperNode(new Coordinate(5, 0), 0), e2 = ids.NewHelperNode(new Coordinate(15, -1), 0);
        long e3 = ids.NewHelperNode(new Coordinate(15, 1), 0), e4 = ids.NewHelperNode(new Coordinate(16, 0), 0);
        var metadata = new NetworkMetadata
        {
            Name = "box",
            Tolerance = Tolerance,
            SupplySources = "[Trafo]",
            Customers = "[Kunde]",
            Facets = [new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Trafo", [2] = "Kasten", [3] = "Kunde", [4] = "Verbinder", [5] = "Leitung" } }],
            Types =
            [
                new NetworkTypeMetadata { Id = TrafoType, Kind = ElementKind.Node, Facets = [1], Used = true },
                new NetworkTypeMetadata { Id = KastenType, Kind = ElementKind.Node, Facets = [2], Used = true },
                new NetworkTypeMetadata { Id = KundeType, Kind = ElementKind.Node, Facets = [3], Used = true },
                new NetworkTypeMetadata { Id = VerbinderType, Kind = ElementKind.Edge, Facets = [4], Used = true },
                new NetworkTypeMetadata { Id = LeitungType, Kind = ElementKind.Edge, Facets = [5], Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = TrafoFc, Layer = "NS/Trafo", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = KastenFc, Layer = "NS/Kasten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = KundeFc, Layer = "NS/Kunde", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = VerbinderFc, Layer = "NS/Verbinder", Kind = ElementKind.Edge },
                new NetworkFeatureClassMetadata { Index = LeitungFc, Layer = "NS/Leitung", Kind = ElementKind.Edge },
            ],
        };
        using (var writer = new FileNetworkStore(_directory).CreateWriter())
        {
            void Node(long id, int type, double x, double y, bool helper = false)
                => writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(id, type, new Coordinate(x, y)), 1, helper, helper ? default : new FeatureRef(NetworkIds.Decompose(id).FeatureClass, NetworkIds.Decompose(id).Oid)));
            void Edge(int fc, long oid, long from, long to, int type)
            {
                // lines are directed away from the station (as in a network built with a direction for lines)
                var direction = type == LeitungType ? EdgeDirection.Forward : EdgeDirection.Both;
                var edge = new GraphEdge<long, long>(NetworkIds.Compose(fc, oid), from, to, type, 1, direction, false, Array.Empty<EdgeWeight>());
                writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(edge, new FeatureRef(fc, oid), 0, 1, 1, 1));
            }
            Node(trafo, TrafoType, 0, 0); Node(kasten, KastenType, 15, 0); Node(kunde3, KundeType, 25, 0); Node(kunde6, KundeType, 15, 10);
            Node(e1, 0, 5, 0, true); Node(e2, 0, 15, -1, true); Node(e3, 0, 15, 1, true); Node(e4, 0, 16, 0, true);
            Edge(VerbinderFc, 10, trafo, e1, VerbinderType);
            Edge(LeitungFc, 20, e1, e2, LeitungType);
            Edge(VerbinderFc, 34, e2, kasten, VerbinderType);
            Edge(VerbinderFc, 28, kasten, e3, VerbinderType);
            Edge(LeitungFc, 40, e3, kunde6, LeitungType);
            Edge(VerbinderFc, 33, kasten, e4, VerbinderType);
            Edge(LeitungFc, 21, e4, kunde3, LeitungType);
            writer.Commit(metadata);
        }

        var result = Service().Run("outage", new NetworkQueryRequest { Disable = "NS/Verbinder#28/0" });

        Assert.Contains(result.Messages, m => m.Contains("1 source(s)"));
        Assert.Equal(["NS/Kunde#6"], result.Nodes.Where(n => n.Kind == "Affected").Select(n => n.Label));
    }

    [Fact]
    public void Run_ReportsProgress_AndStopsWhenCancelled()
    {
        Build();
        var lines = new List<string>();
        var progress = new SyncLines(lines);

        Service().Run("trace", new NetworkQueryRequest { Start = "Trafo#1" }, progress);
        Assert.Contains(lines, l => l.StartsWith("trace: ") && l.EndsWith("node(s) found"));

        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        foreach (var operation in new[] { "trace", "reach", "outage" })
        {
            Assert.ThrowsAny<OperationCanceledException>(() => Service().Run(operation,
                new NetworkQueryRequest { Start = "Trafo#1", Disable = "Leitung#20", MaxCost = 100 }, progress, cancel.Token));
        }
    }

    private sealed class SyncLines(List<string> lines) : IProgress<string>
    {
        public void Report(string value) => lines.Add(value);
    }

    [Fact]
    public void Locations_AreTransformed_TheRestStays()
    {
        var text = NetworkLocations.Transform("Trafo#1, @1.5,2 ~3, [*,Kunde,*], @-4e1, 5", (x, y) => (x * 10, y * 10));

        Assert.Equal("Trafo#1, @15,20 ~3, [*,Kunde,*], @-400,50", text);
    }

    [Fact]
    public void Snap_SetsTheEndsOfASectionOntoTheNodes()
    {
        var result = new NetworkQueryResult();
        var edge = new NetworkResultEdge(1, "L#1/0", "L", 1, "Leitung", 10, 0, 10, 1, 2, 0, 0, 10, 0);
        var section = new List<double[]> { new[] { 0.004, 0.0 }, new[] { 5.0, 1.0 }, new[] { 10.2, 0.0 } };

        var snapped = typeof(NetworkResultGeometry)
            .GetMethod("Snap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, [result, edge, section, 0.01]) as IReadOnlyList<double[]>;

        Assert.Equal([[0.0, 0.0], [5.0, 1.0], [10.0, 0.0]], snapped!);
        Assert.Single(result.Messages); // 0.2 is more than the tolerance
    }
}
