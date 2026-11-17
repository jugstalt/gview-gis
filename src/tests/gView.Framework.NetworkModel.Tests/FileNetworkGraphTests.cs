using Xunit;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using SpatialNET;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Tests;

public class FileNetworkGraphTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    private const int Kasten = 0, Kunde = 1, KastenType = 15, KundeType = 16;

    /// <summary>A box (Kasten#1) with a customer (Kunde#7) merged into it, and a customer of its own (Kunde#8).</summary>
    private FileNetworkGraph BuildNetwork()
    {
        var box = new FeatureRef(Kasten, 1);
        var merged = new FeatureRef(Kunde, 7);
        var alone = new FeatureRef(Kunde, 8);
        var metadata = new NetworkMetadata
        {
            Name = "test",
            Facets = [new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Kasten", [2] = "Kunde" } }],
            Types =
            [
                new NetworkTypeMetadata { Id = KastenType, Kind = ElementKind.Node, Facets = [1], Name = "Kasten", Used = true },
                new NetworkTypeMetadata { Id = KundeType, Kind = ElementKind.Node, Facets = [2], Name = "Kunde", Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = Kasten, Layer = "Niederspannung/Kasten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = Kunde, Layer = "Niederspannung/Kunde", Kind = ElementKind.Node },
            ],
        };

        using (var writer = new FileNetworkStore(_directory).CreateWriter())
        {
            writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(NetworkIds.NodeId(box), KastenType, new Coordinate(0, 0)), 1, false, box)
            {
                Merged = [new CoincidentNode<long, FeatureRef>(NetworkIds.NodeId(merged), KundeType, new Coordinate(0.004, 0), merged)],
            });
            writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(NetworkIds.NodeId(alone), KundeType, new Coordinate(10, 0)), 1, false, alone));
            writer.Commit(metadata);
        }

        return new FileNetworkGraph(_directory);
    }

    [Fact]
    public void ResolveNodes_MergedPointByLayerAndOid_FindsTheNodeItWasMergedInto()
    {
        var graph = BuildNetwork();

        var id = graph.ResolveNode("Kunde#7");

        Assert.Equal(NetworkIds.NodeId(new FeatureRef(Kasten, 1)), id);
    }

    [Fact]
    public void ResolveNodes_TypePattern_IncludesNodesWithMergedPointsOfTheType()
    {
        var graph = BuildNetwork();

        var ids = graph.ResolveNodes("Kunde");

        Assert.Equal(
            new[] { NetworkIds.NodeId(new FeatureRef(Kasten, 1)), NetworkIds.NodeId(new FeatureRef(Kunde, 8)) }.Order(),
            ids.Order());
    }

    [Fact]
    public void AliasesOf_NodeWithMergedPoint_ListsItWithItsType()
    {
        var graph = BuildNetwork();
        var box = NetworkIds.NodeId(new FeatureRef(Kasten, 1));

        var aliases = graph.AliasesOf(box);

        var alias = Assert.Single(aliases);
        Assert.Equal("Niederspannung/Kunde#7", graph.NodeLabel(alias));
        Assert.Equal(KundeType, graph.AliasTypeOf(alias));
        Assert.Equal(1, graph.Metadata.AliasCount);
    }

    [Theory]
    [InlineData("Kasten#1")]
    [InlineData("Niederspannung/Kasten#1")]
    [InlineData("kasten#1")]
    public void ResolveNode_LayerNameOrPath_FindsTheNode(string reference)
    {
        var graph = BuildNetwork();

        Assert.Equal(NetworkIds.NodeId(new FeatureRef(Kasten, 1)), graph.ResolveNode(reference));
    }

    [Fact]
    public void ResolveNodes_RawIdsAndCommaList_AreResolvedDistinct()
    {
        var graph = BuildNetwork();
        var box = NetworkIds.NodeId(new FeatureRef(Kasten, 1));

        var ids = graph.ResolveNodes($"{box}, Kasten#1, Kunde#8");

        Assert.Equal(new[] { box, NetworkIds.NodeId(new FeatureRef(Kunde, 8)) }.Order(), ids.Order());
    }

    [Theory]
    [InlineData("Trafo#1")]
    [InlineData("Kasten#999")]
    [InlineData("12345")]
    public void ResolveNodes_Unknown_Throws(string reference)
    {
        var graph = BuildNetwork();

        Assert.Throws<ArgumentException>(() => graph.ResolveNodes(reference));
    }

    [Fact]
    public void ResolveNode_PatternWithSeveralNodes_Throws()
    {
        var graph = BuildNetwork();

        Assert.Throws<ArgumentException>(() => graph.ResolveNode("Kunde"));
    }

    [Fact]
    public void AliasIndex_MissingFile_IsEmpty()
    {
        var index = AliasIndex.Read(Path.Combine(_directory, AliasIndex.FileName));

        Assert.Equal(0, index.Count);
        Assert.Empty(index.AliasesOf(1));
    }
}
