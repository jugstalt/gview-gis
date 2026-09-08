using System.Data.SQLite;
using IoPath = System.IO.Path;

namespace gView.DataSources.SpatiaLite.Tests;

/// <summary>
/// Locates a usable <c>mod_spatialite</c> for the test run and builds throw-away
/// SpatiaLite databases to exercise <see cref="SpatiaLiteDataset"/> against.
///
/// The datasource itself resolves the extension through
/// <c>gView.DataSources.SpatiaLite.SpatiaLiteNative</c>; here we just make sure the
/// <c>GVIEW_MOD_SPATIALITE</c> override points at a real library (bundled binaries are
/// not committed to the repo), skipping the tests otherwise.
/// </summary>
internal static class SpatiaLiteTestEnvironment
{
    private static readonly Lazy<string?> _modSpatialite = new(Resolve);

    public static string? ModSpatialitePath => _modSpatialite.Value;

    /// <summary>
    /// These tests need a real <c>mod_spatialite</c> (same as the FDB SQLite tests need
    /// the native SQLite build). If none is found, fail with an actionable message rather
    /// than passing silently.
    /// </summary>
    public static void RequireModSpatialite()
    {
        if (string.IsNullOrEmpty(ModSpatialitePath))
        {
            throw new InvalidOperationException(
                "mod_spatialite not found. Set the GVIEW_MOD_SPATIALITE environment variable to a " +
                "mod_spatialite library, or install QGIS / OSGeo4W (Windows) or the " +
                "libsqlite3-mod-spatialite package (Linux) before running these tests.");
        }
    }

    private static string? Resolve()
    {
        var fromEnv = Environment.GetEnvironmentVariable("GVIEW_MOD_SPATIALITE");
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv))
        {
            return fromEnv;
        }

        string[] candidates = OperatingSystem.IsWindows()
            ? EnumerateWindowsCandidates().ToArray()
            : new[]
            {
                "/usr/lib/x86_64-linux-gnu/mod_spatialite.so",
                "/usr/lib/mod_spatialite.so",
                "/usr/local/lib/mod_spatialite.dylib",
                "/opt/homebrew/lib/mod_spatialite.dylib",
            };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                Environment.SetEnvironmentVariable("GVIEW_MOD_SPATIALITE", candidate);
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateWindowsCandidates()
    {
        foreach (var root in new[]
                 {
                     Environment.GetEnvironmentVariable("ProgramFiles"),
                     Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                     @"C:\OSGeo4W",
                     @"C:\OSGeo4W64",
                 })
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                continue;
            }

            if (root.Contains("OSGeo4W", StringComparison.OrdinalIgnoreCase))
            {
                yield return IoPath.Combine(root, "bin", "mod_spatialite.dll");
                continue;
            }

            foreach (var dir in SafeEnumerateDirectories(root, "QGIS*"))
            {
                yield return IoPath.Combine(dir, "bin", "mod_spatialite.dll");
            }

            yield return IoPath.Combine(root, "OSGeo4W", "bin", "mod_spatialite.dll");
        }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string root, string pattern)
    {
        try
        {
            return Directory.EnumerateDirectories(root, pattern);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public static string NewTempPath(string extension)
        => IoPath.Combine(IoPath.GetTempPath(), $"gview_spatialite_{Guid.NewGuid():N}.{extension}");

    private static SQLiteConnection OpenWithSpatialite(string path)
    {
        // Runs the product's resolution + native-search-path preparation for the
        // GVIEW_MOD_SPATIALITE library, so the raw LoadExtension below (and its
        // dependency DLLs) resolve the same way the datasource does.
        Assert.True(SpatiaLiteNative.EnsureAvailable(out var error), error);

        var connection = new SQLiteConnection($"Data Source={path}");
        connection.Open();
        SpatiaLiteNative.LoadInto(connection);

        return connection;
    }

    private static void Exec(SQLiteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>Runs a scalar query against <paramref name="path"/> with mod_spatialite loaded.</summary>
    public static object? Scalar(string path, string sql, bool geoPackage = false)
    {
        using var connection = OpenWithSpatialite(path);
        if (geoPackage)
        {
            Exec(connection, "SELECT EnableGpkgAmphibiousMode()");
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }

    /// <summary>
    /// Creates a SpatiaLite database with a POINT feature class (<c>pts</c>) and a
    /// MULTIPOLYGON feature class (<c>areas</c>), both EPSG:25832, each seeded with rows.
    /// </summary>
    public static void CreateSpatiaLite(string path)
    {
        using var connection = OpenWithSpatialite(path);

        Exec(connection, "SELECT InitSpatialMetaData(1)");
        Exec(connection, "SELECT InsertEpsgSrid(25832)");

        Exec(connection, "CREATE TABLE pts (fid INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT, value INTEGER)");
        Exec(connection, "SELECT AddGeometryColumn('pts', 'geom', 25832, 'POINT', 'XY')");
        Exec(connection, "SELECT CreateSpatialIndex('pts', 'geom')");

        Exec(connection, "CREATE TABLE areas (fid INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT)");
        Exec(connection, "SELECT AddGeometryColumn('areas', 'geom', 25832, 'MULTIPOLYGON', 'XY')");
        Exec(connection, "SELECT CreateSpatialIndex('areas', 'geom')");

        Exec(connection,
            "INSERT INTO pts (name, value, geom) VALUES " +
            "('a', 10, GeomFromText('POINT(100 100)', 25832)), " +
            "('b', 20, GeomFromText('POINT(500 500)', 25832)), " +
            "('c', 30, GeomFromText('POINT(900 900)', 25832))");

        Exec(connection,
            "INSERT INTO areas (name, geom) VALUES " +
            "('poly', CastToMultiPolygon(GeomFromText('POLYGON((0 0, 200 0, 200 200, 0 200, 0 0))', 25832)))");
    }

    public static void TryDelete(string path)
    {
        try
        {
            SQLiteConnection.ClearAllPools();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // temp file - leave it for the OS to clean up
        }
    }
}
