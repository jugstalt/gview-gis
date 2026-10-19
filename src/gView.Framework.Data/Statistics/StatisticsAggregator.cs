using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Framework.Data.Statistics
{
    /// <summary>
    /// In-memory implementation of <see cref="StatisticsQuery"/> (count, sum, min, max, avg, stddev, var,
    /// optionally grouped). Used for all table classes that do not implement
    /// <see cref="IStatisticsTableClass"/>. NULL values are ignored by every statistic (like SQL).
    /// </summary>
    public class StatisticsAggregator
    {
        private readonly StatisticsQuery _query;
        private readonly Dictionary<object[], Accumulator[]> _groups = new(new KeyComparer());
        private long _scanned = 0;

        public StatisticsAggregator(StatisticsQuery query)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
        }

        public long ScannedRecords => _scanned;

        /// <param name="valueOf">Returns the value of a field of the current source row (null/DBNull allowed).</param>
        public void Add(Func<string, object> valueOf)
        {
            if (_query.MaxScanRecords > 0 && _scanned >= _query.MaxScanRecords)
            {
                throw new MapServerException(
                    $"Statistics query exceeds the maximum number of records that can be scanned ({_query.MaxScanRecords}). Use a more selective where clause.");
            }
            _scanned++;

            var key = _query.GroupByFields.Select(f => Normalize(valueOf(f))).ToArray();
            if (!_groups.TryGetValue(key, out var accumulators))
            {
                accumulators = _query.Statistics.Select(s => new Accumulator(s.Type)).ToArray();
                _groups.Add(key, accumulators);
            }

            for (int i = 0; i < accumulators.Length; i++)
            {
                accumulators[i].Add(Normalize(valueOf(_query.Statistics[i].OnField)));
            }
        }

        public StatisticsResult GetResult()
        {
            var result = new StatisticsResult();
            result.Columns = CreateColumns(_query);

            // without group by there is always exactly one row (count = 0, others null)
            if (_groups.Count == 0 && _query.GroupByFields.Count == 0)
            {
                _groups.Add(Array.Empty<object>(), _query.Statistics.Select(s => new Accumulator(s.Type)).ToArray());
            }

            var rows = new List<IDictionary<string, object>>();
            foreach (var group in _groups)
            {
                var row = new Dictionary<string, object>(StringComparer.Ordinal);
                for (int i = 0; i < _query.GroupByFields.Count; i++)
                {
                    row[_query.GroupByFields[i]] = group.Key[i];
                }
                for (int i = 0; i < _query.Statistics.Count; i++)
                {
                    row[_query.Statistics[i].OutName] = group.Value[i].Result();
                }
                rows.Add(row);
            }

            // deterministic order (important for paging): requested order, else group fields ascending
            var order = _query.OrderBy.Count > 0
                ? _query.OrderBy.ToList()
                : _query.GroupByFields.Select(f => new StatisticsOrderBy() { Name = f }).ToList();

            if (order.Count > 0)
            {
                IOrderedEnumerable<IDictionary<string, object>> ordered = null;
                foreach (var o in order)
                {
                    var name = o.Name;
                    Func<IDictionary<string, object>, object> keySelector = r => r.TryGetValue(name, out var v) ? v : null;
                    ordered = ordered == null
                        ? (o.Descending ? rows.OrderByDescending(keySelector, ValueComparer.Instance) : rows.OrderBy(keySelector, ValueComparer.Instance))
                        : (o.Descending ? ordered.ThenByDescending(keySelector, ValueComparer.Instance) : ordered.ThenBy(keySelector, ValueComparer.Instance));
                }
                rows = ordered.ToList();
            }

            if (_query.Offset > 0)
            {
                rows = rows.Skip(_query.Offset).ToList();
            }
            if (_query.Limit > 0 && rows.Count > _query.Limit)
            {
                result.ExceededLimit = true;
                rows = rows.Take(_query.Limit).ToList();
            }

            result.Rows = rows;
            return result;
        }

        /// <summary>
        /// Result columns of <paramref name="query"/>: group-by columns first, then the statistics.
        /// Shared with the SQL based implementations so every implementation reports the same schema.
        /// </summary>
        static public IList<StatisticsColumn> CreateColumns(StatisticsQuery query)
        {
            var columns = new List<StatisticsColumn>();

            for (int i = 0; i < query.GroupByFields.Count; i++)
            {
                columns.Add(new StatisticsColumn()
                {
                    Name = query.GroupByFields[i],
                    Alias = query.GroupByFields[i],
                    Type = i < query.GroupByFieldTypes.Count ? query.GroupByFieldTypes[i] : FieldType.unknown
                });
            }
            foreach (var statistic in query.Statistics)
            {
                columns.Add(new StatisticsColumn()
                {
                    Name = statistic.OutName,
                    Alias = statistic.OutName,
                    Type = OutputType(statistic)
                });
            }

            return columns;
        }

        /// <summary>
        /// Reads all rows matching <paramref name="filter"/> from <paramref name="tableClass"/> and aggregates them.
        /// </summary>
        async static public Task<StatisticsResult> ExecuteAsync(ITableClass tableClass, IQueryFilter filter, StatisticsQuery query)
        {
            var aggregator = new StatisticsAggregator(query);

            var fields = query.GroupByFields
                .Concat(query.Statistics.Select(s => s.OnField))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            filter.SubFields = String.Join(",", fields);
            filter.BeginRecord = 1;
            filter.OrderBy = null;
            // one record more than allowed, so that exceeding the limit is detected by the aggregator
            filter.Limit = query.MaxScanRecords > 0 && query.MaxScanRecords < int.MaxValue
                ? query.MaxScanRecords + 1
                : int.MaxValue;

            using (var cursor = await tableClass.Search(filter))
            {
                if (cursor is IFeatureCursor featureCursor)
                {
                    IFeature feature;
                    while ((feature = await featureCursor.NextFeature()) != null)
                    {
                        var f = feature;
                        aggregator.Add(name => f.FindField(name)?.Value);
                    }
                }
                else if (cursor is IRowCursor rowCursor)
                {
                    IRow row;
                    while ((row = await rowCursor.NextRow()) != null)
                    {
                        var r = row;
                        aggregator.Add(name => r.FindField(name)?.Value);
                    }
                }
                else
                {
                    throw new MapServerException("Statistics are not supported for this table class.");
                }
            }

            return aggregator.GetResult();
        }

        #region Helper

        private static FieldType OutputType(StatisticDefinition statistic) => statistic.Type switch
        {
            StatisticType.Count => FieldType.integer,
            StatisticType.Min or StatisticType.Max => statistic.OnFieldType,
            _ => FieldType.Double
        };

        private static object Normalize(object value) => value is DBNull ? null : value;

        private class Accumulator
        {
            private readonly StatisticType _type;
            private long _count;
            private double _sum;
            private double _mean;
            private double _m2;     // sum of squared differences from the mean (Welford)
            private object _extreme;

            public Accumulator(StatisticType type) { _type = type; }

            public void Add(object value)
            {
                if (value == null)
                {
                    return;
                }

                _count++;

                switch (_type)
                {
                    case StatisticType.Count:
                        break;
                    case StatisticType.Min:
                        if (_extreme == null || ValueComparer.Instance.Compare(value, _extreme) < 0) { _extreme = value; }
                        break;
                    case StatisticType.Max:
                        if (_extreme == null || ValueComparer.Instance.Compare(value, _extreme) > 0) { _extreme = value; }
                        break;
                    default:
                        double d = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        _sum += d;
                        double delta = d - _mean;
                        _mean += delta / _count;
                        _m2 += delta * (d - _mean);
                        break;
                }
            }

            public object Result()
            {
                switch (_type)
                {
                    case StatisticType.Count: return _count;
                    case StatisticType.Sum: return _count > 0 ? _sum : null;
                    case StatisticType.Avg: return _count > 0 ? _sum / _count : null;
                    case StatisticType.Min:
                    case StatisticType.Max: return _extreme;
                    case StatisticType.Var: return _count > 1 ? _m2 / (_count - 1) : null;
                    case StatisticType.StdDev: return _count > 1 ? Math.Sqrt(_m2 / (_count - 1)) : null;
                    default: return null;
                }
            }
        }

        private class KeyComparer : IEqualityComparer<object[]>
        {
            public bool Equals(object[] x, object[] y)
            {
                if (x.Length != y.Length) { return false; }
                for (int i = 0; i < x.Length; i++)
                {
                    if (!Object.Equals(x[i], y[i])) { return false; }
                }
                return true;
            }

            public int GetHashCode(object[] obj)
            {
                var hash = new HashCode();
                foreach (var o in obj) { hash.Add(o); }
                return hash.ToHashCode();
            }
        }

        /// <summary>NULL first, same types by IComparable, other numerics as double, otherwise as string.</summary>
        private class ValueComparer : IComparer<object>
        {
            public static readonly ValueComparer Instance = new();

            public int Compare(object x, object y)
            {
                if (x == null || y == null)
                {
                    return x == null ? (y == null ? 0 : -1) : 1;
                }

                if (x.GetType() == y.GetType() && x is IComparable c)
                {
                    return c.CompareTo(y);
                }

                if (IsNumeric(x) && IsNumeric(y))
                {
                    return Convert.ToDouble(x, CultureInfo.InvariantCulture)
                        .CompareTo(Convert.ToDouble(y, CultureInfo.InvariantCulture));
                }

                return String.CompareOrdinal(Convert.ToString(x, CultureInfo.InvariantCulture),
                                             Convert.ToString(y, CultureInfo.InvariantCulture));
            }

            private static bool IsNumeric(object o) => o is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
        }

        #endregion
    }
}
