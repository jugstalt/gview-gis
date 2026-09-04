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

    /// <summary>True for a database-native geometry column (PostGIS / SQL Server) that manages its own index.</summary>
    public static bool IsNativeDbGeometry(this GeometryStorageType storage)
        => storage is GeometryStorageType.PostGis
                   or GeometryStorageType.SqlServerGeometry
                   or GeometryStorageType.SqlServerGeography;

    /// <summary>Short human-readable label for the content list / ribbon.</summary>
    public static string StorageLabel(this GeometryStorageType storage) => storage switch
    {
        GeometryStorageType.Wkb => "gView (WKB)",
        GeometryStorageType.PostGis => "PostGIS (native)",
        GeometryStorageType.SqlServerGeometry => "SQL Server geometry (native)",
        GeometryStorageType.SqlServerGeography => "SQL Server geography (native)",
        _ => "gView managed",
    };
}
