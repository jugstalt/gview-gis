using gView.Framework.Core.Data;
using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb;

/// <summary>
/// Implemented by FDB dataset / feature-class explorer objects so context tools can tell
/// whether the geometry is stored in the gView-classic blob (with the gView spatial index)
/// or in a database-native geometry column (PostGIS / SQL Server) that manages its own index.
/// </summary>
internal interface IFdbGeometryStorageContext
{
    GeometryStorageType GeometryStorage { get; }
}

internal static class FdbGeometryStorageContextExtensions
{
    /// <summary>
    /// True when the gView-managed spatial index (and its Shrink / Repair / Definition tools)
    /// applies to <paramref name="exObject"/>. Classic and WKB storage keep the gView BinaryTree;
    /// PostGIS / SQL Server geometry columns manage their own index, so those tools do not apply.
    /// Explorer objects that don't expose storage info are treated as classic (tools shown).
    /// </summary>
    public static bool UsesGViewSpatialIndex(IExplorerObject? exObject)
        => exObject is not IFdbGeometryStorageContext ctx
           || ctx.GeometryStorage is GeometryStorageType.Classic or GeometryStorageType.Wkb;
}
