using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Db.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Data.Statistics
{
    /// <summary>
    /// <c>SELECT ... GROUP BY ...</c> for <see cref="StatisticsQuery"/> on FDB feature class tables
    /// (<see cref="IStatisticsTableClass"/>), for all three FDB dialects. Only plain attribute queries are
    /// pushed down; whether a filter qualifies is decided by <see cref="CanExecuteInSql"/> - all
    /// other cases are aggregated in memory by <see cref="StatisticsAggregator"/>.
    /// Field names are quoted; they have already been validated against the table fields.
    /// </summary>
    static public class StatisticsSql
    {
        /// <summary>
        /// A filter can be translated when it is a pure where clause. Spatial filters and row id
        /// filters depend on the spatial index / geometry storage and are not handled here.
        /// </summary>
        static public bool CanExecuteInSql(IQueryFilter filter)
            => filter == null ||
               ((filter is not ISpatialFilter sf || sf.Geometry == null) && filter is not IRowIDFilter);

        static public string BuildCommandText(SqlDialect dialect, string tableName, string whereClause, StatisticsQuery query)
        {
            var select = new StringBuilder("SELECT ");
            bool first = true;

            foreach (var field in query.GroupByFields)
            {
                if (!first) { select.Append(", "); }
                first = false;
                select.Append(Quote(dialect, field)).Append(" AS ").Append(Quote(dialect, field));
            }
            foreach (var statistic in query.Statistics)
            {
                if (!first) { select.Append(", "); }
                first = false;
                select.Append(StatisticExpression(dialect, statistic))
                      .Append(" AS ").Append(Quote(dialect, statistic.OutName));
            }
            select.Append(" FROM ").Append(tableName);

            var sql = new StringBuilder(select.ToString());

            if (!String.IsNullOrEmpty(whereClause))
            {
                sql.Append(" WHERE ").Append(whereClause);
            }

            if (query.GroupByFields.Count > 0)
            {
                sql.Append(" GROUP BY ").Append(String.Join(", ", query.GroupByFields.Select(f => Quote(dialect, f))));

                // same default order as the in memory aggregator: group fields ascending (paging needs a stable order)
                var order = query.OrderBy.Count > 0
                    ? query.OrderBy
                    : query.GroupByFields.Select(f => new StatisticsOrderBy() { Name = f }).ToList();

                sql.Append(" ORDER BY ").Append(String.Join(", ", order.Select(o => OrderByExpression(dialect, o))));

                // one row more than requested => detect "exceeded"
                AppendPaging(sql, dialect, query.Limit > 0 ? query.Limit + 1 : 0, Math.Max(0, query.Offset));
            }

            return sql.ToString();
        }

        /// <summary>
        /// Runs the statistics on an opened connection. <paramref name="whereClause"/> is the where clause
        /// of the (non spatial) filter.
        /// </summary>
        async static public Task<StatisticsResult> ExecuteAsync(
            DbConnection connection, SqlDialect dialect, string tableName, IQueryFilter filter, StatisticsQuery query)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = BuildCommandText(dialect, tableName, filter?.WhereClause, query);
                command.SetCustomCursorTimeout();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    return await ReadResultAsync(reader, query);
                }
            }
        }

        async static public Task<StatisticsResult> ReadResultAsync(DbDataReader reader, StatisticsQuery query)
        {
            var result = new StatisticsResult() { Columns = StatisticsAggregator.CreateColumns(query) };
            var rows = new List<IDictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>(StringComparer.Ordinal);
                for (int i = 0; i < result.Columns.Count; i++)
                {
                    var column = result.Columns[i];
                    object value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    if (value != null && i >= query.GroupByFields.Count)
                    {
                        var statistic = query.Statistics[i - query.GroupByFields.Count];
                        value = statistic.Type switch
                        {
                            StatisticType.Count => Convert.ToInt64(value, CultureInfo.InvariantCulture),
                            StatisticType.Min or StatisticType.Max => value,
                            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture)
                        };
                    }

                    row[column.Name] = value;
                }
                rows.Add(row);
            }

            // no group by => exactly one row, paging does not apply (see BuildCommandText)
            if (query.GroupByFields.Count == 0 && query.Offset > 0)
            {
                rows.Clear();
            }

            if (query.Limit > 0 && rows.Count > query.Limit)
            {
                result.ExceededLimit = true;
                rows = rows.Take(query.Limit).ToList();
            }

            result.Rows = rows;
            return result;
        }

        #region Helper

        /// <summary>Paging after an ORDER BY (always present when grouping): <paramref name="limit"/> 0 = no limit.</summary>
        static private void AppendPaging(StringBuilder sql, SqlDialect dialect, int limit, int offset)
        {
            if (limit <= 0 && offset == 0)
            {
                return;
            }

            switch (dialect)
            {
                case SqlDialect.SqlServer:
                    sql.Append(" OFFSET ").Append(offset).Append(" ROWS");
                    if (limit > 0) { sql.Append(" FETCH NEXT ").Append(limit).Append(" ROWS ONLY"); }
                    break;
                case SqlDialect.PostgreSql:
                    if (limit > 0) { sql.Append(" LIMIT ").Append(limit); }
                    if (offset > 0) { sql.Append(" OFFSET ").Append(offset); }
                    break;
                default: // SQLite: OFFSET is only valid together with LIMIT
                    sql.Append(" LIMIT ").Append(limit > 0 ? limit : -1);
                    if (offset > 0) { sql.Append(" OFFSET ").Append(offset); }
                    break;
            }
        }

        static private string Quote(SqlDialect dialect, string identifier)
            => dialect == SqlDialect.SqlServer
                ? "[" + identifier.Replace("]", "]]") + "]"
                : "\"" + identifier.Replace("\"", "\"\"") + "\"";

        static private string OrderByExpression(SqlDialect dialect, StatisticsOrderBy order)
        {
            var sql = Quote(dialect, order.Name) + (order.Descending ? " DESC" : " ASC");

            // NULL first for ASC, last for DESC (SQLite, SQL Server default) - PostgreSql needs it spelled out
            if (dialect == SqlDialect.PostgreSql)
            {
                sql += order.Descending ? " NULLS LAST" : " NULLS FIRST";
            }

            return sql;
        }

        static private string StatisticExpression(SqlDialect dialect, StatisticDefinition statistic)
        {
            string f = Quote(dialect, statistic.OnField);
            string num = dialect switch
            {
                SqlDialect.SqlServer => $"CAST({f} AS float)",
                SqlDialect.PostgreSql => $"CAST({f} AS double precision)",
                _ => $"(1.0 * {f})"
            };

            switch (statistic.Type)
            {
                case StatisticType.Count:
                    return dialect == SqlDialect.SqlServer ? $"COUNT_BIG({f})" : $"COUNT({f})";
                case StatisticType.Sum:
                    return $"SUM({num})";
                case StatisticType.Avg:
                    return $"AVG({num})";
                case StatisticType.Min:
                    return $"MIN({f})";
                case StatisticType.Max:
                    return $"MAX({f})";
                case StatisticType.Var:
                    return dialect switch
                    {
                        SqlDialect.SqlServer => $"VAR({num})",
                        SqlDialect.PostgreSql => $"VAR_SAMP({num})",
                        _ => SqliteVariance(num, f)
                    };
                case StatisticType.StdDev:
                    return dialect switch
                    {
                        SqlDialect.SqlServer => $"STDEV({num})",
                        SqlDialect.PostgreSql => $"STDDEV_SAMP({num})",
                        _ => $"SQRT({SqliteVariance(num, f)})"
                    };
                default:
                    throw new NotSupportedException($"Statistic type {statistic.Type} is not supported");
            }
        }

        /// <summary>SQLite has no VAR/STDDEV: sample variance from sum, sum of squares and count (NULL for n &lt; 2).</summary>
        static private string SqliteVariance(string num, string field)
            => $"CASE WHEN COUNT({field}) > 1 THEN MAX(0.0, (SUM({num} * {num}) - SUM({num}) * SUM({num}) / COUNT({field})) / (COUNT({field}) - 1)) END";

        #endregion
    }
}
