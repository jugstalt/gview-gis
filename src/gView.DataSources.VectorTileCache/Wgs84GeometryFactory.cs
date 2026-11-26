using gView.Framework.Geometry;
using SpatialNET;
using System;

namespace gView.DataSources.VectorTileCache;

/// <summary>
/// Creates the geometries SpatialNET decodes from a vector tile (WebMercator meters) as gView geometries in WGS84,
/// the spatial reference of the dataset.
/// </summary>
internal sealed class Wgs84GeometryFactory : IGeometryFactory
{
    private const double EarthRadius = 6378137.0;

    public static readonly Wgs84GeometryFactory Instance = new Wgs84GeometryFactory();

    private Wgs84GeometryFactory() { }

    public bool SupportsZ => false;

    public IPoint2D CreatePoint(Coordinate coordinate, IGeometry2D source)
        => ToPoint(coordinate);

    public IMultiPoint2D CreateMultiPoint(ReadOnlySpan<Coordinate> coordinates, IGeometry2D source)
        => Fill(new MultiPoint(), coordinates);

    public IPolyline2D CreatePolyline(ReadOnlySpan<ReadOnlyMemory<Coordinate>> paths, IGeometry2D source)
    {
        var polyline = new Polyline();
        foreach (var path in paths)
        {
            polyline.AddPath(Fill(new Path(), path.Span));
        }

        return polyline;
    }

    public IPolygon2D CreatePolygon(ReadOnlySpan<PolygonPart> parts, IGeometry2D source)
    {
        var polygon = new Polygon();
        foreach (var part in parts)
        {
            polygon.AddRing(Fill(new Ring(), part.Shell.Span));
            foreach (var hole in part.Holes.Span)
            {
                polygon.AddRing(Fill(new Hole(), hole.Span));
            }
        }

        return polygon;
    }

    /// <summary>WebMercator (EPSG:3857) meters to longitude/latitude in degrees.</summary>
    internal static (double Longitude, double Latitude) ToWgs84(double x, double y)
        => (x / EarthRadius * 180.0 / Math.PI,
            (2.0 * Math.Atan(Math.Exp(y / EarthRadius)) - Math.PI / 2.0) * 180.0 / Math.PI);

    private static Point ToPoint(Coordinate coordinate)
    {
        var (lon, lat) = ToWgs84(coordinate.X, coordinate.Y);
        return new Point(lon, lat);
    }

    private static T Fill<T>(T collection, ReadOnlySpan<Coordinate> coordinates)
        where T : PointCollection
    {
        foreach (var coordinate in coordinates)
        {
            collection.AddPoint(ToPoint(coordinate));
        }

        return collection;
    }
}
