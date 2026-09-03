using System.Data.SQLite;
using IoPath = System.IO.Path;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;

namespace gView.DataSources.Fdb.SQLite.Tests;

/// <summary>
/// Tests for the FDB SQLite feature cursors. <see cref="SQLiteFDBFeatureCursor.Create"/> dispatches
/// by query shape:
/// <list type="bullet">
///   <item><c>SQLitePlainFeatureCursor</c> - no spatial filter; SQL does ORDER BY / LIMIT / OFFSET.</item>
///   <item><c>SQLiteMapEnvelopeFeatureCursor</c> - <c>MapEnvelopeIntersects</c> (the map-draw hot
///     path): one combined NID <c>WHERE</c>, a bounding-box test per row, no paging, no forced
///     tiebreaker, ORDER BY only when supplied.</item>
///   <item><c>SQLiteSpatialFeatureCursor</c> - every other spatial relation: one combined NID
///     <c>WHERE</c>, global ORDER BY + <c>FDB_OID</c> tiebreaker, LIMIT/OFFSET enforced cursor-side
///     when a geometry post-filter is active.</item>
/// </list>
/// </summary>
public class SQLiteFDBFeatureCursorTests : IDisposable
{
    private const int NidCount = 5;
    private const int RowsPerNid = 10;
    private const int TotalRows = NidCount * RowsPerNid;

    private readonly string _dbPath;
    private readonly string _connString;
    private readonly GeometryDef _geomDef = new(GeometryType.Point);

    public SQLiteFDBFeatureCursorTests()
    {
        _dbPath = IoPath.Combine(IoPath.GetTempPath(), $"gview_fdb_cursor_test_{Guid.NewGuid():N}.sqlite");
        _connString = "Data Source=" + _dbPath;

        SQLiteConnection.CreateFile(_dbPath);

        using var conn = new SQLiteConnection(_connString);
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText =
                "CREATE TABLE FC_test (FDB_OID INTEGER PRIMARY KEY, FDB_NID INTEGER, FDB_SHAPE BLOB, NAME TEXT, SORTKEY INTEGER);" +
                "CREATE TABLE FC_geo  (FDB_OID INTEGER PRIMARY KEY, FDB_NID INTEGER, FDB_SHAPE BLOB)";
            cmd.ExecuteNonQuery();
        }

        using var tx = conn.BeginTransaction();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText =
                "INSERT INTO FC_test (FDB_OID, FDB_NID, FDB_SHAPE, NAME, SORTKEY) VALUES (@oid, @nid, NULL, @name, @sortkey)";
            var pOid = cmd.Parameters.Add("@oid", System.Data.DbType.Int64);
            var pNid = cmd.Parameters.Add("@nid", System.Data.DbType.Int64);
            var pName = cmd.Parameters.Add("@name", System.Data.DbType.String);
            var pSortKey = cmd.Parameters.Add("@sortkey", System.Data.DbType.Int64);

