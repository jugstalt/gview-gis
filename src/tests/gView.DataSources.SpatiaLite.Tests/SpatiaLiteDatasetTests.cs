using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;

namespace gView.DataSources.SpatiaLite.Tests;

/// <summary>
/// End-to-end tests for <see cref="SpatiaLiteDataset"/>: opens real temp SpatiaLite and
/// GeoPackage files, lists feature classes and round-trips geometry through
/// query / insert / update / delete.
/// </summary>
public class SpatiaLiteDatasetTests : IDisposable
{
    private readonly string _spatiaLitePath;
    private readonly string _geoPackagePath;

    public SpatiaLiteDatasetTests()
    {
        _spatiaLitePath = SpatiaLiteTestEnvironment.NewTempPath("sqlite");
        _geoPackagePath = SpatiaLiteTestEnvironment.NewTempPath("gpkg");
    }

    public void Dispose()
    {
        SpatiaLiteTestEnvironment.TryDelete(_spatiaLitePath);
        SpatiaLiteTestEnvironment.TryDelete(_geoPackagePath);
    }

    private async Task<SpatiaLiteDataset> OpenSpatiaLiteAsync()
    {
        SpatiaLiteTestEnvironment.RequireModSpatialite();
        SpatiaLiteTestEnvironment.CreateSpatiaLite(_spatiaLitePath);

        var dataset = new SpatiaLiteDataset();
        await dataset.SetConnectionString(_spatiaLitePath);
        Assert.True(await dataset.Open(), dataset.LastErrorMessage);

        return dataset;
    }

    private async Task<SpatiaLiteDataset> OpenGeoPackageAsync()
    {
        SpatiaLiteTestEnvironment.RequireModSpatialite();
        SpatiaLiteTestEnvironment.CreateGeoPackage(_geoPackagePath);

        var dataset = new SpatiaLiteDataset();
        await dataset.SetConnectionString(_geoPackagePath);
        Assert.True(await dataset.Open(), dataset.LastErrorMessage);

        return dataset;
    }

    private static async Task<IFeatureClass> GetFeatureClassAsync(SpatiaLiteDataset dataset, string name)
    {
        var element = await dataset.Element(name);
        Assert.NotNull(element);
        return (IFeatureClass)element!.Class;
    }

    private static async Task<List<IFeature>> DrainAsync(IFeatureCursor cursor)
    {
        var features = new List<IFeature>();
        IFeature? feature;
        while ((feature = await cursor.NextFeature()) != null)
        {
            features.Add(feature);
        }
        cursor.Dispose();
        return features;
    }

    private static Feature PointFeature(double x, double y, string name, int value)
    {
        var feature = new Feature { Shape = new Point(x, y) };
        feature.Fields.Add(new FieldValue("name", name));
        feature.Fields.Add(new FieldValue("value", value));
        return feature;
    }

    [Fact]
    public async Task Elements_SpatiaLite_ListsFeatureClassesWithGeometryTypeAndSrid()
    {
        using var dataset = await OpenSpatiaLiteAsync();

        var elements = await dataset.Elements();
        var byName = elements.ToDictionary(e => e.Class.Name, e => (IFeatureClass)e.Class);

        Assert.Contains("pts", byName.Keys);
        Assert.Contains("areas", byName.Keys);

        Assert.Equal(GeometryType.Point, byName["pts"].GeometryType);
        Assert.Equal(GeometryType.Polygon, byName["areas"].GeometryType);

        Assert.Equal(25832, byName["pts"].SpatialReference?.EpsgCode);
    }

