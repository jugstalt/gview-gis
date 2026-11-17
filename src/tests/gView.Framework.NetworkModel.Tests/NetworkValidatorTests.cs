using Xunit;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using gView.Framework.NetworkModel.Validation;
using SpatialNET;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkValidatorTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-network-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    private const int Kasten = 0, Kunde = 1, Kabel = 2;
    private const int KastenType = 1, KundeType = 2, KabelType = 3;

    private static long Node(int fc, long oid) => NetworkIds.NodeId(new FeatureRef(fc, oid));

    /// <summary>
    /// Shard 1: Kasten#1 —Kabel#1— Kunde#8 —Kabel#2 (to shard 2)— Kasten#2; shard 2 also holds Kunde#9 without a cable.
    /// Kunde#7 is merged into Kasten#1.
    /// </summary>
    private FileNetworkGraph BuildNetwork(List<NetworkRuleConfig>? rules = null)
    {
        var metadata = new NetworkMetadata
        {
            Name = "test",
            Facets = [new NetworkFacetMetadata { Name = "Objektart", Values = new() { [1] = "Kasten", [2] = "Kunde", [3] = "Kabel" } }],
            Types =
            [
                new NetworkTypeMetadata { Id = KastenType, Kind = ElementKind.Node, Facets = [1], Name = "Kasten", Used = true },
                new NetworkTypeMetadata { Id = KundeType, Kind = ElementKind.Node, Facets = [2], Name = "Kunde", Used = true },
                new NetworkTypeMetadata { Id = KabelType, Kind = ElementKind.Edge, Facets = [3], Name = "Kabel", Used = true },
            ],
            FeatureClasses =
            [
                new NetworkFeatureClassMetadata { Index = Kasten, Layer = "NS/Kasten", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = Kunde, Layer = "NS/Kunde", Kind = ElementKind.Node },
                new NetworkFeatureClassMetadata { Index = Kabel, Layer = "NS/Kabel", Kind = ElementKind.Edge },
            ],
            Rules = rules ?? [],
        };

        using (var writer = new FileNetworkStore(_directory).CreateWriter())
        {
            void WriteNode(int fc, long oid, int type, double x, long shard, FeatureRef? merged = null)
            {
                var source = new FeatureRef(fc, oid);
                writer.WriteNode(new BuiltNode<long, FeatureRef>(new GraphNode<long>(Node(fc, oid), type, new Coordinate(x, 0)), shard, false, source)
                {
                    Merged = merged is FeatureRef m ? [new CoincidentNode<long, FeatureRef>(NetworkIds.NodeId(m), KundeType, new Coordinate(x, 0), m)] : null,
                });
            }
            void WriteEdge(long oid, long from, long to, long fromShard, long toShard)
            {
                var source = new FeatureRef(Kabel, oid);
                writer.WriteEdge(new BuiltEdge<long, long, FeatureRef>(
                    new GraphEdge<long, long>(NetworkIds.Compose(Kabel, oid), from, to, KabelType, 10), source, 0, 10, fromShard, toShard));
            }

            WriteNode(Kasten, 1, KastenType, 0, 1, merged: new FeatureRef(Kunde, 7));
            WriteNode(Kunde, 8, KundeType, 10, 1);
            WriteNode(Kasten, 2, KastenType, 20, 2);
            WriteNode(Kunde, 9, KundeType, 30, 2);
            WriteEdge(1, Node(Kasten, 1), Node(Kunde, 8), 1, 1);
            WriteEdge(2, Node(Kunde, 8), Node(Kasten, 2), 1, 2);
            writer.Commit(metadata);
        }

        return new FileNetworkGraph(_directory);
    }

    private static NetworkRuleConfig CustomerConnected() => new()
    {
        Id = "kunde-angeschlossen",
        Subject = RuleSubject.Node,
        SubjectTypes = "Kunde",
        CountTypes = "Kabel",
        Min = 1,
    };

    private static NetworkRuleConfig NoOtherCableAtCustomer() => new()
    {
        Id = "kabel-am-kunde",
        Severity = RuleSeverity.Warning,
        Subject = RuleSubject.Edge,
        SubjectTypes = "Kabel",
        EndFilter = RuleEndFilter.NodeIs,
        EndTypes = "Kunde",
        CountTypes = "Kabel",
        Min = 0,
        Max = 0,
    };

    [Fact]
    public void Validate_NodeRule_ReportsTheCustomerWithoutCable()
    {
        var graph = BuildNetwork();
        var validator = new NetworkValidator(graph, [CustomerConnected()]);

        var violations = validator.Validate().ToList();

        var violation = Assert.Single(violations);
        Assert.Equal("kunde-angeschlossen", violation.RuleId);
        Assert.Equal(Node(Kunde, 9), violation.Node);
        Assert.Equal(0, violation.Count);
        var summary = Assert.Single(validator.Summaries);
        Assert.Equal(2, summary.Checked);
        Assert.Equal(1, summary.Violations);
    }

    [Fact]
    public void Validate_EdgeRuleAcrossShards_ChecksEveryEdgeOnce()
    {
        var graph = BuildNetwork();
        var validator = new NetworkValidator(graph, [NoOtherCableAtCustomer()]);

        var violations = validator.Validate().ToList();

        Assert.Equal(2, violations.Count);
        Assert.All(violations, v => Assert.Equal(Node(Kunde, 8), v.Node));
        Assert.Equal(
            new[] { NetworkIds.Compose(Kabel, 1), NetworkIds.Compose(Kabel, 2) },
            violations.Select(v => v.Edge).Order());
        var summary = Assert.Single(validator.Summaries);
        Assert.Equal(2, summary.Checked);
        Assert.Equal(RuleSeverity.Warning, summary.Rule.Severity);
    }

    [Fact]
    public void Validate_NodeWithMergedPoint_IsCheckedAsItsOwnType()
    {
        // Kunde#7 is merged into Kasten#1: the node is a Kasten and is not checked by a customer rule
        var graph = BuildNetwork();
        var validator = new NetworkValidator(graph, [CustomerConnected()]);

        var violations = validator.Validate().ToList();

        Assert.DoesNotContain(violations, v => v.Node == Node(Kasten, 1));
        Assert.Equal("NS/Kunde#7", graph.NodeLabel(Assert.Single(graph.AliasesOf(Node(Kasten, 1)))));
    }

    [Fact]
    public void Constructor_UnknownTypeInPattern_ThrowsNamingTheRule()
    {
        var graph = BuildNetwork();
        var rule = CustomerConnected();
        rule.CountTypes = "Freileitung";

        var ex = Assert.Throws<InvalidDataException>(() => new NetworkValidator(graph, [rule]));

        Assert.Contains("kunde-angeschlossen", ex.Message);
    }

    [Fact]
    public void Constructor_DuplicateRuleIds_Throws()
    {
        var graph = BuildNetwork();

        var ex = Assert.Throws<InvalidDataException>(() => new NetworkValidator(graph, [CustomerConnected(), CustomerConnected()]));

        Assert.Contains("used twice", ex.Message);
    }

    [Fact]
    public void Metadata_RulesWrittenOnCommit_AreReadBack()
    {
        var graph = BuildNetwork([CustomerConnected(), NoOtherCableAtCustomer()]);

        Assert.Equal(["kunde-angeschlossen", "kabel-am-kunde"], graph.Metadata.Rules.Select(r => r.Id));
        Assert.Equal(RuleEndFilter.NodeIs, graph.Metadata.Rules[1].EndFilter);
        Assert.Null(graph.Metadata.Rules[0].Max);
    }
}
