#nullable enable

using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.Extensions.Cql2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class StatisticsResultExtensions
{
    /// <summary>
    /// The statistics as response. With having, the rows are filtered here (the data sources can't) and offset/limit
    /// are applied afterwards; without, the data source has applied them.
    /// </summary>
    static public GetFeaturesStatisticsResponse ToStatisticsResponse(this StatisticsResult result, CqlPredicate? having, int offset, int limit)
    {
        var columns = result.Columns.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        IEnumerable<IDictionary<string, object>> rows = result.Rows;
        bool exceededLimit = result.ExceededLimit;

        if (having is not null)
        {
            object? Value(IDictionary<string, object> row, string name)
            {
                var column = row.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase))
                             ?? (columns.Contains(name) ? null : throw new MapServerException($"having: {name} is no groupBy field or statistic"));
                return column is null || row[column] is DBNull ? null : row[column];
            }

            var filtered = result.Rows.Where(row => CqlEvaluator.Evaluate(having, name => Value(row, name))).ToList();
            rows = filtered.Skip(Math.Max(0, offset)).Take(limit > 0 ? limit : int.MaxValue).ToList();
            exceededLimit = limit > 0 && filtered.Count > Math.Max(0, offset) + limit;
        }

        return new GetFeaturesStatisticsResponse()
        {
            Fields = result.Columns.Select(c => new StatisticsField()
            {
                Name = c.Name,
                Type = c.Type.ToPropertyType()
            }).ToArray(),
            Rows = rows.Select(row => (IDictionary<string, object?>)row.ToDictionary(e => e.Key, e => e.Value is DBNull ? null : (object?)e.Value)).ToArray(),
            ExceededLimit = exceededLimit
        };
    }
}
