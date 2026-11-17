using Xunit;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using SpatialNET;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkQueryTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-query-tests-" + Guid.NewGuid().ToString("N"));

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

    private const int Kasten = 0, Kabel = 1, KastenType = 15, KabelType = 30;

    /// <summary>Kasten#1 (shard 1) -- Kabel#5 (measures 0..10) -- Kasten#2 (shard 2).</summary>
    private void Build(string name)
    {
        var a = new FeatureRef(Kasten, 1);
        var b = new FeatureRef(Kasten, 2);
        var cable = new FeatureRef(Kabel, 5);
        var metadata = new NetworkMetadata
        {
            Name = name,
            Facets = [new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Kasten", [2] = "Kabel" } }],
            Types =
            [
                new NetworkTypeMetadata { Id = KastenType, Kind = ElementKind.Node, Facets = [1], Name = "Kasten", Used = true },
                new NetworkTypeMetadata { Id = KabelType, Kind = ElementKind.Edge, Facets = [2], Name = "Kabel", Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = Kasten, Layer = "NS/Kasten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = Kabel, Layer = "NS/Kabel", Kind = ElementKind.Edge },
            ],
        };

        using var writer = new FileNetworkStore(_directory).CreateWriter();
        long idA = NetworkIds.NodeId(a), idB = NetworkIds.NodeId(b);
        writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(idA, KastenType, new Coordinate(0, 0)), 1, false, a));
        writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(idB, KastenType, new Coordinate(10, 0)), 2, false, b));
        var edge = new GraphEdge<long, long>(NetworkIds.Compose(Kabel, 5), idA, idB, KabelType, 10, EdgeDirection.Both, false, Array.Empty<EdgeWeight>());
        writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(edge, cable, 0, 10, 1, 2));
        writer.Commit(metadata);
    }

    [Fact]
    public void Path_ReturnsEdgeWithSourceFeatureAndMeasures()
    {
        Build("n");
        var service = new NetworkQueryService(new FileNetworkGraph(_directory));

        var result = service.Run("path", new NetworkQueryRequest { From = "Kasten#1", To = "Kasten#2" });

        Assert.True(result.Success);
        var edge = Assert.Single(result.Edges);
        Assert.Equal("NS/Kabel", edge.Layer);
        Assert.Equal(5, edge.Oid);
        Assert.Equal(10, edge.ToMeasure);
        Assert.Equal(["From", "To"], result.Nodes.Select(n => n.Kind));
    }

    [Fact]
    public void Graph_WithShardLimit_StillAnswersAfterEviction()
    {
        Build("n");
        var graph = new FileNetworkGraph(_directory) { MaxLoadedShards = 1 };
        var service = new NetworkQueryService(graph);

        for (int i = 0; i < 3; i++)
        {
            Assert.True(service.Run("path", new NetworkQueryRequest { From = "Kasten#1", To = "Kasten#2" }).Success);
        }
        Assert.True(graph.LoadedShardCount <= 1);
    }

    [Fact]
    public void Graph_ParallelSearches_GiveTheSameResult()
    {
        Build("n");
        var service = new NetworkQueryService(new FileNetworkGraph(_directory) { MaxLoadedShards = 1 });

        var counts = Enumerable.Range(0, 64).AsParallel()
            .Select(_ => service.Run("trace", new NetworkQueryRequest { Start = "Kasten#1" }).Nodes.Count)
            .Distinct().ToList();

        Assert.Single(counts);
    }

    [Fact]
    public void Cache_OpensTheNetworkAgainAfterARebuild()
    {
        Build("first");
        var cache = new NetworkGraphCache();
        var first = cache.Get(_directory);
        Assert.Same(first, cache.Get(_directory));

        File.SetLastWriteTimeUtc(Path.Combine(_directory, FileNetworkStore.MetadataFile), DateTime.UtcNow.AddMinutes(-5));
        Build("second");

        var second = cache.Get(_directory);
        Assert.NotSame(first, second);
        Assert.Equal("second", second.Metadata.Name);
        Assert.False(Directory.Exists(_directory + ".building"));
    }

    [Fact]
    public void Section_CutsTheLineBetweenTheMeasures_ReversedWhenFromIsGreater()
    {
        var path = new gView.Framework.Geometry.Path();
        path.AddPoint(new gView.Framework.Geometry.Point(0, 0));
        path.AddPoint(new gView.Framework.Geometry.Point(10, 0));
        path.AddPoint(new gView.Framework.Geometry.Point(10, 10));
        var line = new gView.Framework.Geometry.Polyline();
        line.AddPath(path);

        var section = NetworkResultGeometry.Section(line, 5, 15);
        Assert.Equal([[5.0, 0.0], [10.0, 0.0], [10.0, 5.0]], section);

        var reversed = NetworkResultGeometry.Section(line, 15, 5);
        Assert.Equal([[10.0, 5.0], [10.0, 0.0], [5.0, 0.0]], reversed);
    }
}
