using gView.Framework.Common;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Simulates the two steps <c>SimpleLabelRenderer.Draw</c> performs at runtime for a converted
/// gView label expression: substitute every <c>[Field]</c> placeholder with its value, then run
/// the result through <see cref="SimpleScriptInterpreter"/> if it's a "@@start" mini-script.
/// Field-value substitution here is a plain string replace (no <c>[Field:Format]</c> handling) -
/// that formatting step lives in a different assembly and isn't part of what
/// <c>AprxLabelExpressionParser</c> itself is responsible for.
/// </summary>
internal static class ScriptSimulator
{
    public static string Evaluate(string expression, params (string Field, string? Value)[] fieldValues)
    {
        var expr = expression;
        foreach (var (field, value) in fieldValues)
        {
            expr = expr.Replace($"[{field}]", value ?? "");
        }

        return SimpleScriptInterpreter.IsSimpleScript(expr)
            ? new SimpleScriptInterpreter(expr).Interpret()
            : expr;
    }
}
