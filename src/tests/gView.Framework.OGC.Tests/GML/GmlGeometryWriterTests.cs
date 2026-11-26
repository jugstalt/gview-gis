using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.OGC.GML;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace gView.Framework.OGC.Tests.GML;

public class GmlGeometryWriterTests
{
    private static readonly XNamespace Gml = GmlGeometryWriter.Gml2Namespace;
    private static readonly XNamespace Gml32 = GmlGeometryWriter.Gml32Namespace;

    [Fact]
    public void Point_Gml2_CoordinatesXY()
    {
        var xml = Write(new Point(10.5, 47.25), GmlFormat.Gml2, swapAxes: false, "EPSG:4326");

        var point = xml.Element(Gml + "Point")!;
        Assert.Equal("EPSG:4326", point.Attribute("srsName")!.Value);
        Assert.Equal("10.5,47.25", point.Element(Gml + "coordinates")!.Value);
    }

    [Fact]
    public void Point_Gml31_SwapAxes_PosNorthingFirst()
    {
        var xml = Write(new Point(10.5, 47.25), GmlFormat.Gml31, swapAxes: true, "urn:ogc:def:crs:EPSG::4326");

        Assert.Equal("47.25 10.5", xml.Element(Gml + "Point")!.Element(Gml + "pos")!.Value);
    }

    [Fact]
    public void Point_Gml32_HasGmlId()
    {
        var xml = Write(new Point(1, 2), GmlFormat.Gml32, swapAxes: false, "urn:ogc:def:crs:EPSG::3857", "c0.1.geom");

        var point = xml.Element(Gml32 + "Point")!;
        Assert.Equal("c0.1.geom", point.Attribute(Gml32 + "id")!.Value);
        Assert.Equal("1 2", point.Element(Gml32 + "pos")!.Value);
    }

    [Fact]
    public void Polyline_Gml2_MultiLineStringWithLineStringMembers()
    {
        var xml = Write(TwoPathPolyline(), GmlFormat.Gml2, swapAxes: false, "EPSG:31256");

        var multi = xml.Element(Gml + "MultiLineString")!;
        var lines = multi.Elements(Gml + "lineStringMember").Select(m => m.Element(Gml + "LineString")!).ToList();
        Assert.Equal(2, lines.Count);
        Assert.Equal("0,0 1,1", lines[0].Element(Gml + "coordinates")!.Value);
        Assert.Null(lines[0].Attribute("srsName"));  // only on the outer geometry
    }

