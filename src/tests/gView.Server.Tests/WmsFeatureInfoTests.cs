using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Interoperability.OGC;
using System.Text;
using System.Text.Json;
using DataFeature = gView.Framework.Core.Data.Feature;

namespace gView.Server.Tests;

/// <summary>
/// WMS GetFeatureInfo returns geometries in the CRS of the request (CRS/SRS), not in the CRS of the data.
/// </summary>
public class WmsFeatureInfoTests
{
    // MGI / Austria GK East: a point in Upper Austria (~15.5° E, 48.3° N)
    private const string DataCrs = "epsg:31256";
    private const double DataX = -61145, DataY = 353796;

    [Fact]
    public void CreateFeatureInfoFilter_SetsRequestCrsForFilterAndFeatures()
    {
        var webMercator = SpatialReference.FromID("epsg:3857")!;

        var filter = WMSRequest.CreateFeatureInfoFilter(new Envelope(0, 0, 10, 10), webMercator, null);

        Assert.Same(webMercator, filter.FilterSpatialReference);
        Assert.Same(webMercator, filter.FeatureSpatialReference);
        Assert.Equal("*", filter.SubFields);
    }

    [Fact]
    public void CreateFeatureInfoFilter_NoRequestCrs_KeepsDataCrs()
    {
        var filter = WMSRequest.CreateFeatureInfoFilter(new Envelope(0, 0, 10, 10), null, null);

        Assert.Null(filter.FeatureSpatialReference);
    }

    [Fact]
    public async Task GeoJson_WebMercatorRequest_GeometryInWebMercatorWithCrsMember()
    {
        var root = await GeoJsonResponse("epsg:3857");

        Assert.Equal("EPSG:3857", root.GetProperty("crs").GetProperty("properties").GetProperty("name").GetString(), ignoreCase: true);
        var (x, y) = FirstCoordinate(root);
        Assert.InRange(x, 1_600_000, 1_900_000);
        Assert.InRange(y, 6_000_000, 6_300_000);
    }

    [Fact]
    public async Task GeoJson_Wgs84Request_GeometryIsLonLat()
    {
        // WMS 1.3.0 EPSG:4326 is lat/lon for BBOX and GML, but GeoJSON stays x/y = lon/lat
        var root = await GeoJsonResponse("epsg:4326");

        var (x, y) = FirstCoordinate(root);
        Assert.InRange(x, 14.5, 16.5);
        Assert.InRange(y, 47.5, 49.0);
    }

    [Fact]
    public async Task Gml_WebMercatorRequest_GeometryAndSrsNameInRequestCrs()
    {
        var requestSRef = SpatialReference.FromID("epsg:3857")!;
        var features = await QueryFeatures(requestSRef);

        var sb = new StringBuilder();
        WMSRequest.GetFeatureResponseGML(features, sb, requestSRef);
        var gml = sb.ToString();

        Assert.Contains($"srsName=\"{requestSRef.Name}\"", gml);
        Assert.DoesNotContain("31256", gml);
        Assert.DoesNotContain("-61145", gml);
    }

    #region Helpers

    private static async Task<JsonElement> GeoJsonResponse(string requestCrs)
    {
        var requestSRef = SpatialReference.FromID(requestCrs)!;
        var features = await QueryFeatures(requestSRef);

        var sb = new StringBuilder();
        WMSRequest.GetFeatureResponseGeoJson(features, sb, requestSRef);

        return JsonDocument.Parse(sb.ToString()).RootElement;
    }

    private static (double x, double y) FirstCoordinate(JsonElement root)
    {
        var coordinates = root.GetProperty("features")[0].GetProperty("geometry").GetProperty("coordinates");
        return (coordinates[0].GetDouble(), coordinates[1].GetDouble());
    }

    /// <summary>Queries the fake data source the way WMS GetFeatureInfo does</summary>
    private static async Task<List<WMSRequest.FeatureType>> QueryFeatures(ISpatialReference requestSRef)
    {
        var featureClass = new PointFeatureClass(SpatialReference.FromID(DataCrs)!, new Point(DataX, DataY));
        var filter = WMSRequest.CreateFeatureInfoFilter(new Envelope(-1e9, -1e9, 1e9, 1e9), requestSRef, null);

        var result = new List<WMSRequest.FeatureType>();
        using var cursor = (IFeatureCursor)await featureClass.Search(filter);
        IFeature? feature;
        while ((feature = await cursor.NextFeature()) is not null)
        {
            result.Add(new WMSRequest.FeatureType("0", featureClass, feature));
        }

        return result;
    }

    /// <summary>A data source that projects into <see cref="IQueryFilter.FeatureSpatialReference"/> like the real ones do.</summary>
    private class PointFeatureClass(ISpatialReference sRef, IPoint point) : IFeatureClass
    {
        public string Name => "points";
        public string Aliasname => "points";
        public IDataset Dataset => null!;
        public IFieldCollection Fields => new FieldCollection();
        public IField FindField(string name) => null!;
        public string IDFieldName => "OBJECTID";
        public string ShapeFieldName => "SHAPE";
        public IEnvelope Envelope => point.Envelope;
        public bool HasZ => false;
        public bool HasM => false;
        public ISpatialReference SpatialReference => sRef;
        public GeometryType GeometryType => GeometryType.Point;

        public Task<ICursor> Search(IQueryFilter filter) =>
            Task.FromResult<ICursor>(new Cursor(sRef, filter.FeatureSpatialReference, filter.DatumTransformations, point));
        public Task<ISelectionSet> Select(IQueryFilter filter) => throw new NotSupportedException();
        public Task<int> CountFeatures() => Task.FromResult(1);
        public Task<IFeatureCursor> GetFeatures(IQueryFilter filter) => throw new NotSupportedException();

        private class Cursor : FeatureCursor
        {
            private IPoint? _point;

            public Cursor(ISpatialReference fcSRef, ISpatialReference toSRef, IDatumTransformations datumTransformations, IPoint point)
                : base(null, fcSRef, toSRef, datumTransformations)
            {
                _point = point;
            }

            public override Task<IFeature> NextFeature()
            {
                if (_point is null)
                {
                    return Task.FromResult<IFeature>(null!);
                }

                var feature = new DataFeature() { OID = 1, Shape = new Point(_point.X, _point.Y) };
                feature.Fields.Add(new FieldValue("OBJECTID", 1));
                _point = null;

                Transform(feature);
                return Task.FromResult<IFeature>(feature);
            }
        }
    }

    #endregion
}
