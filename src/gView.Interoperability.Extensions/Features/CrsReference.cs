#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Globalization;

namespace gView.Interoperability.Extensions.Features;

/// <summary>
/// How a CRS was written in a request. OGC API always uses the axis order of the authority,
/// WFS uses x/y for the short "EPSG:xxxx" form (see <see cref="CrsReference.Notation"/>).
/// </summary>
public enum CrsNotation
{
    /// <summary>http://www.opengis.net/def/crs/EPSG/0/xxxx or .../OGC/1.3/CRS84</summary>
    Uri,
    /// <summary>EPSG:xxxx (or CRS84)</summary>
    EpsgShort,
    /// <summary>urn:ogc:def:crs:EPSG::xxxx (also urn:x-ogc:def:crs:EPSG:xxxx)</summary>
    Urn,
    /// <summary>http://www.opengis.net/gml/srs/epsg.xml#xxxx (GML 2 style, always x/y)</summary>
    GmlSrsUrl
}

/// <summary>
/// A CRS as used by OGC API (Part 2: CRS by reference) and WFS: either OGC CRS84 (lon/lat)
/// or an EPSG code. OGC API writes coordinates in the axis order of the authority,
/// e.g. lat/lon for EPSG:4326 but lon/lat for CRS84.
/// </summary>
public sealed class CrsReference : IEquatable<CrsReference>
{
    public const string Crs84Uri = "http://www.opengis.net/def/crs/OGC/1.3/CRS84";
    private const string EpsgUriPrefix = "http://www.opengis.net/def/crs/EPSG/0/";
    private const string GmlSrsUrlPrefix = "http://www.opengis.net/gml/srs/epsg.xml#";

    private CrsReference(int epsgCode, bool isCrs84, CrsNotation notation = CrsNotation.Uri)
    {
        EpsgCode = epsgCode;
        IsCrs84 = isCrs84;
        Notation = notation;
    }

    public static CrsReference Crs84 { get; } = new CrsReference(4326, true);

    public static CrsReference FromEpsg(int epsgCode, CrsNotation notation = CrsNotation.Uri)
        => new CrsReference(epsgCode, false, notation);

    public int EpsgCode { get; }

    public bool IsCrs84 { get; }

    /// <summary>
    /// The notation of the request the CRS was parsed from. It does not take part in equality.
    /// </summary>
    public CrsNotation Notation { get; }

    public string Uri => IsCrs84 ? Crs84Uri : $"{EpsgUriPrefix}{EpsgCode}";

    public ISpatialReference ToSpatialReference()
        => SpatialReference.FromID($"epsg:{EpsgCode}");

    /// <summary>
    /// The same CRS in another notation (e.g. to echo the form of a request)
    /// </summary>
    public CrsReference WithNotation(CrsNotation notation)
        => notation == Notation ? this : new CrsReference(EpsgCode, IsCrs84, notation);

    /// <summary>
    /// The CRS written in <see cref="Notation"/>
    /// </summary>
    public string Name => Notation switch
    {
        CrsNotation.EpsgShort => IsCrs84 ? "CRS84" : $"EPSG:{EpsgCode}",
        CrsNotation.Urn => IsCrs84 ? "urn:ogc:def:crs:OGC:1.3:CRS84" : $"urn:ogc:def:crs:EPSG::{EpsgCode}",
        CrsNotation.GmlSrsUrl => IsCrs84 ? Crs84Uri : $"{GmlSrsUrlPrefix}{EpsgCode}",
        _ => Uri
    };

    /// <summary>
    /// True, if the first axis of the CRS is northing/southing (e.g. EPSG:4326, EPSG:31256)
    /// and coordinates have to be swapped compared to gView's internal x/y order.
    /// </summary>
    public bool HasNorthingFirst(ISpatialReference? spatialReference)
        => !IsCrs84
           && spatialReference is not null
           && (spatialReference.Gml3AxisX == AxisDirection.North || spatialReference.Gml3AxisX == AxisDirection.South);

    public static bool TryParse(string? value, out CrsReference crs)
    {
        crs = Crs84;

        if (String.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim();
        if (value.StartsWith("[") && value.EndsWith("]"))  // safe CURIE, eg. [EPSG:4326]
        {
            value = value.Substring(1, value.Length - 2);
        }

        string normalized = value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? "http://" + value.Substring("https://".Length)
            : value;

        if (normalized.Equals(Crs84Uri, StringComparison.OrdinalIgnoreCase))
        {
            crs = Crs84;
            return true;
        }
        if (normalized.Equals("OGC:CRS84", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("CRS84", StringComparison.OrdinalIgnoreCase))
        {
            crs = Crs84.WithNotation(CrsNotation.EpsgShort);
            return true;
        }
        if (normalized.StartsWith("urn:ogc:def:crs:OGC:", StringComparison.OrdinalIgnoreCase)
            && normalized.EndsWith(":CRS84", StringComparison.OrdinalIgnoreCase))
        {
            crs = Crs84.WithNotation(CrsNotation.Urn);
            return true;
        }

        string? code = null;
        var notation = CrsNotation.Uri;

        if (normalized.StartsWith(EpsgUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            code = normalized.Substring(EpsgUriPrefix.Length);
        }
        else if (normalized.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase))
        {
            code = normalized.Substring("EPSG:".Length);
            notation = CrsNotation.EpsgShort;
        }
        else if (normalized.StartsWith(GmlSrsUrlPrefix, StringComparison.OrdinalIgnoreCase))
        {
            code = normalized.Substring(GmlSrsUrlPrefix.Length);
            notation = CrsNotation.GmlSrsUrl;
        }
        else if (normalized.StartsWith("urn:ogc:def:crs:EPSG:", StringComparison.OrdinalIgnoreCase)
                 || normalized.StartsWith("urn:x-ogc:def:crs:EPSG:", StringComparison.OrdinalIgnoreCase))
        {
            // urn:ogc:def:crs:EPSG::4326, urn:ogc:def:crs:EPSG:6.6:4326, urn:x-ogc:def:crs:EPSG:4326
            code = normalized.Substring(normalized.LastIndexOf(':') + 1);
            notation = CrsNotation.Urn;
        }

        if (code is not null
            && int.TryParse(code, NumberStyles.None, CultureInfo.InvariantCulture, out int epsgCode)
            && epsgCode > 0)
        {
            crs = FromEpsg(epsgCode, notation);
            return true;
        }

        return false;
    }

    public bool Equals(CrsReference? other)
        => other is not null && other.EpsgCode == EpsgCode && other.IsCrs84 == IsCrs84;

    public override bool Equals(object? obj) => Equals(obj as CrsReference);

    public override int GetHashCode() => HashCode.Combine(EpsgCode, IsCrs84);

    public override string ToString() => Uri;
}
