using System.Data.SQLite;
using IoPath = System.IO.Path;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC;

namespace gView.DataSources.Fdb.SQLite.Tests;

/// <summary>
/// End-to-end tests for a SQLite FDB whose dataset stores geometry as standard WKB
/// (<see cref="GeometryStorageType.Wkb"/>) instead of the gView-proprietary blob. Creates a real
/// temp <c>.sqlite</c> FDB, a dataset, a feature class, inserts geometries and reads them back
/// through the normal cursor path.
/// </summary>
public class SQLiteFdbNativeStorageTests : IDisposable
{
    private readonly string _dbPath;

    public SQLiteFdbNativeStorageTests()
    {
        _dbPath = IoPath.Combine(IoPath.GetTempPath(), $"gview_fdb_native_{Guid.NewGuid():N}.sqlite");
    }

    public void Dispose()
    {
        try
        {
            SQLiteConnection.ClearAllPools();
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch { }
    }

    private async Task<SQLiteFDB> CreateFdbAsync(GeometryStorageType storage)
    {
        var fdb = new SQLiteFDB();
        Assert.True(fdb.Create(_dbPath), "FDB Create failed: " + fdb.LastErrorMessage);
        Assert.True(await fdb.Open("Data Source=" + _dbPath));

        var bounds = new Envelope(0, 0, 1000, 1000);
        var sRef = SpatialReference.FromID("epsg:25832");

        var sIndexDef = new gViewSpatialIndexDef(bounds, 20) { StorageType = storage };

        int dsId = await fdb.CreateDataset("ds", sRef, sIndexDef);
        Assert.True(dsId > 0, fdb.LastErrorMessage);

        var fields = new FieldCollection();
        fields.Add(new Field("NAME", FieldType.String) { size = 50 });

        int fcId = await fdb.CreateFeatureClass("ds", "pts", new GeometryDef(GeometryType.Point), fields);
        Assert.True(fcId > 0, fdb.LastErrorMessage);
        Assert.True(await fdb.SetSpatialIndexBounds("pts", "BinaryTree2", bounds, 0.55, 500, 20), fdb.LastErrorMessage);

        return fdb;
    }

    private static async Task<IFeatureClass> GetFeatureClassAsync(SQLiteFDB fdb, string ds, string fc)
    {
        var dataset = await fdb.GetDataset(ds);
        var element = await dataset.Element(fc);
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
        while ((f = await cursor.NextFeature()) != null)
        {
            list.Add(f);
        }
        cursor.Dispose();
        return list;
    }

    [Theory]
    [InlineData(GeometryStorageType.Wkb)]
    [InlineData(GeometryStorageType.Classic)]
    public async Task InsertAndQuery_RoundTripsGeometry(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage);
        var fc = await GetFeatureClassAsync(fdb, "ds", "pts");

        var features = new List<IFeature>
        {
            PointFeature(100, 100, "a"),
            PointFeature(250.5, 400.25, "b"),
            PointFeature(900, 900, "c"),
        };
        Assert.True(await fdb.Insert(fc, features), fdb.LastErrorMessage);

        var filter = new QueryFilter { SubFields = "*" };
        var read = await DrainAsync(await fdb.Query(fc, filter));

        Assert.Equal(3, read.Count);
        var byName = read.ToDictionary(f => f.FindField("NAME")!.Value!.ToString()!, f => (IPoint)f.Shape);
        Assert.Equal(100, byName["a"].X);
        Assert.Equal(100, byName["a"].Y);
        Assert.Equal(250.5, byName["b"].X);
        Assert.Equal(400.25, byName["b"].Y);
    }