    [Fact]
    public void Polyline_Gml32_MultiCurveWithIdsForEveryGeometry()
    {
        var xml = Write(TwoPathPolyline(), GmlFormat.Gml32, swapAxes: false, "urn:ogc:def:crs:EPSG::31256", "c0.1.geom");

        var multi = xml.Element(Gml32 + "MultiCurve")!;
        var lines = multi.Elements(Gml32 + "curveMember").Select(m => m.Element(Gml32 + "LineString")!).ToList();
        Assert.Equal(2, lines.Count);
        Assert.Equal("0 0 1 1", lines[0].Element(Gml32 + "posList")!.Value);

        var ids = xml.DescendantsAndSelf().Select(e => e.Attribute(Gml32 + "id")?.Value).Where(id => id is not null).ToList();
        Assert.Equal(3, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Polygon_TwoOuterRingsAndHole_Gml2_MultiPolygon()
    {
        var xml = Write(TwoOuterRingsWithHole(), GmlFormat.Gml2, swapAxes: false, null);

        var polygons = xml.Element(Gml + "MultiPolygon")!.Elements(Gml + "polygonMember").Select(m => m.Element(Gml + "Polygon")!).ToList();
        Assert.Equal(2, polygons.Count);
        Assert.Single(polygons.SelectMany(p => p.Elements(Gml + "innerBoundaryIs")));
        Assert.All(polygons, p => Assert.Single(p.Elements(Gml + "outerBoundaryIs")));
    }

    [Fact]
    public void Polygon_Gml31_MultiSurfaceExteriorInteriorClosedRings()
    {
        var xml = Write(TwoOuterRingsWithHole(), GmlFormat.Gml31, swapAxes: false, "EPSG:31256");

        var polygons = xml.Element(Gml + "MultiSurface")!.Elements(Gml + "surfaceMember").Select(m => m.Element(Gml + "Polygon")!).ToList();
        Assert.Equal(2, polygons.Count);

        var posList = polygons[0].Element(Gml + "exterior")!.Element(Gml + "LinearRing")!.Element(Gml + "posList")!.Value.Split(' ');
        Assert.Equal(posList[0], posList[^2]);  // closed
        Assert.Equal(posList[1], posList[^1]);
        Assert.Single(polygons.SelectMany(p => p.Elements(Gml + "interior")));
    }

    [Fact]
    public void Envelope_Gml31_LowerUpperCorner()
    {
        var xml = Write(new Envelope(1, 2, 3, 4), GmlFormat.Gml31, swapAxes: true, "urn:ogc:def:crs:EPSG::4326");

        var envelope = xml.Element(Gml + "Envelope")!;
        Assert.Equal("2 1", envelope.Element(Gml + "lowerCorner")!.Value);
        Assert.Equal("4 3", envelope.Element(Gml + "upperCorner")!.Value);
    }

    [Fact]
    public void Number_InvariantCultureRoundTrip()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-AT");
            Assert.Equal("1234.5678", GmlGeometryWriter.Number(1234.5678));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    [Theory]
    [InlineData(GeometryType.Point, GmlFormat.Gml2, "PointPropertyType")]
    [InlineData(GeometryType.Polyline, GmlFormat.Gml2, "MultiLineStringPropertyType")]
    [InlineData(GeometryType.Polyline, GmlFormat.Gml32, "MultiCurvePropertyType")]
    [InlineData(GeometryType.Polygon, GmlFormat.Gml2, "MultiPolygonPropertyType")]
    [InlineData(GeometryType.Polygon, GmlFormat.Gml31, "MultiSurfacePropertyType")]
    [InlineData(GeometryType.Unknown, GmlFormat.Gml32, "GeometryPropertyType")]
    public void PropertyTypeName_MatchesWrittenGeometry(GeometryType geometryType, GmlFormat format, string expected)
        => Assert.Equal(expected, GmlGeometryWriter.PropertyTypeName(geometryType, format));

    #region Helper

    private static XElement Write(IGeometry geometry, GmlFormat format, bool swapAxes, string? srsName, string? gmlId = null)
    {
        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, new XmlWriterSettings() { OmitXmlDeclaration = true }))
        {
            writer.WriteStartElement("root");
            writer.WriteAttributeString("xmlns", "gml", null, GmlGeometryWriter.NamespaceOf(format));

            var gmlWriter = new GmlGeometryWriter(writer, format, swapAxes);
            if (geometry is IEnvelope envelope)
            {
                gmlWriter.WriteEnvelope(envelope, srsName);
            }
            else
            {
                Assert.True(gmlWriter.WriteGeometry(geometry, srsName, gmlId));
            }

            writer.WriteEndElement();
        }

        return XElement.Parse(sb.ToString());
    }

    private static List<IPoint> Points(params IPoint[] points) => points.ToList();

    private static Polyline TwoPathPolyline()
    {
        var polyline = new Polyline();
        polyline.AddPath(new gView.Framework.Geometry.Path(Points(new Point(0, 0), new Point(1, 1))));
        polyline.AddPath(new gView.Framework.Geometry.Path(Points(new Point(2, 2), new Point(3, 3))));
        return polyline;
    }

    private static Polygon TwoOuterRingsWithHole()
    {
        var polygon = new Polygon();
        polygon.AddRing(new Ring(Points(new Point(0, 0), new Point(0, 10), new Point(10, 10), new Point(10, 0))));
        polygon.AddRing(new Hole(Points(new Point(2, 2), new Point(8, 2), new Point(8, 8), new Point(2, 8))));
        polygon.AddRing(new Ring(Points(new Point(20, 0), new Point(20, 10), new Point(30, 10), new Point(30, 0))));
        return polygon;
    }

    #endregion
}
