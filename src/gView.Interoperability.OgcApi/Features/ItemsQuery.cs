using gView.Framework.Core.Data;
using gView.Interoperability.OgcApi.Cql2;
using gView.Interoperability.OgcApi.Extensions;
using System.Globalization;

namespace gView.Interoperability.OgcApi.Features;

/// <summary>
/// The parsed and validated query parameters of /collections/{collectionId}/items
/// </summary>
public sealed class ItemsQuery
{
    public const int DefaultLimit = 10;

    /// <summary>
    /// Parameters that are not part of the API but must not be rejected (gView token authentication)
    /// </summary>
    private static readonly string[] IgnoredParameters = ["token", "f"];

    private static readonly string[] KnownParameters = ["limit", "offset", "bbox", "bbox-crs", "crs", "datetime", "filter", "filter-lang", "filter-crs"];

    public const string FilterLanguage = "cql2-text";

    public int Limit { get; private set; } = DefaultLimit;
    public int Offset { get; private set; }

    /// <summary>
    /// minx, miny, maxx, maxy in the axis order of <see cref="BBoxCrs"/>
    /// </summary>
    public double[]? BBox { get; private set; }
    public CrsReference BBoxCrs { get; private set; } = CrsReference.Crs84;
    public CrsReference Crs { get; private set; } = CrsReference.Crs84;
    public string? Datetime { get; private set; }

    /// <summary>
    /// Property filters (queryable field name, raw value) - all combined with AND
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> PropertyFilters { get; private set; } = [];

    /// <summary>
    /// The CQL2 filter (Part 3), combined with the property filters by AND
    /// </summary>
    public CqlPredicate? Filter { get; private set; }

    public static ItemsQuery Parse(
                IReadOnlyDictionary<string, string> parameters,
                OgcCollectionLayer collection,
                IReadOnlyList<CrsReference> supportedCrs,
                int maxLimit)
    {
        var query = new ItemsQuery();
        var propertyFilters = new List<KeyValuePair<string, string>>();

        foreach (var parameter in parameters)
        {
            if (IgnoredParameters.Contains(parameter.Key) || KnownParameters.Contains(parameter.Key))
            {
                continue;
            }

            if (collection.FindQueryable(parameter.Key) is null)
            {
                throw OgcApiException.BadRequest($"Unknown query parameter '{parameter.Key}'");
            }

            propertyFilters.Add(parameter);
        }

        query.PropertyFilters = propertyFilters;

        if (parameters.TryGetValue("limit", out var limit))
        {
            if (!int.TryParse(limit, NumberStyles.None, CultureInfo.InvariantCulture, out int limitValue) || limitValue < 1)
            {
                throw OgcApiException.BadRequest("limit must be an integer >= 1");
            }
            query.Limit = Math.Min(limitValue, Math.Max(1, maxLimit));  // a limit above the maximum is not an error
        }
        else
        {
            query.Limit = Math.Min(DefaultLimit, Math.Max(1, maxLimit));
        }

        if (parameters.TryGetValue("offset", out var offset))
        {
            if (!int.TryParse(offset, NumberStyles.None, CultureInfo.InvariantCulture, out int offsetValue))
            {
                throw OgcApiException.BadRequest("offset must be an integer >= 0");
            }
            query.Offset = offsetValue;
        }

        if (parameters.TryGetValue("bbox", out var bbox))
        {
            query.BBox = ParseBBox(bbox);
        }

        if (parameters.TryGetValue("bbox-crs", out var bboxCrs))
        {
            query.BBoxCrs = ParseSupportedCrs(bboxCrs, supportedCrs, "bbox-crs");
        }

        if (parameters.TryGetValue("crs", out var crs))
        {
            query.Crs = ParseSupportedCrs(crs, supportedCrs, "crs");
        }

        if (parameters.TryGetValue("datetime", out var datetime))
        {
            ValidateDatetime(datetime);
            query.Datetime = datetime;  // accepted, but ignored: gView layers have no temporal property (yet)
        }

        if (parameters.TryGetValue("filter-lang", out var filterLang) && filterLang != FilterLanguage)
        {
            throw OgcApiException.BadRequest($"Unsupported filter-lang '{filterLang}', only {FilterLanguage} is supported");
        }

        if (parameters.TryGetValue("filter-crs", out var filterCrs))
        {
            ParseSupportedCrs(filterCrs, supportedCrs, "filter-crs");  // no spatial predicates (yet) => only validated
        }

        if (parameters.TryGetValue("filter", out var filter))
        {
            query.Filter = CqlTextParser.Parse(filter);
        }

        return query;
    }

