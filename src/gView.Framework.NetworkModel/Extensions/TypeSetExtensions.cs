using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Extensions;

internal static class TypeSetExtensions
{
    /// <summary>The type ids both sets contain (may be empty).</summary>
    public static TypeSet Intersect(this TypeSet set, TypeSet other)
        => TypeSet.Of(set.TypeIds.Where(other.Contains));

    /// <summary>The type ids any of the sets contains; the sets must not be empty.</summary>
    public static TypeSet UnionAll(this IEnumerable<TypeSet> sets)
        => sets.Aggregate((a, b) => a.Union(b));
}
