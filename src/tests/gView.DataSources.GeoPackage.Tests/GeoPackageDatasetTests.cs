using System.Data.SQLite;
using gView.DataSources.GeoPackage;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using IoPath = System.IO.Path;

namespace gView.DataSources.GeoPackage.Tests;

/// <summary>
/// End-to-end tests for <see cref="GeoPackageDataset"/>. Every one of these runs with
/// <b>no <c>mod_spatialite</c></b> on PATH and <c>GVIEW_MOD_SPATIALITE</c> unset - a
/// GeoPackage feature database is fully managed (GPB blob + gView-maintained R-Tree).
/// </summary>
public class GeoPackageDatasetTests : IDisposable
{
    private readonly string _path;

    public GeoPackageDatasetTests()
    {
        _path = IoPath.Combine(IoPath.GetTempPath(), $"gview_gpkg_{Guid.NewGuid():N}.gpkg");
    }

    public void Dispose()
    {
        try
        {
            SQLiteConnection.ClearAllPools();
            foreach (var p in new[] { _path, _path + "-wal", _path + "-shm" })
            {
                if (File.Exists(p)) File.Delete(p);
            }
        }
        catch { }
    }

    private async Task<GeoPackageDataset> CreateAsync(GeometryType geometryType, string fcName = "geo")
    {
        var dataset = new GeoPackageDataset();
        Assert.True(dataset.Create(_path), dataset.LastErrorMessage);
        Assert.True(File.Exists(_path));

        await dataset.SetConnectionString(_path);
        Assert.True(await dataset.Open(), dataset.LastErrorMessage);

        var fields = new FieldCollection();
        fields.Add(new Field("name", FieldType.String) { size = 50 });
        fields.Add(new Field("value", FieldType.integer));

        var geomDef = new GeometryDef(geometryType)
        {
            SpatialReference = SpatialReference.FromID("epsg:25832")
        };

        Assert.Equal(0, await dataset.CreateFeatureClass("", fcName, geomDef, fields));
        return dataset;
    }

    private static async Task<IFeatureClass> FcAsync(GeoPackageDataset dataset, string name = "geo")
    {
        var element = await dataset.Element(name);
        Assert.NotNull(element);
        return (IFeatureClass)element!.Class;
    }

    private static async Task<List<IFeature>> DrainAsync(IFeatureCursor cursor)
    {
        var list = new List<IFeature>();
        IFeature? f;
        while ((f = await cursor.NextFeature()) != null) list.Add(f);
        cursor.Dispose();
        return list;
    }

    private static Feature Point(double x, double y, string name)
    {
        var f = new Feature { Shape = new Point(x, y) };
        f.Fields.Add(new FieldValue("name", name));
        f.Fields.Add(new FieldValue("value", 0));
        return f;
    }

    private static object? Scalar(string path, string sql)
    {
        using var connection = new SQLiteConnection($"Data Source={path}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }

    [Fact]
    public async Task Create_ProducesValidGeoPackageMetadata()
    {
        using var dataset = await CreateAsync(GeometryType.Point);
        dataset.Dispose();
        SQLiteConnection.ClearAllPools();

        Assert.Equal(GeoPackageSchema.ApplicationId,
            Convert.ToInt64(Scalar(_path, "PRAGMA application_id")));
        Assert.Equal(1, Convert.ToInt32(Scalar(_path,
            "SELECT count(*) FROM gpkg_contents WHERE table_name='geo' AND data_type='features'")));
        Assert.Equal(1, Convert.ToInt32(Scalar(_path,
            "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='rtree_geo_geom'")));
    }

    [Fact]
    public async Task Elements_ListsFeatureClassWithGeometryTypeAndSrid()
    {
        using var dataset = await CreateAsync(GeometryType.Polygon, "areas");

        var fc = await FcAsync(dataset, "areas");
        Assert.Equal(GeometryType.Polygon, fc.GeometryType);
        Assert.Equal(25832, fc.SpatialReference?.EpsgCode);
    }

    [Fact]
    public async Task InsertQuery_RoundTripsGeometry_AndStoresGpbBlob()
    {
        using var dataset = await CreateAsync(GeometryType.Point);
        var fc = await FcAsync(dataset);

        Assert.True(await dataset.Insert(fc, new List<IFeature>
        {
            Point(100, 100, "a"),
            Point(250.5, 400.25, "b"),
            Point(900, 900, "c"),
        }), dataset.LastErrorMessage);

        var read = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Equal(3, read.Count);

        var byName = read.ToDictionary(f => f.FindField("name")!.Value!.ToString()!, f => (IPoint)f.Shape);
        Assert.Equal(100, byName["a"].X, 6);
        Assert.Equal(400.25, byName["b"].Y, 6);

        dataset.Dispose();
        SQLiteConnection.ClearAllPools();

        var blob = (byte[])Scalar(_path, "SELECT geom FROM geo LIMIT 1")!;
        Assert.True(GpkgGeometry.IsGpb(blob));
        Assert.Equal(3, Convert.ToInt32(Scalar(_path, "SELECT count(*) FROM rtree_geo_geom")));
    }

    [Fact]
    public async Task Query_MapEnvelopeIntersects_UsesRTree_ReturnsOnlyCandidatesInBox()
    {
        using var dataset = await CreateAsync(GeometryType.Point);
        var fc = await FcAsync(dataset);

        await dataset.Insert(fc, new List<IFeature>
        {
            Point(100, 100, "in"),
            Point(500, 500, "in2"),
            Point(9000, 9000, "out"),
        });

        var hits = await DrainAsync(await dataset.Query(fc, new SpatialFilter
        {
            SubFields = "*",
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            Geometry = new Envelope(0, 0, 600, 600),
        }));

        var names = hits.Select(f => f.FindField("name")!.Value!.ToString()).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "in", "in2" }, names);
    }

