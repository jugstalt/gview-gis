#nullable enable

using gView.Framework.Core.Extensions;
using gView.Framework.OGC.WFS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// A WFS request (GetCapabilities, DescribeFeatureType, GetFeature) parsed from KVP (GET) or XML (POST).
/// Parameter names are case insensitive, values are URL decoded.
/// </summary>
public class WfsRequestParameters
{
    public string Request { get; private set; } = "";

    /// <summary>
    /// The requested version, null if missing (GetCapabilities negotiates the version)
    /// </summary>
    public string? VersionString { get; private set; }

    public IReadOnlyList<string> AcceptVersions { get; private set; } = [];

    public IReadOnlyList<string> TypeNames { get; private set; } = [];

    public string? SrsName { get; private set; }

    public string? OutputFormat { get; private set; }

    public string? ResultType { get; private set; }

    /// <summary>
    /// COUNT (2.0.0) or MAXFEATURES (1.x)
    /// </summary>
    public int? Count { get; private set; }

    public int StartIndex { get; private set; }

    /// <summary>
    /// minx,miny,maxx,maxy[,crs] in the axis order of the crs
    /// </summary>
    public string? BBox { get; private set; }

    public XmlNode? Filter { get; private set; }

    /// <summary>
    /// SORTBY / fes:SortBy: property names (as in the request) and the sort direction
    /// </summary>
    public IReadOnlyList<(string Property, bool Descending)> SortBy { get; private set; } = [];

    /// <summary>
    /// PROPERTYNAME / wfs:PropertyName: the properties to return (empty: all)
    /// </summary>
    public IReadOnlyList<string> PropertyNames { get; private set; } = [];

    /// <summary>
    /// RESOURCEID (2.0.0) / FEATUREID (1.x): c{layerId}.{objectId}
    /// </summary>
    public IReadOnlyList<string> ResourceIds { get; private set; } = [];

    /// <summary>
    /// STOREDQUERY_ID (2.0.0)
    /// </summary>
    public string? StoredQueryId { get; private set; }

    /// <summary>
    /// The parameters of a stored query (KVP or wfs:Parameter), e.g. ID of GetFeatureById
    /// </summary>
    public IReadOnlyDictionary<string, string> StoredQueryParameters { get; private set; } = new Dictionary<string, string>();

    /// <summary>
    /// The decoded KVP parameters in request order (to build next/previous links), empty for XML requests
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> KvpParameters { get; private set; } = [];

    public bool IsXmlRequest { get; private set; }

    #region Parse

    public static WfsRequestParameters Parse(string request)
    {
        request = request?.Trim() ?? "";

        return request.StartsWith("<")
            ? ParseXml(request)
            : ParseKvp(request);
    }

    /// <summary>
    /// Parses the (raw, URL encoded) query string of a GET request
    /// </summary>
    public static WfsRequestParameters ParseKvp(string queryString)
    {
        var kvp = ParseQueryString(queryString);
        string? Get(params string[] names)
        {
            foreach (var name in names)
            {
                if (kvp.TryGetValue(name, out var value))
                {
                    return value;
                }
            }
            return null;
        }

        var parameters = new WfsRequestParameters()
        {
            Request = Get("REQUEST") ?? throw WfsException.MissingParameterValue("request"),
            VersionString = Get("VERSION"),
            AcceptVersions = SplitList(Get("ACCEPTVERSIONS")),
            TypeNames = SplitTypeNames(Get("TYPENAMES", "TYPENAME")),
            SrsName = NullIfEmpty(Get("SRSNAME")),
            OutputFormat = NullIfEmpty(Get("OUTPUTFORMAT")),
            ResultType = NullIfEmpty(Get("RESULTTYPE")),
            Count = ParseCount(Get("COUNT", "MAXFEATURES"), "count"),
            StartIndex = ParseCount(Get("STARTINDEX"), "startIndex") ?? 0,
            BBox = NullIfEmpty(Get("BBOX")),
            SortBy = ParseKvpSortBy(Get("SORTBY")),
            PropertyNames = SplitTypeNames(Get("PROPERTYNAME")),
            ResourceIds = SplitList(Get("RESOURCEID", "FEATUREID")),
            StoredQueryId = NullIfEmpty(Get("STOREDQUERY_ID")),
            StoredQueryParameters = kvp,
            KvpParameters = ParseQueryStringOrdered(queryString)
        };

        var filter = NullIfEmpty(Get("FILTER"));
        if (filter is not null && filter.StartsWith("(") && filter.EndsWith(")") && filter.IndexOf(")(", StringComparison.Ordinal) < 0)
        {
            filter = filter.Substring(1, filter.Length - 2);  // (<Filter>...</Filter>) of a single query
        }
        if (filter is not null)
        {
            try
            {
                parameters.Filter = WFSParameterDescriptor.ParseKvpFilter(filter);
            }
            catch (ArgumentException ex)
            {
                throw WfsException.InvalidParameterValue("filter", ex.Message);
            }
        }

        if (new[] { parameters.BBox is not null, parameters.Filter is not null, parameters.ResourceIds.Count > 0 }.Count(b => b) > 1)
        {
            throw WfsException.InvalidParameterValue("bbox", "BBOX, FILTER and RESOURCEID are mutually exclusive");
        }

        return parameters;
    }

