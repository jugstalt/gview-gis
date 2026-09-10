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
/// no gView BinaryTree, an SQLite R-Tree instead.
/// <para>
/// GeoPackage is <b>mod_spatialite-free</b> (GPB blob + R-Tree handled by gView) so those cases run
/// unconditionally; the SpatiaLite cases are skipped when no <c>mod_spatialite</c> is available.
/// </para>
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

    /// <summary>
    /// GeoPackage needs no native library, so its cases always run. The SpatiaLite flavor needs
    /// <c>mod_spatialite</c>; when it is missing the test returns early (treated as inconclusive)
    /// rather than failing on a machine / CI without it.
    /// </summary>
    private static bool ModSpatialiteAvailable()
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

        return _modSpatialiteOk == true;
    }

    private async Task<SQLiteFDB> CreateFdbAsync(GeometryStorageType storage, GeometryType geometryType)
    {
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
        if (storage == GeometryStorageType.SpatiaLite && !ModSpatialiteAvailable()) return;

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
        if (storage == GeometryStorageType.SpatiaLite && !ModSpatialiteAvailable()) return;

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

    [Fact]
    public async Task GeoPackageFile_HasValidGpkgMetadata_WithoutModSpatialite()
    {
        var fdb = await CreateFdbAsync(GeometryStorageType.GeoPackage, GeometryType.Polygon);
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

        // plain SQLite - no mod_spatialite: the file carries valid GeoPackage metadata
        using var conn = new SQLiteConnection("Data Source=" + _dbPath);
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA application_id";
            Assert.Equal(gView.DataSources.GeoPackage.GeoPackageSchema.ApplicationId, Convert.ToInt64(cmd.ExecuteScalar()));
        }

        Assert.True(TableExists(conn, "gpkg_contents"));
        Assert.True(TableExists(conn, "gpkg_geometry_columns"));
        Assert.True(TableExists(conn, "rtree_FC_geo_FDB_SHAPE"), "GeoPackage R-Tree table");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT count(*) FROM gpkg_contents WHERE table_name='FC_geo' AND data_type='features'";
            Assert.Equal(1, Convert.ToInt32(cmd.ExecuteScalar()));

            cmd.CommandText = "SELECT count(*) FROM rtree_FC_geo_FDB_SHAPE";
            Assert.Equal(1, Convert.ToInt32(cmd.ExecuteScalar()));

            // the FDB_SHAPE blob is a standard GeoPackage binary ('GP' magic)
            cmd.CommandText = "SELECT FDB_SHAPE FROM FC_geo LIMIT 1";
            var blob = (byte[])cmd.ExecuteScalar();
            Assert.True(gView.DataSources.GeoPackage.GpkgGeometry.IsGpb(blob), "FDB_SHAPE is a GPB blob");
        }
    }

    [Fact]
    public async Task RebuildNativeSpatialIndex_GeoPackage_RefillsTheRTree()
    {
        var fdb = await CreateFdbAsync(GeometryStorageType.GeoPackage, GeometryType.Point);
        var fc = await GetFcAsync(fdb);

        Assert.True(await fdb.Insert(fc, new List<IFeature>
        {
            PointFeature(100, 100, "a"),
            PointFeature(500, 500, "b"),
            PointFeature(9000, 9000, "c"),
        }), fdb.LastErrorMessage);

        // simulate a corrupt / stale index: empty the R-Tree behind gView's back
        using (var conn = new SQLiteConnection("Data Source=" + _dbPath))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM rtree_FC_geo_FDB_SHAPE";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "SELECT count(*) FROM rtree_FC_geo_FDB_SHAPE";
            Assert.Equal(0, Convert.ToInt32(cmd.ExecuteScalar()));
        }

        Assert.True(await fdb.RebuildNativeSpatialIndexAsync("geo"), fdb.LastErrorMessage);

        using (var conn = new SQLiteConnection("Data Source=" + _dbPath))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT count(*) FROM rtree_FC_geo_FDB_SHAPE";
            Assert.Equal(3, Convert.ToInt32(cmd.ExecuteScalar()));
        }

        // and the rebuilt index actually filters
        var hits = await DrainAsync(await fdb.Query(fc, new SpatialFilter
        {
            SubFields = "*",
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            Geometry = new Envelope(0, 0, 600, 600),
        }));
        Assert.Equal(new[] { "a", "b" },
            hits.Select(f => f.FindField("NAME")!.Value!.ToString()).OrderBy(x => x).ToArray());
    }

    [Fact]
    public async Task SpatiaLiteFile_OpensAsSpatiaLiteDataset()
    {
        if (!ModSpatialiteAvailable()) return;

        var fdb = await CreateFdbAsync(GeometryStorageType.SpatiaLite, GeometryType.Polygon);
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

        var external = new SpatiaLiteDataset();
        Assert.True(await external.SetConnectionString(_dbPath));
        Assert.True(await external.Open(), external.LastErrorMessage);

        var extFc = (await external.Elements()).Select(e => e.Class).OfType<IFeatureClass>()
                            .FirstOrDefault(c => string.Equals(c.Name, "FC_geo", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(extFc);
        Assert.Equal(GeometryType.Polygon, extFc!.GeometryType);
        external.Dispose();
    }
}