    [Fact]
    public async Task Query_SpatiaLite_ReturnsAllFeaturesAndRoundTripsGeometry()
    {
        using var dataset = await OpenSpatiaLiteAsync();
        var fc = await GetFeatureClassAsync(dataset, "pts");

        var features = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));

        Assert.Equal(3, features.Count);

        var byName = features.ToDictionary(f => f.FindField("name")!.Value!.ToString()!, f => (IPoint)f.Shape);
        Assert.Equal(100, byName["a"].X, 6);
        Assert.Equal(100, byName["a"].Y, 6);
        Assert.Equal(900, byName["c"].X, 6);

        Assert.All(features, f => Assert.True(f.OID > 0));
    }

    [Fact]
    public async Task Query_SpatiaLite_SpatialFilterReturnsOnlyIntersectingFeatures()
    {
        using var dataset = await OpenSpatiaLiteAsync();
        var fc = await GetFeatureClassAsync(dataset, "pts");

        var filter = new SpatialFilter
        {
            SubFields = "*",
            Geometry = new Envelope(0, 0, 300, 300),
            SpatialRelation = spatialRelation.SpatialRelationIntersects
        };

        var features = await DrainAsync(await dataset.Query(fc, filter));

        Assert.Single(features);
        Assert.Equal("a", features[0].FindField("name")!.Value!.ToString());
    }

    [Fact]
    public async Task InsertUpdateDelete_SpatiaLite_PointRoundTrips()
    {
        using var dataset = await OpenSpatiaLiteAsync();
        var fc = await GetFeatureClassAsync(dataset, "pts");

        // insert
        var toInsert = PointFeature(250.5, 400.25, "new", 99);
        Assert.True(await dataset.Insert(fc, new List<IFeature> { toInsert }, returnIds: true), dataset.LastErrorMessage);

        // returnIds:true must back-fill the database assigned id onto the passed feature
        Assert.True(toInsert.OID > 0, $"OID not back-filled: {toInsert.OID}");

        var inserted = (await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" })))
            .Single(f => f.FindField("name")!.Value!.ToString() == "new");

        Assert.Equal(250.5, ((IPoint)inserted.Shape).X, 6);
        Assert.Equal(400.25, ((IPoint)inserted.Shape).Y, 6);
        Assert.True(inserted.OID > 0);
        Assert.Equal(inserted.OID, toInsert.OID);

        // update
        var update = new Feature { Shape = new Point(1, 2), OID = inserted.OID };
        update.Fields.Add(new FieldValue("name", "changed"));
        update.Fields.Add(new FieldValue("value", 123));
        Assert.True(await dataset.Update(fc, update), dataset.LastErrorMessage);

        var afterUpdate = (await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" })))
            .Single(f => f.OID == inserted.OID);
        Assert.Equal("changed", afterUpdate.FindField("name")!.Value!.ToString());
        Assert.Equal(1, ((IPoint)afterUpdate.Shape).X, 6);

        // delete
        Assert.True(await dataset.Delete(fc, inserted.OID), dataset.LastErrorMessage);

        var afterDelete = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.DoesNotContain(afterDelete, f => f.OID == inserted.OID);
        Assert.Equal(3, afterDelete.Count);
    }

    [Fact]
    public async Task Insert_SpatiaLite_PromotesPolygonIntoMultiPolygonColumn()
    {
        using var dataset = await OpenSpatiaLiteAsync();
        var fc = await GetFeatureClassAsync(dataset, "areas");

        var ring = new Ring();
        ring.AddPoint(new Point(300, 300));
        ring.AddPoint(new Point(400, 300));
        ring.AddPoint(new Point(400, 400));
        ring.AddPoint(new Point(300, 400));
        ring.AddPoint(new Point(300, 300));

        var feature = new Feature { Shape = new Polygon(ring) };
        feature.Fields.Add(new FieldValue("name", "square"));

        Assert.True(await dataset.Insert(fc, new List<IFeature> { feature }), dataset.LastErrorMessage);

        var read = (await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" })))
            .Single(f => f.FindField("name")!.Value!.ToString() == "square");

        Assert.NotNull(read.Shape);
        Assert.False(read.Shape.Envelope.Width == 0);
        Assert.Equal(300, read.Shape.Envelope.MinX, 6);
        Assert.Equal(400, read.Shape.Envelope.MaxX, 6);
    }

    [Fact]
    public async Task CreateAndDeleteFeatureClass_SpatiaLite_RoundTrips()
    {
        using var dataset = await OpenSpatiaLiteAsync();

        var fields = new FieldCollection();
        fields.Add(new Field("label", FieldType.String) { size = 50 });

        var geomDef = new GeometryDef(GeometryType.Point)
        {
            SpatialReference = SpatialReference.FromID("epsg:25832")
        };

        Assert.Equal(0, await dataset.CreateFeatureClass("", "created_fc", geomDef, fields));

        var fc = await GetFeatureClassAsync(dataset, "created_fc");
        Assert.Equal(GeometryType.Point, fc.GeometryType);
        Assert.Equal(25832, fc.SpatialReference?.EpsgCode);

        var feature = new Feature { Shape = new Point(10, 20) };
        feature.Fields.Add(new FieldValue("label", "x"));
        Assert.True(await dataset.Insert(fc, new List<IFeature> { feature }), dataset.LastErrorMessage);

        var read = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Single(read);
        Assert.Equal(10, ((IPoint)read[0].Shape).X, 6);

        Assert.True(await dataset.DeleteFeatureClass("created_fc"), dataset.LastErrorMessage);
        Assert.Null(await dataset.Element("created_fc"));
    }

    [Fact]
    public async Task Elements_GeoPackage_ListsPointFeatureClass()
    {
        using var dataset = await OpenGeoPackageAsync();

        var fc = await GetFeatureClassAsync(dataset, "pts");

        Assert.Equal(GeometryType.Point, fc.GeometryType);
        Assert.Equal(25832, fc.SpatialReference?.EpsgCode);
    }

    [Fact]
    public async Task InsertUpdateDelete_GeoPackage_RoundTripsAndKeepsValidGpbBlob()
    {
        using var dataset = await OpenGeoPackageAsync();
        var fc = await GetFeatureClassAsync(dataset, "pts");

        var before = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Equal(3, before.Count);

        // insert
        var toInsert = PointFeature(600, 600, "added", 7);
        Assert.True(await dataset.Insert(fc, new List<IFeature> { toInsert }, returnIds: true), dataset.LastErrorMessage);
        Assert.True(toInsert.OID > 0, $"OID not back-filled: {toInsert.OID}");

        var after = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Equal(4, after.Count);
        var added = after.Single(f => f.FindField("name")!.Value!.ToString() == "added");
        Assert.Equal(600, ((IPoint)added.Shape).X, 6);
        Assert.True(added.OID > 0);
        Assert.Equal(added.OID, toInsert.OID);

        // the geometry column must still hold spec-compliant GPB ("GP..") blobs
        var validGpb = Convert.ToInt64(SpatiaLiteTestEnvironment.Scalar(
            _geoPackagePath, "SELECT min(IsValidGPB(geom)) FROM pts", geoPackage: true));
        Assert.Equal(1, validGpb);

        // update
        var update = new Feature { Shape = new Point(11, 22), OID = added.OID };
        update.Fields.Add(new FieldValue("name", "changed"));
        update.Fields.Add(new FieldValue("value", 8));
        Assert.True(await dataset.Update(fc, update), dataset.LastErrorMessage);

        var afterUpdate = (await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" })))
            .Single(f => f.OID == added.OID);
        Assert.Equal("changed", afterUpdate.FindField("name")!.Value!.ToString());
        Assert.Equal(11, ((IPoint)afterUpdate.Shape).X, 6);

        // delete
        Assert.True(await dataset.Delete(fc, added.OID), dataset.LastErrorMessage);
        var afterDelete = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Equal(3, afterDelete.Count);
        Assert.DoesNotContain(afterDelete, f => f.OID == added.OID);
    }

    [Fact]
    public async Task CreateDatabaseAndFeatureClass_GeoPackage_RoundTrips()
    {
        SpatiaLiteTestEnvironment.RequireModSpatialite();

        // create a brand-new empty GeoPackage
        var dataset = new SpatiaLiteDataset();
        Assert.True(dataset.Create(_geoPackagePath), dataset.LastErrorMessage);
        Assert.True(File.Exists(_geoPackagePath));

        await dataset.SetConnectionString(_geoPackagePath);
        Assert.True(await dataset.Open(), dataset.LastErrorMessage);

        var fields = new FieldCollection();
        fields.Add(new Field("name", FieldType.String) { size = 50 });
        fields.Add(new Field("value", FieldType.integer));

        var geomDef = new GeometryDef(GeometryType.Polyline)
        {
            SpatialReference = SpatialReference.FromID("epsg:25832")
        };

        Assert.Equal(0, await dataset.CreateFeatureClass("", "roads", geomDef, fields));

        var fc = await GetFeatureClassAsync(dataset, "roads");
        Assert.Equal(GeometryType.Polyline, fc.GeometryType);
        Assert.Equal(25832, fc.SpatialReference?.EpsgCode);

        var linePath = new gView.Framework.Geometry.Path();
        linePath.AddPoint(new Point(0, 0));
        linePath.AddPoint(new Point(100, 100));
        var line = new Polyline(linePath);

        var feature = new Feature { Shape = line };
        feature.Fields.Add(new FieldValue("name", "r1"));
        feature.Fields.Add(new FieldValue("value", 5));
        Assert.True(await dataset.Insert(fc, new List<IFeature> { feature }), dataset.LastErrorMessage);

        var read = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Single(read);
        Assert.Equal("r1", read[0].FindField("name")!.Value!.ToString());
        Assert.Equal(0, read[0].Shape.Envelope.MinX, 6);
        Assert.Equal(100, read[0].Shape.Envelope.MaxX, 6);

        var validGpb = Convert.ToInt64(SpatiaLiteTestEnvironment.Scalar(
            _geoPackagePath, "SELECT min(IsValidGPB(geom)) FROM roads", geoPackage: true) ?? 0L);
        Assert.Equal(1, validGpb);

        Assert.True(await dataset.DeleteFeatureClass("roads"), dataset.LastErrorMessage);
        Assert.Null(await dataset.Element("roads"));

        // the gpkg metadata rows are gone too
        var leftover = Convert.ToInt64(SpatiaLiteTestEnvironment.Scalar(
            _geoPackagePath,
            "SELECT (SELECT count(*) FROM gpkg_contents WHERE table_name='roads') + " +
            "       (SELECT count(*) FROM gpkg_geometry_columns WHERE table_name='roads')",
            geoPackage: true) ?? 0L);
        Assert.Equal(0, leftover);
    }

    [Theory]
    [InlineData("sqlite")]
    [InlineData("gpkg")]
    public async Task InsertAndQuery_AwkwardFieldNames_RoundTrip(string kind)
    {
        SpatiaLiteTestEnvironment.RequireModSpatialite();
        var path = kind == "gpkg" ? _geoPackagePath : _spatiaLitePath;

        var dataset = new SpatiaLiteDataset();
        Assert.True(dataset.Create(path), dataset.LastErrorMessage);
        await dataset.SetConnectionString(path);
        Assert.True(await dataset.Open(), dataset.LastErrorMessage);

        // OSM-style keys with ":" (common in QGIS/osm2pgsql GeoPackages) are illegal in a
        // raw SQLite bind-parameter token; a name with a space breaks a space-split
        // sub-field list.
        var fields = new FieldCollection();
        fields.Add(new Field("mtb:scale:uphill", FieldType.String) { size = 20 });
        fields.Add(new Field("historic:civilization", FieldType.String) { size = 20 });
        fields.Add(new Field("some name", FieldType.String) { size = 20 });

        var geomDef = new GeometryDef(GeometryType.Point)
        {
            SpatialReference = SpatialReference.FromID("epsg:4326")
        };
        Assert.Equal(0, await dataset.CreateFeatureClass("", "osm", geomDef, fields));

        var fc = await GetFeatureClassAsync(dataset, "osm");

        var feature = new Feature { Shape = new Point(1, 2) };
        feature.Fields.Add(new FieldValue("mtb:scale:uphill", "T4"));
        feature.Fields.Add(new FieldValue("historic:civilization", "roman"));
        feature.Fields.Add(new FieldValue("some name", "hello world"));
        Assert.True(await dataset.Insert(fc, new List<IFeature> { feature }), dataset.LastErrorMessage);

        var read = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Single(read);
        Assert.Equal("T4", read[0].FindField("mtb:scale:uphill")!.Value!.ToString());
        Assert.Equal("roman", read[0].FindField("historic:civilization")!.Value!.ToString());
        Assert.Equal("hello world", read[0].FindField("some name")!.Value!.ToString());

        // and update by id still works with those columns
        var update = new Feature { Shape = new Point(3, 4), OID = read[0].OID };
        update.Fields.Add(new FieldValue("some name", "changed"));
        Assert.True(await dataset.Update(fc, update), dataset.LastErrorMessage);

        var afterUpdate = (await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }))).Single();
        Assert.Equal("changed", afterUpdate.FindField("some name")!.Value!.ToString());
    }

    [Theory]
    [InlineData("db.gpkg")]
    [InlineData("db.sqlite")]
    public async Task IFileFeatureDatabase_CreatesInFolderAndOpens(string fileName)
    {
        SpatiaLiteTestEnvironment.RequireModSpatialite();

        var folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gview_sl_folder_{Guid.NewGuid():N}");
        System.IO.Directory.CreateDirectory(folder);
        try
        {
            gView.Framework.Core.FDB.IFileFeatureDatabase fileDb = new SpatiaLiteDataset();

            Assert.False(fileDb.IsFolderBased);
            Assert.Equal("SpatiaLite / GeoPackage", fileDb.DatabaseName);

            var target = System.IO.Path.Combine(folder, fileName);
            Assert.Equal(0, await fileDb.CreateDataset(target, null));
            Assert.True(System.IO.File.Exists(target));

            var dataset = await ((gView.Framework.Core.FDB.IFeatureDatabase)fileDb).GetDataset(target) as SpatiaLiteDataset;
            Assert.NotNull(dataset);

            var geomDef = new GeometryDef(GeometryType.Point)
            {
                SpatialReference = SpatialReference.FromID("epsg:4326")
            };
            Assert.Equal(0, await dataset!.CreateFeatureClass("", "p", geomDef, new FieldCollection()));

            var fc = await GetFeatureClassAsync(dataset, "p");
            Assert.True(await dataset.Insert(fc, new List<IFeature> { new Feature { Shape = new Point(1, 2) } }),
                dataset.LastErrorMessage);
            Assert.Single(await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" })));
        }
        finally
        {
            try { System.IO.Directory.Delete(folder, true); } catch { }
        }
    }

    [Fact]
    public async Task IFileFeatureDatabase_OpenAndGetDataset_AcceptDataSourceConnectionString()
    {
        // the CopyFeatureClass command parameter builders take the IFileFeatureDatabase
        // branch: fileDB.Open(connstr) then fileDB.GetDataset(connstr), with connstr being
        // "Data Source=<path>". Both must work (regression for "method not implemented").
        using var source = await OpenSpatiaLiteAsync();

        gView.Framework.Core.FDB.IFileFeatureDatabase fileDb = new SpatiaLiteDataset();
        var connStr = $"Data Source={_spatiaLitePath}";

        Assert.True(await ((gView.Framework.Core.FDB.IDatabase)fileDb).Open(connStr),
            ((gView.Framework.Core.FDB.IDatabase)fileDb).LastException?.Message);

        var dataset = await ((gView.Framework.Core.FDB.IFeatureDatabase)fileDb).GetDataset(connStr) as SpatiaLiteDataset;
        Assert.NotNull(dataset);

        var fc = await GetFeatureClassAsync(dataset!, "pts");
        Assert.Equal(3, (await DrainAsync(await dataset!.Query(fc, new QueryFilter { SubFields = "*" }))).Count);
    }

    [Fact]
    public async Task CopyFeatureClass_SpatiaLiteToNewGeoPackage_RoundTrips()
    {
        // mirrors what FeatureImport / CopyFeatureClassCommand do when pasting a feature
        // class onto a SpatiaLite/GeoPackage node in the DataExplorer.
        using var source = await OpenSpatiaLiteAsync();
        var sourceFc = await GetFeatureClassAsync(source, "pts");

        var dest = new SpatiaLiteDataset();
        Assert.True(dest.Create(_geoPackagePath), dest.LastErrorMessage);
        await dest.SetConnectionString(_geoPackagePath);
        Assert.True(await dest.Open(), dest.LastErrorMessage);

        var geomDef = new GeometryDef(sourceFc);
        Assert.Equal(0, await dest.CreateFeatureClass(dest.DatasetName, "pts_copy", geomDef,
            (IFieldCollection)sourceFc.Fields.Clone()));

        var destFc = await GetFeatureClassAsync(dest, "pts_copy");

        var copied = new List<IFeature>();
        using (var cursor = await sourceFc.GetFeatures(new QueryFilter { SubFields = "*" }))
        {
            IFeature f;
            while ((f = await cursor.NextFeature()) != null)
            {
                copied.Add(f);
            }
        }
        Assert.True(await dest.Insert(destFc, copied), dest.LastErrorMessage);

        var readBack = await DrainAsync(await dest.Query(destFc, new QueryFilter { SubFields = "*" }));
        Assert.Equal(3, readBack.Count);
        Assert.Contains(readBack, f => f.FindField("name")!.Value!.ToString() == "a"
                                    && Math.Abs(((IPoint)f.Shape).X - 100) < 1e-6);

        var validGpb = Convert.ToInt64(SpatiaLiteTestEnvironment.Scalar(
            _geoPackagePath, "SELECT min(IsValidGPB(geom)) FROM pts_copy", geoPackage: true) ?? 0L);
        Assert.Equal(1, validGpb);
    }

    [Fact]
    public async Task CreateDatabase_SpatiaLite_ProducesUsableFile()
    {
        SpatiaLiteTestEnvironment.RequireModSpatialite();

        var dataset = new SpatiaLiteDataset();
        Assert.True(dataset.Create(_spatiaLitePath), dataset.LastErrorMessage);

        await dataset.SetConnectionString(_spatiaLitePath);
        Assert.True(await dataset.Open(), dataset.LastErrorMessage);

        var geomDef = new GeometryDef(GeometryType.Point)
        {
            SpatialReference = SpatialReference.FromID("epsg:4326")
        };
        Assert.Equal(0, await dataset.CreateFeatureClass("", "poi", geomDef, new FieldCollection()));

        var fc = await GetFeatureClassAsync(dataset, "poi");
        var feature = new Feature { Shape = new Point(7.5, 47.5) };
        Assert.True(await dataset.Insert(fc, new List<IFeature> { feature }), dataset.LastErrorMessage);

        var read = await DrainAsync(await dataset.Query(fc, new QueryFilter { SubFields = "*" }));
        Assert.Single(read);
        Assert.Equal(7.5, ((IPoint)read[0].Shape).X, 6);
    }

    [Fact]
    public async Task Query_GeoPackage_SpatialFilterReturnsOnlyIntersectingFeatures()
    {
        using var dataset = await OpenGeoPackageAsync();
        var fc = await GetFeatureClassAsync(dataset, "pts");

        // pts seeded at (100,100), (500,500), (900,900) in EPSG:25832
        var envelopeFilter = new SpatialFilter
        {
            SubFields = "*",
            Geometry = new Envelope(0, 0, 300, 300),
            SpatialRelation = spatialRelation.SpatialRelationMapEnvelopeIntersects
        };
        var envelopeHits = await DrainAsync(await dataset.Query(fc, envelopeFilter));
        Assert.Single(envelopeHits);
        Assert.Equal("a", envelopeHits[0].FindField("name")!.Value!.ToString());

        var intersectsFilter = new SpatialFilter
        {
            SubFields = "*",
            Geometry = new Envelope(0, 0, 600, 600),
            SpatialRelation = spatialRelation.SpatialRelationIntersects
        };
        var intersectHits = await DrainAsync(await dataset.Query(fc, intersectsFilter));
        Assert.Equal(2, intersectHits.Count);
    }
}
