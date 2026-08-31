using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxLabelExpressionParser"/>'s support of ArcGIS Pro's per-branch
/// <c>&lt;CLR red='R' green='G' blue='B'&gt;...&lt;/CLR&gt;</c> rich-text color tag: when a whole
/// branch's output is wrapped in exactly one such tag, the parser now produces a parallel
/// <see cref="AprxLabelExpressionParser.ConversionResult.ColorExpression"/> (a gView expression
/// that resolves to <c>"rgb(R,G,B)"</c>) alongside the usual text <c>Expression</c>, instead of
/// just stripping the tag to plain text. As with every other conditional conversion, every test
/// round-trips the generated <c>ColorExpression</c> through the real
/// <c>SimpleScriptInterpreter</c> (via <see cref="ScriptSimulator"/>) rather than asserting exact
/// generated text - the merge/guard structure is already exhaustively covered for text elsewhere;
/// what's new here is that the exact same machinery, fed a color instead of text, still evaluates
/// correctly.
/// </summary>
public class AprxLabelExpressionParserColorTests
{
    private static AprxLabelExpressionParser.ConversionResult Convert(string source)
    {
        var ok = AprxLabelExpressionParser.TryConvert(source, out var result);
        Assert.True(ok, $"Expected the expression to convert successfully, but it was rejected:\n{source}");
        return result!;
    }

    // -----------------------------------------------------------------------
    // The user's real-world example (gas_ld_sdep.aprx): color depends only on [SA1], text only on
    // [SA2] - two independent condition structures over the same two fields.
    // -----------------------------------------------------------------------

    private const string GasleitungenSource = """
        Function FindLabel ([SA4],[SA1],[SA2],[GDR],[SA3])

        If [SA1] ="N" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='255' green='0' blue='0'>" &[SA4]&"</CLR>"
        ElseIf [SA1] ="M" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='56' green='168' blue='0'>" &[SA4]&"</CLR>"
        ElseIf [SA1] ="H" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='0' green='0' blue='255'>" &[SA4]&"</CLR>"
        ElseIf [SA1] ="F" AND [SA2] ="HA" Then
          FindLabel = "<CLR red='255' green='0' blue='197'>" &[SA4]&"</CLR>"
        ElseIf [SA1] ="N" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='255' green='0' blue='0'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"
        ElseIf [SA1] ="M" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='56' green='168' blue='0'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"
        ElseIf [SA1] ="H" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='0' green='0' blue='255'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"
        ElseIf [SA1] ="F" AND ([SA2] ="HL" OR [SA2]="SL" OR [SA2] ="VL") Then
          FindLabel = "<CLR red='255' green='0' blue='197'>" &[SA4]&[GDR]&" "&[SA3]&"</CLR>"
        End If
        End Function
        """;

