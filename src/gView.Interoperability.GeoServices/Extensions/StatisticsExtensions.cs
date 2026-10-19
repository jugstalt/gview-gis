using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Exceptions;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace gView.Interoperability.GeoServices.Extensions
{
    /// <summary>
    /// Translates the ArcGIS REST <c>outStatistics</c>, <c>groupByFieldsForStatistics</c> and
    /// <c>orderByFields</c> query parameters into a <see cref="StatisticsQuery"/>.
    /// All field names are checked against the fields of the table class (whitelist).
    /// </summary>
    static internal class StatisticsExtensions
    {
        private static readonly Regex OutNameRegex = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        static public bool IsStatisticsQuery(this JsonQueryLayerDTO query)
            => !String.IsNullOrWhiteSpace(query.OutStatistics) ||
               !String.IsNullOrWhiteSpace(query.GroupByFieldsForStatistics);

        static public void CheckStatisticsParameters(this JsonQueryLayerDTO query)
        {
            if (String.IsNullOrWhiteSpace(query.OutStatistics))
            {
                throw new MapServerException("groupByFieldsForStatistics requires outStatistics");
            }

            if (query.ReturnIdsOnly || query.ReturnCountOnly || query.ReturnDistinctValues || query.ReturnExtentOnly)
            {
                throw new MapServerException(
                    "outStatistics can't be combined with returnIdsOnly, returnCountOnly, returnDistinctValues or returnExtentOnly");
            }
        }

        static public StatisticsQuery ToStatisticsQuery(this JsonQueryLayerDTO query, ITableClass tableClass, int limit, int maxScanRecords)
        {
            List<JsonOutStatisticDTO> dtos;
            try
            {
                dtos = System.Text.Json.JsonSerializer.Deserialize<List<JsonOutStatisticDTO>>(query.OutStatistics) ?? new();
            }
            catch (System.Text.Json.JsonException)
            {
                throw new MapServerException("Invalid outStatistics: a JSON array of {statisticType, onStatisticField, outStatisticFieldName} is expected");
            }

            if (dtos.Count == 0)
            {
                throw new MapServerException("Invalid outStatistics: at least one statistic is required");
            }

            var result = new StatisticsQuery()
            {
                Offset = Math.Max(0, query.ResultOffset),
                Limit = limit,
                MaxScanRecords = maxScanRecords
            };

            foreach (var groupByName in (query.GroupByFieldsForStatistics ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var field = tableClass.FindStatisticsField(groupByName);
                if (result.GroupByFields.Contains(field.name, StringComparer.OrdinalIgnoreCase))
                {
                    throw new MapServerException($"Duplicate group by field: {groupByName}");
                }

                result.GroupByFields.Add(field.name);
                result.GroupByFieldTypes.Add(field.type);
            }

            int index = 0;
            foreach (var dto in dtos)
            {
                index++;

                var type = ParseStatisticType(dto.StatisticType);
                var field = tableClass.FindStatisticsField(dto.OnStatisticField);

                if (type is StatisticType.Sum or StatisticType.Avg or StatisticType.StdDev or StatisticType.Var &&
                    !field.type.IsNumeric())
                {
                    throw new MapServerException($"Statistic '{dto.StatisticType}' requires a numeric field: {dto.OnStatisticField}");
                }

                var outName = String.IsNullOrWhiteSpace(dto.OutStatisticFieldName) ? $"EXPR_{index}" : dto.OutStatisticFieldName.Trim();
                if (!OutNameRegex.IsMatch(outName))
                {
                    throw new MapServerException($"Invalid outStatisticFieldName: {outName}");
                }
                if (result.Statistics.Any(s => s.OutName.Equals(outName, StringComparison.OrdinalIgnoreCase)) ||
                    result.GroupByFields.Contains(outName, StringComparer.OrdinalIgnoreCase))
                {
                    throw new MapServerException($"Duplicate output field name: {outName}");
                }

                result.Statistics.Add(new StatisticDefinition()
                {
                    Type = type,
                    OnField = field.name,
                    OnFieldType = field.type,
                    OutName = outName
                });
            }

            foreach (var orderBy in (query.OrderByFields ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length is < 1 or > 2)
                {
                    throw new MapServerException($"Invalid orderByFields: {orderBy}");
                }

                bool descending = false;
                if (parts.Length == 2)
                {
                    if (parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase)) { descending = true; }
                    else if (!parts[1].Equals("ASC", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new MapServerException($"Invalid orderByFields: {orderBy}");
                    }
                }

                var name = result.GroupByFields.Concat(result.Statistics.Select(s => s.OutName))
                                 .FirstOrDefault(n => n.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
                if (name == null)
                {
                    throw new MapServerException(
                        $"Invalid orderByFields: '{parts[0]}' must be a group by field or an outStatisticFieldName");
                }

                result.OrderBy.Add(new StatisticsOrderBy() { Name = name, Descending = descending });
            }

            return result;
        }

        static private StatisticType ParseStatisticType(string statisticType)
        {
            switch ((statisticType ?? "").Trim().ToLowerInvariant())
            {
                case "count": return StatisticType.Count;
                case "sum": return StatisticType.Sum;
                case "min": return StatisticType.Min;
                case "max": return StatisticType.Max;
                case "avg": return StatisticType.Avg;
                case "stddev": return StatisticType.StdDev;
                case "var":
                case "variance": return StatisticType.Var;
                default:
                    throw new MapServerException(
                        $"Unsupported statisticType '{statisticType}'. Supported: count, sum, min, max, avg, stddev, var");
            }
        }

        static private IField FindStatisticsField(this ITableClass tableClass, string fieldName)
        {
            fieldName = (fieldName ?? "").Trim();

            // whitelist: only real table fields, no expressions / functions
            var field = String.IsNullOrEmpty(fieldName) || fieldName.IsFieldFunction() ? null : tableClass.FindField(fieldName);
            if (field == null)
            {
                throw new MapServerException($"Unknown field: {fieldName}");
            }

            return field;
        }

        static private bool IsNumeric(this FieldType type) => type is
            FieldType.ID or FieldType.integer or FieldType.smallinteger or FieldType.biginteger or
            FieldType.Float or FieldType.Double;
    }
}
