using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System.Globalization;

namespace gView.Interoperability.OgcApi;

/// <summary>
/// A CRS as used by OGC API (Part 2: CRS by reference): either OGC CRS84 (lon/lat)
/// or an EPSG code. Coordinates are always written in the axis order of the authority,
/// e.g. lat/lon for EPSG:4326 but lon/lat for CRS84.
/// </summary>
public sealed class CrsReference : IEquatable<CrsReference>
{
    public const string Crs84Uri = "http://www.opengis.net/def/crs/OGC/1.3/CRS84";
    private const string EpsgUriPrefix = "http://www.opengis.net/def/crs/EPSG/0/";

    private CrsReference(int epsgCode, bool isCrs84)
    {
        EpsgCode = epsgCode;
        IsCrs84 = isCrs84;
    }

    public static CrsReference Crs84 { get; } = new CrsReference(4326, true);

    public static CrsReference FromEpsg(int epsgCode) => new CrsReference(epsgCode, false);

    public int EpsgCode { get; }

    public bool IsCrs84 { get; }

    public string Uri => IsCrs84 ? Crs84Uri : $"{EpsgUriPrefix}{EpsgCode}";

    public ISpatialReference ToSpatialReference()
        => SpatialReference.FromID($"epsg:{EpsgCode}");

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

        if (normalized.Equals(Crs84Uri, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("OGC:CRS84", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("CRS84", StringComparison.OrdinalIgnoreCase))
        {
            crs = Crs84;
            return true;
        }

        string? code = null;
        if (normalized.StartsWith(EpsgUriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            code = normalized.Substring(EpsgUriPrefix.Length);
        }
        else if (normalized.StartsWith("EPSG:", StringComparison.OrdinalIgnoreCase))
        {
            code = normalized.Substring("EPSG:".Length);
        }

        if (code is not null
            && int.TryParse(code, NumberStyles.None, CultureInfo.InvariantCulture, out int epsgCode)
            && epsgCode > 0)
        {
            crs = FromEpsg(epsgCode);
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