    [Fact]
    public void Gasleitungen_ProducesBothTextAndColorExpressions()
    {
        var result = Convert(GasleitungenSource);

        Assert.True(result.IsConditional);
        Assert.DoesNotContain("<CLR", result.Expression, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(result.ColorExpression);
        Assert.Contains("rgb(", result.ColorExpression);
    }

    [Theory]
    [InlineData("N", "HA", "A", "rgb(255,0,0)")]
    [InlineData("M", "HA", "A", "rgb(56,168,0)")]
    [InlineData("H", "HA", "A", "rgb(0,0,255)")]
    [InlineData("F", "HA", "A", "rgb(255,0,197)")]
    [InlineData("N", "HL", "AGGG C", "rgb(255,0,0)")]
    [InlineData("M", "SL", "AGGG C", "rgb(56,168,0)")]
    [InlineData("H", "VL", "AGGG C", "rgb(0,0,255)")]
    [InlineData("F", "HL", "AGGG C", "rgb(255,0,197)")]
    [InlineData("X", "HA", "", "")]  // SA1 outside the enumerated set -> nothing matches (VB's implicit empty)
    [InlineData("N", "XX", "", "")]  // SA2 outside the enumerated set -> nothing matches
    public void Gasleitungen_TextAndColorMatchOriginalVbSemantics(string sa1, string sa2, string expectedText, string expectedColor)
    {
        var result = Convert(GasleitungenSource);

        var fields = new (string, string?)[] { ("SA1", sa1), ("SA2", sa2), ("SA3", "C"), ("SA4", "A"), ("GDR", "GGG") };

        Assert.Equal(expectedText, ScriptSimulator.Evaluate(result.Expression, fields));
        Assert.Equal(expectedColor, ScriptSimulator.Evaluate(result.ColorExpression!, fields));
    }

    // -----------------------------------------------------------------------
    // A mix of <FNT> (unsupported, no color) and <CLR> branches, some with only a single
    // color attribute set - reuses the existing FontSizeByTextgrSource fixture from
    // AprxLabelExpressionParserConditionalTests (kept in sync deliberately).
    // -----------------------------------------------------------------------

    private const string FontSizeByTextgrSource = """
        Function FindLabel ( [TYP], [TEXT], [TEXTGR] )
          If ([TYP] = 200 And [TEXTGR] = 2) Then
            FindLabel = "<FNT size = '8'>" & [TEXT] & "</FNT>"
          ElseIf ([TYP] = 200 And [TEXTGR] = 3) Then
            FindLabel = "<FNT size = '12'>" & [TEXT] & "</FNT>"
          ElseIf ([TYP] = 200 And [TEXTGR] = 20) Then
            FindLabel = "<FNT size = '100'>" & [TEXT] & "</FNT>"
          ElseIf ([TYP] = 130) Then
            FindLabel = "<CLR blue = '255'>" & [TEXT] & "</CLR>"
          ElseIf ([TYP] = 300) Then
            FindLabel = "<CLR red = '255'>" & [TEXT] & "</CLR>"
          End if
        End Function
        """;

    [Fact]
    public void FontSizeByTextgr_ProducesColorExpressionForOnlyTheClrBranches()
    {
        var result = Convert(FontSizeByTextgrSource);

        Assert.NotNull(result.ColorExpression);
    }

    [Theory]
    [InlineData("200", "2", "")]     // <FNT> branch - no color, falls back to renderer's base color
    [InlineData("200", "20", "")]    // <FNT> branch
    [InlineData("130", "999", "rgb(0,0,255)")]   // <CLR blue='255'> - red/green default to 0
    [InlineData("300", "999", "rgb(255,0,0)")]   // <CLR red='255'> - green/blue default to 0
    [InlineData("999", "999", "")]   // matches nothing
    public void FontSizeByTextgr_ColorMatchesOriginalVbSemantics(string typ, string textgr, string expectedColor)
    {
        var result = Convert(FontSizeByTextgrSource);

        var actual = ScriptSimulator.Evaluate(result.ColorExpression!, ("TYP", typ), ("TEXT", "hi"), ("TEXTGR", textgr));

        Assert.Equal(expectedColor, actual);
    }

    // -----------------------------------------------------------------------
    // Python equivalent
    // -----------------------------------------------------------------------

    [Fact]
    public void Python_IfElifElse_ProducesColorExpression()
    {
        const string source = """
            def F([A],[B]):
                if [A] == "X":
                    return "<CLR red='255' green='0' blue='0'>" + [B] + "</CLR>"
                else:
                    return "<CLR red='0' green='255' blue='0'>" + [B] + "</CLR>"
            """;
        var result = Convert(source);

        Assert.Equal("hi", ScriptSimulator.Evaluate(result.Expression, ("A", "X"), ("B", "hi")));
        Assert.Equal("rgb(255,0,0)", ScriptSimulator.Evaluate(result.ColorExpression!, ("A", "X"), ("B", "hi")));
        Assert.Equal("hi", ScriptSimulator.Evaluate(result.Expression, ("A", "other"), ("B", "hi")));
        Assert.Equal("rgb(0,255,0)", ScriptSimulator.Evaluate(result.ColorExpression!, ("A", "other"), ("B", "hi")));
    }

    [Fact]
    public void Python_BareReturn_UnconditionalColor()
    {
        var result = Convert("def F([A]):\n    return \"<CLR red='10' green='20' blue='30'>\" + [A] + \"</CLR>\"");

        Assert.False(result.IsConditional);
        Assert.Equal("rgb(10,20,30)", result.ColorExpression);
        Assert.Equal("x", ScriptSimulator.Evaluate(result.Expression, ("A", "x")));
    }

    // -----------------------------------------------------------------------
    // Unconditional (non-Function) single expression with a color wrapper
    // -----------------------------------------------------------------------

    [Fact]
    public void BareExpression_ColorWrappedWholeExpression_ProducesPlainColorText()
    {
        var result = Convert("\"<CLR red='1' green='2' blue='3'>\" & [A] & \"</CLR>\"");

        Assert.False(result.IsConditional);
        Assert.Equal("rgb(1,2,3)", result.ColorExpression);
        Assert.Equal("[A]", result.Expression);
    }

    // -----------------------------------------------------------------------
    // Regressions: shapes that must keep behaving exactly as before this feature
    // -----------------------------------------------------------------------

    [Fact]
    public void TagSurroundedByOtherText_StillJustStripsTag_NoColorDetected()
    {
        // The tag isn't alone in its own literal (mixed with "prefix "/" suffix"), so it's not
        // recognized as a whole-branch color wrapper - must still just strip to plain text exactly
        // as before this feature, with no ColorExpression produced.
        var result = Convert("\"prefix <CLR red='0'>\" & [A] & \"</CLR> suffix\"");

        Assert.Null(result.ColorExpression);
        Assert.Equal("prefix [A] suffix", result.Expression);
    }

    [Fact]
    public void ReplaceChain_WithCleanClrTagAsReplaceSource_StillJustStripsNoColorLeak()
    {
        // The CLR tag sits inside the *source* argument of the (only) Replace(...) call - the
        // seed value for a replace chain is always an argument, never a separate plain-assignment
        // line (that shape belongs to TryParseConditionalAppendChain instead).
        const string source = """
            Function FindLabel ([TYP])
            Beschriftung = Replace("<CLR red='255'>" & [TYP] & "</CLR>", "X", "Y")
            FindLabel = Beschriftung
            End Function
            """;
        var result = Convert(source);

        Assert.Null(result.ColorExpression);
        Assert.DoesNotContain("<CLR", result.Expression, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rgb(", result.Expression, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AppendChain_WithCleanClrTag_StillJustStripsNoColorLeak()
    {
        // The append's own text must start with vbNewLine to convert at all (TryParseConditionalAppendChain's
        // exhaustive verification relies on gView's automatic line-join reproducing that break -
        // see its class doc) - unrelated to color, just what makes this shape convertible in the
        // first place.
        const string source = """
            Function FindLabel ([A],[B])
            FindLabel = "<CLR red='255'>" & [A] & "</CLR>"
            if [B] = "Ja" then
                FindLabel = FindLabel & vbnewline & "extra"
            end if
            End Function
            """;
        var result = Convert(source);

        Assert.Null(result.ColorExpression);
        Assert.DoesNotContain("<CLR", result.Expression, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rgb(", result.Expression, StringComparison.OrdinalIgnoreCase);
    }
}
