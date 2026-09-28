using System;
using System.Data;

namespace gView.Framework.Data.Extensions;

public static class DataRowExtensions
{
    /// <summary>
    /// Reads the "AllowDBNull" column of a DbDataReader.GetSchemaTable() row.
    /// Returns true (nullable) if the provider does not report this information.
    /// </summary>
    public static bool IsNullableColumn(this DataRow schemaRow)
    {
        if (schemaRow?.Table?.Columns.Contains("AllowDBNull") != true)
        {
            return true;
        }

        object allowDbNull = schemaRow["AllowDBNull"];
        if (allowDbNull == null || allowDbNull == DBNull.Value)
        {
            return true;
        }

        return Convert.ToBoolean(allowDbNull);
    }
}