    /// <summary>
    /// Builds the SQL where clause for the property filters and the CQL2 filter
    /// </summary>
    public string BuildWhereClause(OgcCollectionLayer collection)
    {
        var predicates = PropertyFilters
            .Select(p => PropertyFilterPredicate(
                collection.FindQueryable(p.Key) ?? throw OgcApiException.BadRequest($"Unknown query parameter '{p.Key}'"),
                p.Value))
            .ToList();

        if (Filter is not null)
        {
            predicates.Add(Filter);
        }

        if (predicates.Count == 0)
        {
            return "";
        }

        var translator = new CqlSqlTranslator(collection.FindQueryable, collection.SqlDialect);
        return translator.Translate(predicates.Aggregate((left, right) => new CqlAnd(left, right)));
    }

    #region Helper

    /// <summary>
    /// A property filter query parameter (?field=value) as CQL2 predicate. '*' is a wildcard for strings.
    /// </summary>
    internal static CqlPredicate PropertyFilterPredicate(IField field, string value)
    {
        var property = new CqlProperty(field.name);

        if (field.IsStringType())
        {
            if (value.Contains('*'))
            {
                string pattern = value
                    .Replace("\\", "\\\\")
                    .Replace("%", "\\%")
                    .Replace("_", "\\_")
                    .Replace('*', '%');
                return new CqlLike(property, new CqlString(pattern), false);
            }
            return new CqlComparison("=", property, new CqlString(value));
        }

        if (field.IsIntegerType())
        {
            if (!long.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long longValue))
            {
                throw OgcApiException.BadRequest($"Value of '{field.name}' must be an integer");
            }
            return new CqlComparison("=", property, new CqlNumber(longValue));
        }

        if (field.IsFloatingPointType())
        {
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue)
                || !double.IsFinite(doubleValue))
            {
                throw OgcApiException.BadRequest($"Value of '{field.name}' must be a number");
            }
            return new CqlComparison("=", property, new CqlNumber(doubleValue));
        }

        if (field.type == FieldType.boolean)
        {
            if (!bool.TryParse(value, out bool boolValue))
            {
                throw OgcApiException.BadRequest($"Value of '{field.name}' must be true or false");
            }
            return new CqlComparison("=", property, new CqlBoolean(boolValue));
        }

        if (field.type == FieldType.Date)
        {
            if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return new CqlComparison("=", property, new CqlDate(date));
            }
            if (value.Contains('T') && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
            {
                return new CqlComparison("=", property, new CqlTimestamp(timestamp.UtcDateTime));
            }
            throw OgcApiException.BadRequest($"Value of '{field.name}' must be a date (yyyy-MM-dd) or timestamp (RFC 3339)");
        }

        throw OgcApiException.BadRequest($"Property '{field.name}' can't be used as a filter");
    }

    internal static double[] ParseBBox(string bbox)
    {
        var parts = bbox.Split(',');
        if (parts.Length != 4 && parts.Length != 6)
        {
            throw OgcApiException.BadRequest("bbox must have 4 or 6 numbers");
        }

        var values = new double[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i])
                || !double.IsFinite(values[i]))
            {
                throw OgcApiException.BadRequest("bbox must contain numbers only");
            }
        }

        return parts.Length == 6
            ? [values[0], values[1], values[3], values[4]]  // ignore the vertical extent
            : values;
    }

    private static CrsReference ParseSupportedCrs(string value, IReadOnlyList<CrsReference> supportedCrs, string parameterName)
    {
        if (!CrsReference.TryParse(value, out var crs) || !supportedCrs.Contains(crs))
        {
            throw OgcApiException.BadRequest($"Unsupported {parameterName}: {value}");
        }
        return crs;
    }

    internal static void ValidateDatetime(string datetime)
    {
        var parts = datetime.Split('/');
        if (parts.Length > 2 || parts.All(p => p == ".." || p == ""))
        {
            throw OgcApiException.BadRequest("Invalid datetime");
        }

        foreach (var part in parts)
        {
            if (part == ".." || (part == "" && parts.Length == 2))
            {
                continue;
            }

            if (!DateTimeOffset.TryParse(part, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _))
            {
                throw OgcApiException.BadRequest($"Invalid datetime: {part}");
            }
        }
    }

    #endregion
}
