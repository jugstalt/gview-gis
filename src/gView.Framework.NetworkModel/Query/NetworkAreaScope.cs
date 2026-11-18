using SpatialNET.Network;
using System.Collections.Concurrent;

namespace gView.Framework.NetworkModel.Query;

/// <summary>
/// An area resolved for one request: the nodes inside it, and the edges a search came to that leave it. Nodes outside do
/// not exist for the search: every edge with an end outside is switched off (<see cref="Wrap"/>).
/// </summary>
internal sealed class NetworkAreaScope(NetworkArea area, HashSet<long> inside)
{
    public NetworkArea Area => area;

    public int Count => inside.Count;

    public bool Contains(long node) => inside.Contains(node);

    public IReadOnlySet<long> Nodes => inside;

    /// <summary>The edges with one end inside and one outside that a search came to: edge id → the end inside.</summary>
    public ConcurrentDictionary<long, long> Boundary { get; } = new();

    /// <summary>The switching state of a search restricted to the area, on top of <paramref name="inner"/> (null: the normal state).</summary>
    public INetworkState<long, long> Wrap(INetworkState<long, long>? inner) => new State(this, inner);

    private sealed class State(NetworkAreaScope scope, INetworkState<long, long>? inner) : INetworkState<long, long>
    {
        public bool IsBlocked(in GraphNode<long> node) => inner?.IsBlocked(node) ?? node.Blocked;

        public bool IsDisabled(in GraphEdge<long, long> edge)
        {
            if (inner?.IsDisabled(edge) ?? edge.Disabled)
            {
                return true;
            }
            bool fromInside = scope.Contains(edge.From), toInside = scope.Contains(edge.To);
            if (fromInside && toInside)
            {
                return false;
            }
            if (fromInside || toInside)
            {
                scope.Boundary.TryAdd(edge.Id, fromInside ? edge.From : edge.To);
            }
            return true;
        }

        public EdgeDirection GetDirection(in GraphEdge<long, long> edge) => inner?.GetDirection(edge) ?? edge.Direction;
    }
}
