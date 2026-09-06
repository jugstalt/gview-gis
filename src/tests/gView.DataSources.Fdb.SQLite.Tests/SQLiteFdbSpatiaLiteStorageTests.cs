using System.Data.SQLite;
using IoPath = System.IO.Path;
using gView.DataSources.Fdb.SQLite;
using gView.DataSources.SpatiaLite;   // SpatiaLiteDataset (public)
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;

namespace gView.DataSources.Fdb.SQLite.Tests;

/// <summary>
/// End-to-end tests for a SQLite FDB whose datasets store geometry natively as SpatiaLite or
/// GeoPackage (<see cref="GeometryStorageType.SpatiaLite"/> / <see cref="GeometryStorageType.GeoPackage"/>):
/// no gView BinaryTree, an SQLite R-Tree instead, and the resulting file is a valid
/// SpatiaLite / GeoPackage that <see cref="SpatiaLiteDataset"/> can open directly.
/// Skipped when no <c>mod_spatialite</c> is available on the machine.
/// </summary>
public class SQLiteFdbSpatiaLiteStorageTests : IDisposable
{
    private readonly string _dbPath;

    public SQLiteFdbSpatiaLiteStorageTests()
    {
        _dbPath = IoPath.Combine(IoPath.GetTempPath(), $"gview_fdb_slgpkg_{Guid.NewGuid():N}.sqlite");
    }

    public void Dispose()
    {
        try
        {
            SQLiteConnection.ClearAllPools();
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
        }
        catch { }
    }

    private static bool? _modSpatialiteOk;

    private static void RequireModSpatialite()
    {
        if (_modSpatialiteOk is null)
        {
            var probe = IoPath.Combine(IoPath.GetTempPath(), $"gview_probe_{Guid.NewGuid():N}.gpkg");
            try
            {
                _modSpatialiteOk = new SpatiaLiteDataset().Create(probe);
            }
            finally
            {
                try { if (File.Exists(probe)) File.Delete(probe); } catch { }
            }
        }

        if (_modSpatialiteOk != true)
        {
            // Same requirement as gView.DataSources.SpatiaLite.Tests: a real mod_spatialite.
            throw new InvalidOperationException(
                "mod_spatialite not found. Set GVIEW_MOD_SPATIALITE or install QGIS / OSGeo4W " +
                "(Windows) or libsqlite3-mod-spatialite (Linux) to run the SpatiaLite / GeoPackage FDB tests.");
        }
    }

    private async Task<SQLiteFDB> CreateFdbAsync(GeometryStorageType storage, GeometryType geometryType)
    {
        RequireModSpatialite();

        var fdb = new SQLiteFDB();
        Assert.True(fdb.Create(_dbPath), "FDB Create failed: " + fdb.LastErrorMessage);
        Assert.True(await fdb.Open("Data Source=" + _dbPath));

        var sRef = SpatialReference.FromID("epsg:25832");
        var sIndexDef = new gViewSpatialIndexDef() { StorageType = storage, SpatialReference = sRef };

        int dsId = await fdb.CreateDataset("ds", sRef, sIndexDef);
        Assert.True(dsId > 0, fdb.LastErrorMessage);

        var fields = new FieldCollection();
        fields.Add(new Field("NAME", FieldType.String) { size = 50 });

        int fcId = await fdb.CreateFeatureClass("ds", "geo", new GeometryDef(geometryType), fields);
        Assert.True(fcId > 0, fdb.LastErrorMessage);

        return fdb;
    }

    private static async Task<IFeatureClass> GetFcAsync(SQLiteFDB fdb)
    {
        var dataset = await fdb.GetDataset("ds");
        var element = await dataset.Element("geo");
        return (IFeatureClass)element.Class;
    }

    private static Feature PointFeature(double x, double y, string name)
    {
        var f = new Feature { Shape = new Point(x, y) };
        f.Fields.Add(new FieldValue("NAME", name));
        return f;
    }

    private static async Task<List<IFeature>> DrainAsync(IFeatureCursor cursor)
    {
        var list = new List<IFeature>();
        IFeature? f;
        while ((f = await cursor.NextFeature()) != null) list.Add(f);
        cursor.Dispose();
        return list;
    }

