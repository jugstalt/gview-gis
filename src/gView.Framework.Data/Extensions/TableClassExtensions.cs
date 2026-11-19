#nullable enable

using gView.Framework.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Checks field names from a public request (outFields, subfields, SLD property names, ...):
    /// every name must be a field of the table (case-insensitive) and is returned with the
    /// table's spelling. "*" and the names in <paramref name="passThrough"/> are returned unchanged,
    /// empty names are skipped. Field functions (e.g. "SHAPE.STArea()") are only allowed if the
    /// table defines a field with exactly this name.
    /// </summary>
    static public IEnumerable<string> ProjectRequestFieldNames(this ITableClass tableClass, IEnumerable<string>? fieldNames, params string[] passThrough)
    {
        var result = new List<string>();

        foreach (var name in (fieldNames ?? []).Select(n => n?.Trim() ?? ""))
        {
            if (name.Length == 0)
            {
                continue;
            }
            if (name == "*" || passThrough.Contains(name))
            {
                result.Add(name);
                continue;
            }

            var field = tableClass.FindFieldIgnoreCase(name);
            if (field == null)
            {
                throw new SqlDangerousStatementExceptions(
                    name.Any(c => c == '(' || c == ')' || c == '.' || c == ';' || Char.IsWhiteSpace(c))
                        ? $"Forbidden field function detected: {name}"
                        : $"Field '{name}' not contained in table");
            }

            result.Add(field.name);
        }

        return result;
    }

    /// <summary>
    /// Checks an order by clause from a public request: a comma separated list of
    /// "field [ASC|DESC]" where each field must be a field of the table (case-insensitive).
    /// Returns the normalized clause, e.g. "NAME ASC,OBJECTID DESC".
    /// </summary>
    static public string ProjectRequestOrderBy(this ITableClass tableClass, string? orderBy)
    {
        if (String.IsNullOrWhiteSpace(orderBy))
        {
            return String.Empty;
        }

        var items = new List<string>();

        foreach (var item in orderBy.Split(','))
        {
            var parts = item.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);  // any whitespace
            if (parts.Length == 0 || parts.Length > 2)
            {
                throw new SqlDangerousStatementExceptions($"Invalid order by item '{item.Trim()}'");
            }

            var fieldName = tableClass.ProjectRequestFieldNames([parts[0]]).Single();
            if (fieldName == "*")
            {
                throw new SqlDangerousStatementExceptions("Invalid order by item '*'");
            }

            if (parts.Length == 2)
            {
                var direction = parts[1].ToUpperInvariant();
                if (direction != "ASC" && direction != "DESC")
                {
                    throw new SqlDangerousStatementExceptions($"Invalid order by direction '{parts[1]}'");
                }
                items.Add($"{fieldName} {direction}");
            }
            else
            {
                items.Add(fieldName);
            }
        }

        return String.Join(",", items);
    }
}
