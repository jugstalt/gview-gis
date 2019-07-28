using gView.Framework.Data;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MongoDb
{
    class MongoDbFeatureCursor : FeatureCursor
    {
        private IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        private IAsyncCursor<Json.GeometryDocument> _cursor = null;

        public MongoDbFeatureCursor(MongoDbFeatureClass fc, IQueryFilter filter)
            : base(fc.SpatialReference, filter.FeatureSpatialReference)
        {
            string sql = String.Empty;
            if (filter is ISpatialFilter)
            {
                ISpatialFilter sFilter = (ISpatialFilter)filter;
                var env = sFilter.Geometry.Envelope;

                env.minx = Math.Max(-180, env.minx);
                env.miny = Math.Max(-90, env.miny);
                env.maxx = Math.Min(180, env.maxx);
                env.maxy = Math.Min(90, env.maxy);

                GeoJson2DGeographicCoordinates bottomleft = new GeoJson2DGeographicCoordinates(env.minx, env.miny);
                GeoJson2DGeographicCoordinates topleft = new GeoJson2DGeographicCoordinates(env.minx, env.maxy);
                GeoJson2DGeographicCoordinates topright = new GeoJson2DGeographicCoordinates(env.maxx, env.maxy);
                GeoJson2DGeographicCoordinates bottomright = new GeoJson2DGeographicCoordinates(env.maxx, env.miny);
                GeoJson2DGeographicCoordinates[] coord_array = new GeoJson2DGeographicCoordinates[] { bottomleft, topleft, topright, bottomright, bottomleft };
                GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates> ringcoord = new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(coord_array);
                GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates> boxcoord = new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(ringcoord);
                GeoJsonPolygon<GeoJson2DGeographicCoordinates> box = new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(boxcoord);

                var bsonFilter = Builders<Json.GeometryDocument>.Filter
                    .Eq(f => f.FeatureClassName, fc.Name);
                bsonFilter = bsonFilter &
                    Builders<Json.GeometryDocument>.Filter
                    .GeoIntersects(x => x.Shape, box);

                _cursor = fc.MongoCollection.Find(bsonFilter)
                             .ToCursor();
            }
            else
            {
                _cursor = fc.MongoCollection.Find(f => f.FeatureClassName == fc.Name)
                             .ToCursor();
            }
        }

        private List<Json.GeometryDocument> _bsonDocuments = new List<Json.GeometryDocument>();
        private int _pos = 0;

        async public override Task<IFeature> NextFeature()
        {
            if (_bsonDocuments.Count > _pos)
            {
                var bsonDocument = _bsonDocuments[_pos++];

                var feature = new Feature();
                if (bsonDocument.Shape != null)
                {
                    feature.Shape = bsonDocument.Shape.ToGeometry();
                }
                return feature;
            }

            _bsonDocuments.Clear();
            _pos = 0;

            while (await _cursor.MoveNextAsync())
            {
                _bsonDocuments.AddRange(_cursor.Current);
                if (_bsonDocuments.Count > 10000)
                {
                    break;
                }
            }

            if (_bsonDocuments.Count > _pos)
            {
                return await NextFeature();
            }

            return null;
        }
    }
}