    [Fact]
    public async Task Query_Intersects_ReCheckedInManagedCode()
    {
        using var dataset = await CreateAsync(GeometryType.Point);
        var fc = await FcAsync(dataset);

        await dataset.Insert(fc, new List<IFeature>
        {
            Point(100, 100, "a"),
            Point(500, 500, "b"),
            Point(900, 900, "c"),
        });

        var hits = await DrainAsync(await dataset.Query(fc, new SpatialFilter
        {
            SubFields = "*",
            SpatialRelation = spatialRelation.SpatialRelationIntersects,
            Geometry = new Envelope(0, 0, 300, 300),
        }));

        Assert.Single(hits);
        Assert.Equal("a", hits[0].FindField("name")!.Value!.ToString());
    }

    [Fact]
    public async Task InsertUpdateDelete_KeepsRTreeInSync()
    {
        using var dataset = await CreateAsync(GeometryType.Point);
        var fc = await FcAsync(dataset);

        var f = Point(250.5, 400.25, "x");
        Assert.True(await dataset.Insert(fc, new List<IFeature> { f }, returnIds: true), dataset.LastErrorMessage);
        Assert.True(f.OID > 0, $"OID not back-filled: {f.OID}");

        var update = new Feature { Shape = new Point(1, 2), OID = f.OID };
        update.Fields.Add(new FieldValue("name", "changed"));
        update.Fields.Add(new FieldValue("value", 1));
        Assert.True(await dataset.Update(fc, update), dataset.LastErrorMessage);

        var afterUpdate = (await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" })))
            .Single(x => x.OID == f.OID);
        Assert.Equal("changed", afterUpdate.FindField("name")!.Value!.ToString());
        Assert.Equal(1, ((IPoint)afterUpdate.Shape).X, 6);

        // R-Tree tracked the move: the feature is found near (1,2), not near the old (250,400)
        var near = await DrainAsync(await dataset.Query(fc, new SpatialFilter
        {
            SubFields = "*",
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects,
            Geometry = new Envelope(-5, -5, 5, 5),
        }));
        Assert.Single(near);

        Assert.True(await dataset.Delete(fc, f.OID), dataset.LastErrorMessage);
        Assert.Empty(await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" })));

        dataset.Dispose();
        SQLiteConnection.ClearAllPools();
        Assert.Equal(0, Convert.ToInt32(Scalar(_path, "SELECT count(*) FROM rtree_geo_geom")));
    }

    [Fact]
    public async Task Insert_PolygonRoundTrips()
    {
        using var dataset = await CreateAsync(GeometryType.Polygon, "areas");
        var fc = await FcAsync(dataset, "areas");

        var ring = new Ring();
        ring.AddPoint(new Point(0, 0));
        ring.AddPoint(new Point(0, 10));
        ring.AddPoint(new Point(10, 10));
        ring.AddPoint(new Point(10, 0));
        var feature = new Feature { Shape = new Polygon(ring) };
        feature.Fields.Add(new FieldValue("name", "square"));
        feature.Fields.Add(new FieldValue("value", 1));

        Assert.True(await dataset.Insert(fc, new List<IFeature> { feature }), dataset.LastErrorMessage);

        var read = (await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }))).Single();
        Assert.Equal(0, read.Shape.Envelope.MinX, 6);
        Assert.Equal(10, read.Shape.Envelope.MaxX, 6);
    }

    [Fact]
    public async Task Reopen_WithoutModSpatialite_ListsAndReadsFeatures()
    {
        using (var dataset = await CreateAsync(GeometryType.Point))
        {
            var fc = await FcAsync(dataset);
            await dataset.Insert(fc, new List<IFeature> { Point(1, 2, "a"), Point(3, 4, "b") });
        }
        SQLiteConnection.ClearAllPools();

        var reopened = new GeoPackageDataset();
        await reopened.SetConnectionString(_path);
        Assert.True(await reopened.Open(), reopened.LastErrorMessage);

        var fc2 = await FcAsync(reopened);
        var read = await DrainAsync(await reopened.Query(fc2, new QueryFilter { SubFields = "*" }));
        Assert.Equal(2, read.Count);
        reopened.Dispose();
    }

    [Fact]
    public async Task DeleteFeatureClass_RemovesTableMetadataAndRTree()
    {
        using var dataset = await CreateAsync(GeometryType.Point, "temp_fc");
        Assert.True(await dataset.DeleteFeatureClass("temp_fc"), dataset.LastErrorMessage);
        Assert.Null(await dataset.Element("temp_fc"));

        dataset.Dispose();
        SQLiteConnection.ClearAllPools();

        Assert.Equal(0, Convert.ToInt32(Scalar(_path,
            "SELECT (SELECT count(*) FROM gpkg_contents WHERE table_name='temp_fc') + " +
            "       (SELECT count(*) FROM gpkg_geometry_columns WHERE table_name='temp_fc') + " +
            "       (SELECT count(*) FROM sqlite_master WHERE name='rtree_temp_fc_geom')")));
    }

    [Fact]
    public async Task IFileFeatureDatabase_DatabaseName_IsGeoPackage()
    {
        gView.Framework.Core.FDB.IFileFeatureDatabase fileDb = new GeoPackageDataset();
        Assert.False(fileDb.IsFolderBased);
        Assert.Equal("GeoPackage", fileDb.DatabaseName);
        await Task.CompletedTask;
    }
}