    [Fact]
    public async Task WkbDataset_BumpsFdbVersionTo8AndStoresRealWkb()
    {
        var fdb = await CreateFdbAsync(GeometryStorageType.Wkb);
        Assert.True(fdb.FdbVersion >= new Version(8, 0, 0), $"FdbVersion={fdb.FdbVersion}");

        var fc = await GetFeatureClassAsync(fdb, "ds", "pts");
        Assert.True(await fdb.Insert(fc, new List<IFeature> { PointFeature(123, 456, "x") }), fdb.LastErrorMessage);

        // the raw FDB_SHAPE blob must be parseable as standard WKB
        using var conn = new SQLiteConnection("Data Source=" + _dbPath);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT FDB_SHAPE FROM FC_pts LIMIT 1";
        var blob = (byte[])cmd.ExecuteScalar();

        Assert.Equal(0x01, blob[0]); // NDR
        var geom = OGC.WKBToGeometry(blob);
        var p = Assert.IsAssignableFrom<IPoint>(geom);
        Assert.Equal(123, p.X);
        Assert.Equal(456, p.Y);
    }

    [Fact]
    public async Task ClassicDataset_DoesNotBumpFdbVersion()
    {
        var fdb = await CreateFdbAsync(GeometryStorageType.Classic);
        Assert.Equal(new Version(1, 2, 0), fdb.FdbVersion);
    }

    [Fact]
    public async Task NativeStorageDataset_DoesNotCreateGViewSpatialIndex()
    {
        // A PostGIS dataset must not get an FDB_NID column, no FCSI_ spatial-index table,
        // and must persist its storage kind so tools (FdbImport, the DataExplorer) can detect it.
        var fdb = new SQLiteFDB();
        Assert.True(fdb.Create(_dbPath), "FDB Create failed: " + fdb.LastErrorMessage);
        Assert.True(await fdb.Open("Data Source=" + _dbPath));

        // no bounds / max levels given - native storage must still be persisted
        var sIndexDef = new PostGisSpatialIndexDef();
        int dsId = await fdb.CreateDataset("nds", SpatialReference.FromID("epsg:25832"), sIndexDef);
        Assert.True(dsId > 0, fdb.LastErrorMessage);

        int fcId = await fdb.CreateFeatureClass("nds", "npts", new GeometryDef(GeometryType.Point), new FieldCollection());
        Assert.True(fcId > 0, fdb.LastErrorMessage);

        // read the dataset's storage kind back the way FdbImport does
        var readBack = await fdb.SpatialIndexDef("nds");
        Assert.Equal(GeometryStorageType.PostGis, readBack.StorageType);

        Assert.True(fdb.FdbVersion >= new Version(8, 0, 0));

        using var conn = new SQLiteConnection("Data Source=" + _dbPath);
        conn.Open();

        Assert.False(TableExists(conn, "FCSI_npts"), "FCSI_npts (gView spatial index) must not be created for native storage");
        Assert.False(ColumnExists(conn, "FC_npts", "FDB_NID"), "FDB_NID must not be created for native storage");
    }

    private static bool TableExists(SQLiteConnection conn, string name)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name=@n";
        cmd.Parameters.AddWithValue("@n", name);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    private static bool ColumnExists(SQLiteConnection conn, string table, string column)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT count(*) FROM pragma_table_info('{table}') WHERE name=@c";
        cmd.Parameters.AddWithValue("@c", column);
        return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
    }

    [Fact]
    public async Task WkbDataset_SpatialQueryUsesNidIndex()
    {
        var fdb = await CreateFdbAsync(GeometryStorageType.Wkb);
        var fc = await GetFeatureClassAsync(fdb, "ds", "pts");

        await fdb.Insert(fc, new List<IFeature>
        {
            PointFeature(100, 100, "in"),
            PointFeature(500, 500, "in2"),
            PointFeature(950, 950, "out"),
        });

        var filter = new SpatialFilter
        {
            SubFields = "*",
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            Geometry = new Envelope(0, 0, 600, 600),
        };
        var read = await DrainAsync(await fdb.Query(fc, filter));

        var names = read.Select(f => f.FindField("NAME")!.Value!.ToString()).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "in", "in2" }, names);
    }
}
