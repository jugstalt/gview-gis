using Xunit;
using gView.Framework.NetworkModel.Query;

namespace gView.Framework.NetworkModel.Tests;

public class NetworkQueryResultTests
{
    private static NetworkQueryResult Result(int nodes, int edges)
    {
        var result = new NetworkQueryResult { Operation = "feeders" };
        for (int i = 0; i < nodes; i++)
        {
            result.Nodes.Add(new NetworkResultNode(i, $"n{i}", "Trafo", i, "Trafo", "Source", 0, "", "", 0, 0));
        }
        for (int i = 0; i < edges; i++)
        {
            result.Edges.Add(new NetworkResultEdge(i, $"e{i}", "Leitung", i, "Leitung", 1, 0, 1, 0, 1, 0, 0, 1, 0));
        }
        return result;
    }

    [Fact]
    public void Truncate_FewNodesButManyEdges_CutsTheEdges()
    {
        var result = Result(nodes: 3, edges: 25);

        result.Truncate(10);

        Assert.True(result.ExceededLimit);
        Assert.Equal(3, result.Nodes.Count);
        Assert.Equal(10, result.Edges.Count);
    }

    [Fact]
    public void Truncate_BelowTheLimit_KeepsEverything()
    {
        var result = Result(nodes: 10, edges: 10);

        result.Truncate(10);

        Assert.False(result.ExceededLimit);
        Assert.Equal(10, result.Edges.Count);
    }
}
