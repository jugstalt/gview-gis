using gView.Framework.Geometry;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.DataSources.MongoDb
{
    static public class GeometryHelper
    {
        #region ToGeoJson

        static public GeoJsonGeometry<GeoJson2DGeographicCoordinates> ToGeoJsonGeometry(this IGeometry geometry)
        {
            if (geometry is IPoint)
            {
                var point = (IPoint)geometry;

                return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                    new GeoJson2DGeographicCoordinates(point.X, point.Y));
            }
            if (geometry is IPolyline)
            {
                var polyline = (IPolyline)geometry;

                if (polyline.PathCount == 1)
                {
                    var lineStringCoordinates = new GeoJsonLineStringCoordinates<GeoJson2DGeographicCoordinates>(polyline[0].ToGeoJsonCoordinates());
                    return new GeoJsonLineString<GeoJson2DGeographicCoordinates>(lineStringCoordinates);
                }
                if (polyline.PathCount > 1)
                {
                    var lineStrings = new List<GeoJsonLineStringCoordinates<GeoJson2DGeographicCoordinates>>();
                    for (int p = 0, p_to = polyline.PathCount; p < p_to; p++)
                    {
                        lineStrings.Add(new GeoJsonLineStringCoordinates<GeoJson2DGeographicCoordinates>(polyline[p].ToGeoJsonCoordinates()));
                    }

                    var multiLineStrings = new GeoJsonMultiLineStringCoordinates<GeoJson2DGeographicCoordinates>(lineStrings);
                    return new GeoJsonMultiLineString<GeoJson2DGeographicCoordinates>(multiLineStrings);
                }
            }
            if (geometry is IPolygon)
            {
                var polygon = (IPolygon)geometry;

                var linearRings = new List<GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>>();
                for (int r = 0, r_to = polygon.RingCount; r < r_to; r++)
                {
                    linearRings.Add(new GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates>(polygon[r].ToGeoJsonCoordinates()));
                }

                if (linearRings.Count == 1)
                {
                    var polygonCoordinates = new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(linearRings[0]);
                    return new GeoJsonPolygon<GeoJson2DGeographicCoordinates>(polygonCoordinates);
                }
                else if(linearRings.Count>1)
                {
                    var multiPolygonCoordinates = new GeoJsonMultiPolygonCoordinates<GeoJson2DGeographicCoordinates>(
                        linearRings.Select(r => new GeoJsonPolygonCoordinates<GeoJson2DGeographicCoordinates>(r)));

                    return new GeoJsonMultiPolygon<GeoJson2DGeographicCoordinates>(multiPolygonCoordinates);
                }
            }

            return null;
        }

        static public IEnumerable<GeoJson2DGeographicCoordinates> ToGeoJsonCoordinates(this IPointCollection pCollection)
        {
            List<GeoJson2DGeographicCoordinates> coordinates = new List<GeoJson2DGeographicCoordinates>();

            for(int i=0,i_to=pCollection.PointCount;i<i_to;i++)
            {
                coordinates.Add(new GeoJson2DGeographicCoordinates(pCollection[i].X, pCollection[i].Y));
            }

            return coordinates;
        }

        #endregion

        #region ToGeometry

        static public IGeometry ToGeometry(this GeoJsonGeometry<GeoJson2DGeographicCoordinates> geoJsonGeometry)
        {
            if (geoJsonGeometry is GeoJsonPoint<GeoJson2DGeographicCoordinates>)
            {
                var geoJsonPoint = (GeoJsonPoint<GeoJson2DGeographicCoordinates>)geoJsonGeometry;

                if (geoJsonPoint.Coordinates != null)
                {
                    return new Point(geoJsonPoint.Coordinates.Longitude, geoJsonPoint.Coordinates.Latitude);
                }
            }
            if (geoJsonGeometry is GeoJsonPolygon<GeoJson2DGeographicCoordinates>)
            {
                var polygon = new Polygon();
                var geoJsonPolygon = (GeoJsonPolygon<GeoJson2DGeographicCoordinates>)geoJsonGeometry;

                var polygonCoordinates = geoJsonPolygon.Coordinates;
                if (polygonCoordinates.Exterior != null)
                {
                    polygon.AddRing(polygonCoordinates.Exterior.ToRing());
                }
                if (polygonCoordinates.Holes != null)
                {
                    foreach (var hole in polygonCoordinates.Holes)
                    {
                        polygon.AddRing(hole.ToRing());
                    }
                }

                return polygon;
            }

            return null;
        }

        static public IRing ToRing(this GeoJsonLinearRingCoordinates<GeoJson2DGeographicCoordinates> linearRing)
        {
            var ring = new Ring();

            foreach(var position in linearRing.Positions)
            {
                ring.AddPoint(new Point(position.Longitude, position.Latitude));
            }

            return ring;
        }

        #endregion
    }
}
