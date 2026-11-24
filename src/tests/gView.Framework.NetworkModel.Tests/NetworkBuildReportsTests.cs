using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Query;
using gView.Framework.NetworkModel.Store;
using SpatialNET.Network;
using Xunit;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkBuildReportsTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "gview-reports-" + Guid.NewGuid().ToString("N"));

    public NetworkBuildReportsTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, true);

    private void WriteFile(string name, string content) => File.WriteAllText(Path.Combine(_directory, name), content);

    private static NetworkMetadata Metadata() => new()
    {
        FeatureClasses =
        [
            new NetworkFeatureClassMetadata { Index = 1, Layer = "Wasser/Schacht", Kind = ElementKind.Node },
            new NetworkFeatureClassMetadata { Index = 2, Layer = "Wasser/Leitung", Kind = ElementKind.Edge },
        ],
        Facets = [new NetworkFacetMetadata { Name = "Art", Values = new() { [1] = "Schacht", [2] = "Leitung" } }],
        Types =
        [
            new NetworkTypeMetadata { Id = 1, Kind = ElementKind.Node, Facets = [1], Name = "Schacht" },
            new NetworkTypeMetadata { Id = 2, Kind = ElementKind.Edge, Facets = [2], Name = "Leitung" },
        ]
    };

    [Fact]
    public void Read_NoFiles_HasNothing()
    {
        var reports = NetworkBuildReports.Read(_directory);

        Assert.False(reports.Any);
        Assert.Null(reports.Written);
        Assert.All(NetworkBuildReports.Categories, c => Assert.Empty(reports.Of(c)));
    }

    [Fact]
    public void Read_Duplicates_DroppedFirst()
    {
        WriteFile("duplicates.json", """[ { "layer": "Wasser/Schacht", "keptOid": 1, "droppedOid": 2, "x": 10.5, "y": 20 } ]""");

        var reports = NetworkBuildReports.Read(_directory);
        var problem = Assert.Single(reports.Of(NetworkProblemCategory.Duplicates));

        Assert.True(reports.Present.Contains(NetworkProblemCategory.Duplicates));
        Assert.NotNull(reports.Written);
        Assert.Equal("Wasser/Schacht", problem.Group);
        Assert.Equal((10.5, 20.0), (problem.X, problem.Y));
        Assert.Equal(new NetworkProblemFeature("Wasser/Schacht", 2, "dropped"), problem.Features[0]);
        Assert.Equal(new NetworkProblemFeature("Wasser/Schacht", 1, "kept"), problem.Features[1]);
    }

    [Fact]
    public void Read_Merged_GroupedByLayers()
    {
        WriteFile("merged.json", """[ { "keptLayer": "A", "keptOid": 7, "mergedLayer": "B", "mergedOid": 9, "x": 1, "y": 2 } ]""");

        var problem = Assert.Single(NetworkBuildReports.Read(_directory).Of(NetworkProblemCategory.Merged));

        Assert.Equal("B → A", problem.Group);
        Assert.Equal("B#9 merged into A#7", problem.Message);
        Assert.Equal(["B#9", "A#7"], problem.Features.Select(f => f.Label));
    }

    [Fact]
    public void Read_Conflicts_TooCloseByLayers_SourcesParsed()
    {
        WriteFile("conflicts.json", """
            [
              { "kind": "RealNodesTooClose", "x": 1, "y": 1, "sources": [ "Wasser/Schieber #4", "Wasser/Hydrant #8", "Wasser/Hydrant #9" ] },
              { "kind": "DuplicateEdgeId", "x": 2, "y": 2, "sources": [ "123" ] }
            ]
            """);

        var problems = NetworkBuildReports.Read(_directory).Of(NetworkProblemCategory.Conflicts);

        Assert.Equal("Wasser/Hydrant / Wasser/Schieber", problems[0].Group);
        Assert.Equal(3, problems[0].Features.Count);
        Assert.Equal("DuplicateEdgeId", problems[1].Group);
        Assert.Empty(problems[1].Features);
        Assert.Equal("123", problems[1].Message);
    }

    [Fact]
    public void Read_Diagnostics_NodeAndLine_HelperNodeLeftOut()
    {
        WriteFile("diagnostics.json", """
            [
              { "kind": "ConnectionRefused", "x": 1, "y": 1, "message": "m", "sources": [ "Wasser/Schacht #3", "Wasser/Leitung #4" ] },
              { "kind": "SplitRefused", "x": 1, "y": 1, "message": "m", "sources": [ null, "Wasser/Leitung #5" ] },
              { "kind": "IsolatedNode", "x": 1, "y": 1, "message": "old build without sources" }
            ]
            """);

        var reports = NetworkBuildReports.Read(_directory);
        var rules = reports.Of(NetworkProblemCategory.Rules);

        Assert.Equal(["node", "line"], rules[0].Features.Select(f => f.Role));
        Assert.Equal(new NetworkProblemFeature("Wasser/Leitung", 5, "line"), Assert.Single(rules[1].Features));
        Assert.Empty(Assert.Single(reports.Of(NetworkProblemCategory.Diagnostics)).Features);
    }

    [Fact]
    public void Read_Diagnostics_ConnectionsGoToRules_ByRule()
    {
        WriteFile("diagnostics.json", """
            [
              { "kind": "SplitReported", "x": 1, "y": 1, "message": "m1", "sources": [ "K #1", "L #2" ], "rule": "LS auf Muffe", "ruleIndex": 3 },
              { "kind": "SplitRefused", "x": 2, "y": 2, "message": "m2", "sources": [ null, "L #5" ], "rule": "default: line ends on a line" },
              { "kind": "ConnectionRefused", "x": 3, "y": 3, "message": "older build", "sources": [ "K #4", "L #6" ] },
              { "kind": "IsolatedNode", "x": 4, "y": 4, "message": "m4" }
            ]
            """);

        var reports = NetworkBuildReports.Read(_directory);
        var rules = reports.Of(NetworkProblemCategory.Rules);

        Assert.True(reports.Present.Contains(NetworkProblemCategory.Rules));
        Assert.Equal(["3: LS auf Muffe", "default: line ends on a line", "ConnectionRefused"], rules.Select(p => p.Group));
        Assert.Equal("Split reported: m1", rules[0].Message);
        Assert.Equal("IsolatedNode", Assert.Single(reports.Of(NetworkProblemCategory.Diagnostics)).Group);
        Assert.Equal("Warning", reports.ToResult(NetworkProblemCategory.Rules, null).Violations[0].Severity);
    }

    [Fact]
    public void ResolveMessage_NodeIdsAndTypes()
    {
        var metadata = Metadata();
        long node = NetworkIds.Compose(1, 42);
        long other = NetworkIds.Compose(1, 43);

        Assert.Equal("The node Wasser/Schacht#42 has no line.", NetworkBuildReports.ResolveMessage($"The node {node} has no line.", metadata));
        Assert.Equal(
            "A line end of type Leitung within the tolerance of the node Wasser/Schacht#42 (type Schacht) does not connect to it.",
            NetworkBuildReports.ResolveMessage($"A line end of type 2 within the tolerance of the node {node} (type 1) does not connect to it.", metadata));
        Assert.Equal(
            "1 node(s) merged into the node Wasser/Schacht#42: Wasser/Schacht#43.",
            NetworkBuildReports.ResolveMessage($"1 node(s) merged into the node {node}: {other}.", metadata));
    }

    [Fact]
    public void ResolveMessage_IsIdempotent_AndKeepsCounts()
    {
        var metadata = Metadata();
        var once = NetworkBuildReports.ResolveMessage($"The node {NetworkIds.Compose(1, 42)} has no line.", metadata);

        Assert.Equal(once, NetworkBuildReports.ResolveMessage(once, metadata));
        Assert.Equal("12 node(s) outside every shard region got the shard x.",
            NetworkBuildReports.ResolveMessage("12 node(s) outside every shard region got the shard x.", metadata));
    }

    [Fact]
    public void ToResult_ViolationsWithFeatures()
    {
        WriteFile("duplicates.json", """[ { "layer": "L", "keptOid": 1, "droppedOid": 2, "x": 3, "y": 4 } ]""");

        var result = NetworkBuildReports.Read(_directory).ToResult(NetworkProblemCategory.Duplicates, "epsg:31256");
        var violation = Assert.Single(result.Violations);

        Assert.Equal(NetworkQueryService.ProblemsOperation, result.Operation);
        Assert.Equal("epsg:31256", result.Srs);
        Assert.Equal("Info", violation.Severity);
        Assert.Equal("L", violation.Rule);
        Assert.Equal(("L", 2L), (violation.Layer, violation.Oid));
        Assert.Equal(("L", (long?)1), (violation.EdgeLayer, violation.EdgeOid));
        Assert.Equal(2, violation.Features!.Count);
    }

    [Fact]
    public void ToResult_MissingConflicts_SaysSo()
    {
        var result = NetworkBuildReports.Read(_directory).ToResult(NetworkProblemCategory.Conflicts, null);

        Assert.Empty(result.Violations);
        Assert.Contains(result.Messages, m => m.Contains("no conflicts"));
    }

    [Theory]
    [InlineData("Wasser/Schacht #12", "Wasser/Schacht", 12L)]
    [InlineData("Schacht#7", "Schacht", 7L)]
    public void ParseFeature_Valid(string source, string layer, long oid)
        => Assert.Equal((layer, oid), NetworkBuildReports.ParseFeature(source));

    [Theory]
    [InlineData("123")]
    [InlineData("#5")]
    [InlineData("Layer #x")]
    [InlineData(null)]
    public void ParseFeature_Invalid(string? source) => Assert.Null(NetworkBuildReports.ParseFeature(source));
}
