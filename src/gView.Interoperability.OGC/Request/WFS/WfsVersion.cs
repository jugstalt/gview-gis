#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.OGC.GML;
using gView.Interoperability.Extensions.Features;
using System;

namespace gView.Interoperability.OGC.Request.WFS;

public enum WfsVersion
{
    V100,
    V110,
    V200
}

/// <summary>
/// What differs between the WFS versions: namespaces, GML version, output formats and the axis order rule
/// </summary>
public static class WfsVersions
{
    public const string Wfs1Namespace = "http://www.opengis.net/wfs";
    public const string Wfs2Namespace = "http://www.opengis.net/wfs/2.0";
    public const string OgcNamespace = "http://www.opengis.net/ogc";
    public const string FesNamespace = "http://www.opengis.net/fes/2.0";
    public const string Ows1Namespace = "http://www.opengis.net/ows";
    public const string Ows11Namespace = "http://www.opengis.net/ows/1.1";
    public const string XLinkNamespace = "http://www.w3.org/1999/xlink";
    public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
    public const string XsdNamespace = "http://www.w3.org/2001/XMLSchema";

    /// <summary>
    /// The namespace of the feature types (prefix <see cref="FeatureTypePrefix"/>)
    /// </summary>
    public const string FeatureTypeNamespace = "http://www.gViewGIS.com/server";
    public const string FeatureTypePrefix = "gv";

    public const string Gml2OutputFormat = "text/xml; subtype=gml/2.1.2";
    public const string Gml31OutputFormat = "text/xml; subtype=gml/3.1.1";
    public const string Gml32OutputFormat = "application/gml+xml; version=3.2";

    /// <summary>
    /// The newest version first (version negotiation)
    /// </summary>
    public static readonly WfsVersion[] Supported = [WfsVersion.V200, WfsVersion.V110, WfsVersion.V100];

    public static WfsVersion? Parse(string? version)
        => version?.Trim() switch
        {
            "1.0.0" or "1.0" => WfsVersion.V100,
            "1.1.0" or "1.1" => WfsVersion.V110,
            "2.0.0" or "2.0.2" or "2.0" => WfsVersion.V200,
            _ => null
        };

    extension(WfsVersion version)
    {
        public string VersionString => version switch
        {
            WfsVersion.V100 => "1.0.0",
            WfsVersion.V110 => "1.1.0",
            _ => "2.0.0"
        };

        public string WfsNamespace => version == WfsVersion.V200 ? Wfs2Namespace : Wfs1Namespace;

        public GmlFormat DefaultGmlFormat => version switch
        {
            WfsVersion.V100 => GmlFormat.Gml2,
            WfsVersion.V110 => GmlFormat.Gml31,
            _ => GmlFormat.Gml32
        };

        /// <summary>
        /// The outputFormat value of a GML format as advertised in the capabilities of the version
        /// </summary>
        public string OutputFormatName(GmlFormat format)
            => (version, format) switch
            {
                (WfsVersion.V100, GmlFormat.Gml2) => "GML2",
                (_, GmlFormat.Gml2) => Gml2OutputFormat,
                (_, GmlFormat.Gml31) => Gml31OutputFormat,
                _ => Gml32OutputFormat
            };

        /// <summary>
        /// The GML formats a GetFeature of this version can return (default first)
        /// </summary>
        public GmlFormat[] GmlFormats => version switch
        {
            WfsVersion.V100 => [GmlFormat.Gml2],
            WfsVersion.V110 => [GmlFormat.Gml31, GmlFormat.Gml2],
            _ => [GmlFormat.Gml32, GmlFormat.Gml31, GmlFormat.Gml2]
        };

        /// <summary>
        /// The default notation of srsName/DefaultSRS: EPSG:xxxx for 1.0.0, URN for 1.1.0 and 2.0.0
        /// </summary>
        public CrsNotation DefaultCrsNotation
            => version == WfsVersion.V100 ? CrsNotation.EpsgShort : CrsNotation.Urn;

        /// <summary>
        /// The axis order rule: WFS 1.0.0 is always x/y. WFS 1.1.0 and 2.0.0 use the axis order of the
        /// authority for URN and http URI CRS names (e.g. lat/lon for urn:ogc:def:crs:EPSG::4326),
        /// but x/y for the short EPSG:xxxx form (GeoServer convention, expected by QGIS and ArcGIS).
        /// </summary>
        public bool SwapAxes(CrsReference? crs, ISpatialReference? spatialReference)
            => version != WfsVersion.V100
               && crs is not null
               && crs.Notation is CrsNotation.Urn or CrsNotation.Uri
               && crs.HasNorthingFirst(spatialReference);
    }

    /// <summary>
    /// Parses an outputFormat value. Null for formats that are not supported.
    /// </summary>
    public static GmlFormat? ParseGmlOutputFormat(string? outputFormat, WfsVersion version)
    {
        if (String.IsNullOrWhiteSpace(outputFormat))
        {
            return version.DefaultGmlFormat;
        }

        string value = outputFormat.Replace(" ", "").Replace("\"", "").ToLowerInvariant();

        GmlFormat? format = value switch
        {
            "gml2" or "text/xml;subtype=gml/2.1.2" or "text/xml;subtype=gml/2" => GmlFormat.Gml2,
            "gml3" or "gml31" or "text/xml;subtype=gml/3.1.1" or "text/xml;subtype=gml/3.1" => GmlFormat.Gml31,
            "gml32" or "application/gml+xml;version=3.2" or "text/xml;subtype=gml/3.2" or "text/xml;subtype=gml/3.2.1" => GmlFormat.Gml32,
            "text/xml" or "application/xml" or "xml" or "gml" => version.DefaultGmlFormat,
            _ => null
        };

        return format.HasValue && Array.IndexOf(version.GmlFormats, format.Value) >= 0
            ? format
            : null;
    }

    /// <summary>
    /// The HTTP content type of a GML response. The subtype value is quoted ("gml/3.1.1"), because
    /// a / is not allowed in an unquoted media type parameter value (RFC 9110)
    /// </summary>
    public static string ContentType(GmlFormat format)
        => format switch
        {
            GmlFormat.Gml2 => "text/xml; subtype=\"gml/2.1.2\"",
            GmlFormat.Gml31 => "text/xml; subtype=\"gml/3.1.1\"",
            _ => Gml32OutputFormat
        };

    public static string GmlSchemaLocation(GmlFormat format)
        => format switch
        {
            GmlFormat.Gml2 => "http://schemas.opengis.net/gml/2.1.2/feature.xsd",
            GmlFormat.Gml31 => "http://schemas.opengis.net/gml/3.1.1/base/gml.xsd",
            _ => "http://schemas.opengis.net/gml/3.2.1/gml.xsd"
        };

    public static string WfsSchemaLocation(WfsVersion version)
        => version switch
        {
            WfsVersion.V100 => "http://schemas.opengis.net/wfs/1.0.0/WFS-basic.xsd",
            WfsVersion.V110 => "http://schemas.opengis.net/wfs/1.1.0/wfs.xsd",
            _ => "http://schemas.opengis.net/wfs/2.0/wfs.xsd"
        };
}