    public static WfsRequestParameters ParseXml(string xml)
    {
        var doc = new XmlDocument();
        try
        {
            doc.LoadXmlSafe(xml);
        }
        catch (XmlException ex)
        {
            throw WfsException.OperationParsingFailed("request", $"Invalid XML request: {ex.Message}");
        }

        var root = doc.DocumentElement
            ?? throw WfsException.OperationParsingFailed("request", "Empty XML request");

        var parameters = new WfsRequestParameters()
        {
            IsXmlRequest = true,
            Request = root.LocalName,
            VersionString = Attribute(root, "version")
        };

        switch (root.LocalName)
        {
            case "GetCapabilities":
                parameters.AcceptVersions = Children(root, "AcceptVersions")
                    .SelectMany(a => Children(a, "Version"))
                    .Select(v => v.InnerText.Trim())
                    .ToArray();
                break;

            case "DescribeFeatureType":
                parameters.TypeNames = Children(root, "TypeName")
                    .SelectMany(t => SplitTypeNames(t.InnerText))
                    .ToArray();
                parameters.OutputFormat = Attribute(root, "outputFormat");
                break;

            case "GetFeature":
                parameters.OutputFormat = Attribute(root, "outputFormat");
                parameters.ResultType = Attribute(root, "resultType");
                parameters.Count = ParseCount(Attribute(root, "count") ?? Attribute(root, "maxFeatures") ?? Attribute(root, "maxfeatures"), "count");
                parameters.StartIndex = ParseCount(Attribute(root, "startIndex"), "startIndex") ?? 0;

                var storedQuery = Children(root, "StoredQuery").FirstOrDefault();
                if (storedQuery is not null)
                {
                    parameters.StoredQueryId = Attribute(storedQuery, "id");
                    parameters.StoredQueryParameters = Children(storedQuery, "Parameter")
                        .Where(p => Attribute(p, "name") is not null)
                        .GroupBy(p => Attribute(p, "name")!, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First().InnerText.Trim(), StringComparer.OrdinalIgnoreCase);
                    break;
                }

                var queries = Children(root, "Query").ToList();
                if (queries.Count == 0)
                {
                    throw WfsException.MissingParameterValue("Query");
                }
                if (queries.Count > 1)
                {
                    throw WfsException.OptionNotSupported("Query", "Only one wfs:Query per GetFeature request is supported");
                }

                var query = queries[0];
                parameters.TypeNames = SplitTypeNames(Attribute(query, "typeNames") ?? Attribute(query, "typeName"));
                parameters.SrsName = Attribute(query, "srsName");
                parameters.Filter = Children(query, "Filter").FirstOrDefault();
                parameters.SortBy = ParseXmlSortBy(Children(query, "SortBy").FirstOrDefault());
                parameters.PropertyNames = Children(query, "PropertyName").Select(p => p.InnerText.Trim()).Where(p => p.Length > 0).ToArray();
                break;
        }

        return parameters;
    }

