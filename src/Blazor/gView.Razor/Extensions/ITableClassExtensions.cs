using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Data.Filters;

namespace gView.Razor.Extensions;

/// <summary>
/// Row counts of table/feature classes (attribute table status line, layer statistics in Carto).
/// <see cref="IFeatureClass.CountFeatures"/> is not used: many classes return 0, -1 or
/// int.MaxValue there and it knows no where clause.
/// </summary>
public static class ITableClassExtensions
{
    /// <summary>
    /// Counts the rows matching <paramref name="whereClause"/> only if the class can count on
    /// its own (e.g. SQL <c>COUNT(*)</c>, <see cref="ITableClass2"/>).
    /// </summary>
    /// <returns>The count, or null if the class can not count without reading all rows</returns>
    public static async Task<long?> TryCountRowsFastAsync(this ITableClass tableClass, string? whereClause)
        => tableClass is ITableClass2 tableClass2
            ? await tableClass2.ExecuteCount(new QueryFilter() { WhereClause = whereClause ?? "" })
            : null;

    /// <summary>
    /// Counts the rows matching <paramref name="whereClause"/>: fast if the class supports it,
    /// otherwise by reading all matching rows (only the id field). Can be expensive for big
    /// classes or web services.
    /// </summary>
    public static async Task<long> CountRowsAsync(this ITableClass tableClass, string? whereClause)
    {
        var fastCount = await tableClass.TryCountRowsFastAsync(whereClause);
        if (fastCount.HasValue)
        {
            return fastCount.Value;
        }

        var filter = new QueryFilter() { WhereClause = whereClause ?? "" };
        if (!String.IsNullOrEmpty(tableClass.IDFieldName))
        {
            filter.SubFields = tableClass.IDFieldName;
        }
        else
        {
            filter.SubFields = "*";
        }

        long count = 0;
        using (var cursor = await tableClass.Search(filter))
        {
            switch (cursor)
            {
                case IFeatureCursor featureCursor:
                    while (await featureCursor.NextFeature() is not null) { count++; }
                    break;
                case IRowCursor rowCursor:
                    while (await rowCursor.NextRow() is not null) { count++; }
                    break;
                default:
                    throw new NotSupportedException("Counting is not supported for this class");
            }
        }

        return count;
    }
}
