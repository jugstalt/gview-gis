#nullable enable

using gView.Framework.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace gView.Framework.OGC.GML;

/// <summary>
/// The GML encodings of the WFS versions
/// </summary>
public enum GmlFormat
{
    /// <summary>GML 2.1.2 (WFS 1.0.0)</summary>
    Gml2,
    /// <summary>GML 3.1.1 (WFS 1.1.0)</summary>
    Gml31,
    /// <summary>GML 3.2 (WFS 2.0.0)</summary>
    Gml32
}

/// <summary>
/// Writes gView geometries as valid GML 2.1.2, 3.1.1 or 3.2 with an <see cref="XmlWriter"/>.
/// Lines and polygons are always written as multi geometries (MultiLineString/MultiPolygon in GML 2,
/// MultiCurve/MultiSurface in GML 3), so they match the property types of the feature type schema
/// (see <see cref="PropertyTypeName"/>). Polygons are grouped into outer rings with their holes,
/// rings are always closed.
/// </summary>
public sealed class GmlGeometryWriter
{
    public const string Gml2Namespace = "http://www.opengis.net/gml";
    public const string Gml32Namespace = "http://www.opengis.net/gml/3.2";

    private readonly XmlWriter _writer;
    private readonly bool _swapAxes;
    private int _idCounter;
    private string? _idBase;

    /// <param name="writer">The writer, the gml prefix should already be declared</param>
    /// <param name="format">The GML version</param>
    /// <param name="swapAxes">Write y x (northing first) instead of gView's x y order</param>
    public GmlGeometryWriter(XmlWriter writer, GmlFormat format, bool swapAxes)
    {
        _writer = writer;
        Format = format;
        _swapAxes = swapAxes;
    }

    public GmlFormat Format { get; }

    public string Namespace => NamespaceOf(Format);

    public static string NamespaceOf(GmlFormat format)
        => format == GmlFormat.Gml32 ? Gml32Namespace : Gml2Namespace;

    /// <summary>
    /// The GML property type of a feature type's geometry property
    /// </summary>
    public static string PropertyTypeName(GeometryType geometryType, GmlFormat format)
        => (geometryType, format) switch
        {
            (GeometryType.Point, _) => "PointPropertyType",
            (GeometryType.Multipoint, _) => "MultiPointPropertyType",
            (GeometryType.Polyline, GmlFormat.Gml2) => "MultiLineStringPropertyType",
            (GeometryType.Polyline, _) => "MultiCurvePropertyType",
            (GeometryType.Polygon, GmlFormat.Gml2) => "MultiPolygonPropertyType",
            (GeometryType.Polygon, _) => "MultiSurfacePropertyType",
            _ => "GeometryPropertyType"
        };

    /// <summary>
    /// Writes the geometry element (without the property element around it).
    /// </summary>
    /// <param name="srsName">srsName of the geometry, null to omit it</param>
    /// <param name="gmlId">The base of the gml:id attributes (GML 3.2 requires an id for every geometry)</param>
    /// <returns>false, if there is nothing to write (null or empty geometry)</returns>
    public bool WriteGeometry(IGeometry? geometry, string? srsName, string? gmlId = null)
    {
        if (geometry is null || IsEmpty(geometry))
        {
            return false;
        }

        _idBase = gmlId;
        _idCounter = 0;

        WriteGeometryElement(geometry, srsName);
        return true;
    }

    /// <summary>
    /// Writes gml:Box (GML 2) or gml:Envelope (GML 3)
    /// </summary>
    public void WriteEnvelope(IEnvelope envelope, string? srsName)
    {
        if (Format == GmlFormat.Gml2)
        {
            StartGeometry("Box", srsName, withId: false);
            WriteCoordinates([(envelope.MinX, envelope.MinY), (envelope.MaxX, envelope.MaxY)]);
            _writer.WriteEndElement();
        }
        else
        {
            _writer.WriteStartElement("gml", "Envelope", Namespace);
            if (!String.IsNullOrEmpty(srsName))
            {
                _writer.WriteAttributeString("srsName", srsName);
            }
            _writer.WriteElementString("gml", "lowerCorner", Namespace, Position(envelope.MinX, envelope.MinY));
            _writer.WriteElementString("gml", "upperCorner", Namespace, Position(envelope.MaxX, envelope.MaxY));
            _writer.WriteEndElement();
        }
    }

    #region Geometries

