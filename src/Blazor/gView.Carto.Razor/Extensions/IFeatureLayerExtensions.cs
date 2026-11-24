using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Statistics;
using gView.Framework.Cartography.Rendering.Classification;
using gView.Framework.Data.Filters;
using gView.Framework.Data.Statistics;
using gView.Razor.Extensions;

namespace gView.Carto.Razor.Extensions;

/// <summary>
/// Feature counts for the classes of the class renderers (value map, many value map, quantity).
/// All counts respect the layer filter (<see cref="IFeatureLayer.FilterQuery"/>).
/// </summary>
static internal class IFeatureLayerExtensions
{
    private const string CountOutName = "gv_count";

    /// <summary>
    /// Number of features of the layer's feature class, with or without the layer filter.
    /// Unlike the other counts here this one can ignore the layer filter (Source page shows both).
    /// Reads all features if the class can not count on its own.
    /// </summary>
    static public Task<long> CountFeaturesAsync(this IFeatureLayer layer, bool withLayerFilter)
    {
        var featureClass = layer.FeatureClass ?? throw new ArgumentException("Layer has no feature class");

        return featureClass.CountRowsAsync(withLayerFilter ? layer.FilterQuery?.WhereClause : null);
    }

    /// <summary>
    /// Distinct values of <paramref name="field"/> with their feature counts, ordered by value.
    /// Runs as one grouped statistics query (in the database, if the feature class supports it).
    /// The key is the value as string, like the value map renderer builds it.
    /// </summary>
    static public async Task<List<KeyValuePair<string, long>>> CountByFieldAsync(this IFeatureLayer layer, string field)
    {
        var featureClass = layer.FeatureClass ?? throw new ArgumentException("Layer has no feature class");
        var fieldDef = featureClass.FindField(field) ?? throw new ArgumentException($"Unknown field {field}");

        var countField = !String.IsNullOrEmpty(featureClass.IDFieldName)
            ? featureClass.IDFieldName
            : field;

        var query = new StatisticsQuery()
        {
            GroupByFields = [fieldDef.name],
            GroupByFieldTypes = [fieldDef.type],
            Statistics =
            [
                new StatisticDefinition()
                {
                    Type = StatisticType.Count,
                    OnField = countField,
                    OnFieldType = featureClass.FindField(countField)?.type ?? FieldType.ID,
                    OutName = CountOutName
                }
            ],
            OrderBy = [new StatisticsOrderBy() { Name = fieldDef.name }]
        };

        var filter = layer.CreateCountFilter();
        var result = featureClass is IStatisticsTableClass statisticsTableClass
            ? await statisticsTableClass.ExecuteStatistics(filter, query)
            : await StatisticsAggregator.ExecuteAsync(featureClass, filter, query);

        var counts = new Dictionary<string, long>();
        var ordered = new List<string>();
        foreach (var row in result.Rows)
        {
            var key = row.ValueOf(fieldDef.name)?.ToString() ?? "";
            var count = Convert.ToInt64(row.ValueOf(CountOutName) ?? 0L);

            if (!counts.TryAdd(key, count))
            {
                counts[key] += count;   // e.g. null and "" end up in the same key
            }
            else
            {
                ordered.Add(key);
            }
        }

        return ordered.Select(k => new KeyValuePair<string, long>(k, counts[k])).ToList();
    }

    /// <summary>
    /// Feature counts per key: reads the <paramref name="fields"/> of all features and builds
    /// the key with <paramref name="keyOf"/> (e.g. the many value map renderer key).
    /// </summary>
    static public async Task<Dictionary<string, long>> CountByKeyAsync(this IFeatureLayer layer,
                                                                      IEnumerable<string> fields,
                                                                      Func<IFeature, string> keyOf)
    {
        var counts = new Dictionary<string, long>();

        await layer.ScanAsync(fields, feature =>
        {
            var key = keyOf(feature) ?? "";
            counts[key] = counts.TryGetValue(key, out long count) ? count + 1 : 1;
        });

        return counts;
    }

    /// <summary>
    /// Feature counts per range, like the quantity renderer assigns features:
    /// the first range with min &lt;= value &lt;= max gets the feature.
    /// </summary>
    /// <returns>One count per range (same order); features in no range are not counted</returns>
    static public async Task<long[]> CountByRangesAsync(this IFeatureLayer layer,
                                                       string field,
                                                       IReadOnlyList<(double Min, double Max)> ranges)
        => ClassBreaks.CountByRanges(await layer.ReadNumericValuesAsync(field), ranges);

    /// <summary>
    /// All numeric values of <paramref name="field"/> (NULL and non numeric values are skipped).
    /// </summary>
    static public async Task<List<double>> ReadNumericValuesAsync(this IFeatureLayer layer, string field)
    {
        var values = new List<double>();

        await layer.ScanAsync([field], feature =>
        {
            var value = feature[field];
            if (value is null || value is DBNull)
            {
                return;
            }

            try
            {
                values.Add(Convert.ToDouble(value));
            }
            catch
            {
                // not numeric => like the renderer: no class
            }
        });

        return values;
    }

    #region Helper

    static private QueryFilter CreateCountFilter(this IFeatureLayer layer)
        => new QueryFilter()
        {
            WhereClause = layer.FilterQuery?.WhereClause ?? ""
        };

    static private async Task ScanAsync(this IFeatureLayer layer, IEnumerable<string> fields, Action<IFeature> onFeature)
    {
        var featureClass = layer.FeatureClass ?? throw new ArgumentException("Layer has no feature class");

        var filter = layer.CreateCountFilter();
        foreach (var field in fields.Where(f => !String.IsNullOrEmpty(f)).Distinct())
        {
            filter.AddField(field);
        }

        using (var cursor = await featureClass.Search(filter) as IFeatureCursor)
        {
            if (cursor is null)
            {
                return;
            }

            IFeature feature;
            while ((feature = await cursor.NextFeature()) is not null)
            {
                onFeature(feature);
            }
        }
    }

    static private object? ValueOf(this IDictionary<string, object> row, string name)
        => row.TryGetValue(name, out var value)
            ? value
            : row.FirstOrDefault(kv => kv.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;

    #endregion
}
