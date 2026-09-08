using gView.Framework.Core.Data;
using System;

namespace gView.DataSources.Fdb.SQLite;

/// <summary>
/// File-naming convention for a SQLite gView Feature Database. The extension advertises the
/// geometry-storage flavor of the whole file to other tools while keeping the <c>.fdb</c> marker
/// so gView still recognises it:
/// <list type="bullet">
///   <item><c>*.fdb</c>        - classic (gView proprietary blob + BinaryTree)</item>
///   <item><c>*.fdb.gpkg</c>   - GeoPackage (QGIS / GDAL open it as a GeoPackage)</item>
///   <item><c>*.fdb.sqlite</c> - SpatiaLite</item>
/// </list>
/// One flavor per file, fixed at file creation; every dataset in the file uses it.
/// </summary>
public static class SqliteFdbFile
{
    public const string ClassicExtension = ".fdb";
    public const string GeoPackageExtension = ".fdb.gpkg";
    public const string SpatiaLiteExtension = ".fdb.sqlite";

    /// <summary>
    /// Whether the <c>*.fdb.sqlite</c> (SpatiaLite) flavor can be used - it needs a deployed
    /// <c>mod_spatialite</c>. GeoPackage (<c>*.fdb.gpkg</c>) and Classic have no native dependency.
    /// </summary>
    public static bool SpatiaLiteAvailable
        => gView.DataSources.SpatiaLite.SpatiaLiteNative.EnsureAvailable(out _);

    /// <summary>True for any of the three gView SQLite FDB file names.</summary>
    public static bool IsFdbFileName(string path)
        => path is not null &&
           (path.EndsWith(GeoPackageExtension, StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(SpatiaLiteExtension, StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(ClassicExtension, StringComparison.OrdinalIgnoreCase));

    /// <summary>The whole-file geometry storage implied by the file name (<see cref="GeometryStorageType.Classic"/> when unknown).</summary>
    public static GeometryStorageType StorageFromFileName(string path)
    {
        if (path is null)
        {
            return GeometryStorageType.Classic;
        }

        // order matters: ".fdb.gpkg" / ".fdb.sqlite" also end with a broader extension
        if (path.EndsWith(GeoPackageExtension, StringComparison.OrdinalIgnoreCase))
        {
            return GeometryStorageType.GeoPackage;
        }
        if (path.EndsWith(SpatiaLiteExtension, StringComparison.OrdinalIgnoreCase))
        {
            return GeometryStorageType.SpatiaLite;
        }

        return GeometryStorageType.Classic;
    }

    public static string ExtensionFor(GeometryStorageType storage) => storage switch
    {
        GeometryStorageType.GeoPackage => GeoPackageExtension,
        GeometryStorageType.SpatiaLite => SpatiaLiteExtension,
        _ => ClassicExtension,
    };

    /// <summary>Appends the flavor's extension to <paramref name="name"/> unless it already carries a recognised one.</summary>
    public static string EnsureExtension(string name, GeometryStorageType storage)
        => IsFdbFileName(name) ? name : name + ExtensionFor(storage);
}
