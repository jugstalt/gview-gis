using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Extensions;

internal static class TypeSetExtensions
{
    /// <summary>The type ids both sets contain (may be empty).</summary>
    public static TypeSet Intersect(this TypeSet set, TypeSet other)
        => TypeSet.Of(set.TypeIds.Where(other.Contains));
}
