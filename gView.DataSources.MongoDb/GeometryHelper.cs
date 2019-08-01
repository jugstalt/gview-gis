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
                polygon.MakeValid();
                polygon.CloseAllRings();

                if (polygon.OuterRingCount == 1)
                {
                    var outerRing = new GeoJsonLinearRingCoordinates<T>(polygon.OuterRings().First().ToGeoJsonCoordinates<T>());
                    var innerRings = new List<GeoJsonLinearRingCoordinates<T>>();
                    if (polygon.InnerRingCount > 0)
                    {
                        foreach (var hole in polygon.InnerRings(polygon.OuterRings().First()))
                        {
                            innerRings.Add(new GeoJsonLinearRingCoordinates<T>(hole.ToGeoJsonCoordinates<T>()));
                        }
                    }
                    var polygonCoordinates = innerRings.Count > 0 ? new GeoJsonPolygonCoordinates<T>(outerRing, innerRings) : new GeoJsonPolygonCoordinates<T>(outerRing);
                    return new GeoJsonPolygon<T>(polygonCoordinates);
                }
                else if (polygon.OuterRingCount > 1)
                {
                    List<GeoJsonPolygonCoordinates<T>> polygonCoordinatesArray = new List<GeoJsonPolygonCoordinates<T>>();
                    foreach (var ring in polygon.OuterRings())
                    {
                        var outerRing = new GeoJsonLinearRingCoordinates<T>(ring.ToGeoJsonCoordinates<T>());
                        var innerRings = new List<GeoJsonLinearRingCoordinates<T>>();
                        foreach (var hole in polygon.InnerRings(ring))
                        {
                            innerRings.Add(new GeoJsonLinearRingCoordinates<T>(hole.ToGeoJsonCoordinates<T>()));
                        }
                        polygonCoordinatesArray.Add(innerRings.Count > 0 ? new GeoJsonPolygonCoordinates<T>(outerRing, innerRings) : new GeoJsonPolygonCoordinates<T>(outerRing));
                    }

                    var multiPolygonCoordinates = new GeoJsonMultiPolygonCoordinates<T>(polygonCoordinatesArray);
                    return new GeoJsonMultiPolygon<T>(multiPolygonCoordinates);
                }
            }

            return null;
        }

        static public IEnumerable<T> ToGeoJsonCoordinates<T>(this IPointCollection pCollection) where T : GeoJsonCoordinates
        {
            List<T> coordinates = new List<T>();

            if (pCollection is IRing)
            {
                ((IRing)pCollection).Close();
            }

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
            if (geoJsonGeometry is GeoJsonMultiPolygon<T>)
            {
                var polygon = new Polygon();
                var geoJsonMultiPolygon = (GeoJsonMultiPolygon<T>)geoJsonGeometry;

                if (geoJsonMultiPolygon.Coordinates?.Polygons != null)
                {
                    foreach (var geoJsonPolygon in geoJsonMultiPolygon.Coordinates.Polygons)
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

        #region Generalizatoin

        public static double[] Resolutions =
            new double[]
            {
                156543.04,                                      // 0
                78271.52,                                       // 1     1 : 295,829,355.45           
                39135.76,                                       // 2   
                19567.88,                                       // 3
                9783.94,                                        // 4
                4891.97,                                        // 5
                2445.985,                                       // 6    
                1222.9925,                                      // 7
                611.49625,                                      // 8
                305.748125,                                     // 9
                152.8740625,                                    // 10
                76.43703125,                                    // 11
                38.218515625,                                   // 12    
                19.1092578125,                                  // 13   
                9.55462890625,                                  // 14       1 : 36,111.98
                //4.777314453125‬,
                //2.3886572265625‬
            };

        public const double R = 6378137D;
        public const double ToDeg = 180.0 / Math.PI;
        static public double ToDegrees(this double meters)
        {
            return meters / R * ToDeg;
        }

        static public int BestResolutionLevel(this double resolution)
        {
            for (int r = 0; r < Resolutions.Length; r++)
            {
                if (Math.Abs(resolution - Resolutions[r]) < 1e-3)
                {
                    return r;
                }

                if (resolution > Resolutions[r])
                {
                    return r;
                }
            }

            return -1;
        }

        public static IGeometry[] Generalize(this IGeometry geometry, ISpatialReference sRef, int generalizationLevel)
        {

            var geometries = new IGeometry[Resolutions.Length];

            if(generalizationLevel < 0)
            {
                return geometries;
            }

            for (int r = Resolutions.Length - 1; r >= 0; r--)
            {
                if (r < generalizationLevel)
                {
                    break;
                }

                var res = Resolutions[r];
                if (sRef.SpatialParameters.IsGeographic == true)
                {
                    res = res.ToDegrees();
                }

                var generalizedGeometry = gView.Framework.SpatialAlgorithms.Algorithm.Generalize(geometry, res * 2, true);

                if (generalizedGeometry == null)
                {
                    break;
                }

                geometries[r] = generalizedGeometry;

                geometry = generalizedGeometry;
            }

            return geometries;
        }

        public static Json.GeometryDocument AppendGeneralizedShapes(
            this Json.GeometryDocument geometryDocument,
            IGeometry geometry, ISpatialReference sRef,
            int generalizationLevel)
        {
            if (generalizationLevel < 0)
            {
                return geometryDocument;
            }

            var generalized = geometry.Generalize(sRef, generalizationLevel);

            for (int i = 0; i < generalized.Length; i++)
            {
                if (generalized[i] != null)
                {
                    var propertyInfo = geometryDocument.GetType().GetProperty($"ShapeGeneralized{i}");
                    if (propertyInfo != null)
                    {
                        var wkb = gView.Framework.OGC.OGC.GeometryToWKB(generalized[i], 0, Framework.OGC.OGC.WkbByteOrder.Ndr);
                        propertyInfo.SetValue(geometryDocument, wkb);
                        //propertyInfo.SetValue(geometryDocument, generalized[i].ToGeoJsonGeometry<GeoJson2DGeographicCoordinates>());
                    }
                }
            }
            return geometryDocument;
        }

        #endregion
    }
}
