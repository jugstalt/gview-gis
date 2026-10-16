using System.Data.SQLite;
using gView.DataSources.GeoPackage;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Editor.Core;
using gView.Framework.Geometry;
using gView.Interoperability.Extensions.Editing;
using IoPath = System.IO.Path;

namespace gView.Interoperability.Extensions.Tests.Editing;

/// <summary>
/// The edit rules shared by GeoServices REST and OGC API - Features, tested against a real GeoPackage
/// </summary>
public class FeatureEditServiceTests : IDisposable
{
    private const int LayerId = 3;
    private readonly string _path = IoPath.Combine(IoPath.GetTempPath(), $"gview_edit_{Guid.NewGuid():N}.gpkg");

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

    private async Task<(GeoPackageDataset dataset, IFeatureClass featureClass)> CreateAsync(GeometryType geometryType = GeometryType.Point)
    {
        var dataset = new GeoPackageDataset();
        Assert.True(dataset.Create(_path), dataset.LastErrorMessage);
        await dataset.SetConnectionString(_path);
        Assert.True(await dataset.Open(), dataset.LastErrorMessage);

        var fields = new FieldCollection();
        fields.Add(new Field("name", FieldType.String) { size = 50 });

        var geomDef = new GeometryDef(geometryType) { SpatialReference = SpatialReference.FromID("epsg:25832") };
        Assert.Equal(0, await dataset.CreateFeatureClass("", "geo", geomDef, fields));

        var featureClass = (IFeatureClass)(await dataset.Element("geo"))!.Class;
        return (dataset, featureClass);
    }

    private static FeatureEditService Service(IFeatureClass featureClass, EditStatements statements = EditStatements.INSERT | EditStatements.UPDATE | EditStatements.DELETE, string? statementsError = null)
        => new FeatureEditService(LayerId, featureClass, featureClass.Dataset.Database as IFeatureUpdater, statements, statementsError: statementsError);

    private static Feature Point(double x, double y, string name, int oid = 0)
    {
        var feature = new Feature { Shape = new Point(x, y), OID = oid };
        feature.Fields.Add(new FieldValue("name", name));
        return feature;
    }

    private static async Task<List<IFeature>> ReadAllAsync(IFeatureClass featureClass)
    {
        var filter = new QueryFilter() { SubFields = "*" };
        using var cursor = await featureClass.GetFeatures(filter);
        var features = new List<IFeature>();
        IFeature? feature;
        while ((feature = await cursor.NextFeature()) != null)
        {
            features.Add(feature);
        }
        return features;
    }

    #region Statements

    [Fact]
    public async Task CheckStatement_AllowedStatement_DoesNotThrow()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            Service(fc, EditStatements.INSERT).CheckStatement(EditStatements.INSERT);
        }
    }

    [Fact]
    public async Task CheckStatement_NotAllowedStatement_Throws()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var ex = Assert.Throws<MapServerException>(() => Service(fc, EditStatements.INSERT).CheckStatement(EditStatements.DELETE));
            Assert.Equal($"Editoperation DELETE not allowed for layer with id={LayerId}", ex.Message);
        }
    }

    [Fact]
    public async Task CheckStatement_NoEditLayer_ThrowsStatementsError()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var service = Service(fc, EditStatements.NONE, "No editor module available for service");

            var ex = Assert.Throws<MapServerException>(() => service.CheckStatement(EditStatements.INSERT));
            Assert.Equal("No editor module available for service", ex.Message);
            Assert.False(service.IsEditable);
        }
    }

    #endregion

    #region Validation

    [Fact]
    public async Task PrepareInserts_FeatureWithObjectId_Throws()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var ex = Assert.Throws<MapServerException>(() => Service(fc).PrepareInserts([Point(1, 2, "a", oid: 5)]));
            Assert.Equal("Can't insert features with existing ObjectId", ex.Message);
        }
    }

    [Fact]
    public async Task PrepareInserts_FeatureWithoutGeometry_Throws()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var feature = Point(1, 2, "a");
            feature.Shape = null;

            var ex = Assert.Throws<MapServerException>(() => Service(fc).PrepareInserts([feature]));
            Assert.Equal("Insert features without geometry are not allowed", ex.Message);
        }
    }

    [Fact]
    public async Task PrepareUpdates_FeatureWithoutObjectId_Throws()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var ex = Assert.Throws<MapServerException>(() => Service(fc).PrepareUpdates([Point(1, 2, "a")]));
            Assert.Equal("Can't update features without existing ObjectId", ex.Message);
        }
    }

    [Fact]
    public async Task ProjectToFeatureClass_GeometryInOtherCrs_IsTransformed()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var feature = Point(9.0, 48.0, "a");   // WGS84 => UTM 32N (EPSG:25832)
            feature.Shape.Srs = 4326;

            Service(fc).ProjectToFeatureClass([feature]);

            var point = (IPoint)feature.Shape;
            Assert.Equal(25832, point.Srs);
            Assert.InRange(point.X, 499_000, 501_000);
            Assert.InRange(point.Y, 5_316_000, 5_318_000);
        }
    }

    #endregion

    #region Execute

    [Fact]
    public async Task InsertUpdateDelete_RoundTrip()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var service = Service(fc);

            var inserts = new List<IFeature> { Point(10, 20, "a"), Point(30, 40, "b") };
            service.PrepareInserts(inserts);
            Assert.True(await service.Insert(inserts), service.LastErrorMessage());
            Assert.All(inserts, f => Assert.True(f.OID > 0));   // ids are written back

            var update = Point(11, 21, "a2", oid: inserts[0].OID);
            service.PrepareUpdates([update]);
            Assert.True(await service.Update([update]), service.LastErrorMessage());

            Assert.True(await service.Delete(inserts[1].OID), service.LastErrorMessage());

            var features = await ReadAllAsync(fc);
            var remaining = Assert.Single(features);
            Assert.Equal("a2", remaining["name"]);
            Assert.Equal(11, ((IPoint)remaining.Shape).X, 6);
        }
    }

    [Fact]
    public async Task EditSession_WithoutCommit_RollsBack()
    {
        var (dataset, fc) = await CreateAsync();
        using (dataset)
        {
            var service = Service(fc);

            await using (var session = await service.BeginEditSession())
            {
                Assert.NotNull(session);
                Assert.True(await service.Insert([Point(1, 2, "a")], session));
                // disposed without Commit()
            }

            Assert.Empty(await ReadAllAsync(fc));

            await using (var session = await service.BeginEditSession())
            {
                Assert.True(await service.Insert([Point(1, 2, "a")], session));
                Assert.True(await session!.Commit());
            }

            Assert.Single(await ReadAllAsync(fc));
        }
    }

    #endregion
}