    #endregion

    #region Helper

    /// <summary>
    /// Splits the raw query string at &amp; and the first =, URL decodes names and values
    /// (+ is a space). The first occurrence of a parameter wins.
    /// </summary>
    internal static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in (queryString ?? "").TrimStart('?').Split('&'))
        {
            if (String.IsNullOrEmpty(part))
            {
                continue;
            }

            int pos = part.IndexOf('=');
            string name = UrlDecode(pos < 0 ? part : part.Substring(0, pos)).Trim();
            string value = pos < 0 ? "" : UrlDecode(part.Substring(pos + 1)).Trim();

            if (name.Length > 0 && !result.ContainsKey(name))
            {
                result[name] = value;
            }
        }

        return result;
    }

    internal static List<KeyValuePair<string, string>> ParseQueryStringOrdered(string queryString)
    {
        var result = new List<KeyValuePair<string, string>>();
        foreach (var part in (queryString ?? "").TrimStart('?').Split('&'))
        {
            if (String.IsNullOrEmpty(part))
            {
                continue;
            }
            int pos = part.IndexOf('=');
            string name = UrlDecode(pos < 0 ? part : part.Substring(0, pos)).Trim();
            if (name.Length > 0)
            {
                result.Add(new KeyValuePair<string, string>(name, pos < 0 ? "" : UrlDecode(part.Substring(pos + 1)).Trim()));
            }
        }
        return result;
    }

    /// <summary>
    /// SORTBY=NAME ASC,VALUE DESC (2.0.0) or NAME A,VALUE D (1.1.0, also NAME+A before URL decoding)
    /// </summary>
    private static (string, bool)[] ParseKvpSortBy(string? value)
        => SplitList(value)
            .Select(item =>
            {
                var tokens = item.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0 || tokens.Length > 2)
                {
                    throw WfsException.InvalidParameterValue("sortBy", $"Invalid sortBy: {item}");
                }
                return (tokens[0], tokens.Length == 2 && ParseSortOrder(tokens[1]));
            })
            .ToArray();

    private static (string, bool)[] ParseXmlSortBy(XmlNode? sortBy)
        => sortBy is null
            ? []
            : Children(sortBy, "SortProperty")
                .Select(sp =>
                {
                    var property = Children(sp, "ValueReference").Concat(Children(sp, "PropertyName")).FirstOrDefault()?.InnerText.Trim();
                    if (String.IsNullOrEmpty(property))
                    {
                        throw WfsException.InvalidParameterValue("sortBy", "SortProperty without property");
                    }
                    var order = Children(sp, "SortOrder").FirstOrDefault()?.InnerText.Trim();
                    return (property, !String.IsNullOrEmpty(order) && ParseSortOrder(order));
                })
                .ToArray();

    private static bool ParseSortOrder(string order)
        => order.ToUpperInvariant() switch
        {
            "ASC" or "A" => false,
            "DESC" or "D" => true,
            _ => throw WfsException.InvalidParameterValue("sortBy", $"Invalid sort order: {order}")
        };

    private static string UrlDecode(string value)
    {
        try
        {
            return Uri.UnescapeDataString(value.Replace('+', ' '));
        }
        catch (UriFormatException)
        {
            return value;
        }
    }

    private static string? NullIfEmpty(string? value) => String.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] SplitList(string? value)
        => String.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0).ToArray();

    /// <summary>
    /// TYPENAMES=a,b or (a)(b) (2.0.0 KVP lists of queries without joins)
    /// </summary>
    private static string[] SplitTypeNames(string? value)
        => SplitList(value?.Replace(")(", ",").Trim('(', ')'));

    private static int? ParseCount(string? value, string locator)
    {
        if (String.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        if (!int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int count))
        {
            throw WfsException.InvalidParameterValue(locator, $"Invalid {locator}: {value}");
        }
        return count;
    }

    private static string? Attribute(XmlNode node, string name)
        => NullIfEmpty(node.Attributes?[name]?.Value);

    private static IEnumerable<XmlNode> Children(XmlNode node, string localName)
        => node.ChildNodes.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Element && n.LocalName == localName);

    #endregion
}
