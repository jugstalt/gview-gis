using gView.DataSources.Fdb;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using GeomPath = gView.Framework.Geometry.Path;

namespace gView.DataSources.Fdb.Tests;

public class FdbGeometryCodecTests
{
    // ---- codec selection --------------------------------------------------

    [Fact]
    public void For_MapsStorageTypesToCodecs()
    {
        Assert.Same(FdbGeometryCodec.Proprietary, FdbGeometryCodec.For(GeometryStorageType.Classic));
        Assert.Null(FdbGeometryCodec.For(GeometryStorageType.PostGis));
        Assert.Null(FdbGeometryCodec.For(GeometryStorageType.SqlServerGeometry));
        Assert.Null(FdbGeometryCodec.For(GeometryStorageType.SpatiaLite));
        Assert.Null(FdbGeometryCodec.For(GeometryStorageType.GeoPackage));
    }

    [Fact]
    public void IsDatabaseNative_Mapping()
    {
        Assert.False(GeometryStorageType.Classic.IsDatabaseNative());
        Assert.True(GeometryStorageType.PostGis.IsDatabaseNative());
        Assert.True(GeometryStorageType.SqlServerGeometry.IsDatabaseNative());
        Assert.True(GeometryStorageType.SqlServerGeography.IsDatabaseNative());
        Assert.True(GeometryStorageType.SpatiaLite.IsDatabaseNative());
        Assert.True(GeometryStorageType.GeoPackage.IsDatabaseNative());
    }

    [Fact]
    public void Wkb_EmitsLittleEndianWkb()
    {
        var def = new GeometryDef(GeometryType.Point);
        byte[] wkb = FdbGeometryCodec.Wkb.Encode(new Point(1, 2), def);

        Assert.Equal(0x01, wkb[0]);          // NDR byte order
        // base type 1 (Point), no Z/M/SRID high bits
        Assert.Equal((byte)0x01, wkb[1]);
        Assert.Equal((byte)0x00, wkb[4]);
    }

    // ---- round trips: both codecs must preserve the geometry ----------------

    public static IEnumerable<object[]> Codecs() =>
        new[] { new object[] { FdbGeometryCodec.Proprietary }, new object[] { FdbGeometryCodec.Wkb } };

    [Theory]
    [MemberData(nameof(Codecs))]
    public void RoundTrip_Point2D(IFdbGeometryCodec codec)
    {
        var def = new GeometryDef(GeometryType.Point);
        var p = (IPoint)RoundTrip(codec, new Point(12.5, -7.25), def);

        Assert.Equal(12.5, p.X);
        Assert.Equal(-7.25, p.Y);
    }

    [Theory]
    [MemberData(nameof(Codecs))]
    public void RoundTrip_PointZ(IFdbGeometryCodec codec)
    {
        var def = new GeometryDef(GeometryType.Point, null, hasZ: true);
        var p = (IPoint)RoundTrip(codec, new Point(1, 2, 33.0), def);

        Assert.Equal(1, p.X);
        Assert.Equal(2, p.Y);
        Assert.Equal(33.0, p.Z);
    }

    [Theory]
    [MemberData(nameof(Codecs))]
    public void RoundTrip_PointM(IFdbGeometryCodec codec)
    {
        var def = new GeometryDef(GeometryType.Point) { HasM = true };
        var src = new Point(1, 2) { M = 99.0 };

        var p = (IPoint)RoundTrip(codec, src, def);

        Assert.Equal(1, p.X);
        Assert.Equal(2, p.Y);
        Assert.Equal(99.0, p.M);
    }

    [Theory]
    [MemberData(nameof(Codecs))]
    public void RoundTrip_MultiPathPolyline(IFdbGeometryCodec codec)
    {
        var def = new GeometryDef(GeometryType.Polyline);

        var line = new Polyline();
        line.AddPath(PathOf((0, 0), (1, 1), (2, 0)));
        line.AddPath(PathOf((10, 10), (11, 12)));

        var result = (IPolyline)RoundTrip(codec, line, def);

        Assert.Equal(2, result.PathCount);
        Assert.Equal(3, result[0].PointCount);
        Assert.Equal(2, result[1].PointCount);
        Assert.Equal(11, result[1][1].X);
        Assert.Equal(12, result[1][1].Y);
    }

    [Theory]
    [MemberData(nameof(Codecs))]
    public void RoundTrip_PolygonWithHole(IFdbGeometryCodec codec)
    {
        var def = new GeometryDef(GeometryType.Polygon);

        var poly = new Polygon();
        poly.AddRing(RingOf((0, 0), (0, 10), (10, 10), (10, 0)));
        poly.AddRing(RingOf((2, 2), (2, 4), (4, 4), (4, 2)));

        var result = (IPolygon)RoundTrip(codec, poly, def);

        Assert.Equal(2, result.RingCount);
        Assert.Equal(4, result[0].PointCount);
        Assert.Equal(4, result[1].PointCount);
        // outer ring vertex preserved
        Assert.True(HasVertex(result[0], 10, 10));
    }

    // ---- helpers --------------------------------------------------------

    private static IGeometry RoundTrip(IFdbGeometryCodec codec, IGeometry shape, IGeometryDef def)
        => codec.Decode(codec.Encode(shape, def), def);

    private static bool HasVertex(IPointCollection pc, double x, double y)
    {
        for (int i = 0; i < pc.PointCount; i++)
        {
            if (pc[i].X == x && pc[i].Y == y)
            {
                return true;
            }
        }
        return false;
    }

    private static GeomPath PathOf(params (double x, double y)[] pts)
    {
        var path = new GeomPath();
        foreach (var (x, y) in pts)
        {
            path.AddPoint(new Point(x, y));
        }
        return path;
    }

    private static Ring RingOf(params (double x, double y)[] pts)
    {
        var ring = new Ring();
        foreach (var (x, y) in pts)
        {
            ring.AddPoint(new Point(x, y));
        }
        return ring;
    }
}
