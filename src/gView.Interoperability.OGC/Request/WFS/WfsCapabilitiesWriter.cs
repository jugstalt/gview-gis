#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.OGC.GML;
using gView.Interoperability.Extensions.Features;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// Writes the GetCapabilities response of WFS 1.0.0, 1.1.0 and 2.0.0
/// </summary>
public class WfsCapabilitiesWriter
{
    private readonly IServiceMap _map;
    private readonly WfsFeatureTypes _featureTypes;
    private readonly string _onlineResource;
    private readonly WfsVersion _version;

    /// <param name="onlineResource">The URL of the service with ?SERVICE=WFS&amp;</param>
    public WfsCapabilitiesWriter(IServiceMap map, WfsFeatureTypes featureTypes, string onlineResource, WfsVersion version)
    {
        _map = map;
        _featureTypes = featureTypes;
        _onlineResource = onlineResource;
        _version = version;
    }

    public byte[] Write()
    {
        using var ms = new MemoryStream();
        using (var writer = XmlWriter.Create(ms, WfsXml.WriterSettings))
        {
            writer.WriteStartDocument();
            switch (_version)
            {
                case WfsVersion.V100:
                    Write100(writer);
                    break;
                case WfsVersion.V110:
                    Write110(writer);
                    break;
                default:
                    Write200(writer);
                    break;
            }
            writer.WriteEndDocument();
        }
        return ms.ToArray();
    }

    private string Title
        => !String.IsNullOrWhiteSpace(_map.Title)
            ? _map.Title
            : _map.Name.Contains('/') ? _map.Name.Substring(_map.Name.LastIndexOf('/') + 1) : _map.Name;

    private string Abstract => $"gView GIS map service {_map.Name}";

    #region 1.0.0

