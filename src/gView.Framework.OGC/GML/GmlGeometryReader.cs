#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace gView.Framework.OGC.GML;

/// <summary>
/// Reads GML 2.1.2, 3.1.1 and 3.2 geometries (e.g. of filters) independent of the GML namespace:
/// Point, LineString, LinearRing, Polygon, Box/Envelope, MultiPoint, MultiLineString/MultiCurve,
/// MultiPolygon/MultiSurface and MultiGeometry with coordinates, coord, pos or posList.
/// </summary>
public static class GmlGeometryReader
{
    /// <summary>
    /// The srsName of the geometry element, null if there is none
    /// </summary>
    public static string? SrsName(XmlElement element)
        => element.GetAttribute("srsName") is { Length: > 0 } srsName ? srsName : null;

    /// <param name="swapAxes">The coordinates are written northing first (y x)</param>
    /// <exception cref="FormatException">Unknown or invalid geometry</exception>
    public static IGeometry Read(XmlElement element, bool swapAxes)
    {
        switch (element.LocalName)
        {
            case "Point":
                return ReadPoints(element, swapAxes).FirstOrDefault()
                       ?? throw new FormatException("gml:Point without coordinates");

            case "LineString":
                {
                    var polyline = new Polyline();
                    polyline.AddPath(new Path(ReadPoints(element, swapAxes)));
                    return polyline;
                }

            case "LinearRing":
            case "Polygon":
                {
                    var polygon = new Polygon();
                    AddPolygon(polygon, element, swapAxes);
                    return polygon;
                }

            case "Box":
            case "Envelope":
                {
                    var corners = element.LocalName == "Envelope" && Child(element, "lowerCorner") is { } lower && Child(element, "upperCorner") is { } upper
                        ? ParsePositions(lower.InnerText, swapAxes).Concat(ParsePositions(upper.InnerText, swapAxes)).ToList()
                        : ReadPoints(element, swapAxes);
                    if (corners.Count != 2)
                    {
                        throw new FormatException($"gml:{element.LocalName} needs two corners");
                    }
                    return new Envelope(
                        Math.Min(corners[0].X, corners[1].X), Math.Min(corners[0].Y, corners[1].Y),
                        Math.Max(corners[0].X, corners[1].X), Math.Max(corners[0].Y, corners[1].Y));
                }

            case "MultiPoint":
                {
                    var multiPoint = new MultiPoint();
                    foreach (var point in Members(element, "pointMember", "pointMembers"))
                    {
                        multiPoint.AddPoint((IPoint)Read(point, swapAxes));
                    }
                    return multiPoint;
                }

            case "MultiLineString":
            case "MultiCurve":
                {
                    var polyline = new Polyline();
                    foreach (var line in Members(element, "lineStringMember", "curveMember", "curveMembers"))
                    {
                        if (Read(line, swapAxes) is IPolyline part)
                        {
                            for (int i = 0; i < part.PathCount; i++)
                            {
                                polyline.AddPath(part[i]);
                            }
                        }
                    }
                    return polyline;
                }

            case "MultiPolygon":
            case "MultiSurface":
                {
                    var polygon = new Polygon();
                    foreach (var member in Members(element, "polygonMember", "surfaceMember", "surfaceMembers"))
                    {
                        AddPolygon(polygon, member, swapAxes);
                    }
                    return polygon;
                }

            case "Surface":
                {
                    var polygon = new Polygon();
                    foreach (var patch in Descendants(element, "PolygonPatch"))
                    {
                        AddPolygon(polygon, patch, swapAxes);
                    }
                    return polygon;
                }

            case "MultiGeometry":
                {
                    var aggregate = new AggregateGeometry();
                    foreach (var member in Members(element, "geometryMember", "geometryMembers"))
                    {
                        aggregate.AddGeometry(Read(member, swapAxes));
                    }
                    return aggregate;
                }

            default:
                throw new FormatException($"Unsupported GML geometry {element.LocalName}");
        }
    }

    #region Helper

    private static void AddPolygon(Polygon polygon, XmlElement element, bool swapAxes)
    {
        if (element.LocalName == "LinearRing")
        {
            polygon.AddRing(new Ring(ReadPoints(element, swapAxes)));
            return;
        }
        if (element.LocalName == "Surface")
        {
            foreach (var patch in Descendants(element, "PolygonPatch"))
            {
                AddPolygon(polygon, patch, swapAxes);
            }
            return;
        }

        foreach (XmlNode child in element.ChildNodes)
        {
            if (child is not XmlElement boundary)
            {
                continue;
            }

            bool outer = boundary.LocalName is "exterior" or "outerBoundaryIs";
            bool inner = boundary.LocalName is "interior" or "innerBoundaryIs";
            if (!outer && !inner)
            {
                continue;
            }

            var ring = Child(boundary, "LinearRing")
                ?? throw new FormatException($"gml:{boundary.LocalName} without gml:LinearRing");
            var points = ReadPoints(ring, swapAxes);
            polygon.AddRing(outer ? new Ring(points) : new Hole(points));
        }
    }

