using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Interoperability.Extensions.Features;
using DataFeature = gView.Framework.Core.Data.Feature;

namespace gView.Server.Tests;

/// <summary>
/// A WFS feature type with point features in MGI / Austria GK East (EPSG:31256), the first one in
/// Lower Austria (~16.5° E, 48.3° N), object ids 7, 8, 9, ...
/// </summary>
internal static class WfsTestData
{
    public const string DataCrs = "epsg:31256";
    public const double DataX = 10000, DataY = 355000;

    public static OgcCollectionLayer Collection(int layerId = 0, int featureCount = 1)
        => Collection(new PointFeatureClass(SpatialReference.FromID(DataCrs)!, featureCount), layerId);

    public static OgcCollectionLayer Collection(PointFeatureClass featureClass, int layerId = 0)
    {
        var layer = new FeatureLayer(featureClass) { ID = layerId };

        return new OgcCollectionLayer(layer, $"Points {layerId}", null, featureClass.Envelope, featureClass.SpatialReference);
    }

    /// <summary>
    /// Projects into <see cref="IQueryFilter.FeatureSpatialReference"/> like the real data sources do,
    /// ignores where clauses and limits (the <see cref="FeatureReader"/> skips and limits itself).
    /// </summary>
    public class PointFeatureClass(ISpatialReference sRef, int featureCount = 1) : IFeatureClass
    {
        private readonly FieldCollection _fields = new FieldCollection(new IField[]
        {
            new Field("OBJECTID", FieldType.ID),
            new Field("SHAPE", FieldType.Shape),
            new Field("NAME", FieldType.String),
            new Field("VALUE", FieldType.Double),
            new Field("DATUM", FieldType.Date),
            new Field("REMARK", FieldType.String)
        });

        /// <summary>The filters of the queries</summary>
        public List<IQueryFilter> Filters { get; } = new();

        public string Name => "points";
        public string Aliasname => "points";
        public IDataset Dataset => null!;
        public IFieldCollection Fields => _fields;
        public IField FindField(string name) => _fields.FindField(name);
        public string IDFieldName => "OBJECTID";
        public string ShapeFieldName => "SHAPE";
        public IEnvelope Envelope => new Envelope(DataX, DataY, DataX + 10 * featureCount, DataY);
        public bool HasZ => false;
        public bool HasM => false;
        public ISpatialReference SpatialReference => sRef;
        public GeometryType GeometryType => GeometryType.Point;

        public Task<ICursor> Search(IQueryFilter filter) => GetFeatures(filter).ContinueWith(t => (ICursor)t.Result);
        public Task<ISelectionSet> Select(IQueryFilter filter) => throw new NotSupportedException();
        public Task<int> CountFeatures() => Task.FromResult(featureCount);
        public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            Filters.Add(filter);
            return Task.FromResult<IFeatureCursor>(new Cursor(sRef, filter.FeatureSpatialReference, filter.DatumTransformations, featureCount));
        }

        private class Cursor : gView.Framework.Data.FeatureCursor
        {
            private readonly int _count;
            private int _index;

            public Cursor(ISpatialReference fcSRef, ISpatialReference toSRef, IDatumTransformations datumTransformations, int count)
                : base(null, fcSRef, toSRef, datumTransformations)
            {
                _count = count;
            }

            public override Task<IFeature> NextFeature()
            {
                if (_index >= _count)
                {
                    return Task.FromResult<IFeature>(null!);
                }

                int oid = 7 + _index;
                var feature = new DataFeature() { OID = oid, Shape = new Point(DataX + 10 * _index, DataY) };
                feature.Fields.Add(new FieldValue("OBJECTID", oid));
                feature.Fields.Add(new FieldValue("NAME", "Linz & Co"));
                feature.Fields.Add(new FieldValue("VALUE", 1234.5));
                feature.Fields.Add(new FieldValue("DATUM", new DateTime(2024, 5, 1, 12, 30, 0)));
                feature.Fields.Add(new FieldValue("REMARK", DBNull.Value));
                _index++;

                Transform(feature);
                return Task.FromResult<IFeature>(feature);
            }
        }
    }
}
