using gView.Framework.Common.Json;
using gView.GeoJsonService.DTOs;

namespace gView.Framework.GeoJsonService.Extensions;

static public class GeometryExtensions
{
    static public Core.Geometry.IGeometry? ToGeometry(this gView.GeoJsonService.DTOs.Geometry geoJsonGeometry)
    {
        if (geoJsonGeometry?.Coordinates == null)
        {
            return null;
        }

        switch (geoJsonGeometry.Type)
        {
            case GeometryType.Point:
                double[] coordinates = JSerializer.Deserialize<double[]>(geoJsonGeometry.Coordinates.ToString());
                return new Geometry.Point(coordinates[0], coordinates[1]);

            case GeometryType.LineString:
                double[][] lineString = JSerializer.Deserialize<double[][]>(geoJsonGeometry.Coordinates.ToString());
                var line = new Geometry.Polyline();
                var path = new Geometry.Path();
                line.AddPath(path);

                for (int i = 0, to = lineString.GetLength(0); i < to; i++)
                {
                    path.AddPoint(new Geometry.Point(lineString[i][0], lineString[i][1]));
                }

                return line;

            case GeometryType.MultiLineString:
                double[][][] paths = geoJsonGeometry.Coordinates is double[][][]
                    ? (double[][][])geoJsonGeometry.Coordinates 
                    : JSerializer.Deserialize<double[][][]>(geoJsonGeometry.Coordinates.ToString());

                var multiLine = new Geometry.Polyline();
                for (int p = 0, to = paths.GetLength(0); p < to; p++)
                {
                    var multiLinePath = new Geometry.Path();
                    multiLine.AddPath(multiLinePath);

                    var coords = paths[p];
                    for (int i = 0, to2 = coords.GetLength(0); i < to2; i++)
                    {
                        multiLinePath.AddPoint(new Geometry.Point(coords[i][0], coords[i][1]));
                    }
                }
                return multiLine;

            case GeometryType.MultiPolygon:
            case GeometryType.Polygon:
                object polygonString = geoJsonGeometry.Coordinates;
                if (!(polygonString is double[][]) && !(polygonString is double[][][]))
                {
                    try
                    {
                        polygonString = JSerializer.Deserialize<double[][]>(geoJsonGeometry.Coordinates.ToString());
                    }
                    catch
                    {
                        try
                        {
                            polygonString = JSerializer.Deserialize<double[][][]>(geoJsonGeometry.Coordinates.ToString());
                        }
                        catch
                        {
                            polygonString = JSerializer.Deserialize<double[][][][]>(geoJsonGeometry.Coordinates.ToString());
                        }
                    }
                }
                var polygon = new Geometry.Polygon();

                if (polygonString is double[][])
                {
                    var ring = new Geometry.Ring();
                    polygon.AddRing(ring);

                    var coords = (double[][])polygonString;
                    for (int i = 0, to = coords.GetLength(0); i < to; i++)
                    {
                        ring.AddPoint(new Geometry.Point(coords[i][0], coords[i][1]));
                    }
                }
                else if (polygonString is double[][][])
                {
                    var rings = (double[][][])polygonString;
                    for (int r = 0, to = rings.GetLength(0); r < to; r++)
                    {
                        var ring = new Geometry.Ring();
                        polygon.AddRing(ring);

                        var coords = rings[r];
                        for (int i = 0, to2 = coords.GetLength(0); i < to2; i++)
                        {
                            ring.AddPoint(new Geometry.Point(coords[i][0], coords[i][1]));
                        }
                    }
                }
                else if (polygonString is double[][][][]) // Multipolygon
                {
                    var subPolygons = (double[][][][])polygonString;

                    for (int p = 0, to_p = subPolygons.GetLength(0); p < to_p; p++)
                    {
                        var rings = subPolygons[p];
                        for (int r = 0, to_r = rings.GetLength(0); r < to_r; r++)
                        {
                            var ring = new Geometry.Ring();
                            polygon.AddRing(ring);

                            var coords = rings[r];
                            for (int i = 0, to2 = coords.GetLength(0); i < to2; i++)
                            {
                                ring.AddPoint(new Geometry.Point(coords[i][0], coords[i][1]));
                            }
                        }
                    }
                }

                return polygon;
        }

        return null;
    }

    static public gView.GeoJsonService.DTOs.Geometry? ToGeoJsonGeometry(this Core.Geometry.IGeometry shape)
    {
        gView.GeoJsonService.DTOs.Geometry? geoJsonGeometry = null;

        if (shape is Core.Geometry.IPoint point)
        {
            geoJsonGeometry = new gView.GeoJsonService.DTOs.Geometry()
            {
                Type = GeometryType.Point,
                Coordinates = new double[] { point.X, point.Y }
            };
        }
        else if (shape is Core.Geometry.IPolyline polyline)
        {
            if (polyline.PathCount == 1)
            {
                geoJsonGeometry = new gView.GeoJsonService.DTOs.Geometry()
                {
                    Type = GeometryType.LineString,
                    Coordinates = polyline[0]
                        .ToArray()
                        .Select(point => new double[] { point.X, point.Y })
                        .ToArray()
                };
            }
            else if (polyline.PathCount > 1)
            {
                geoJsonGeometry = new gView.GeoJsonService.DTOs.Geometry()
                {
                    Type = GeometryType.MultiLineString,
                    Coordinates = polyline
                        .ToArray()  // Paths
                        .Select(path => path
                                            .ToArray()  // Points
                                            .Select(point => new double[] { point.X, point.Y })
                                            .ToArray())
                        .ToArray()
                };
            }
        }
        else if (shape is Core.Geometry.IPolygon polygon)
        {
            geoJsonGeometry = new gView.GeoJsonService.DTOs.Geometry()
            {
                Type = GeometryType.Polygon,
                Coordinates = polygon
                    .ToArray()  // Rings
                    .Select(ring => ring
                                        .ToArray() // Points
                                        .Select(point => new double[] { point.X, point.Y })
                                        .ToArray())
                    .ToArray()
            };
        }

        return geoJsonGeometry;
    }
}
