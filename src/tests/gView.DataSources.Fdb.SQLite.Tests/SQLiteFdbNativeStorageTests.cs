using System.Data.SQLite;
using IoPath = System.IO.Path;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC;

namespace gView.DataSources.Fdb.SQLite.Tests;

/// <summary>
/// End-to-end tests for a SQLite FDB whose dataset stores geometry as standard WKB
/// in a database-native format instead of the gView-proprietary blob. Creates a real
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

    [Theory]
    [InlineData(GeometryStorageType.Classic)]
    public async Task Insert_BackfillsObjectId(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage);
        var fc = await GetFeatureClassAsync(fdb, "ds", "pts");

        var features = new List<IFeature>
        {
            PointFeature(10, 10, "a"),
            PointFeature(20, 20, "b"),
            PointFeature(30, 30, "c"),
        };
        Assert.True(await fdb.Insert(fc, features, returnIds: true), fdb.LastErrorMessage);

        // returnIds:true => every inserted feature got the database assigned FDB_OID written back
        Assert.All(features, f => Assert.True(f.OID > 0, $"OID not back-filled: {f.OID}"));
        Assert.Equal(3, features.Select(f => f.OID).Distinct().Count());

        // and those OIDs address exactly the rows we inserted
        var oids = features.ToDictionary(f => f.OID, f => f.FindField("NAME")!.Value!.ToString());
        var filter = new QueryFilter
        {
            SubFields = "*",
            WhereClause = "FDB_OID IN (" + string.Join(",", oids.Keys) + ")",
        };
        var read = await DrainAsync(await fdb.Query(fc, filter));

        Assert.Equal(3, read.Count);
        Assert.All(read, f => Assert.Equal(oids[f.OID], f.FindField("NAME")!.Value!.ToString()));

        // default (returnIds:false) keeps the plain insert path - no id back-fill
        var plain = PointFeature(40, 40, "d");
        Assert.True(await fdb.Insert(fc, new List<IFeature> { plain }), fdb.LastErrorMessage);
        Assert.Equal(0, plain.OID);
    }

    [Theory]
    [InlineData(GeometryStorageType.Classic)]
    public async Task EditSession_Commit_AppliesAllPhases(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage);
        var fc = await GetFeatureClassAsync(fdb, "ds", "pts");

        var seed = new List<IFeature> { PointFeature(1, 1, "keep"), PointFeature(2, 2, "toupdate"), PointFeature(3, 3, "todelete") };
        Assert.True(await fdb.Insert(fc, seed, returnIds: true), fdb.LastErrorMessage);
        int updateOid = seed[1].OID, deleteOid = seed[2].OID;

        var session = await ((ISupportsFeatureEditSession)fdb).BeginEditSession();
        Assert.NotNull(session);
        await using (session)
        {
            var add = PointFeature(9, 9, "added");
            Assert.True(await session.Insert(fc, new List<IFeature> { add }, returnIds: true), session.LastErrorMessage);
            Assert.True(add.OID > 0);

            var upd = new Feature { Shape = new Point(20, 20), OID = updateOid };
            upd.Fields.Add(new FieldValue("NAME", "updated"));
            Assert.True(await session.Update(fc, new List<IFeature> { upd }), session.LastErrorMessage);

            Assert.True(await session.Delete(fc, deleteOid), session.LastErrorMessage);

            Assert.True(await session.Commit(), session.LastErrorMessage);
        }

        var all = await DrainAsync(await fdb.Query(fc, new QueryFilter { SubFields = "*" }));
        var byName = all.ToDictionary(f => f.FindField("NAME")!.Value!.ToString()!, f => f);
        Assert.Equal(3, all.Count);
        Assert.True(byName.ContainsKey("keep"));
        Assert.True(byName.ContainsKey("added"));
        Assert.True(byName.ContainsKey("updated"));
        Assert.False(byName.ContainsKey("toupdate"));
        Assert.False(byName.ContainsKey("todelete"));
        Assert.DoesNotContain(all, f => f.OID == deleteOid);
    }

    [Theory]
    [InlineData(GeometryStorageType.Classic)]
    public async Task EditSession_DisposeWithoutCommit_RollsBack(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage);
        var fc = await GetFeatureClassAsync(fdb, "ds", "pts");

        var seed = new List<IFeature> { PointFeature(1, 1, "a"), PointFeature(2, 2, "b") };
        Assert.True(await fdb.Insert(fc, seed, returnIds: true), fdb.LastErrorMessage);

        var session = await ((ISupportsFeatureEditSession)fdb).BeginEditSession();
        Assert.NotNull(session);
        await using (session)
        {
            Assert.True(await session.Insert(fc, new List<IFeature> { PointFeature(8, 8, "x"), PointFeature(9, 9, "y") }), session.LastErrorMessage);
            Assert.True(await session.Delete(fc, seed[0].OID), session.LastErrorMessage);
            // no Commit()
        }

        var all = await DrainAsync(await fdb.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Equal(2, all.Count);
        Assert.DoesNotContain(all, f => f.FindField("NAME")!.Value!.ToString() is "x" or "y");
    }

    [Theory]
    [InlineData(GeometryStorageType.Classic)]
    public async Task EditSession_FailingOperation_RollsBackEarlierInserts(GeometryStorageType storage)
    {
        var fdb = await CreateFdbAsync(storage);
        var fc = await GetFeatureClassAsync(fdb, "ds", "pts");

        var session = await ((ISupportsFeatureEditSession)fdb).BeginEditSession();
        Assert.NotNull(session);
        await using (session)
        {
            Assert.True(await session.Insert(fc, new List<IFeature> { PointFeature(1, 1, "one"), PointFeature(2, 2, "two") }), session.LastErrorMessage);

            // an update of a feature with an invalid OID must fail...
            var bad = new Feature { Shape = new Point(0, 0), OID = -1 };
            bad.Fields.Add(new FieldValue("NAME", "bad"));
            Assert.False(await session.Update(fc, new List<IFeature> { bad }));

            // ...and a session that saw a failure must not commit
            Assert.False(await session.Commit());
        }

        var all = await DrainAsync(await fdb.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Empty(all);
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

    [Fact]
    public async Task NativeStorageImageDataset_DoesNotBuildGViewSpatialIndexForCatalog()
    {
        // CreateImageDataset must route a native ISpatialIndexDef through the
        // CreateNativeSpatialIndexAsync hook instead of force-building a gView BinaryTree2
        // on the <ds>_IMAGE_POLYGONS catalog feature class.
        var fdb = new SQLiteFDB();
        Assert.True(fdb.Create(_dbPath), "FDB Create failed: " + fdb.LastErrorMessage);
        Assert.True(await fdb.Open("Data Source=" + _dbPath));

        var sIndexDef = new PostGisSpatialIndexDef();
        int dsId = await fdb.CreateImageDataset("img", SpatialReference.FromID("epsg:25832"),
            sIndexDef, string.Empty, new FieldCollection());
        Assert.True(dsId > 0, fdb.LastErrorMessage);

        var readBack = await fdb.SpatialIndexDef("img");
        Assert.Equal(GeometryStorageType.PostGis, readBack.StorageType);

        using var conn = new SQLiteConnection("Data Source=" + _dbPath);
        conn.Open();

        Assert.False(TableExists(conn, "FCSI_img_IMAGE_POLYGONS"),
            "no gView spatial-index table for a native image catalog");
        Assert.False(ColumnExists(conn, "FC_img_IMAGE_POLYGONS", "FDB_NID"),
            "no FDB_NID on a native image catalog");
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

}
