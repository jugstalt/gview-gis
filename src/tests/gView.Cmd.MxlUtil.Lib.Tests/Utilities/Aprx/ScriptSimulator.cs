using System.Globalization;
using System.Text.RegularExpressions;
using gView.Framework.Common;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Simulates the two steps <c>SimpleLabelRenderer.Draw</c> performs at runtime for a converted
/// gView label expression: substitute every <c>[Field]</c> / <c>[Field:Format]</c> placeholder
/// with its value, then run the result through <see cref="SimpleScriptInterpreter"/> if it's a
/// "@@start" mini-script.
/// </summary>
internal static class ScriptSimulator
{
    public static string Evaluate(string expression, params (string Field, string? Value)[] fieldValues)
    {
        var expr = expression;
        foreach (var (field, value) in fieldValues)
        {
            // Plain "[Field]" placeholder: bare value substitution.
            expr = expr.Replace($"[{field}]", value ?? "");

            // "[Field:Format]" placeholder: mirrors ExpressionExtensions.EvaluateExpression in
            // gView.Framework.Cartography.Rendering - if the value parses as a number (trying the
            // invariant culture first, then the current one, since a real field's numeric
            // ToString() uses the current culture, e.g. "125,4" under a German locale), format it
            // via IFormattable; otherwise fall back to the raw value.
            expr = Regex.Replace(expr, $@"\[{Regex.Escape(field)}:(?<format>[^\]]+)\]", m =>
            {
                if (value != null && TryParseNumber(value, out var d))
                {
                    return d.ToString(m.Groups["format"].Value, CultureInfo.InvariantCulture);
                }
                return value ?? "";
            });
        }

        return SimpleScriptInterpreter.IsSimpleScript(expr)
            ? new SimpleScriptInterpreter(expr).Interpret()
            : expr;
    }

    private static bool TryParseNumber(string s, out double value) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
        || double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
}
