#nullable enable

using gView.Framework.Common;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

static public class AttributeFilterExtensions
{
    /// <summary>
    /// The where clause with its parameters as SQL literals: <c>@name</c> in the clause is the parameter <c>name</c>
    /// (the key may also be written as <c>@name</c>, as in the specification's examples both forms occur). Numbers and
    /// booleans become plain literals, everything else a string literal with doubled quotes. Only whole names are
    /// replaced, so <c>@id</c> does not touch <c>@id2</c>.
    /// </summary>
    static public string? WhereClauseWithResolveParameters(this AttributeFilter filter)
    {
        if (string.IsNullOrEmpty(filter.WhereClause)
            || filter.Parameters?.Any() != true)
        {
            return filter.WhereClause;
        }

        var values = filter.Parameters.ToDictionary(
            p => p.Key.TrimStart('@'),
            p => Literal(p.Key, p.Value),
            System.StringComparer.OrdinalIgnoreCase);

        // string literals ('...', quotes doubled) are kept as they are: 'a@b.com' is no parameter
        return Regex.Replace(filter.WhereClause, @"'(?:[^']|'')*'|@([A-Za-z_][A-Za-z0-9_]*)\b", match =>
            match.Groups[1].Success && values.TryGetValue(match.Groups[1].Value, out var literal)
                ? literal
                : match.Value);
    }

    private static string Literal(string name, object? value)
        => value switch
        {
            null => "NULL",
            JsonElement json => json.ValueKind switch
            {
                JsonValueKind.Number => json.GetDouble().ToDoubleString(),
                JsonValueKind.True => "1",
                JsonValueKind.False => "0",
                JsonValueKind.Null => "NULL",
                JsonValueKind.String => StringLiteral(json.GetString()),
                _ => throw new MapServerException($"The parameter {name} must be a number, a string or a boolean")
            },
            bool b => b ? "1" : "0",
            byte or short or int or long or ushort or uint or ulong => System.Convert.ToString(value, CultureInfo.InvariantCulture)!,
            float n => n.ToFloatString(),
            double n => n.ToDoubleString(),
            decimal n => n.ToDecimalString(),
            _ => StringLiteral(value.ToString())
        };

    private static string StringLiteral(string? text) => $"'{text?.Replace("'", "''")}'";  // SQL string literal: quotes doubled
}
