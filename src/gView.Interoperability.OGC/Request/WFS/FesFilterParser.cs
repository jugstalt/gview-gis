#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.OGC.GML;
using gView.Interoperability.Extensions.Cql2;
using gView.Interoperability.Extensions.Extensions;
using gView.Interoperability.Extensions.Features;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// A spatial condition of a WFS filter (gView queries can only have one geometry)
/// </summary>
public sealed record WfsSpatialCondition(spatialRelation Relation, IGeometry Geometry, ISpatialReference? SpatialReference);

/// <summary>
/// A parsed WFS filter: the attribute predicate (CQL2 syntax tree), at most one spatial condition
/// and/or resource ids (c{layerId}.{objectId})
/// </summary>
public sealed record WfsFilter(CqlPredicate? Predicate, WfsSpatialCondition? Spatial, IReadOnlyList<string>? ResourceIds)
{
    public static WfsFilter Empty { get; } = new WfsFilter(null, null, null);
}

/// <summary>
/// Parses OGC Filter Encoding 1.0/1.1 (ogc namespace or none) and FES 2.0 (fes namespace) into a
/// <see cref="WfsFilter"/>. Elements are compared by local name, property names (PropertyName or
/// ValueReference, with or without a gv: prefix or a path) are resolved against the queryables of the
/// feature type and literals are converted to the type of the property, so the SQL is written by the
/// <see cref="CqlSqlTranslator"/> only (no SQL injection).
/// Spatial operators (BBOX, Intersects, Within, Contains) are only allowed at the top level or in a
/// top-level And, because a gView query has only one spatial filter.
/// </summary>
public class FesFilterParser
{
    private readonly OgcCollectionLayer _collection;
    private readonly WfsVersion _version;
    private readonly CrsReference? _defaultCrs;

    /// <param name="defaultCrs">The CRS of filter geometries without srsName (the CRS of the request)</param>
    public FesFilterParser(OgcCollectionLayer collection, WfsVersion version, CrsReference? defaultCrs)
    {
        _collection = collection;
        _version = version;
        _defaultCrs = defaultCrs;
    }

    /// <exception cref="WfsException">Invalid or unsupported filter</exception>
    public WfsFilter Parse(XmlNode? filter)
    {
        if (filter is null)
        {
            return WfsFilter.Empty;
        }

        var children = Elements(filter).ToList();
        if (children.Count == 0)
        {
            return WfsFilter.Empty;
        }

        if (children.All(IsResourceId))
        {
            return new WfsFilter(null, null, children.Select(ResourceId).ToArray());
        }
        if (children.Count > 1)
        {
            throw Invalid("a filter has exactly one operator (combine operators with And/Or)");
        }

        var root = children[0];

        if (IsSpatial(root))
        {
            return new WfsFilter(null, Spatial(root), null);
        }

        if (root.LocalName == "And")
        {
            // flatten nested And, take the spatial condition (at most one) out of it
            var operands = FlattenAnd(root).ToList();
            var spatialOperands = operands.Where(IsSpatial).ToList();
            if (spatialOperands.Count > 1)
            {
                throw NotSupported("only one spatial operator per filter is supported");
            }

            var predicates = operands.Where(o => !IsSpatial(o)).Select(Predicate).ToList();

            return new WfsFilter(
                predicates.Count == 0 ? null : predicates.Aggregate((l, r) => new CqlAnd(l, r)),
                spatialOperands.Count == 1 ? Spatial(spatialOperands[0]) : null,
                null);
        }

        return new WfsFilter(Predicate(root), null, null);
    }

    #region Predicates

