#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.OGC.GML;
using gView.Interoperability.Extensions.Features;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// Writes the XML schema of feature types (DescribeFeatureType) for GML 2.1.2, 3.1.1 or 3.2.
/// The element names are the same as in the GetFeature response (see <see cref="WfsFeatureWriter"/>).
/// </summary>
public static class WfsSchemaWriter
{
    public static byte[] Write(IEnumerable<OgcCollectionLayer> collections, GmlFormat format)
    {
        string xsd = WfsVersions.XsdNamespace;
        string gml = GmlGeometryWriter.NamespaceOf(format);

        using var ms = new MemoryStream();
        using (var w = XmlWriter.Create(ms, WfsXml.WriterSettings))
        {
            w.WriteStartDocument();
            w.WriteStartElement("xsd", "schema", xsd);
            w.WriteAttributeString("xmlns", "gml", null, gml);
            w.WriteAttributeString("xmlns", WfsVersions.FeatureTypePrefix, null, WfsVersions.FeatureTypeNamespace);
            w.WriteAttributeString("targetNamespace", WfsVersions.FeatureTypeNamespace);
            w.WriteAttributeString("elementFormDefault", "qualified");
            w.WriteAttributeString("version", "1.0");

            w.WriteStartElement("xsd", "import", xsd);
            w.WriteAttributeString("namespace", gml);
            w.WriteAttributeString("schemaLocation", WfsVersions.GmlSchemaLocation(format));
            w.WriteEndElement();

            foreach (var collection in collections)
            {
                var featureClass = collection.FeatureClass;
                if (featureClass is null)
                {
                    continue;
                }

                w.WriteStartElement("xsd", "complexType", xsd);
                w.WriteAttributeString("name", $"{collection.Id}Type");
                w.WriteStartElement("xsd", "complexContent", xsd);
                w.WriteStartElement("xsd", "extension", xsd);
                w.WriteAttributeString("base", "gml:AbstractFeatureType");
                w.WriteStartElement("xsd", "sequence", xsd);

                // the geometry
                w.WriteStartElement("xsd", "element", xsd);
                w.WriteAttributeString("name", WfsFeatureTypes.GeometryElementName(collection));
                w.WriteAttributeString("type", $"gml:{GmlGeometryWriter.PropertyTypeName(featureClass.GeometryType, format)}");
                w.WriteAttributeString("minOccurs", "0");
                w.WriteAttributeString("maxOccurs", "1");
                w.WriteAttributeString("nillable", "true");
                w.WriteEndElement();

                foreach (var field in collection.PropertyFields)
                {
                    w.WriteStartElement("xsd", "element", xsd);
                    w.WriteAttributeString("name", WfsFeatureTypes.ElementName(field.name));
                    w.WriteAttributeString("type", $"xsd:{WfsFeatureTypes.XsdType(field)}");
                    w.WriteAttributeString("minOccurs", "0");
                    w.WriteAttributeString("maxOccurs", "1");
                    w.WriteAttributeString("nillable", "true");
                    w.WriteEndElement();
                }

                w.WriteEndElement();  // sequence
                w.WriteEndElement();  // extension
                w.WriteEndElement();  // complexContent
                w.WriteEndElement();  // complexType

                w.WriteStartElement("xsd", "element", xsd);
                w.WriteAttributeString("name", collection.Id);
                w.WriteAttributeString("type", $"{WfsVersions.FeatureTypePrefix}:{collection.Id}Type");
                w.WriteAttributeString("substitutionGroup", format == GmlFormat.Gml32 ? "gml:AbstractFeature" : "gml:_Feature");
                w.WriteEndElement();
            }

            w.WriteEndElement();  // schema
            w.WriteEndDocument();
        }

        return ms.ToArray();
    }
}
