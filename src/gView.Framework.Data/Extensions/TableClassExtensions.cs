#nullable enable

using gView.Framework.Core.Data;
using System;

namespace gView.Framework.Data.Extensions;

static public class TableClassExtensions
{
    /// <summary>
    /// Finds a field by name: an exact match first, otherwise the first field whose name
    /// matches ignoring case (e.g. feature field "NAME" for a database column "name").
    /// </summary>
    static public IField? FindFieldIgnoreCase(this ITableClass tableClass, string name)
    {
        if (tableClass == null || String.IsNullOrEmpty(name))
        {
            return null;
        }

        var field = tableClass.FindField(name);
        if (field != null || tableClass.Fields == null)
        {
            return field;
        }

        foreach (var candidate in tableClass.Fields.ToEnumerable())
        {
            if (name.Equals(candidate.name, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return null;
    }
}
