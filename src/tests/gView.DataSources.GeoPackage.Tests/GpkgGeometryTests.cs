using gView.DataSources.GeoPackage;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using OgcConvert = gView.Framework.OGC.OGC;

namespace gView.DataSources.GeoPackage.Tests;

/// <summary>
/// Pure GeoPackage Binary (GPB) blob codec tests - no SQLite, no mod_spatialite.
/// </summary>
public class GpkgGeometryTests
{
    private static byte[] PointWkb(double x, double y)
        => OgcConvert.GeometryToWKB(new Point(x, y), 0, OgcConvert.WkbByteOrder.Ndr);

    [Fact]
    public void ToGpb_ThenToWkb_RoundTripsTheWkbUnchanged()
    {
        var wkb = PointWkb(12.5, -7.25);

        var gpb = GpkgGeometry.ToGpb(wkb, 25832, new Envelope(12.5, -7.25, 12.5, -7.25));

        Assert.True(GpkgGeometry.IsGpb(gpb));
        Assert.Equal(25832, GpkgGeometry.ReadSrid(gpb));
        Assert.Equal(wkb, GpkgGeometry.ToWkb(gpb));
    }

    [Fact]
    public void ToGpb_WithEnvelope_WritesGeoPackageEnvelopeOrder_MinXMaxXMinYMaxY()
    {
        var wkb = PointWkb(0, 0);
        var env = new Envelope(1, 2, 3, 4); // minx=1 miny=2 maxx=3 maxy=4

        var gpb = GpkgGeometry.ToGpb(wkb, 4326, env);

        // header: 2 magic + version + flags + int32 srid + 4 doubles (little-endian)
        int flags = gpb[3];
        Assert.Equal(1, (flags >> 1) & 0x07); // envelope indicator 1 => [minx,maxx,miny,maxy]

        double minx = BitConverter.ToDouble(gpb, 8);
        double maxx = BitConverter.ToDouble(gpb, 16);
        double miny = BitConverter.ToDouble(gpb, 24);
        double maxy = BitConverter.ToDouble(gpb, 32);

        Assert.Equal(1, minx);
        Assert.Equal(3, maxx);
        Assert.Equal(2, miny);
        Assert.Equal(4, maxy);
    }

    [Fact]
    public void ToGpb_WithoutEnvelope_HasNoEnvelopeAndShorterHeader()
    {
        var wkb = PointWkb(5, 5);

        var gpb = GpkgGeometry.ToGpb(wkb, 0, null);

        Assert.Equal(0, (gpb[3] >> 1) & 0x07);
        Assert.Equal(8 + wkb.Length, gpb.Length);
        Assert.Equal(wkb, GpkgGeometry.ToWkb(gpb));
    }

    [Fact]
    public void ToWkb_IsTolerantOfPlainWkbInput()
    {
        var wkb = PointWkb(9, 9);

        Assert.False(GpkgGeometry.IsGpb(wkb));
        Assert.Equal(wkb, GpkgGeometry.ToWkb(wkb));
        Assert.Equal(0, GpkgGeometry.ReadSrid(wkb));
    }

    [Fact]
    public void ToWkb_DecodesGeometryBackToTheSamePoint()
    {
        var gpb = GpkgGeometry.ToGpb(PointWkb(700123.5, 5300456.75), 25832,
            new Envelope(700123.5, 5300456.75, 700123.5, 5300456.75));

        var geometry = OgcConvert.WKBToGeometry(GpkgGeometry.ToWkb(gpb));

        var point = Assert.IsAssignableFrom<IPoint>(geometry);
        Assert.Equal(700123.5, point.X, 6);
        Assert.Equal(5300456.75, point.Y, 6);
    }
}