            for (int oid = 1; oid <= TotalRows; oid++)
            {
                pOid.Value = oid;
                pNid.Value = ((oid - 1) / RowsPerNid) + 1; // NID 1..5, 10 rows each, in OID order
                pName.Value = "feature-" + oid;
                pSortKey.Value = TotalRows + 1 - oid;      // reverse of OID: SORTKEY asc <=> OID desc
                cmd.ExecuteNonQuery();
            }
        }

        using (var cmd = conn.CreateCommand())
        {
            // FC_geo: point OID k sits at (k, k)
            cmd.CommandText = "INSERT INTO FC_geo (FDB_OID, FDB_NID, FDB_SHAPE) VALUES (@oid, @nid, @shape)";
            var pOid = cmd.Parameters.Add("@oid", System.Data.DbType.Int64);
            var pNid = cmd.Parameters.Add("@nid", System.Data.DbType.Int64);
            var pShape = cmd.Parameters.Add("@shape", System.Data.DbType.Binary);

            for (int oid = 1; oid <= TotalRows; oid++)
            {
                pOid.Value = oid;
                pNid.Value = ((oid - 1) / RowsPerNid) + 1;
                pShape.Value = SerializePoint(oid, oid);
                cmd.ExecuteNonQuery();
            }
        }

        tx.Commit();
    }

    private byte[] SerializePoint(double x, double y)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        new Point(x, y).Serialize(bw, _geomDef);
        bw.Flush();
        return ms.ToArray();
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
        catch
        {
            // best effort temp-file cleanup
        }
    }

    private static List<long> AllNids()
        => Enumerable.Range(1, NidCount).Select(i => (long)i).ToList();

    private static SpatialFilter MapEnvelope(IEnvelope? geometry = null)
        => new()
        {
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            Geometry = geometry
        };

    private Task<IFeatureCursor> CreateCursor(
        int limit = 0, int beginRecord = 0, List<long>? nids = null, string orderby = "",
        ISpatialFilter? filter = null, string where = "", string table = "FC_test")
        => SQLiteFDBFeatureCursor.Create(
            _connString,
            "SELECT * FROM " + table,
            where: where,
            orderby: orderby,
            limit: limit,
            beginRecord: beginRecord,
            nids: nids,
            filter: filter,
            geomDef: _geomDef,
            toSRef: null,
            datumTransformations: null);

    private static async Task<List<int>> DrainOids(IFeatureCursor cursor)
    {
        var oids = new List<int>();

        IFeature? feature;
        while ((feature = await cursor.NextFeature()) != null)
        {
            oids.Add(feature.OID);
        }
        cursor.Dispose();

        return oids;
    }

    // ===== Plain cursor (no spatial filter) ==================================

    [Fact]
    public async Task Plain_WithLimit_HonorsLimit()
    {
        var oids = await DrainOids(await CreateCursor(limit: 15, orderby: "FDB_OID"));

        Assert.Equal(Enumerable.Range(1, 15), oids);
    }

    [Fact]
    public async Task Plain_WithLimitAndOffset_HonorsBoth()
    {
        var oids = await DrainOids(await CreateCursor(limit: 10, beginRecord: 21, orderby: "FDB_OID"));

        Assert.Equal(Enumerable.Range(21, 10), oids);
    }

    [Fact]
    public async Task Plain_OrderBy_SortsGlobally()
    {
        var oids = await DrainOids(await CreateCursor(orderby: "SORTKEY"));

        Assert.Equal(Enumerable.Range(1, TotalRows).Reverse(), oids);
    }

    // ===== MapEnvelope cursor (the map-draw hot path) =======================

    [Fact]
    public async Task MapEnvelope_ReturnsEveryRowInTheNidSet()
    {
        var oids = await DrainOids(await CreateCursor(nids: AllNids(), filter: MapEnvelope()));

        Assert.Equal(Enumerable.Range(1, TotalRows), oids.OrderBy(x => x));
    }

    [Fact]
    public async Task MapEnvelope_IgnoresLimitAndOffset()
    {
        // the render path never pages; limit/beginRecord passed here must not truncate
        var oids = await DrainOids(await CreateCursor(limit: 15, beginRecord: 8, nids: AllNids(), filter: MapEnvelope()));

        Assert.Equal(TotalRows, oids.Count);
    }

    [Fact]
    public async Task MapEnvelope_OrderBy_IsAppliedGlobally()
    {
        var oids = await DrainOids(await CreateCursor(nids: AllNids(), orderby: "SORTKEY", filter: MapEnvelope()));

        Assert.Equal(Enumerable.Range(1, TotalRows).Reverse(), oids);
    }

    [Fact]
    public async Task MapEnvelope_RangeEncodedNids_SelectsUnionOfNodes()
    {
        // [-1, 3] -> BETWEEN 1 AND 3 ; 5 -> = 5  ->  NID in {1,2,3,5}
        var oids = await DrainOids(await CreateCursor(nids: new List<long> { -1L, 3L, 5L }, filter: MapEnvelope()));

        var expected = Enumerable.Range(1, 30).Concat(Enumerable.Range(41, 10));
        Assert.Equal(expected, oids.OrderBy(x => x));
    }

    [Fact]
    public async Task MapEnvelope_UserWhere_IsCombinedWithNidClause()
    {
        var oids = await DrainOids(await CreateCursor(nids: AllNids(), where: "FDB_OID <= 25", filter: MapEnvelope()));

        Assert.Equal(Enumerable.Range(1, 25), oids.OrderBy(x => x));
    }

    [Fact]
    public async Task MapEnvelope_EmptyNidList_ReturnsNoFeatures()
    {
        var oids = await DrainOids(await CreateCursor(nids: new List<long>(), filter: MapEnvelope()));

        Assert.Empty(oids);
    }

    [Fact]
    public async Task MapEnvelope_BoundingBoxTest_DropsRowsOutsideTheQueryEnvelope()
    {
        // NID selection returns all 50 points (at (k,k)); the bbox test keeps only 12..18.
        var env = new Envelope(12, 12, 18, 18);

        var oids = await DrainOids(await CreateCursor(nids: AllNids(), filter: MapEnvelope(env), table: "FC_geo"));

        Assert.Equal(Enumerable.Range(12, 7), oids.OrderBy(x => x));
    }

    [Fact]
    public async Task MapEnvelope_NoQueryEnvelope_KeepsEveryRow()
    {
        var oids = await DrainOids(await CreateCursor(nids: AllNids(), filter: MapEnvelope(geometry: null), table: "FC_geo"));

        Assert.Equal(TotalRows, oids.Count);
    }

    // ===== Spatial cursor (precise relations) ===============================

    [Fact]
    public async Task Spatial_OrderBy_IsGloballySorted()
    {
        var oids = await DrainOids(await CreateCursor(nids: AllNids(), orderby: "SORTKEY", filter: new SpatialFilter()));

        Assert.Equal(Enumerable.Range(1, TotalRows).Reverse(), oids);
    }

    [Fact]
    public async Task Spatial_OrderByWithLimit_ReturnsGlobalTopN()
    {
        const int limit = 15;

        var oids = await DrainOids(await CreateCursor(
            limit: limit, nids: AllNids(), orderby: "SORTKEY", filter: new SpatialFilter()));

        // globally first 15 by SORTKEY asc == OID 50..36
        Assert.Equal(Enumerable.Range(TotalRows - limit + 1, limit).Reverse(), oids);
    }

    [Fact]
    public async Task Spatial_OrderByWithLimitAndBeginRecord_ReturnsGlobalPage()
    {
        var oids = await DrainOids(await CreateCursor(
            limit: 10, beginRecord: 21, nids: AllNids(), orderby: "SORTKEY", filter: new SpatialFilter()));

        // SORTKEY asc rows 21..30 == OID 30..21
        Assert.Equal(Enumerable.Range(21, 10).Reverse(), oids);
    }

    [Fact]
    public async Task Spatial_OrderByDuplicateKeys_PagesPartitionTheResultSet()
    {
        // ORDER BY FDB_NID has 10 equal keys per value; the appended FDB_OID tiebreaker must make
        // LIMIT/OFFSET paging deterministic so three consecutive pages cover every row once.
        var page1 = await DrainOids(await CreateCursor(20, 1, AllNids(), "FDB_NID", new SpatialFilter()));
        var page2 = await DrainOids(await CreateCursor(20, 21, AllNids(), "FDB_NID", new SpatialFilter()));
        var page3 = await DrainOids(await CreateCursor(20, 41, AllNids(), "FDB_NID", new SpatialFilter()));

        var all = page1.Concat(page2).Concat(page3).ToList();

        Assert.Equal(TotalRows, all.Count);
        Assert.Equal(Enumerable.Range(1, TotalRows), all.OrderBy(x => x));
    }
}