    /// <summary>
    /// The coordinates of a simple geometry: gml:coordinates, gml:coord, gml:pos(s) or gml:posList
    /// </summary>
    private static List<IPoint> ReadPoints(XmlElement element, bool swapAxes)
    {
        var points = new List<IPoint>();

        foreach (XmlNode node in element.ChildNodes)
        {
            if (node is not XmlElement child)
            {
                continue;
            }

            switch (child.LocalName)
            {
                case "coordinates":
                    {
                        string cs = child.GetAttribute("cs") is { Length: > 0 } c ? c : ",";
                        string ts = child.GetAttribute("ts") is { Length: > 0 } t ? t : " ";
                        string dec = child.GetAttribute("decimal") is { Length: > 0 } d ? d : ".";

                        foreach (var tuple in Normalize(child.InnerText, ts).Split(ts == " " ? ' ' : ts[0], StringSplitOptions.RemoveEmptyEntries))
                        {
                            var values = tuple.Split(cs[0]).Select(v => ParseNumber(dec == "." ? v : v.Replace(dec, "."))).ToArray();
                            if (values.Length < 2)
                            {
                                throw new FormatException($"Invalid gml:coordinates {child.InnerText}");
                            }
                            points.Add(ToPoint(values[0], values[1], swapAxes));
                        }
                        break;
                    }
                case "coord":
                    {
                        var x = Child(child, "X") ?? throw new FormatException("gml:coord without X");
                        var y = Child(child, "Y") ?? throw new FormatException("gml:coord without Y");
                        points.Add(ToPoint(ParseNumber(x.InnerText), ParseNumber(y.InnerText), swapAxes));
                        break;
                    }
                case "pos":
                    points.AddRange(ParsePositions(child.InnerText, swapAxes, Dimension(child, element)));
                    break;
                case "posList":
                    points.AddRange(ParsePositions(child.InnerText, swapAxes, Dimension(child, element)));
                    break;
                case "pointMember":
                case "pointProperty":
                    if (Child(child, "Point") is { } point)
                    {
                        points.AddRange(ReadPoints(point, swapAxes));
                    }
                    break;
            }
        }

        return points;
    }

    private static string Normalize(string text, string ts)
        => ts == " "
            ? String.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            : text.Trim();

    private static int Dimension(XmlElement positions, XmlElement geometry)
    {
        foreach (var element in new[] { positions, geometry })
        {
            if (int.TryParse(element.GetAttribute("srsDimension"), out int dimension) && dimension >= 2)
            {
                return dimension;
            }
        }
        return 2;
    }

    private static List<IPoint> ParsePositions(string text, bool swapAxes, int dimension = 2)
    {
        var values = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(ParseNumber).ToArray();
        if (values.Length % dimension != 0)
        {
            throw new FormatException($"Invalid GML positions {text}");
        }

        var points = new List<IPoint>();
        for (int i = 0; i < values.Length; i += dimension)
        {
            points.Add(ToPoint(values[i], values[i + 1], swapAxes));
        }
        return points;
    }

    private static IPoint ToPoint(double first, double second, bool swapAxes)
        => swapAxes ? new Point(second, first) : new Point(first, second);

    private static double ParseNumber(string value)
        => Double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
            ? number
            : throw new FormatException($"Invalid GML coordinate {value}");

    private static XmlElement? Child(XmlElement element, string localName)
        => element.ChildNodes.OfType<XmlElement>().FirstOrDefault(e => e.LocalName == localName);

    /// <summary>
    /// The geometries of member (one geometry) and members (several geometries) properties
    /// </summary>
    private static IEnumerable<XmlElement> Members(XmlElement element, params string[] memberNames)
        => element.ChildNodes.OfType<XmlElement>()
                  .Where(e => memberNames.Contains(e.LocalName))
                  .SelectMany(e => e.ChildNodes.OfType<XmlElement>());

    private static IEnumerable<XmlElement> Descendants(XmlElement element, string localName)
        => element.GetElementsByTagName("*").OfType<XmlElement>().Where(e => e.LocalName == localName);

    #endregion
}
