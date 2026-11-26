#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// The stored queries of WFS 2.0.0: only the mandatory GetFeatureById (no own, no CreateStoredQuery)
/// </summary>
public static class WfsStoredQueries
{
    public const string GetFeatureById = "urn:ogc:def:query:OGC-WFS::GetFeatureById";

    /// <summary>
    /// The id of GetFeatureById, also the short form "GetFeatureById"
    /// </summary>
    public static bool IsGetFeatureById(string? storedQueryId)
        => storedQueryId is not null
           && (storedQueryId.Equals(GetFeatureById, StringComparison.OrdinalIgnoreCase)
               || storedQueryId.Equals("GetFeatureById", StringComparison.OrdinalIgnoreCase));

    public static byte[] WriteList(IEnumerable<string> featureTypeNames)
    {
        string wfs = WfsVersions.Wfs2Namespace;

        using var ms = new MemoryStream();
        using (var w = XmlWriter.Create(ms, WfsXml.WriterSettings))
        {
            w.WriteStartDocument();
            w.WriteStartElement("wfs", "ListStoredQueriesResponse", wfs);
            w.WriteAttributeString("xmlns", WfsVersions.FeatureTypePrefix, null, WfsVersions.FeatureTypeNamespace);

            w.WriteStartElement("wfs", "StoredQuery", wfs);
            w.WriteAttributeString("id", GetFeatureById);
            w.WriteElementString("wfs", "Title", wfs, "Get feature by identifier");
            foreach (var typeName in featureTypeNames)
            {
                w.WriteElementString("wfs", "ReturnFeatureType", wfs, typeName);
            }
            w.WriteEndElement();

            w.WriteEndElement();
            w.WriteEndDocument();
        }
        return ms.ToArray();
    }

    /// <exception cref="WfsException">An unknown stored query id</exception>
    public static byte[] WriteDescriptions(IEnumerable<string> storedQueryIds)
    {
        var ids = storedQueryIds.ToList();
        foreach (var id in ids.Where(id => !IsGetFeatureById(id)))
        {
            throw WfsException.InvalidParameterValue("storedQuery_Id", $"Unknown stored query {id}");
        }

        string wfs = WfsVersions.Wfs2Namespace;

        using var ms = new MemoryStream();
        using (var w = XmlWriter.Create(ms, WfsXml.WriterSettings))
        {
            w.WriteStartDocument();
            w.WriteStartElement("wfs", "DescribeStoredQueriesResponse", wfs);
            w.WriteAttributeString("xmlns", "xsd", null, WfsVersions.XsdNamespace);

            w.WriteStartElement("wfs", "StoredQueryDescription", wfs);
            w.WriteAttributeString("id", GetFeatureById);
            w.WriteElementString("wfs", "Title", wfs, "Get feature by identifier");
            w.WriteElementString("wfs", "Abstract", wfs, "Returns the single feature with the given resource id (c{layerId}.{objectId}).");
            w.WriteStartElement("wfs", "Parameter", wfs);
            w.WriteAttributeString("name", "ID");
            w.WriteAttributeString("type", "xsd:string");
            w.WriteEndElement();
            w.WriteStartElement("wfs", "QueryExpressionText", wfs);
            w.WriteAttributeString("returnFeatureTypes", "");
            w.WriteAttributeString("language", "urn:ogc:def:queryLanguage:OGC-WFS::WFSQueryExpression");
            w.WriteAttributeString("isPrivate", "true");
            w.WriteEndElement();
            w.WriteEndElement();

            w.WriteEndElement();
            w.WriteEndDocument();
        }
        return ms.ToArray();
    }
}