    private static bool TableExists(SQLiteConnection c, string name)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type IN ('table','view') AND lower(name)=lower(@n)";
        cmd.Parameters.AddWithValue("@n", name);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    private static bool ColumnExists(SQLiteConnection c, string table, string column)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = $"SELECT count(*) FROM pragma_table_info('{table}') WHERE name=@c";
        cmd.Parameters.AddWithValue("@c", column);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    [Theory]
    [InlineData(GeometryStorageType.GeoPackage)]
    [InlineData(GeometryStorageType.SpatiaLite)]
    public async Task InsertQuery_RoundTripsGeometry_AndNoGViewIndex(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage, GeometryType.Point);
        var fc = await GetFcAsync(fdb);

        Assert.True(await fdb.Insert(fc, new List<IFeature>
        {
            PointFeature(100, 100, "a"),
            PointFeature(250.5, 400.25, "b"),
            PointFeature(900, 900, "c"),
        }), fdb.LastErrorMessage);

        var read = await DrainAsync(await fdb.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Equal(3, read.Count);
        var byName = read.ToDictionary(f => f.FindField("NAME")!.Value!.ToString()!, f => (IPoint)f.Shape);
        Assert.Equal(100, byName["a"].X);
        Assert.Equal(400.25, byName["b"].Y);

        Assert.True(fdb.FdbVersion >= new Version(8, 0, 0), $"FdbVersion={fdb.FdbVersion}");

        using var conn = new SQLiteConnection("Data Source=" + _dbPath);
        conn.Open();
        Assert.False(TableExists(conn, "FCSI_geo"), "no gView spatial-index table for native storage");
        Assert.False(ColumnExists(conn, "FC_geo", "FDB_NID"), "no FDB_NID for native storage");
    }

    [Theory]
    [InlineData(GeometryStorageType.GeoPackage)]
    [InlineData(GeometryStorageType.SpatiaLite)]
    public async Task SpatialFilter_UsesRTree_ReturnsOnlyIntersecting(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage, GeometryType.Point);
        var fc = await GetFcAsync(fdb);

        await fdb.Insert(fc, new List<IFeature>
        {
            PointFeature(100, 100, "in"),
            PointFeature(500, 500, "in2"),
            PointFeature(9000, 9000, "out"),
        });

        var read = await DrainAsync(await fdb.Query(fc, new SpatialFilter
        {
            SubFields = "*",
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            Geometry = new Envelope(0, 0, 600, 600),
        }));

        var names = read.Select(f => f.FindField("NAME")!.Value!.ToString()).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "in", "in2" }, names);
    }

    [Theory]
    [InlineData(GeometryStorageType.GeoPackage)]
    [InlineData(GeometryStorageType.SpatiaLite)]
    public async Task ResultingFile_OpensAsSpatiaLiteOrGeoPackage(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage, GeometryType.Polygon);
        var fc = await GetFcAsync(fdb);

        var poly = new Polygon();
        var ring = new Ring();
        ring.AddPoint(new Point(0, 0)); ring.AddPoint(new Point(0, 10));
        ring.AddPoint(new Point(10, 10)); ring.AddPoint(new Point(10, 0));
        poly.AddRing(ring);
        var pf = new Feature { Shape = poly };
        pf.Fields.Add(new FieldValue("NAME", "square"));
        Assert.True(await fdb.Insert(fc, new List<IFeature> { pf }), fdb.LastErrorMessage);

        fdb.Dispose();
        SQLiteConnection.ClearAllPools();

        // the same file is a valid SpatiaLite / GeoPackage: SpatiaLiteDataset lists FC_geo
        var external = new SpatiaLiteDataset();
        Assert.True(await external.SetConnectionString(_dbPath));
        Assert.True(await external.Open(), external.LastErrorMessage);

        var elements = await external.Elements();
        var extFc = elements.Select(e => e.Class).OfType<IFeatureClass>()
                            .FirstOrDefault(c => string.Equals(c.Name, "FC_geo", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(extFc);
        Assert.Equal(GeometryType.Polygon, extFc!.GeometryType);
        external.Dispose();
    }
}
