using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxLabelExpressionParser"/>'s Python-flavored <c>def name(...): ...</c>
/// handling - one of the label-expression types ArcGIS Pro supports alongside VBScript and
/// Arcade. Conditions and branch-guard reconstruction reuse the same machinery as the VB
/// <c>Function</c> path (see <see cref="AprxLabelExpressionParserConditionalTests"/>); these
/// tests focus on what's specific to Python: indentation-delimited blocks, "\n"/"\t" escape
/// decoding, and <c>int()</c>/<c>str()</c>/<c>is None</c> syntax.
/// </summary>
public class AprxLabelExpressionParserPythonTests
{
    private static AprxLabelExpressionParser.ConversionResult Convert(string source)
    {
        var ok = AprxLabelExpressionParser.TryConvert(source, out var result);
        Assert.True(ok, $"Expected the expression to convert successfully, but it was rejected:\n{source}");
        return result!;
    }

    private static void AssertRejected(string source)
    {
        var ok = AprxLabelExpressionParser.TryConvert(source, out var result);
        Assert.False(ok, $"Expected the expression to be rejected, but it converted to:\n{result?.Expression}");
    }

    [Fact]
    public void BareReturn_NoIf_ReducesToConcatenation()
    {
        var result = Convert("def F([A],[B]):\n    return [A] + \" \" + [B]");

        Assert.Equal("[A] [B]", result.Expression);
        Assert.False(result.IsConditional);
    }

    [Fact]
    public void BareReturn_EmptyLiteral_ReducesToEmptyString()
    {
        var result = Convert("def F([A]):\n    return \"\"");

        Assert.Equal("", result.Expression);
    }

    [Fact]
    public void IfElse_BuildsConditionalScript()
    {
        const string source = """
            def FindLabel([GERAETE_ANZAHL], [BESCHR_1]):
                if int([GERAETE_ANZAHL]) > 1:
                    return [BESCHR_1]
                else:
                    return ""
            """;
        var result = Convert(source);
        Assert.True(result.IsConditional);

        Assert.Equal("Text", ScriptSimulator.Evaluate(result.Expression, ("GERAETE_ANZAHL", "2"), ("BESCHR_1", "Text")));
        Assert.Equal("", ScriptSimulator.Evaluate(result.Expression, ("GERAETE_ANZAHL", "1"), ("BESCHR_1", "Text")));
    }

    [Fact]
    public void If_NoElse_ReturnsEmptyWhenConditionFalse()
    {
        const string source = """
            def F([A]):
                if [A] > 1:
                    return "x"
            """;
        var script = Convert(source).Expression;

        Assert.Equal("x", ScriptSimulator.Evaluate(script, ("A", "2")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "1")));
        Assert.Equal("", ScriptSimulator.Evaluate(script, ("A", "")));
    }

    [Fact]
    public void ElifChain_MatchesOriginalPythonSemantics()
    {
        const string source = """
            def F([TYP]):
                if [TYP] == "Schieber":
                    return "S"
                elif [TYP] == "Ventil":
                    return "V"
                else:
                    return [TYP]
            """;
        var script = Convert(source).Expression;

        Assert.Equal("S", ScriptSimulator.Evaluate(script, ("TYP", "Schieber")));
        Assert.Equal("V", ScriptSimulator.Evaluate(script, ("TYP", "Ventil")));
        Assert.Equal("Hydrant", ScriptSimulator.Evaluate(script, ("TYP", "Hydrant")));
    }

    // -----------------------------------------------------------------------
    // is None / is not None
    // -----------------------------------------------------------------------

    [Fact]
    public void IsNotNone_MatchesOriginalPythonSemantics()
    {
        const string source = """
            def FindLabel([E_TP], [PLTXT]):
                if [PLTXT] is not None:
                    return [E_TP] + "\n" + [PLTXT]
                else:
                    return [E_TP]
            """;
        var script = Convert(source).Expression;

        Assert.Equal($"E1{Environment.NewLine}Text", ScriptSimulator.Evaluate(script, ("E_TP", "E1"), ("PLTXT", "Text")));
        Assert.Equal("E1", ScriptSimulator.Evaluate(script, ("E_TP", "E1"), ("PLTXT", "")));
    }

    [Fact]
    public void IsNone_MatchesOriginalPythonSemantics()
    {
        const string source = """
            def F([A]):
                if [A] is None:
                    return "empty"
                else:
                    return [A]
            """;
        var script = Convert(source).Expression;

        Assert.Equal("empty", ScriptSimulator.Evaluate(script, ("A", "")));
        Assert.Equal("x", ScriptSimulator.Evaluate(script, ("A", "x")));
    }

    // -----------------------------------------------------------------------
    // "\n"/"\t" escape decoding (only within a Python "def" body) and numeric/string casts
    // -----------------------------------------------------------------------

    [Fact]
    public void BackslashN_InSingleQuotedString_BecomesLineBreak()
    {
        var result = Convert("def F([A],[B]):\n    return [A] + '\\n' + [B]");

        Assert.Equal($"[A]{Environment.NewLine}[B]", result.Expression);
    }

    [Fact]
    public void BackslashN_InDoubleQuotedString_BecomesLineBreak()
    {
        var result = Convert("def F([A],[B]):\n    return [A] + \"\\n\" + [B]");

        Assert.Equal($"[A]{Environment.NewLine}[B]", result.Expression);
    }

    [Fact]
    public void BackslashT_BecomesTabCharacter()
    {
        var result = Convert("def F([A]):\n    return 'x\\ty'");

        Assert.Equal("x\ty", result.Expression);
    }

    [Fact]
    public void StrCast_UnwrapsToPlainField()
    {
        var result = Convert("def F([N_TP],[STPKT_NR]):\n    return [N_TP] + str([STPKT_NR])");

        Assert.Equal("[N_TP][STPKT_NR]", result.Expression);
    }

    [Fact]
    public void IntCastInCondition_UnwrapsForNumericComparison()
    {
        var result = Convert("def F([A]):\n    if int([A]) > 1:\n        return \"big\"");

        Assert.Equal("big", ScriptSimulator.Evaluate(result.Expression, ("A", "5")));
        Assert.Equal("", ScriptSimulator.Evaluate(result.Expression, ("A", "1")));
    }

    // -----------------------------------------------------------------------
    // Rejections: still a deliberately small subset
    // -----------------------------------------------------------------------

    [Fact]
    public void MultiStatementBody_IsRejected()
    {
        AssertRejected("""
            def F([A]):
                if [A] > 1:
                    x = 1
                    return "big"
            """);
    }

    [Fact]
    public void NestedIf_IsRejected()
    {
        AssertRejected("""
            def F([A],[B]):
                if [A] > 1:
                    if [B] > 1:
                        return "both"
            """);
    }

    [Fact]
    public void UnsupportedFirstStatement_IsRejected()
    {
        AssertRejected("def F([A]):\n    x = [A]\n    return x");
    }
}
