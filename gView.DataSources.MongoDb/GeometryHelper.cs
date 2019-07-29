using gView.Framework.Geometry;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.DataSources.MongoDb
{
    static public class GeometryHelper
    {
        #region ToGeoJson

        static public GeoJsonGeometry<T> ToGeoJsonGeometry<T>(this IGeometry geometry) where T : GeoJsonCoordinates
        {
            if (geometry is IPoint)
            {
                var point = (IPoint)geometry;

                return new GeoJsonPoint<T>(
                    (T)Activator.CreateInstance(typeof(T), new object[] { point.X, point.Y }));
            }
            if (geometry is IPolyline)
            {
                var polyline = (IPolyline)geometry;

                if (polyline.PathCount == 1)
                {
                    var lineStringCoordinates = new GeoJsonLineStringCoordinates<T>(polyline[0].ToGeoJsonCoordinates<T>());
                    return new GeoJsonLineString<T>(lineStringCoordinates);
                }
                if (polyline.PathCount > 1)
                {
                    var lineStrings = new List<GeoJsonLineStringCoordinates<T>>();
                    for (int p = 0, p_to = polyline.PathCount; p < p_to; p++)
                    {
                        lineStrings.Add(new GeoJsonLineStringCoordinates<T>(polyline[p].ToGeoJsonCoordinates<T>()));
                    }

                    var multiLineStrings = new GeoJsonMultiLineStringCoordinates<T>(lineStrings);
                    return new GeoJsonMultiLineString<T>(multiLineStrings);
                }
            }
            if (geometry is IPolygon)
            {
                var polygon = (IPolygon)geometry;
                polygon.VerifyHoles();

                var linearRings = new List<GeoJsonLinearRingCoordinates<T>>();
                for (int r = 0, r_to = polygon.RingCount; r < r_to; r++)
                {
                    linearRings.Add(new GeoJsonLinearRingCoordinates<T>(polygon[r].ToGeoJsonCoordinates<T>()));
                }

                if (linearRings.Count == 1)
                {
                    var polygonCoordinates = new GeoJsonPolygonCoordinates<T>(linearRings[0]);
                    return new GeoJsonPolygon<T>(polygonCoordinates);
                }
                else if (linearRings.Count > 1)
                {
                    var multiPolygonCoordinates = new GeoJsonMultiPolygonCoordinates<T>(
                        linearRings.Select(r => new GeoJsonPolygonCoordinates<T>(r)));

                    return new GeoJsonMultiPolygon<T>(multiPolygonCoordinates);
                }
            }

            return null;
        }

        static public IEnumerable<T> ToGeoJsonCoordinates<T>(this IPointCollection pCollection) where T : GeoJsonCoordinates
        {
            List<T> coordinates = new List<T>();

            for (int i = 0, i_to = pCollection.PointCount; i < i_to; i++)
            {
                coordinates.Add((T)Activator.CreateInstance(typeof(T), new object[] { pCollection[i].X, pCollection[i].Y }));
            }

            return coordinates;
        }

        #endregion

        #region ToGeometry

        static public IGeometry ToGeometry<T>(this GeoJsonGeometry<T> geoJsonGeometry) where T : GeoJsonCoordinates
        {
            if (geoJsonGeometry is GeoJsonPoint<T>)
            {
                var geoJsonPoint = (GeoJsonPoint<T>)geoJsonGeometry;

                if (geoJsonPoint.Coordinates != null)
                {
                    var values = geoJsonPoint.Coordinates.Values;
                    return new Point(values[0], values[1]);
                }
            }
            if (geoJsonGeometry is GeoJsonLineString<T>)
            {
                var polyline = new Polyline();
                var geoJsonLineString = (GeoJsonLineString<T>)geoJsonGeometry;

                var lineStringCoordinates = geoJsonLineString.Coordinates;
                if (lineStringCoordinates != null)
                {
                    polyline.AddPath(lineStringCoordinates.ToPath<T>());
                }

                return polyline;
            }
            if (geoJsonGeometry is GeoJsonMultiLineString<T>)
            {
                var polyline = new Polyline();
                var geoJsonMultiLineString = (GeoJsonMultiLineString<T>)geoJsonGeometry;
                if (geoJsonMultiLineString.Coordinates?.LineStrings != null)
                {
                    foreach (var lineString in geoJsonMultiLineString.Coordinates.LineStrings)
                    {
                        polyline.AddPath(lineString.ToPath<T>());
                    }
                }

                return polyline;
            }
            if (geoJsonGeometry is GeoJsonPolygon<T>)
            {
                var polygon = new Polygon();
                var geoJsonPolygon = (GeoJsonPolygon<T>)geoJsonGeometry;

                var polygonCoordinates = geoJsonPolygon.Coordinates;
                if (polygonCoordinates.Exterior != null)
                {
                    polygon.AddRing(polygonCoordinates.Exterior.ToRing<T>());
                }
                if (polygonCoordinates.Holes != null)
                {
                    foreach (var hole in polygonCoordinates.Holes)
                    {
                        polygon.AddRing(hole.ToRing<T>());
                    }
                }

                return polygon;
            }
            if(geoJsonGeometry is GeoJsonMultiPolygon<T>)
            {
                var polygon = new Polygon();
                var geoJsonMultiPolygon = (GeoJsonMultiPolygon<T>)geoJsonGeometry;

                if(geoJsonMultiPolygon.Coordinates?.Polygons!=null)
                {
                    foreach(var geoJsonPolygon in geoJsonMultiPolygon.Coordinates.Polygons)
                    {
                        if (geoJsonPolygon.Exterior != null)
                        {
                            polygon.AddRing(geoJsonPolygon.Exterior.ToRing<T>());
                        }
                        if (geoJsonPolygon.Holes != null)
                        {
                            foreach (var hole in geoJsonPolygon.Holes)
                            {
                                polygon.AddRing(hole.ToRing<T>());
                            }
                        }
                    }
                }

                return polygon;
            }

            return null;
        }

        static public IRing ToRing<T>(this GeoJsonLinearRingCoordinates<T> linearRing) where T : GeoJsonCoordinates
        {
            var ring = new Ring();

            foreach (var position in linearRing.Positions)
            {
                var values = position.Values;
                ring.AddPoint(new Point(values[0], values[1]));
            }

            return ring;
        }

        static public IPath ToPath<T>(this GeoJsonLineStringCoordinates<T> lineString) where T : GeoJsonCoordinates
        {
            var path = new Path();

            foreach (var position in lineString.Positions)
            {
                var values = position.Values;

                path.AddPoint(new Point(values[0], values[1]));
            }

            return path;
        }

        #endregion
    }
}
