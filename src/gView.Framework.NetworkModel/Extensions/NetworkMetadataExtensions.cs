using gView.Framework.NetworkModel.Store;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Extensions;

internal static class NetworkMetadataExtensions
{
    /// <summary>The types of the network, to resolve patterns against.</summary>
    public static TypeRegistry ToTypeRegistry(this NetworkMetadata metadata)
        => new(
            metadata.Facets.Select(f => new Facet(f.Name, f.Values)).ToList(),
            metadata.Types.Select(t => new NetworkType(t.Id, t.Kind, t.Facets, t.Name)));
}
