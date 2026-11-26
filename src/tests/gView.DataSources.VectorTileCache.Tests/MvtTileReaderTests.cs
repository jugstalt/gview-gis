using gView.Framework.Core.Geometry;
using SpatialNET.IO;
using GvPoint = gView.Framework.Geometry.Point;
using GvPolygon = gView.Framework.Geometry.Polygon;
using GvPolyline = gView.Framework.Geometry.Polyline;
using GvPath = gView.Framework.Geometry.Path;
using GvRing = gView.Framework.Geometry.Ring;
using GvHole = gView.Framework.Geometry.Hole;

namespace gView.DataSources.VectorTileCache.Tests;

public class MvtTileReaderTests
{
    // a tile in Carinthia, Austria
    private const int Level = 14, Col = 8841, Row = 5787;

    // one MVT unit at level 14 is about 0.6 m, far below 1e-5 degrees
    private const double Tolerance = 1e-5;

    private static readonly TileGrid Grid = TileGrid.WebMercator();

    [Fact]
    public void Read_Point_ReturnsWgs84PointWithAttributesAndId()
    {
        var envelope = Grid.TileEnvelope(Level, Row, Col);
        double x = envelope.MinX + envelope.Width * 0.25, y = envelope.MinY + envelope.Height * 0.75;
        var bytes = Build(builder => builder.AddFeature("poi", new GvPoint(x, y),
            [new("name", "Klagenfurt"), new("rank", 3L), new("open", true)], id: 42));

        var layers = MvtTileReader.Read(bytes, Level, Col, Row);

        var feature = Assert.Single(layers["poi"]);
        var point = Assert.IsAssignableFrom<IPoint>(feature.Shape);
        var (lon, lat) = Wgs84GeometryFactory.ToWgs84(x, y);
        Assert.Equal(lon, point.X, Tolerance);
        Assert.Equal(lat, point.Y, Tolerance);
        Assert.Equal(42UL, feature.Id);
        var attributes = feature.Attributes.ToDictionary(a => a.Key, a => a.Value.Value);
        Assert.Equal("Klagenfurt", attributes["name"]);
        Assert.Equal(3L, Convert.ToInt64(attributes["rank"]));
        Assert.Equal(true, attributes["open"]);
    }

    [Fact]
    public void Read_Polyline_ReturnsWgs84Path()
    {
        var envelope = Grid.TileEnvelope(Level, Row, Col);
        var path = new GvPath();
        path.AddPoint(new GvPoint(envelope.MinX + 100, envelope.MinY + 100));
        path.AddPoint(new GvPoint(envelope.MaxX - 100, envelope.MaxY - 100));
        var polyline = new GvPolyline();
        polyline.AddPath(path);
        var bytes = Build(builder => builder.AddFeature("roads", polyline));

        var layers = MvtTileReader.Read(bytes, Level, Col, Row);

        var shape = Assert.IsAssignableFrom<IPolyline>(Assert.Single(layers["roads"]).Shape);
        Assert.Equal(1, shape.PathCount);
        var first = shape[0][0];
        var (lon, lat) = Wgs84GeometryFactory.ToWgs84(envelope.MinX + 100, envelope.MinY + 100);
        Assert.Equal(lon, first.X, Tolerance);
        Assert.Equal(lat, first.Y, Tolerance);
    }

    [Fact]
    public void Read_PolygonWithHole_ReturnsRingAndHole()
    {
        var envelope = Grid.TileEnvelope(Level, Row, Col);
        double cx = envelope.CenterX(), cy = envelope.CenterY();
        var polygon = new GvPolygon();
        polygon.AddRing(Square<GvRing>(cx, cy, 800));
        polygon.AddRing(Square<GvHole>(cx, cy, 200));
        var bytes = Build(builder => builder.AddFeature("parcels", polygon, id: 7));

        var layers = MvtTileReader.Read(bytes, Level, Col, Row);

        var shape = Assert.IsAssignableFrom<IPolygon>(Assert.Single(layers["parcels"]).Shape);
        Assert.Equal(2, shape.RingCount);
        Assert.IsNotType<GvHole>(shape[0]);
        Assert.IsType<GvHole>(shape[1]);
        var (lon, lat) = Wgs84GeometryFactory.ToWgs84(cx - 800, cy - 800);
        var envelopeWgs84 = shape.Envelope;
        Assert.Equal(lon, envelopeWgs84.MinX, Tolerance);
        Assert.Equal(lat, envelopeWgs84.MinY, Tolerance);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Read_GzippedOrPlain_DecodesBoth(bool gzip)
    {
        var envelope = Grid.TileEnvelope(Level, Row, Col);
        var bytes = Build(builder => builder.AddFeature("poi", new GvPoint(envelope.CenterX(), envelope.CenterY())), gzip);

        var layers = MvtTileReader.Read(bytes, Level, Col, Row);

        Assert.Single(layers["poi"]);
    }

    [Fact]
    public void Read_EmptyData_ReturnsNoLayers()
    {
        var layers = MvtTileReader.Read(ReadOnlySpan<byte>.Empty, Level, Col, Row);

        Assert.Empty(layers);
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(20037508.342789244, 0, 180, 0)]
    [InlineData(-20037508.342789244, 20037508.342789244, -180, 85.0511287798066)]
    public void ToWgs84_KnownCoordinates(double x, double y, double expectedLon, double expectedLat)
    {
        var (lon, lat) = Wgs84GeometryFactory.ToWgs84(x, y);

        Assert.Equal(expectedLon, lon, 1e-9);
        Assert.Equal(expectedLat, lat, 1e-9);
    }

    private static byte[] Build(Action<MvtTileBuilder> add, bool gzip = true)
    {
        var builder = new MvtTileBuilder(Grid, Level, Row, Col);
        add(builder);
        return builder.ToBytes(gzip);
    }

    private static T Square<T>(double cx, double cy, double half) where T : gView.Framework.Geometry.PointCollection, new()
    {
        var ring = new T();
        ring.AddPoint(new GvPoint(cx - half, cy - half));
        ring.AddPoint(new GvPoint(cx + half, cy - half));
        ring.AddPoint(new GvPoint(cx + half, cy + half));
        ring.AddPoint(new GvPoint(cx - half, cy + half));
        ring.AddPoint(new GvPoint(cx - half, cy - half));
        return ring;
    }
}

internal static class Envelope2DExtensions
{
    public static double CenterX(this SpatialNET.Envelope2D envelope) => (envelope.MinX + envelope.MaxX) / 2.0;

    public static double CenterY(this SpatialNET.Envelope2D envelope) => (envelope.MinY + envelope.MaxY) / 2.0;
}
