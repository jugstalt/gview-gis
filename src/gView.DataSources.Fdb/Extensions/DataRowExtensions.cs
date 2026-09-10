using System;
using System.Data;
using System.Globalization;

namespace gView.DataSources.Fdb.Extensions;

/// <summary>
/// Tolerant reads for optional / nullable <see cref="DataRow"/> columns. A plain cast
/// (<c>(double)row["x"]</c>) or <see cref="Convert"/> throws <see cref="InvalidCastException"/>
/// on <see cref="DBNull"/>; these return the fallback instead. A column that is absent from
/// the table is treated the same as a <c>DBNull</c> value.
/// </summary>
internal static class DataRowExtensions
{
    /// <summary>
    /// Reads <paramref name="column"/> as a <see cref="double"/>. Returns <c>false</c> (and
    /// <paramref name="value"/> = 0) when the column is missing, <see cref="DBNull"/> or not
    /// convertible.
    /// </summary>
    public static bool TryGetDouble(this DataRow row, string column, out double value)
    {
        value = 0d;

        if (row is null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
        {
            return false;
        }

        try
        {
            value = Convert.ToDouble(row[column], CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Reads <paramref name="column"/> as a <see cref="double"/>, or <paramref name="fallback"/>.</summary>
    public static double GetDouble(this DataRow row, string column, double fallback)
        => row.TryGetDouble(column, out double value) ? value : fallback;

    /// <summary>Reads <paramref name="column"/> as an <see cref="int"/>, or <paramref name="fallback"/>.</summary>
    public static int GetInt32(this DataRow row, string column, int fallback)
        => row.TryGetDouble(column, out double value) ? (int)value : fallback;
}
