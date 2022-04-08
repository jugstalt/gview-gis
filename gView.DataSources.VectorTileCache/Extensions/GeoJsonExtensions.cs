using gView.Framework.Geometry;

namespace gView.DataSources.VectorTileCache.Extensions
{
    static class GeoJsonExtensions
    {
        static public IGeometry ToGeometry(this GeoJSON.Net.Geometry.IGeometryObject geoJsonGeometry)
        {
            IGeometry geometry = null;

            switch (geoJsonGeometry.Type)
            {
                case GeoJSON.Net.GeoJSONObjectType.Point:
                    geometry = (geoJsonGeometry as GeoJSON.Net.Geometry.Point).ToPoint();
                    break;
                case GeoJSON.Net.GeoJSONObjectType.MultiPoint:
                    geometry = (geoJsonGeometry as GeoJSON.Net.Geometry.MultiPoint).ToMultiPoint();
                    break;
                case GeoJSON.Net.GeoJSONObjectType.LineString:
                    geometry = (geoJsonGeometry as GeoJSON.Net.Geometry.LineString).ToPolyline();
                    break;
                case GeoJSON.Net.GeoJSONObjectType.MultiLineString:
                    geometry = (geoJsonGeometry as GeoJSON.Net.Geometry.MultiLineString).ToPolyline();
                    break;
                case GeoJSON.Net.GeoJSONObjectType.Polygon:
                    geometry = (geoJsonGeometry as GeoJSON.Net.Geometry.Polygon).ToPolygon();
                    break;
                case GeoJSON.Net.GeoJSONObjectType.MultiPolygon:
                    geometry = (geoJsonGeometry as GeoJSON.Net.Geometry.MultiPolygon).ToPolygon();
                    break;
            }

            return geometry;
        }

        static public Point ToPoint(this GeoJSON.Net.Geometry.Point geoJsonPoint)
        {
            if (geoJsonPoint?.Coordinates != null)
            {
                return new Point(geoJsonPoint.Coordinates.Longitude, geoJsonPoint.Coordinates.Latitude);
            }

            return null;
        }

        static public MultiPoint ToMultiPoint(this GeoJSON.Net.Geometry.MultiPoint geoJsonMultiPoint)
        {
            var multiPoint = new MultiPoint();

            if (geoJsonMultiPoint?.Coordinates != null)
            {
                foreach (var geoJsonPoint in geoJsonMultiPoint.Coordinates)
                {
                    var point = geoJsonPoint.ToPoint();
                    if (point != null)
                    {
                        multiPoint.AddPoint(point);
                    }
                }
            }

            return multiPoint;
        }

        static public Polyline ToPolyline(this GeoJSON.Net.Geometry.LineString geoJsonLineString)
        {
            var polyline = new Polyline();
            var path = new Path();
            polyline.AddPath(path);

            if (geoJsonLineString?.Coordinates != null)
            {
                foreach (var position in geoJsonLineString.Coordinates)
                {
                    path.AddPoint(new Point(position.Longitude, position.Latitude));
                }
            }

            return polyline;
        }

        static public Polyline ToPolyline(this GeoJSON.Net.Geometry.MultiLineString geoJsonMultiLineString)
        {
            var polyline = new Polyline();

            if (geoJsonMultiLineString?.Coordinates != null)
            {
                foreach (var geoJsonLineString in geoJsonMultiLineString.Coordinates)
                {
                    if (geoJsonLineString.Coordinates != null)
                    {
                        var path = new Path();
                        polyline.AddPath(path);

                        foreach (var position in geoJsonLineString.Coordinates)
                        {
                            path.AddPoint(new Point(position.Longitude, position.Latitude));
                        }
                    }
                }
            }

            return polyline;
        }

        static public Polygon ToPolygon(this GeoJSON.Net.Geometry.Polygon geoJsonPolygon)
        {
            var polygon = new Polygon();

            if (geoJsonPolygon?.Coordinates != null)
            {
                foreach (var geoJsonLineString in geoJsonPolygon.Coordinates)
                {
                    if (geoJsonLineString?.Coordinates != null)
                    {
                        var ring = new Ring();
                        polygon.AddRing(ring);

                        foreach (var position in geoJsonLineString.Coordinates)
                        {
                            ring.AddPoint(new Point(position.Longitude, position.Latitude));
                        }
                    }
                }
            }

            return polygon;
        }

        static public Polygon ToPolygon(this GeoJSON.Net.Geometry.MultiPolygon geoJsonMultiPolygon)
        {
            var polygon = new Polygon();

            if (geoJsonMultiPolygon?.Coordinates != null)
            {
                foreach (var geoJsonPolygon in geoJsonMultiPolygon.Coordinates)
                {
                    if (geoJsonPolygon?.Coordinates != null)
                    {
                        foreach (var geoJsonLineString in geoJsonPolygon.Coordinates)
                        {
                            if (geoJsonLineString?.Coordinates != null)
                            {
                                var ring = new Ring();
                                polygon.AddRing(ring);

                                foreach (var position in geoJsonLineString.Coordinates)
                                {
                                    ring.AddPoint(new Point(position.Longitude, position.Latitude));
                                }
                            }
                        }
                    }
                }
            }

            return polygon;
        }
    }
}
