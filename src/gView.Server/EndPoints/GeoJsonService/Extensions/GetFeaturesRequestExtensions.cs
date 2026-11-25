#nullable enable

using gView.Framework.Common.Extensions;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static partial class GetFeaturesRequestExtensions
{
    /// <summary>Source rows an in-memory statistic may read (data sources without SQL statistics)</summary>
    public const int MaxStatisticsScanRecords = 1_000_000;

    /// <summary>
    /// The statistics of the request, checked against the table: known fields, numbers for sum/avg/stddev/var,
    /// unique column names; order by group-by fields or statistic names. With having, all rows are read
    /// (offset/limit are applied after the having filter).
    /// </summary>
    static public StatisticsQuery ToStatisticsQuery(this GetFeaturesRequest request, ITableClass tableClass, int limit, bool withHaving)
    {
        var statistics = request.Statistics?.ToList() ?? [];
        if (statistics.Count == 0)
        {
            throw new MapServerException("statistics: at least one statistic is required, e.g. { \"type\": \"count\", \"field\": \"OBJECTID\" }");
        }

        var query = new StatisticsQuery()
        {
            Offset = withHaving ? 0 : Math.Max(0, request.Offset ?? 0),
            Limit = withHaving ? 0 : limit,
            MaxScanRecords = MaxStatisticsScanRecords
        };

        foreach (var name in request.GroupBy ?? [])
        {
            var field = Field(tableClass, name, "groupBy");
            if (query.GroupByFields.Contains(field.name, StringComparer.OrdinalIgnoreCase))
            {
                throw new MapServerException($"Duplicate groupBy field: {name}");
            }
            query.GroupByFields.Add(field.name);
            query.GroupByFieldTypes.Add(field.type);
        }

        foreach (var statistic in statistics)
        {
            var type = Enum.TryParse<StatisticType>(statistic.Type, true, out var value) && Enum.IsDefined(value)
                ? value
                : throw new MapServerException($"Unknown statistic type '{statistic.Type}': {String.Join(", ", Enum.GetNames<StatisticType>()).ToLowerInvariant()}");
            var field = Field(tableClass, statistic.Field, "statistics");

            if (type is StatisticType.Sum or StatisticType.Avg or StatisticType.StdDev or StatisticType.Var && !IsNumeric(field.type))
            {
                throw new MapServerException($"The statistic {statistic.Type} needs a numeric field: {statistic.Field}");
            }

            var outName = String.IsNullOrWhiteSpace(statistic.Alias) ? $"{type.ToString().ToLowerInvariant()}_{field.name}" : statistic.Alias.Trim();
            if (!OutNameRegex().IsMatch(outName))
            {
                throw new MapServerException($"Invalid statistic alias '{outName}': letters, digits and _");
            }
            if (query.Statistics.Any(s => s.OutName.Equals(outName, StringComparison.OrdinalIgnoreCase))
                || query.GroupByFields.Contains(outName, StringComparer.OrdinalIgnoreCase))
            {
                throw new MapServerException($"Duplicate column name: {outName}");
            }

            query.Statistics.Add(new StatisticDefinition() { Type = type, OnField = field.name, OutName = outName, OnFieldType = field.type });
        }

        foreach (var orderBy in request.OrderByFields ?? [])
        {
            var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            bool descending = parts.Length == 2 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
            if (parts.Length is 0 or > 2 || parts.Length == 2 && !descending && !parts[1].Equals("ASC", StringComparison.OrdinalIgnoreCase))
            {
                throw new MapServerException($"Invalid orderBy: {orderBy} (name [ASC|DESC])");
            }

            var name = query.GroupByFields.FirstOrDefault(n => n.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                       ?? query.Statistics.FirstOrDefault(s => s.OutName.Equals(parts[0], StringComparison.OrdinalIgnoreCase))?.OutName
                       ?? throw new MapServerException($"orderBy of statistics: {parts[0]} is no groupBy field or statistic");
            query.OrderBy.Add(new StatisticsOrderBy() { Name = name, Descending = descending });
        }

        return query;
    }

    private static IField Field(ITableClass tableClass, string? name, string parameter)
        => String.IsNullOrWhiteSpace(name)
            ? throw new MapServerException($"{parameter}: a field is required")
            : (tableClass.Fields?.FindField(name.Trim())  // like the other field names of a query: case-insensitive
               ?? tableClass.Fields?.ToEnumerable().FirstOrDefault(f => f.name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
              is IField field && field.type is not (FieldType.Shape or FieldType.GEOMETRY or FieldType.GEOGRAPHY or FieldType.binary)
                ? field
                : throw new MapServerException($"{parameter}: unknown field '{name}'");

    private static bool IsNumeric(FieldType type)
        => type is FieldType.ID or FieldType.integer or FieldType.smallinteger or FieldType.biginteger or FieldType.Float or FieldType.Double;

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex OutNameRegex();
}
