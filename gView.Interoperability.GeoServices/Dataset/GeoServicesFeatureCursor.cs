using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Rest.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Dataset
{
    internal class GeoServicesFeatureCursor : IFeatureCursor
    {
        private readonly GeoServicesDataset _dataset;
        private readonly GeoServicesFeatureClass _featureClass;
        private readonly string _queryUrl;
        private readonly string _postData;
        private readonly string _where;

        private List<IFeature> _features = new List<IFeature>();
        private int _featureIndex = 0;
        private int _lastOid = 0;
        private bool _hasMore = true;

        public GeoServicesFeatureCursor(GeoServicesDataset dataset,
                                        GeoServicesFeatureClass featureClass,
                                        string queryUrl,
                                        string postData,
                                        string where)
        {
            _dataset = dataset;
            _featureClass = featureClass;
            _queryUrl = queryUrl;
            _postData = postData;
            _where = where;
        }

        async private Task Query()
        {
            _features.Clear();

            string where = String.IsNullOrEmpty(_where) ?
                $"{ _featureClass.IDFieldName }>{ _lastOid }" :
                $"{ _where } and { _featureClass.IDFieldName }>{ _lastOid }";

            var postData = $"{ _postData }&orderByFields={ _featureClass.IDFieldName }&where={ where.UrlEncodeWhereClause() }";

            var jsonFeatureResponse = await _dataset.TryPostAsync<JsonFeatureResponse>(_queryUrl, postData);

            #region Parse Field Types (eg. is Date?)

            List<string> dateColumns = new List<string>();
            if (_featureClass?.Fields != null)
            {
                foreach (var field in _featureClass.Fields.ToEnumerable())
                {
                    if (field.type == FieldType.Date)
                    {
                        dateColumns.Add(field.name);
                    }
                }
            }

            #endregion

            foreach (var jsonFeature in jsonFeatureResponse.Features)
            {
                Feature feature = new Feature();

                #region Geometry

                if (_featureClass.GeometryType == GeometryType.Polyline && jsonFeature.Geometry?.Paths != null)
                {
                    Polyline polyline = new Polyline();
                    for (int p = 0, to = jsonFeature.Geometry.Paths.Length; p < to; p++)
                    {
                        Path path = new Path();

                        var pathsPointsArray = jsonFeature.Geometry.Paths[p];
                        var dimension = pathsPointsArray.GetLength(1); // 2D 3D 3D+M ?
                        int pathsPointsArrayLength = (pathsPointsArray.Length / dimension);

                        for (int multiArrayIndex = 0; multiArrayIndex < pathsPointsArrayLength; multiArrayIndex++)
                        {
                            path.AddPoint(ArrayToPoint(pathsPointsArray, multiArrayIndex, dimension));
                        }
                        polyline.AddPath(path);
                    }

                    feature.Shape = polyline;
                }
                else if (_featureClass.GeometryType == GeometryType.Polygon && jsonFeature.Geometry?.Rings != null)
                {
                    Polygon polygon = new Polygon();
                    for (int r = 0, to = jsonFeature.Geometry.Rings.Length; r < to; r++)
                    {
                        Ring ring = new Ring();

                        var ringsPointsArray = jsonFeature.Geometry.Rings[r];
                        var dimension = ringsPointsArray.GetLength(1); // 2D 3D 3D+M ?
                        int ringsPointsArrayLength = (ringsPointsArray.Length / dimension);

                        for (int multiArrayIndex = 0; multiArrayIndex < ringsPointsArrayLength; multiArrayIndex++)
                        {
                            //Point point = new Point();
                            //point.X = ringsPointsArray[multiArrayIndex, 0];
                            //point.Y = ringsPointsArray[multiArrayIndex, 1];

                            ring.AddPoint(ArrayToPoint(ringsPointsArray, multiArrayIndex, dimension));
                        }
                        polygon.AddRing(ring);
                    }

                    feature.Shape = polygon;
                }
                else if (_featureClass.GeometryType == GeometryType.Point &&
                            (jsonFeature.Geometry?.X != null) &&
                            (jsonFeature.Geometry?.Y != null)
                        )
                {
                    Point shape = _featureClass.HasM ? new PointM() : new Point();
                    shape.X = jsonFeature.Geometry.X.Value;
                    shape.Y = jsonFeature.Geometry.Y.Value;

                    if (_featureClass.HasZ && jsonFeature.Geometry.Z.HasValue)
                    {
                        shape.Z = jsonFeature.Geometry.Z.Value;
                    }

                    if (this._featureClass.HasM && jsonFeature.Geometry.M.HasValue)
                    {
                        ((PointM)shape).M = jsonFeature.Geometry.M.Value;
                    }

                    feature.Shape = shape;
                }
                else if (_featureClass.GeometryType == GeometryType.Multipoint &&
                        jsonFeature.Geometry?.Points != null &&
                        jsonFeature.Geometry.Points.Length > 0)
                {
                    MultiPoint multiPoint = new MultiPoint();

                    for (int p = 0, pointCount = jsonFeature.Geometry.Points.Length; p < pointCount; p++)
                    {
                        var doubleArray = jsonFeature.Geometry.Points[p];
                        if (doubleArray.Length >= 2)
                        {
                            var point = new Point(doubleArray[0].Value, doubleArray[1].Value);

                            multiPoint.AddPoint(point);
                        }
                    }

                    feature.Shape = multiPoint;
                }
                else { }

                #endregion

                #region Properties

                if (jsonFeature.Attributes != null)
                {
                    var attribiutes = (IDictionary<string, object>)jsonFeature.Attributes;
                    foreach (var name in attribiutes.Keys)
                    {
                        object value = attribiutes[name];

                        if (dateColumns.Contains(name))
                        {
                            try
                            {
                                long esriDate = Convert.ToInt64(value);
                                DateTime td = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(esriDate);
                                feature.Fields.Add(new FieldValue(name, td));
                            }
                            catch // if date is not a long?!
                            {
                                feature.Fields.Add(new FieldValue(name, value?.ToString()));
                            }
                        }
                        else
                        {
                            //if (!filter.SuppressResolveAttributeDomains && _domains != null && _domains.ContainsKey(name))
                            //{
                            //    value = DomainValue(name, value?.ToString());
                            //}
                            feature.Fields.Add(new FieldValue(name, value?.ToString()));
                        }

                        if (name == _featureClass.IDFieldName)
                        {
                            feature.OID = int.Parse(value.ToString());
                            _lastOid = Math.Max(_lastOid, feature.OID);
                        }
                    }
                }

                #endregion

                _features.Add(feature);
            }

            _hasMore = _lastOid > 0 && jsonFeatureResponse.ExceededTransferLimit;

            //_hasMore = false;
        }

        #region IFeatureCursor

        public void Dispose()
        {

        }

        async public Task<IFeature> NextFeature()
        {
            if (_featureIndex >= _features.Count)
            {
                if (_hasMore)
                {
                    await Query();
                    _featureIndex = 0;
                    return await NextFeature();
                }
                return null;
            }

            return _features[_featureIndex++];
        }

        #endregion

        #region Helper

        private Point ArrayToPoint(double?[,] pointArray, int index, int dimension)
        {
            var point = _featureClass.HasM ? new PointM() : new Point();

            int dimensionIndex = 0;

            point.X = pointArray[index, dimensionIndex++] ?? 0D;
            point.Y = pointArray[index, dimensionIndex++] ?? 0D;

            if (_featureClass.HasZ && dimensionIndex < dimension)
            {
                point.Z = pointArray[index, dimensionIndex++] ?? 0D;
            }

            if (_featureClass.HasM && dimensionIndex < dimension)
            {
                if (pointArray[index, dimensionIndex].HasValue)
                {
                    ((PointM)point).M = pointArray[index, dimensionIndex++];
                }
            }

            return point;
        }

        #endregion
    }
}
