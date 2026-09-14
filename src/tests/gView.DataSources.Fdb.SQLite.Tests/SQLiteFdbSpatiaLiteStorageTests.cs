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

    /// <summary>
    /// Regression test: GeoPackage has no in-database ST_Intersects, only the R-Tree bbox
    /// pre-filter - the precise relation is re-checked per row in managed code. A SQL-level
    /// LIMIT applied before that check would truncate the candidate set to bbox-only matches,
    /// so true matches beyond the (false-positive-laden) LIMIT window would silently be lost.
    /// </summary>
    [Fact]
    public async Task SpatialFilter_Intersects_WithLimitSmallerThanBboxCandidates_GeoPackage_StillFindsTrueMatches()
    {
        var fdb = await CreateFdbAsync(GeometryStorageType.GeoPackage, GeometryType.Point);
        var fc = await GetFcAsync(fdb);

        // Right triangle (0,0)-(100,0)-(0,100): points with x+y<=100 are inside. The other
        // points sit inside the triangle's bounding envelope (0,0)-(100,100) - so the R-Tree
        // bbox pre-filter accepts them as candidates - but are outside the triangle itself.
        var falsePositives = new (double x, double y)[] { (90, 90), (95, 50), (50, 95), (80, 80), (99, 99) };
        var truePositives = new (double x, double y)[] { (10, 10), (20, 20), (30, 10) };

        var features = new List<IFeature>();
        foreach (var (x, y) in falsePositives) features.Add(PointFeature(x, y, "false"));
        foreach (var (x, y) in truePositives) features.Add(PointFeature(x, y, "true"));

        Assert.True(await fdb.Insert(fc, features), fdb.LastErrorMessage);

        var triangleRing = new Ring();
        triangleRing.AddPoint(new Point(0, 0));
        triangleRing.AddPoint(new Point(100, 0));
        triangleRing.AddPoint(new Point(0, 100));

        var filter = new SpatialFilter
        {
            SubFields = "*",
            SpatialRelation = spatialRelation.SpatialRelationIntersects,
            Geometry = new Polygon(triangleRing),
            Limit = 2,
        };

        // Insertion order puts the 5 false positives first (lower FDB_OID), so a SQL-level
        // "LIMIT 2" on the bbox candidates would grab only false positives and, after the
        // precise check discards them, return zero features.
        var read = await DrainAsync(await fdb.Query(fc, filter));

        Assert.Equal(2, read.Count);
        Assert.All(read, f => Assert.Equal("true", f.FindField("NAME")!.Value!.ToString()));
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
    public async Task Insert_SpatiaLite_PolygonWithHoleOutsideShell_IsRepairedNotNulled()
    {
        if (!ModSpatialiteAvailable()) return;

        var fdb = await CreateFdbAsync(GeometryStorageType.SpatiaLite, GeometryType.Polygon);
        var fc = await GetFcAsync(fdb);

        // outer 10x10 shell, inner "hole" ring that pokes out to x=15 -> ST_MakeValid hands
        // back a collection; ST_CollectionExtract(...,3) must keep it a MULTIPOLYGON so the
        // row is not stored as NULL.
        var poly = new Polygon();
        var shell = new Ring();
        shell.AddPoint(new Point(0, 0)); shell.AddPoint(new Point(10, 0));
        shell.AddPoint(new Point(10, 10)); shell.AddPoint(new Point(0, 10));
        poly.AddRing(shell);
        var hole = new Ring();
        hole.AddPoint(new Point(5, 4)); hole.AddPoint(new Point(15, 4));
        hole.AddPoint(new Point(15, 8)); hole.AddPoint(new Point(5, 8));
        poly.AddRing(hole);

        var pf = new Feature { Shape = poly };
        pf.Fields.Add(new FieldValue("NAME", "bad"));
        Assert.True(await fdb.Insert(fc, new List<IFeature> { pf }), fdb.LastErrorMessage);

        // the row must exist with a non-NULL blob (before the fix ST_MakeValid returned a
        // GEOMETRYCOLLECTION -> CastToMultiPolygon -> NULL)
        using (var conn = new SQLiteConnection("Data Source=" + _dbPath))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT count(*) FROM FC_geo WHERE FDB_SHAPE IS NULL";
            Assert.Equal(0, Convert.ToInt32(cmd.ExecuteScalar()));
        }

        // and it round-trips as a polygon geometry
        var read = await DrainAsync(await fdb.Query(fc, new QueryFilter { SubFields = "*" }));
        var f = Assert.Single(read);
        Assert.Equal("bad", f.FindField("NAME")!.Value!.ToString());
        Assert.IsAssignableFrom<IPolygon>(f.Shape);
        Assert.False(f.Shape.IsEmpty());
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
