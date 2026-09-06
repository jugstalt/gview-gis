using gView.Framework.Core.Data;
using gView.Framework.DataExplorer.Abstraction;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb;

/// <summary>
/// Implemented by FDB dataset / feature-class explorer objects so context tools can tell
/// whether the geometry is stored in the gView-classic blob (with the gView spatial index)
/// or in a database-native format (PostGIS / SQL Server / SpatiaLite / GeoPackage) that
/// manages its own index.
/// </summary>
internal interface IFdbGeometryStorageContext
{
    GeometryStorageType GeometryStorage { get; }
}

internal static class FdbGeometryStorageContextExtensions
{
    /// <summary>
    /// True when the gView-managed spatial index (and its Shrink / Repair / Definition tools)
    /// applies to <paramref name="exObject"/>. Only <see cref="GeometryStorageType.Classic"/> keeps
    /// the gView BinaryTree; the database-native formats manage their own index. Explorer objects
    /// that don't expose storage info are treated as classic (tools shown).
    /// </summary>
    public static bool UsesGViewSpatialIndex(IExplorerObject? exObject)
        => exObject is not IFdbGeometryStorageContext ctx
           || !ctx.GeometryStorage.IsDatabaseNative();

    /// <summary>Short human-readable label for the content list / ribbon.</summary>
    public static string StorageLabel(this GeometryStorageType storage) => storage switch
    {
        GeometryStorageType.PostGis => "PostGIS (native)",
        GeometryStorageType.SqlServerGeometry => "SQL Server geometry (native)",
        GeometryStorageType.SqlServerGeography => "SQL Server geography (native)",
        GeometryStorageType.SpatiaLite => "SpatiaLite (native)",
        GeometryStorageType.GeoPackage => "GeoPackage (native)",
        _ => "gView managed",
    };
}
