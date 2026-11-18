using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using SpatialNET;
using SpatialNET.Network;
using Xunit;

namespace gView.Framework.NetworkModel.Tests;

/// <summary>
/// Searches and validation restricted to an area (box, polygon, shards). The network:
/// <code>
///                         Kunde#4 (0,15)
///                            | Leitung#22
///                         end (0,5)
///                            | Verbinder#11
/// Trafo#1 (0,0) --Verbinder#10-- end (5,0) --Leitung#20-- Kasten#2 (15,0) --Leitung#21-- Kunde#3 (25,0)
/// </code>
/// Kunde#3 is in shard "S2", all others in shard "S1".
/// </summary>
public class NetworkAreaTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-area-tests-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>Everything but Kunde#3 (25,0).</summary>
    private static readonly NetworkArea WithoutKunde3 = NetworkArea.Box(-1, -1, 20, 20);

    private NetworkQueryService Build(List<NetworkRuleConfig>? rules = null)
    {
        var metadata = new NetworkMetadata
        {
            Name = "area",
            Tolerance = Tolerance,
            SupplySources = "[Trafo]",
            Customers = "[Kunde]",
            ShardKeys = new() { ["S1"] = 1, ["S2"] = 2 },
            Rules = rules ?? [],
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
        long end1 = ids.NewHelperNode(new Coordinate(5, 0)), end2 = ids.NewHelperNode(new Coordinate(0, 5));

        using (var writer = new FileNetworkStore(_directory).CreateWriter())
        {
            writer.WriteNode(Real(trafo, TrafoType, 0, 0, TrafoFc, 1, 1));
            writer.WriteNode(Real(kasten, KastenType, 15, 0, KastenFc, 2, 1));
            writer.WriteNode(Real(kunde3, KundeType, 25, 0, KundeFc, 3, 2));
            writer.WriteNode(Real(kunde4, KundeType, 0, 15, KundeFc, 4, 1));
            writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(end1, 0, new Coordinate(5, 0)), 1, true, default));
            writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(end2, 0, new Coordinate(0, 5)), 1, true, default));

            Edge(writer, VerbinderFc, 10, trafo, end1, VerbinderType, 5, 1, 1);
            Edge(writer, LeitungFc, 20, end1, kasten, LeitungType, 10, 1, 1);
            Edge(writer, LeitungFc, 21, kasten, kunde3, LeitungType, 10, 1, 2);
            Edge(writer, VerbinderFc, 11, trafo, end2, VerbinderType, 5, 1, 1);
            Edge(writer, LeitungFc, 22, end2, kunde4, LeitungType, 10, 1, 1);
            writer.Commit(metadata);
        }
        return new NetworkQueryService(new FileNetworkGraph(_directory));

        static long Node(int fc, long oid) => NetworkIds.NodeId(new FeatureRef(fc, oid));

        static BuiltNode<long, FeatureRef> Real(long id, int type, double x, double y, int fc, long oid, long shard)
            => new(new GraphNode<long>(id, type, new Coordinate(x, y)), shard, false, new FeatureRef(fc, oid));

        static void Edge(INetworkStoreWriter writer, int fc, long oid, long from, long to, int type, double length, long fromShard, long toShard)
        {
            var edge = new GraphEdge<long, long>(NetworkIds.Compose(fc, oid), from, to, type, length, EdgeDirection.Both, false, Array.Empty<EdgeWeight>());
            writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(edge, new FeatureRef(fc, oid), 0, length, fromShard, toShard));
        }
    }

    private static List<string> Labels(NetworkQueryResult result)
        => result.Nodes.Where(n => n.AliasOf.Length == 0).Select(n => n.Label).ToList();

    #region Parse

    [Fact]
    public void Parse_Box()
    {
        var area = NetworkArea.Parse(" 10, 20 ,30,40 ");

        Assert.Equal(NetworkAreaKind.Box, area.Kind);
        Assert.True(area.Contains(10, 20));
        Assert.True(area.Contains(29, 39));
        Assert.False(area.Contains(31, 30));
    }

    [Fact]
    public void Parse_GeoJson_PolygonWithHole()
    {
        var area = NetworkArea.Parse("""{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]],[[4,4],[6,4],[6,6],[4,6],[4,4]]]}""");

        Assert.Equal(NetworkAreaKind.Polygon, area.Kind);
        Assert.True(area.Contains(2, 2));
        Assert.False(area.Contains(5, 5));
        Assert.False(area.Contains(11, 5));
        // on the border within the tolerance counts as inside
        Assert.True(area.Contains(10.005, 5, 0.01));
        Assert.False(area.Contains(10.05, 5, 0.01));
    }

    [Fact]
    public void Parse_GeoJson_FeatureCollection_IsTheUnion()
    {
        var area = NetworkArea.Parse("""
            {"type":"FeatureCollection","features":[
              {"type":"Feature","properties":{},"geometry":{"type":"Polygon","coordinates":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}},
              {"type":"Feature","properties":{},"geometry":{"type":"MultiPolygon","coordinates":[[[[5,5],[20,5],[20,20],[5,20],[5,5]]]]}}
            ]}
            """);

        Assert.Equal(2, area.Polygons.Count);
        // the overlap of two polygons is inside (no even-odd between polygons)
        Assert.True(area.Contains(7, 7));
        Assert.True(area.Contains(15, 15));
        Assert.False(area.Contains(15, 2));
    }

    [Fact]
    public void Parse_EsriJson_RingsAndEnvelope()
    {
        var rings = NetworkArea.Parse("""{"rings":[[[0,0],[0,10],[10,10],[10,0],[0,0]]],"spatialReference":{"wkid":31256}}""");
        var envelope = NetworkArea.Parse("""{"xmin":0,"ymin":0,"xmax":10,"ymax":10}""");

        Assert.True(rings.Contains(5, 5));
        Assert.Equal(NetworkAreaKind.Box, envelope.Kind);
        Assert.True(envelope.Contains(5, 5));
    }

    [Fact]
    public void Parse_Wkt()
    {
        var polygon = NetworkArea.Parse("POLYGON ((0 0, 10 0, 10 10, 0 10, 0 0), (4 4, 6 4, 6 6, 4 6, 4 4))");
        var multi = NetworkArea.Parse("MULTIPOLYGON (((0 0, 1 0, 1 1, 0 0)), ((5 5, 9 5, 9 9, 5 9, 5 5)))");

        Assert.True(polygon.Contains(2, 2));
        Assert.False(polygon.Contains(5, 5));
        Assert.Equal(2, multi.Polygons.Sum(p => p.Count)); // one polygon with the rings of both parts
        Assert.True(multi.Contains(7, 7));
        Assert.Throws<ArgumentException>(() => NetworkArea.Parse("POINT (1 2)"));
    }

    [Fact]
    public void Parse_Shards_And_Errors()
    {
        var area = NetworkArea.Parse("shard: 4711, 4712");

        Assert.Equal(NetworkAreaKind.Shards, area.Kind);
        Assert.Equal(["4711", "4712"], area.ShardKeys);
        Assert.Throws<ArgumentException>(() => NetworkArea.Parse("1,2,3"));
        Assert.Throws<ArgumentException>(() => NetworkArea.Parse("10,10,0,0"));
        Assert.Throws<ArgumentException>(() => NetworkArea.Parse("shard:"));
    }

    [Fact]
    public void Transform_MakesABoxAPolygon()
    {
        var area = NetworkArea.Box(0, 0, 1, 1).Transform((x, y) => (x * 10, y * 10));

        Assert.Equal(NetworkAreaKind.Polygon, area.Kind);
        Assert.True(area.Contains(9, 9));
    }

    #endregion

    #region Searches

    [Fact]
    public void Trace_StopsAtTheArea_AndReportsTheBoundaryEdge()
    {
        var service = Build();

        var result = service.Run("trace", new NetworkQueryRequest { Start = "Trafo#1", Area = WithoutKunde3 });

        var labels = Labels(result);
        Assert.Contains("NS/Kasten#2", labels);
        Assert.Contains("NS/Kunde#4", labels);
        Assert.DoesNotContain("NS/Kunde#3", labels);
        Assert.Equal("Boundary", Assert.Single(result.Edges, e => e.Oid == 21).Kind);
        Assert.Contains(result.Messages, m => m.StartsWith("Area: box") && m.EndsWith("5 node(s) inside"));
        Assert.Contains(result.Messages, m => m.Contains("boundary edges: 1"));
    }

    [Fact]
    public void ExplicitStartOutsideTheArea_IsAnError()
    {
        var service = Build();

        var ex = Assert.Throws<ArgumentException>(() => service.Run("trace", new NetworkQueryRequest { Start = "Kunde#3", Area = WithoutKunde3 }));
        Assert.Contains("outside the area", ex.Message);
        Assert.Throws<ArgumentException>(() => service.Run("path", new NetworkQueryRequest { From = "Trafo#1", To = "Kunde#3", Area = WithoutKunde3 }));
    }

    [Fact]
    public void Supply_LeavesOutTheCustomersOfThePatternOutside()
    {
        var service = Build();

        var result = service.Run("supply", new NetworkQueryRequest { Area = WithoutKunde3 });

        Assert.Equal(["NS/Kunde#4"], Labels(result));
        Assert.Contains(result.Messages, m => m.Contains("1 sink node(s) outside the area left out"));
    }

    [Fact]
    public void Path_DoesNotLeaveTheArea()
    {
        var service = Build();

        var inside = service.Run("path", new NetworkQueryRequest { From = "Kunde#4", To = "Kasten#2", Area = WithoutKunde3 });
        // the polygon leaves Kunde#4 outside: the way along the main line stays
        var cut = service.Run("path", new NetworkQueryRequest
        {
            From = "Kasten#2",
            To = "Trafo#1",
            Area = NetworkArea.Parse("POLYGON ((-1 -1, 20 -1, 20 1, -1 1, -1 -1))")
        });

        Assert.True(inside.Success);
        Assert.True(cut.Success);
        Assert.Throws<ArgumentException>(() => service.Run("path", new NetworkQueryRequest
        {
            From = "Kasten#2",
            To = "Kunde#4",
            Area = NetworkArea.Parse("POLYGON ((-1 -1, 20 -1, 20 1, -1 1, -1 -1))")
        }));
    }

    [Fact]
    public void Shards_TakeTheNodesOfTheShard()
    {
        var service = Build();

        var result = service.Run("trace", new NetworkQueryRequest { Start = "[Kunde]", Area = NetworkArea.Parse("shard:S2") });

        Assert.Equal(["NS/Kunde#3"], Labels(result).Distinct());
        Assert.Contains(result.Messages, m => m.Contains("1 start node(s) outside the area left out"));
        Assert.Equal("Boundary", Assert.Single(result.Edges).Kind);
        Assert.Throws<ArgumentException>(() => service.Run("trace", new NetworkQueryRequest { Start = "[Kunde]", Area = NetworkArea.Parse("shard:S9") }));
    }

    [Fact]
    public void Outage_InTheArea()
    {
        var service = Build();

        var result = service.Run("outage", new NetworkQueryRequest { Disable = "Leitung#20", Sinks = "[Kasten]", Area = NetworkArea.Box(-1, -1, 20, 1) });

        Assert.Equal(["NS/Kasten#2"], result.Nodes.Where(n => n.Kind == "Affected").Select(n => n.Label));
        Assert.Throws<ArgumentException>(() => service.Run("outage", new NetworkQueryRequest { Disable = "Leitung#21", Area = WithoutKunde3 }));
    }

    #endregion

    #region Validate

    private static NetworkRuleConfig CustomerWithoutLine() => new()
    {
        Id = "kunde-ohne-leitung",
        Subject = RuleSubject.Node,
        SubjectTypes = "Kunde",
        CountTypes = "Leitung",
        Max = 0,
    };

    [Fact]
    public void Validate_ChecksOnlyTheNodesInside()
    {
        var service = Build([CustomerWithoutLine()]);

        var all = service.Run("validate", new NetworkQueryRequest());
        var area = service.Run("validate", new NetworkQueryRequest { Area = WithoutKunde3 });

        Assert.Equal(2, all.Violations.Count);
        Assert.Equal("NS/Kunde#4", Assert.Single(area.Violations).NodeLabel);
        Assert.Equal(1, Assert.Single(area.Rules).Checked);
    }

    [Fact]
    public void Validate_CountsTheNeighborsOutsideToo()
    {
        var service = Build([CustomerWithoutLine()]);

        // only Kunde#3 is inside; the other end of its line (Kasten#2) is outside but counts
        var result = service.Run("validate", new NetworkQueryRequest { Area = NetworkArea.Box(20, -1, 30, 1) });

        Assert.Equal("NS/Kunde#3", Assert.Single(result.Violations).NodeLabel);
    }

    #endregion
}