    private void Write100(XmlWriter w)
    {
        string wfs = WfsVersions.Wfs1Namespace, ogc = WfsVersions.OgcNamespace;

        w.WriteStartElement("WFS_Capabilities", wfs);
        w.WriteAttributeString("version", "1.0.0");
        w.WriteAttributeString("xmlns", "ogc", null, ogc);
        w.WriteAttributeString("xmlns", WfsVersions.FeatureTypePrefix, null, WfsVersions.FeatureTypeNamespace);
        w.WriteAttributeString("xmlns", "xsi", null, WfsVersions.XsiNamespace);
        w.WriteAttributeString("xsi", "schemaLocation", WfsVersions.XsiNamespace,
            $"{wfs} http://schemas.opengis.net/wfs/1.0.0/WFS-capabilities.xsd");

        w.WriteStartElement("Service", wfs);
        w.WriteElementString("Name", wfs, "WFS");
        w.WriteElementString("Title", wfs, Title);
        w.WriteElementString("Abstract", wfs, Abstract);
        w.WriteElementString("OnlineResource", wfs, _onlineResource);
        w.WriteEndElement();

        w.WriteStartElement("Capability", wfs);
        w.WriteStartElement("Request", wfs);
        foreach (var operation in new[] { "GetCapabilities", "DescribeFeatureType", "GetFeature" })
        {
            w.WriteStartElement(operation, wfs);
            if (operation == "DescribeFeatureType")
            {
                w.WriteStartElement("SchemaDescriptionLanguage", wfs);
                w.WriteElementString("XMLSCHEMA", wfs, "");
                w.WriteEndElement();
            }
            else if (operation == "GetFeature")
            {
                w.WriteStartElement("ResultFormat", wfs);
                w.WriteElementString("GML2", wfs, "");
                w.WriteEndElement();
            }
            foreach (var method in new[] { "Get", "Post" })
            {
                w.WriteStartElement("DCPType", wfs);
                w.WriteStartElement("HTTP", wfs);
                w.WriteStartElement(method, wfs);
                w.WriteAttributeString("onlineResource", _onlineResource);
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }
        w.WriteEndElement();  // Request
        w.WriteEndElement();  // Capability

        w.WriteStartElement("FeatureTypeList", wfs);
        w.WriteStartElement("Operations", wfs);
        w.WriteElementString("Query", wfs, "");
        w.WriteEndElement();

        foreach (var collection in _featureTypes.All)
        {
            w.WriteStartElement("FeatureType", wfs);
            w.WriteElementString("Name", wfs, WfsFeatureTypes.QualifiedName(collection));
            w.WriteElementString("Title", wfs, collection.Title);
            if (!String.IsNullOrWhiteSpace(collection.Description))
            {
                w.WriteElementString("Abstract", wfs, collection.Description);
            }
            var crs = WfsFeatureTypes.DefaultCrs(_map, collection, _version);
            w.WriteElementString("SRS", wfs, crs?.Name ?? "EPSG:4326");  // mandatory in 1.0.0
            var extent = Wgs84ExtentOrWorld(collection);
            {
                w.WriteStartElement("LatLongBoundingBox", wfs);
                w.WriteAttributeString("minx", Number(extent.MinX));
                w.WriteAttributeString("miny", Number(extent.MinY));
                w.WriteAttributeString("maxx", Number(extent.MaxX));
                w.WriteAttributeString("maxy", Number(extent.MaxY));
                w.WriteEndElement();
            }
            w.WriteEndElement();
        }
        w.WriteEndElement();  // FeatureTypeList

        w.WriteStartElement("ogc", "Filter_Capabilities", ogc);
        w.WriteStartElement("ogc", "Spatial_Capabilities", ogc);
        w.WriteStartElement("ogc", "Spatial_Operators", ogc);
        w.WriteElementString("ogc", "BBOX", ogc, "");
        w.WriteElementString("ogc", "Intersect", ogc, "");
        w.WriteElementString("ogc", "Within", ogc, "");
        w.WriteElementString("ogc", "Contains", ogc, "");
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteStartElement("ogc", "Scalar_Capabilities", ogc);
        w.WriteElementString("ogc", "Logical_Operators", ogc, "");
        w.WriteStartElement("ogc", "Comparison_Operators", ogc);
        w.WriteElementString("ogc", "Simple_Comparisons", ogc, "");
        w.WriteElementString("ogc", "Like", ogc, "");
        w.WriteElementString("ogc", "Between", ogc, "");
        w.WriteElementString("ogc", "NullCheck", ogc, "");
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteEndElement();  // Filter_Capabilities

        w.WriteEndElement();  // WFS_Capabilities
    }

    #endregion

    #region 1.1.0

    private void Write110(XmlWriter w)
    {
        string wfs = WfsVersions.Wfs1Namespace, ows = WfsVersions.Ows1Namespace, ogc = WfsVersions.OgcNamespace;

        w.WriteStartElement("wfs", "WFS_Capabilities", wfs);
        w.WriteAttributeString("version", "1.1.0");
        w.WriteAttributeString("xmlns", "ows", null, ows);
        w.WriteAttributeString("xmlns", "ogc", null, ogc);
        w.WriteAttributeString("xmlns", "gml", null, GmlGeometryWriter.Gml2Namespace);
        w.WriteAttributeString("xmlns", "xlink", null, WfsVersions.XLinkNamespace);
        w.WriteAttributeString("xmlns", WfsVersions.FeatureTypePrefix, null, WfsVersions.FeatureTypeNamespace);
        w.WriteAttributeString("xmlns", "xsi", null, WfsVersions.XsiNamespace);
        w.WriteAttributeString("xsi", "schemaLocation", WfsVersions.XsiNamespace,
            $"{wfs} {WfsVersions.WfsSchemaLocation(WfsVersion.V110)}");

        WriteServiceIdentification(w, ows, null);
        WriteOperationsMetadata(w, ows, constraints: false);

        w.WriteStartElement("wfs", "FeatureTypeList", wfs);
        w.WriteStartElement("wfs", "Operations", wfs);
        w.WriteElementString("wfs", "Operation", wfs, "Query");
        w.WriteEndElement();
        WriteFeatureTypes(w, wfs, ows, "DefaultSRS", "OtherSRS", "NoSRS");
        w.WriteEndElement();

        w.WriteStartElement("ogc", "Filter_Capabilities", ogc);

        w.WriteStartElement("ogc", "Spatial_Capabilities", ogc);
        w.WriteStartElement("ogc", "GeometryOperands", ogc);
        foreach (var operand in new[] { "gml:Envelope", "gml:Point", "gml:LineString", "gml:Polygon" })
        {
            w.WriteElementString("ogc", "GeometryOperand", ogc, operand);
        }
        w.WriteEndElement();
        w.WriteStartElement("ogc", "SpatialOperators", ogc);
        foreach (var op in SpatialOperators)
        {
            w.WriteStartElement("ogc", "SpatialOperator", ogc);
            w.WriteAttributeString("name", op);
            w.WriteEndElement();
        }
        w.WriteEndElement();
        w.WriteEndElement();  // Spatial_Capabilities

        w.WriteStartElement("ogc", "Scalar_Capabilities", ogc);
        w.WriteElementString("ogc", "LogicalOperators", ogc, "");
        w.WriteStartElement("ogc", "ComparisonOperators", ogc);
        foreach (var op in new[] { "LessThan", "GreaterThan", "LessThanEqualTo", "GreaterThanEqualTo", "EqualTo", "NotEqualTo", "Like", "Between", "NullCheck" })
        {
            w.WriteElementString("ogc", "ComparisonOperator", ogc, op);
        }
        w.WriteEndElement();
        w.WriteEndElement();  // Scalar_Capabilities

        w.WriteStartElement("ogc", "Id_Capabilities", ogc);
        w.WriteElementString("ogc", "EID", ogc, "");
        w.WriteElementString("ogc", "FID", ogc, "");
        w.WriteEndElement();

        w.WriteEndElement();  // Filter_Capabilities

        w.WriteEndElement();  // WFS_Capabilities
    }

    #endregion

    #region 2.0.0

    private void Write200(XmlWriter w)
    {
        string wfs = WfsVersions.Wfs2Namespace, ows = WfsVersions.Ows11Namespace, fes = WfsVersions.FesNamespace;

        w.WriteStartElement("wfs", "WFS_Capabilities", wfs);
        w.WriteAttributeString("version", "2.0.0");
        w.WriteAttributeString("xmlns", "ows", null, ows);
        w.WriteAttributeString("xmlns", "fes", null, fes);
        w.WriteAttributeString("xmlns", "gml", null, GmlGeometryWriter.Gml32Namespace);
        w.WriteAttributeString("xmlns", "xlink", null, WfsVersions.XLinkNamespace);
        w.WriteAttributeString("xmlns", WfsVersions.FeatureTypePrefix, null, WfsVersions.FeatureTypeNamespace);
        w.WriteAttributeString("xmlns", "xsi", null, WfsVersions.XsiNamespace);
        w.WriteAttributeString("xsi", "schemaLocation", WfsVersions.XsiNamespace,
            $"{wfs} {WfsVersions.WfsSchemaLocation(WfsVersion.V200)}");

        WriteServiceIdentification(w, ows, "OGC");
        WriteOperationsMetadata(w, ows, constraints: true);

        w.WriteStartElement("wfs", "FeatureTypeList", wfs);
        WriteFeatureTypes(w, wfs, ows, "DefaultCRS", "OtherCRS", "NoCRS");
        w.WriteEndElement();

        w.WriteStartElement("fes", "Filter_Capabilities", fes);

        w.WriteStartElement("fes", "Conformance", fes);
        foreach (var (name, value) in new[]
        {
            ("ImplementsQuery", true),
            ("ImplementsAdHocQuery", true),
            ("ImplementsFunctions", false),
            ("ImplementsResourceId", true),
            ("ImplementsMinStandardFilter", true),
            ("ImplementsStandardFilter", true),
            ("ImplementsMinSpatialFilter", true),
            ("ImplementsSpatialFilter", false),
            ("ImplementsMinTemporalFilter", false),
            ("ImplementsTemporalFilter", false),
            ("ImplementsVersionNav", false),
            ("ImplementsSorting", true),
            ("ImplementsExtendedOperators", false),
            ("ImplementsMinimumXPath", false),
            ("ImplementsSchemaElementFunc", false)
        })
        {
            WriteConstraint(w, "fes", fes, ows, name, value ? "TRUE" : "FALSE");
        }
        w.WriteEndElement();  // Conformance

        w.WriteStartElement("fes", "Id_Capabilities", fes);
        w.WriteStartElement("fes", "ResourceIdentifier", fes);
        w.WriteAttributeString("name", "fes:ResourceId");
        w.WriteEndElement();
        w.WriteEndElement();

        w.WriteStartElement("fes", "Scalar_Capabilities", fes);
        w.WriteElementString("fes", "LogicalOperators", fes, "");
        w.WriteStartElement("fes", "ComparisonOperators", fes);
        foreach (var op in new[] { "PropertyIsEqualTo", "PropertyIsNotEqualTo", "PropertyIsLessThan", "PropertyIsGreaterThan", "PropertyIsLessThanOrEqualTo", "PropertyIsGreaterThanOrEqualTo", "PropertyIsLike", "PropertyIsNull", "PropertyIsNil", "PropertyIsBetween" })
        {
            w.WriteStartElement("fes", "ComparisonOperator", fes);
            w.WriteAttributeString("name", op);
            w.WriteEndElement();
        }
        w.WriteEndElement();
        w.WriteEndElement();  // Scalar_Capabilities

        w.WriteStartElement("fes", "Spatial_Capabilities", fes);
        w.WriteStartElement("fes", "GeometryOperands", fes);
        foreach (var operand in new[] { "gml:Envelope", "gml:Point", "gml:LineString", "gml:Polygon" })
        {
            w.WriteStartElement("fes", "GeometryOperand", fes);
            w.WriteAttributeString("name", operand);
            w.WriteEndElement();
        }
        w.WriteEndElement();
        w.WriteStartElement("fes", "SpatialOperators", fes);
        foreach (var op in SpatialOperators)
        {
            w.WriteStartElement("fes", "SpatialOperator", fes);
            w.WriteAttributeString("name", op);
            w.WriteEndElement();
        }
        w.WriteEndElement();
        w.WriteEndElement();  // Spatial_Capabilities

        w.WriteEndElement();  // Filter_Capabilities

        w.WriteEndElement();  // WFS_Capabilities
    }

    #endregion

    #region OWS (1.1.0, 2.0.0)

    private void WriteServiceIdentification(XmlWriter w, string ows, string? codeSpace)
    {
        w.WriteStartElement("ows", "ServiceIdentification", ows);
        w.WriteElementString("ows", "Title", ows, Title);
        w.WriteElementString("ows", "Abstract", ows, Abstract);
        w.WriteStartElement("ows", "ServiceType", ows);
        if (codeSpace is not null)
        {
            w.WriteAttributeString("codeSpace", codeSpace);
        }
        w.WriteString("WFS");
        w.WriteEndElement();
        foreach (var version in WfsVersions.Supported.Where(v => _version == WfsVersion.V200 || v != WfsVersion.V200))
        {
            w.WriteElementString("ows", "ServiceTypeVersion", ows, version.VersionString);
        }
        w.WriteElementString("ows", "Fees", ows, "NONE");
        w.WriteElementString("ows", "AccessConstraints", ows, "NONE");
        w.WriteEndElement();
    }

    private void WriteOperationsMetadata(XmlWriter w, string ows, bool constraints)
    {
        w.WriteStartElement("ows", "OperationsMetadata", ows);

        foreach (var operation in OwsOperations)
        {
            w.WriteStartElement("ows", "Operation", ows);
            w.WriteAttributeString("name", operation);

            w.WriteStartElement("ows", "DCP", ows);
            w.WriteStartElement("ows", "HTTP", ows);
            foreach (var method in new[] { "Get", "Post" })
            {
                w.WriteStartElement("ows", method, ows);
                w.WriteAttributeString("xlink", "href", WfsVersions.XLinkNamespace, _onlineResource);
                w.WriteEndElement();
            }
            w.WriteEndElement();
            w.WriteEndElement();

            switch (operation)
            {
                case "GetCapabilities":
                    WriteParameter(w, ows, "AcceptVersions", WfsVersions.Supported
                        .Where(v => _version == WfsVersion.V200 || v != WfsVersion.V200)
                        .Select(v => v.VersionString).ToArray());
                    break;
                case "DescribeFeatureType":
                    WriteParameter(w, ows, "outputFormat", _version.GmlFormats.Select(f => DescribeFeatureTypeFormat(_version, f)).ToArray());
                    break;
                case "GetFeature":
                    WriteParameter(w, ows, "resultType", "results", "hits");
                    WriteParameter(w, ows, "outputFormat", OutputFormats.ToArray());
                    break;
            }

            w.WriteEndElement();  // Operation
        }

        if (constraints)
        {
            foreach (var (name, value) in new[]
            {
                ("ImplementsBasicWFS", false),
                ("ImplementsTransactionalWFS", false),
                ("ImplementsLockingWFS", false),
                ("KVPEncoding", true),
                ("XMLEncoding", true),
                ("SOAPEncoding", false),
                ("ImplementsInheritance", false),
                ("ImplementsRemoteResolve", false),
                ("ImplementsResultPaging", true),
                ("ImplementsStandardJoins", false),
                ("ImplementsSpatialJoins", false),
                ("ImplementsTemporalJoins", false),
                ("ImplementsFeatureVersioning", false),
                ("ManageStoredQueries", false)
            })
            {
                WriteConstraint(w, "ows", ows, ows, name, value ? "TRUE" : "FALSE");
            }

            int maxRecordCount = _map.MapServiceProperties?.MaxRecordCount ?? 0;
            if (maxRecordCount > 0)
            {
                WriteConstraint(w, "ows", ows, ows, "CountDefault", maxRecordCount.ToString(CultureInfo.InvariantCulture));
            }
        }

        w.WriteEndElement();  // OperationsMetadata
    }

    private static void WriteParameter(XmlWriter w, string ows, string name, params string[] values)
    {
        w.WriteStartElement("ows", "Parameter", ows);
        w.WriteAttributeString("name", name);
        if (ows == WfsVersions.Ows11Namespace)
        {
            w.WriteStartElement("ows", "AllowedValues", ows);
        }
        foreach (var value in values)
        {
            w.WriteElementString("ows", "Value", ows, value);
        }
        if (ows == WfsVersions.Ows11Namespace)
        {
            w.WriteEndElement();
        }
        w.WriteEndElement();
    }

    private static void WriteConstraint(XmlWriter w, string prefix, string ns, string ows, string name, string defaultValue)
    {
        w.WriteStartElement(prefix, "Constraint", ns);
        w.WriteAttributeString("name", name);
        w.WriteElementString("ows", "NoValues", ows, "");
        w.WriteElementString("ows", "DefaultValue", ows, defaultValue);
        w.WriteEndElement();
    }

    private void WriteFeatureTypes(XmlWriter w, string wfs, string ows, string defaultCrsElement, string otherCrsElement, string noCrsElement)
    {
        foreach (var collection in _featureTypes.All)
        {
            w.WriteStartElement("wfs", "FeatureType", wfs);
            w.WriteElementString("wfs", "Name", wfs, WfsFeatureTypes.QualifiedName(collection));
            w.WriteElementString("wfs", "Title", wfs, collection.Title);
            if (!String.IsNullOrWhiteSpace(collection.Description))
            {
                w.WriteElementString("wfs", "Abstract", wfs, collection.Description);
            }

            var defaultCrs = WfsFeatureTypes.DefaultCrs(_map, collection, _version);
            if (defaultCrs is not null)
            {
                w.WriteElementString("wfs", defaultCrsElement, wfs, defaultCrs.Name);
                foreach (var otherCrs in WfsFeatureTypes.OtherCrs(_map, defaultCrs, _version))
                {
                    w.WriteElementString("wfs", otherCrsElement, wfs, otherCrs.Name);
                }
            }
            else
            {
                w.WriteElementString("wfs", noCrsElement, wfs, "");
            }

            w.WriteStartElement("wfs", "OutputFormats", wfs);
            foreach (var format in OutputFormats)
            {
                w.WriteElementString("wfs", "Format", wfs, format);
            }
            w.WriteEndElement();

            var extent = Wgs84ExtentOrWorld(collection);  // mandatory in 1.1.0
            {
                w.WriteStartElement("ows", "WGS84BoundingBox", ows);
                w.WriteElementString("ows", "LowerCorner", ows, $"{Number(extent.MinX)} {Number(extent.MinY)}");
                w.WriteElementString("ows", "UpperCorner", ows, $"{Number(extent.MaxX)} {Number(extent.MaxY)}");
                w.WriteEndElement();
            }

            w.WriteEndElement();  // FeatureType
        }
    }

    #endregion

    /// <summary>
    /// Only one spatial operator per query, at the top level or in a top-level And (see <see cref="FesFilterParser"/>)
    /// </summary>
    private static readonly string[] SpatialOperators = ["BBOX", "Intersects", "Within", "Contains"];

    /// <summary>
    /// The GetFeature output formats: the GML versions and GeoJSON
    /// </summary>
    private System.Collections.Generic.IEnumerable<string> OutputFormats
        => _version.GmlFormats.Select(f => _version.OutputFormatName(f)).Append(WfsVersions.GeoJsonOutputFormat);

    /// <summary>
    /// The operations in the OWS operations metadata (1.1.0, 2.0.0)
    /// </summary>
    private string[] OwsOperations
        => _version == WfsVersion.V200
            ? ["GetCapabilities", "DescribeFeatureType", "GetFeature", "ListStoredQueries", "DescribeStoredQueries"]
            : ["GetCapabilities", "DescribeFeatureType", "GetFeature"];

    internal static string DescribeFeatureTypeFormat(WfsVersion version, GmlFormat format)
        => version == WfsVersion.V100 ? "XMLSCHEMA" : version.OutputFormatName(format);

    private Framework.Core.Geometry.IEnvelope Wgs84ExtentOrWorld(OgcCollectionLayer collection)
        => WfsFeatureTypes.Wgs84Extent(_map, collection)
           ?? new Framework.Geometry.Envelope(-180, -90, 180, 90);

    private static string Number(double value) => GmlGeometryWriter.Number(value);
}
