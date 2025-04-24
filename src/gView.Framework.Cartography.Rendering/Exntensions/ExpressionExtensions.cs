#nullable enable

using gView.Framework.Core.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace gView.Framework.Cartography.Rendering.Exntensions;
internal static class ExpressionExtensions
{
    // Regex kompile once → fast
    private static readonly Regex Placeholder =
        new Regex(@"\[(?<name>[^\]:]+)(:(?<format>[^\]]+))?\]",
                  RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string EvaluateExpression(this string expression, FieldValue field)
    {
        if (string.IsNullOrEmpty(expression) || field == null)
        {
            return expression;
        }

        if(!expression.Contains($"[{field.Name}", StringComparison.OrdinalIgnoreCase))
        {
            return expression;
        }

        if (!expression.Contains($"[{field.Name}:", StringComparison.OrdinalIgnoreCase))
        {
            // simple Expression => simple Replace
            return expression.Replace($"[{field.Name}]", field.Value?.ToString() ?? "");
        }

        return Placeholder.Replace(expression, m =>
        {
            // [FELD1] bzw. [FELD1:0.00]
            var name = m.Groups["name"].Value;

            // Name not fits? → keep original string
            if (!name.Equals(field.Name, StringComparison.OrdinalIgnoreCase))
            {
                return m.Value;
            }

            var format = m.Groups["format"].Success
                ? m.Groups["format"].Value
                : null;

            var value = field.Value;

            if (value == null)
            {
                return string.Empty;
            }

            // format, if posible
            if (format != null && value is IFormattable formattable)
            {
                return formattable.ToString(format, CultureInfo.CurrentCulture);
            }

            return value?.ToString() ?? "";
        });
    }

    private static readonly Regex FieldRegex =
        new Regex(@"\[(?<name>[^\]:]+)(:[^\]]+)?\]",
                  RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static IReadOnlyList<string> ExtractFieldNames(
        this string expression,
        bool distinct = true,
        StringComparer? comparer = null)
    {
        if (string.IsNullOrEmpty(expression))
            return Array.Empty<string>();

        var names = FieldRegex
            .Matches(expression)
            .Select(m => m.Groups["name"].Value);

        if (!distinct)
            return names.ToArray();

        comparer ??= StringComparer.OrdinalIgnoreCase;
        return names.Distinct(comparer).ToArray();
    }
}