    private CqlPredicate Predicate(XmlElement element)
    {
        switch (element.LocalName)
        {
            case "And":
            case "Or":
                {
                    var operands = Elements(element).ToList();
                    if (operands.Count < 2)
                    {
                        throw Invalid($"{element.LocalName} needs at least two operands");
                    }
                    var predicates = operands.Select(Predicate).ToList();
                    return element.LocalName == "And"
                        ? predicates.Aggregate((l, r) => new CqlAnd(l, r))
                        : predicates.Aggregate((l, r) => new CqlOr(l, r));
                }
            case "Not":
                {
                    var operands = Elements(element).ToList();
                    if (operands.Count != 1)
                    {
                        throw Invalid("Not needs exactly one operand");
                    }
                    return new CqlNot(Predicate(operands[0]));
                }

            case "PropertyIsEqualTo": return Comparison(element, "=");
            case "PropertyIsNotEqualTo": return Comparison(element, "<>");
            case "PropertyIsLessThan": return Comparison(element, "<");
            case "PropertyIsGreaterThan": return Comparison(element, ">");
            case "PropertyIsLessThanOrEqualTo":
            case "PropertyIsLessThanEqualTo": return Comparison(element, "<=");
            case "PropertyIsGreaterThanOrEqualTo":
            case "PropertyIsGreaterThanEqualTo": return Comparison(element, ">=");

            case "PropertyIsLike": return Like(element);
            case "PropertyIsBetween": return Between(element);
            case "PropertyIsNull":
            case "PropertyIsNil":
                {
                    var operands = Elements(element).ToList();
                    if (operands.Count != 1 || !IsPropertyReference(operands[0]))
                    {
                        throw Invalid($"{element.LocalName} needs a property");
                    }
                    return new CqlIsNull(new CqlProperty(ResolveField(operands[0]).name), Negated: false);
                }

            default:
                if (IsSpatial(element))
                {
                    throw NotSupported("spatial operators are only supported at the top level of the filter or in a top-level And");
                }
                if (IsResourceId(element))
                {
                    throw Invalid("resource ids can't be combined with other operators");
                }
                if (IsKnownUnsupported(element.LocalName))
                {
                    throw NotSupported($"the operator {element.LocalName} is not supported");
                }
                throw Invalid($"unknown operator {element.LocalName}");
        }
    }

    private CqlPredicate Comparison(XmlElement element, string op)
    {
        var operands = Elements(element).ToList();
        if (operands.Count != 2)
        {
            throw Invalid($"{element.LocalName} needs two operands");
        }

        bool caseInsensitive = element.GetAttribute("matchCase").Equals("false", StringComparison.OrdinalIgnoreCase);

        var (left, right) = Operands(operands[0], operands[1]);
        if (caseInsensitive && IsText(left, right))
        {
            left = new CqlCaseInsensitive(left);
            right = new CqlCaseInsensitive(right);
        }

        return new CqlComparison(op, left, right);
    }

    private CqlPredicate Like(XmlElement element)
    {
        var operands = Elements(element).ToList();
        if (operands.Count != 2 || !IsPropertyReference(operands[0]) || operands[1].LocalName != "Literal")
        {
            throw Invalid("PropertyIsLike needs a property and a literal");
        }

        var field = ResolveField(operands[0]);
        if (!field.IsStringType())
        {
            throw Invalid($"PropertyIsLike can only be used with text properties ({field.name})");
        }

        string wildCard = element.GetAttribute("wildCard") is { Length: > 0 } w ? w : "*";
        string singleChar = element.GetAttribute("singleChar") is { Length: > 0 } s ? s : "?";
        string escapeChar = element.GetAttribute("escapeChar") is { Length: > 0 } e ? e
                          : element.GetAttribute("escape") is { Length: > 0 } e1 ? e1  // Filter 1.0
                          : "\\";

        CqlExpression value = new CqlProperty(field.name);
        CqlExpression pattern = new CqlString(ToCqlLikePattern(operands[1].InnerText, wildCard, singleChar, escapeChar));

        if (element.GetAttribute("matchCase").Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            value = new CqlCaseInsensitive(value);
            pattern = new CqlCaseInsensitive(pattern);
        }

        return new CqlLike(value, pattern, Negated: false);
    }

    private CqlPredicate Between(XmlElement element)
    {
        var operands = Elements(element).ToList();
        var lower = operands.FirstOrDefault(o => o.LocalName == "LowerBoundary");
        var upper = operands.FirstOrDefault(o => o.LocalName == "UpperBoundary");
        if (operands.Count != 3 || !IsPropertyReference(operands[0]) || lower is null || upper is null)
        {
            throw Invalid("PropertyIsBetween needs a property, LowerBoundary and UpperBoundary");
        }

        var field = ResolveField(operands[0]);

        return new CqlBetween(
            new CqlProperty(field.name),
            Literal(field, SingleElement(lower)),
            Literal(field, SingleElement(upper)),
            Negated: false);
    }

    #endregion

    #region Operands

    private (CqlExpression left, CqlExpression right) Operands(XmlElement first, XmlElement second)
    {
        if (IsPropertyReference(first) && IsPropertyReference(second))
        {
            return (new CqlProperty(ResolveField(first).name), new CqlProperty(ResolveField(second).name));
        }
        if (IsPropertyReference(first))
        {
            var field = ResolveField(first);
            return (new CqlProperty(field.name), Literal(field, second));
        }
        if (IsPropertyReference(second))
        {
            var field = ResolveField(second);
            return (Literal(field, first), new CqlProperty(field.name));
        }

        throw Invalid("a comparison needs at least one property");
    }

