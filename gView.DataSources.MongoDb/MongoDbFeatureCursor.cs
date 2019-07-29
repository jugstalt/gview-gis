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
            IFindFluent<Json.GeometryDocument, Json.GeometryDocument> findFluent = null;

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

                var bsonFilter =
                    fc.GeometryType == Framework.Geometry.geometryType.Point ?
                    Builders<Json.GeometryDocument>.Filter
                        .GeoIntersects(x => x.Shape, box) :
                    Builders<Json.GeometryDocument>.Filter
                        .GeoIntersects(x => x.Bounds, box);
                    //.GeoWithinBox(x => x.Shape, env.minx, env.miny, env.maxx, env.maxy);

                var findOptions = new FindOptions()
                {
                    BatchSize = 10000
                };

                findFluent = fc.MongoCollection.Find(bsonFilter, options: findOptions);
            }
            else
            {
                findFluent = fc.MongoCollection.Find(_ => true);
                filter.Limit = 50;
            }

            if (filter.Limit > 0)
            {
                findFluent = findFluent.Limit(filter.Limit);
            }
            if(filter.BeginRecord>1)
            {
                findFluent = findFluent.Skip(filter.BeginRecord - 1);
            }
            if (filter.SubFields != "*")
            {
                StringBuilder project = new StringBuilder();
                project.Append("{");
                foreach (var field in filter.SubFields.Split(' '))
                {
                    if(project.Length>1)
                    {
                        project.Append(",");
                    }
                    if (field == "_id" || field == "_shape")
                    {
                        project.Append($"{field}:1");
                    }
                    else
                    {
                        project.Append($"\"properties.{field}\":1");
                    }
                }
                project.Append("}");
                findFluent = findFluent.Project<Json.GeometryDocument>(project.ToString());
            }

            _cursor = findFluent.ToCursor();
        }

        private List<Json.GeometryDocument> _document = new List<Json.GeometryDocument>();
        private int _pos = 0;

        async public override Task<IFeature> NextFeature()
        {
            if (_document.Count > _pos)
            {
                var document = _document[_pos++];

                var feature = new Feature();
                if (document.Shape != null)
                {
                    feature.Shape = document.Shape.ToGeometry();
                }
                if(document.Properties!=null)
                {
                    foreach(var property in document.Properties.Keys)
                    {
                        feature.Fields.Add(new FieldValue(property, document.Properties[property]));
                    }
                }
                return feature;
            }

            _document.Clear();
            _pos = 0;

            while (await _cursor.MoveNextAsync())
            {
                _document.AddRange(_cursor.Current);
                if (_document.Count >= 10000)
                {
                    break;
                }
            }

            if (_document.Count > _pos)
            {
                return await NextFeature();
            }

            return null;
        }

        public override void Dispose()
        {
            if (_cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
            }
        }
    }
}