    private void WriteGeometryElement(IGeometry geometry, string? srsName)
    {
        switch (geometry)
        {
            case IPoint point:
                WritePoint(point, srsName);
                break;
            case IMultiPoint multiPoint:
                StartGeometry("MultiPoint", srsName);
                for (int i = 0; i < multiPoint.PointCount; i++)
                {
                    _writer.WriteStartElement("gml", "pointMember", Namespace);
                    WritePoint(multiPoint[i], null);
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
                break;
            case IPolyline polyline:
                WritePolyline(polyline, srsName);
                break;
            case IPolygon polygon:
                WritePolygons(polygon, srsName);
                break;
            case IEnvelope envelope:
                WriteEnvelope(envelope, srsName);
                break;
            case IAggregateGeometry aggregate:
                StartGeometry("MultiGeometry", srsName);
                for (int i = 0; i < aggregate.GeometryCount; i++)
                {
                    if (aggregate[i] is null || IsEmpty(aggregate[i]))
                    {
                        continue;
                    }
                    _writer.WriteStartElement("gml", "geometryMember", Namespace);
                    WriteGeometryElement(aggregate[i], null);
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
                break;
            default:
                throw new NotSupportedException($"Geometry type {geometry.GetType().Name} can't be written as GML");
        }
    }

    private void WritePoint(IPoint point, string? srsName)
    {
        StartGeometry("Point", srsName);
        if (Format == GmlFormat.Gml2)
        {
            WriteCoordinates([(point.X, point.Y)]);
        }
        else
        {
            _writer.WriteElementString("gml", "pos", Namespace, Position(point.X, point.Y));
        }
        _writer.WriteEndElement();
    }

    private void WritePolyline(IPolyline polyline, string? srsName)
    {
        bool gml2 = Format == GmlFormat.Gml2;

        StartGeometry(gml2 ? "MultiLineString" : "MultiCurve", srsName);
        for (int p = 0; p < polyline.PathCount; p++)
        {
            var path = polyline[p];
            if (path is null || path.PointCount < 2)
            {
                continue;
            }

            _writer.WriteStartElement("gml", gml2 ? "lineStringMember" : "curveMember", Namespace);
            StartGeometry("LineString", null);
            WritePoints(path, closeRing: false);
            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }
        _writer.WriteEndElement();
    }

    private void WritePolygons(IPolygon polygon, string? srsName)
    {
        bool gml2 = Format == GmlFormat.Gml2;

        StartGeometry(gml2 ? "MultiPolygon" : "MultiSurface", srsName);
        foreach (var outerRing in polygon.OuterRings())
        {
            if (outerRing is null || outerRing.PointCount < 3)
            {
                continue;
            }

            _writer.WriteStartElement("gml", gml2 ? "polygonMember" : "surfaceMember", Namespace);
            StartGeometry("Polygon", null);

            WriteRing(outerRing, gml2 ? "outerBoundaryIs" : "exterior");
            foreach (var hole in polygon.InnerRings(outerRing) ?? [])
            {
                if (hole is not null && hole.PointCount >= 3)
                {
                    WriteRing(hole, gml2 ? "innerBoundaryIs" : "interior");
                }
            }

            _writer.WriteEndElement();  // Polygon
            _writer.WriteEndElement();  // polygonMember/surfaceMember
        }
        _writer.WriteEndElement();
    }

    private void WriteRing(IPointCollection ring, string boundaryElement)
    {
        _writer.WriteStartElement("gml", boundaryElement, Namespace);
        _writer.WriteStartElement("gml", "LinearRing", Namespace);  // no gml:id: LinearRing is no AbstractGMLType in 3.2
        WritePoints(ring, closeRing: true);
        _writer.WriteEndElement();
        _writer.WriteEndElement();
    }

    #endregion

    #region Coordinates

    private void WritePoints(IPointCollection points, bool closeRing)
    {
        var coordinates = new List<(double x, double y)>(points.PointCount + 1);
        for (int i = 0; i < points.PointCount; i++)
        {
            coordinates.Add((points[i].X, points[i].Y));
        }
        if (closeRing && coordinates.Count > 0 && coordinates[0] != coordinates[coordinates.Count - 1])
        {
            coordinates.Add(coordinates[0]);
        }

        if (Format == GmlFormat.Gml2)
        {
            WriteCoordinates(coordinates);
        }
        else
        {
            _writer.WriteStartElement("gml", "posList", Namespace);
            _writer.WriteAttributeString("srsDimension", "2");
            _writer.WriteString(String.Join(" ", coordinates.Select(c => Position(c.x, c.y))));
            _writer.WriteEndElement();
        }
    }

    private void WriteCoordinates(IEnumerable<(double x, double y)> coordinates)
    {
        _writer.WriteStartElement("gml", "coordinates", Namespace);
        _writer.WriteAttributeString("decimal", ".");
        _writer.WriteAttributeString("cs", ",");
        _writer.WriteAttributeString("ts", " ");

        var sb = new StringBuilder();
        foreach (var (x, y) in coordinates)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }
            var (first, second) = _swapAxes ? (y, x) : (x, y);
            sb.Append(Number(first)).Append(',').Append(Number(second));
        }
        _writer.WriteString(sb.ToString());
        _writer.WriteEndElement();
    }

    private string Position(double x, double y)
        => _swapAxes
            ? $"{Number(y)} {Number(x)}"
            : $"{Number(x)} {Number(y)}";

    public static string Number(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    #endregion

    #region Helper

    private void StartGeometry(string name, string? srsName, bool withId = true)
    {
        _writer.WriteStartElement("gml", name, Namespace);

        if (withId && Format == GmlFormat.Gml32 && !String.IsNullOrEmpty(_idBase))
        {
            // GML 3.2: every geometry (AbstractGMLType) needs a gml:id
            _writer.WriteAttributeString("gml", "id", Namespace, _idCounter == 0 ? _idBase : $"{_idBase}.{_idCounter}");
            _idCounter++;
        }
        if (!String.IsNullOrEmpty(srsName))
        {
            _writer.WriteAttributeString("srsName", srsName);
        }
    }

    private static bool IsEmpty(IGeometry geometry)
        => geometry switch
        {
            IPoint => false,
            IMultiPoint multiPoint => multiPoint.PointCount == 0,
            IPolyline polyline => polyline.PathCount == 0 || Enumerable.Range(0, polyline.PathCount).All(i => polyline[i] is null || polyline[i].PointCount < 2),
            IPolygon polygon => polygon.RingCount == 0,
            IAggregateGeometry aggregate => aggregate.GeometryCount == 0,
            _ => false
        };

    #endregion
}