    /// <summary>
    /// A literal as CQL2 literal of the property's type
    /// </summary>
    private static CqlExpression Literal(IField field, XmlElement element)
    {
        if (element.LocalName != "Literal")
        {
            throw element.LocalName == "Function"
                ? NotSupported("functions are not supported")
                : Invalid($"a Literal is expected, not {element.LocalName}");
        }

        string value = element.InnerText.Trim();

        if (field.IsIntegerType() || field.IsFloatingPointType())
        {
            return Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                ? new CqlNumber(number)
                : throw Invalid($"invalid number literal for {field.name}: {value}");
        }

        switch (field.type)
        {
            case FieldType.boolean:
                return value.ToLowerInvariant() switch
                {
                    "true" or "1" => new CqlBoolean(true),
                    "false" or "0" => new CqlBoolean(false),
                    _ => throw Invalid($"invalid boolean literal for {field.name}: {value}")
                };
            case FieldType.Date:
                if (value.Length == 10 && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return new CqlDate(date);
                }
                return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var timestamp)
                    ? new CqlTimestamp(DateTime.SpecifyKind(timestamp, DateTimeKind.Utc))
                    : throw Invalid($"invalid date literal for {field.name}: {value}");
            default:
                return new CqlString(element.InnerText);  // text: not trimmed
        }
    }

    private static bool IsText(CqlExpression left, CqlExpression right)
        => left is CqlString || right is CqlString;

    /// <summary>
    /// Resolves PropertyName/ValueReference: "NAME", "gv:NAME", "gv:c0/gv:NAME" or the element name of the field
    /// </summary>
    private IField ResolveField(XmlElement propertyReference)
    {
        string name = LocalPropertyName(propertyReference.InnerText);

        var queryables = _collection.Queryables.ToList();
        var field = queryables.FirstOrDefault(f => f.name == name || WfsFeatureTypes.ElementName(f.name) == name)
                 ?? queryables.FirstOrDefault(f => f.name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                                || WfsFeatureTypes.ElementName(f.name).Equals(name, StringComparison.OrdinalIgnoreCase));

        return field
            ?? throw Invalid(_collection.PropertyFields.Any(f => f.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    ? $"the property {name} can't be used in filters of this data source"
                    : $"unknown property {name}");
    }

    internal static string LocalPropertyName(string propertyName)
    {
        string name = propertyName.Trim();
        if (name.Contains('/'))
        {
            name = name.Substring(name.LastIndexOf('/') + 1);
        }
        if (name.Contains(':'))
        {
            name = name.Substring(name.LastIndexOf(':') + 1);
        }
        return name.TrimStart('@');
    }

    /// <summary>
    /// Converts an OGC like pattern (wildCard, singleChar, escapeChar) into a CQL2 pattern (%, _ and \ as escape)
    /// </summary>
    internal static string ToCqlLikePattern(string pattern, string wildCard, string singleChar, string escapeChar)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < pattern.Length;)
        {
            if (pattern.AsSpan(i).StartsWith(escapeChar) && i + escapeChar.Length < pattern.Length)
            {
                i += escapeChar.Length;
                AppendLiteral(sb, pattern[i]);
                i++;
            }
            else if (pattern.AsSpan(i).StartsWith(wildCard))
            {
                sb.Append('%');
                i += wildCard.Length;
            }
            else if (pattern.AsSpan(i).StartsWith(singleChar))
            {
                sb.Append('_');
                i += singleChar.Length;
            }
            else
            {
                AppendLiteral(sb, pattern[i]);
                i++;
            }
        }

        return sb.ToString();

        static void AppendLiteral(StringBuilder sb, char c)
        {
            if (c is '%' or '_' or '\\')
            {
                sb.Append('\\');
            }
            sb.Append(c);
        }
    }

    #endregion

    #region Spatial

    private static readonly string[] SpatialOperators = ["BBOX", "Intersects", "Within", "Contains", "Disjoint", "Equals", "Touches", "Crosses", "Overlaps", "DWithin", "Beyond", "Intersect"];

    private static bool IsSpatial(XmlElement element) => SpatialOperators.Contains(element.LocalName);

    private WfsSpatialCondition Spatial(XmlElement element)
    {
        var relation = element.LocalName switch
        {
            "BBOX" or "Intersects" or "Intersect" => spatialRelation.SpatialRelationIntersects,
            "Within" => spatialRelation.SpatialRelationWithin,
            "Contains" => spatialRelation.SpatialRelationContains,
            _ => throw NotSupported($"the spatial operator {element.LocalName} is not supported")
        };

        // the geometry property (PropertyName/ValueReference) is optional, the geometry is the other operand
        var geometryElement = Elements(element).FirstOrDefault(e => !IsPropertyReference(e))
            ?? throw Invalid($"{element.LocalName} needs a geometry");

        var crs = _defaultCrs;
        if (GmlGeometryReader.SrsName(geometryElement) is { } srsName)
        {
            crs = WFSRequest.ParseCrs(srsName, "filter");
        }
        var sref = crs?.ToSpatialReference();

        IGeometry geometry;
        try
        {
            geometry = GmlGeometryReader.Read(geometryElement, _version.SwapAxes(crs, sref));
        }
        catch (FormatException ex)
        {
            throw Invalid(ex.Message);
        }

        if (element.LocalName == "BBOX" && geometry is not IEnvelope)
        {
            geometry = geometry.Envelope;
        }

        return new WfsSpatialCondition(relation, geometry, sref);
    }

    #endregion

    #region Resource ids

    private static bool IsResourceId(XmlElement element)
        => element.LocalName is "ResourceId" or "FeatureId" or "GmlObjectId";

    private static string ResourceId(XmlElement element)
    {
        string id = element.LocalName switch
        {
            "ResourceId" => element.GetAttribute("rid"),
            "FeatureId" => element.GetAttribute("fid"),
            _ => element.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.LocalName == "id")?.Value ?? ""
        };

        return String.IsNullOrWhiteSpace(id)
            ? throw Invalid($"{element.LocalName} without id")
            : id.Trim();
    }

    /// <summary>
    /// The where clause of resource ids (c{layerId}.{objectId} or {objectId}) of a feature type:
    /// ids of other feature types are ignored, "1=0" if none is left.
    /// </summary>
    public static string ResourceIdWhereClause(OgcCollectionLayer collection, IEnumerable<string> resourceIds)
    {
        var idField = collection.FeatureClass?.IDFieldName;
        if (String.IsNullOrEmpty(idField))
        {
            throw NotSupported($"the feature type {collection.Id} has no object ids");
        }

        var oids = new List<long>();
        foreach (var resourceId in resourceIds)
        {
            string? typeName = ResourceIdTypeName(resourceId);
            if (typeName is not null && !typeName.Equals(collection.Id, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string oidText = typeName is null ? resourceId : resourceId.Substring(resourceId.LastIndexOf('.') + 1);
            if (long.TryParse(oidText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long oid))
            {
                oids.Add(oid);
            }
        }

        return oids.Count == 0
            ? "1=0"
            : $"{idField} IN ({String.Join(",", oids.Distinct().Select(o => o.ToString(CultureInfo.InvariantCulture)))})";
    }

    /// <summary>
    /// The type name part of a resource id (c0 of c0.12), null for plain object ids
    /// </summary>
    public static string? ResourceIdTypeName(string resourceId)
    {
        int pos = resourceId.LastIndexOf('.');
        return pos > 0 ? WfsFeatureTypes.LocalName(resourceId.Substring(0, pos)) : null;
    }

    #endregion

    #region Helper

    private static IEnumerable<XmlElement> Elements(XmlNode node)
        => node.ChildNodes.OfType<XmlElement>();

    private static XmlElement SingleElement(XmlElement element)
    {
        var children = Elements(element).ToList();
        return children.Count == 1 ? children[0] : throw Invalid($"{element.LocalName} needs exactly one value");
    }

    private static IEnumerable<XmlElement> FlattenAnd(XmlElement and)
        => Elements(and).SelectMany(e => e.LocalName == "And" ? FlattenAnd(e) : [e]);

    private static bool IsPropertyReference(XmlElement element)
        => element.LocalName is "PropertyName" or "ValueReference";

    private static bool IsKnownUnsupported(string localName)
        => localName is "Function" or "After" or "Before" or "Begins" or "BegunBy" or "TContains" or "During"
           or "EndedBy" or "Ends" or "TEquals" or "Meets" or "MetBy" or "TOverlaps" or "OverlappedBy" or "AnyInteracts"
           or "Add" or "Sub" or "Mul" or "Div";

    private static WfsException Invalid(string message)
        => WfsException.InvalidParameterValue("filter", $"Invalid filter: {message}");

    private static WfsException NotSupported(string message)
        => WfsException.OptionNotSupported("filter", $"Unsupported filter: {message}");

    #endregion
}
