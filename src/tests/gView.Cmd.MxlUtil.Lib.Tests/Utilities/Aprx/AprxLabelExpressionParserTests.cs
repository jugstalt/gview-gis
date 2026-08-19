using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for the non-conditional shapes <see cref="AprxLabelExpressionParser"/> reduces to plain
/// gView placeholder text: string literals, "&amp;"/"+" concatenation, <c>round(...)</c> calls,
/// and ArcGIS Pro rich-text tag stripping. Branching (<c>If/ElseIf/Else</c>), value-comparison
/// conditions and <c>Replace(...)</c> chains are covered in
/// <see cref="AprxLabelExpressionParserConditionalTests"/>; shapes that must be rejected are
/// covered in <see cref="AprxLabelExpressionParserRejectionTests"/>.
/// </summary>
public class AprxLabelExpressionParserTests
{
    private static string Convert(string source)
    {
        var ok = AprxLabelExpressionParser.TryConvert(source, out var result);
        Assert.True(ok, $"Expected '{source}' to convert successfully, but it was rejected.");
        return result!.Expression;
    }

    // -----------------------------------------------------------------------
    // Plain string literals
    // -----------------------------------------------------------------------

    [Fact]
    public void TryConvert_PlainStringLiteral_StripsQuotes()
    {
        var result = Convert("\"Leerrohr vorh.\"");

        Assert.Equal("Leerrohr vorh.", result);
    }

    [Fact]
    public void TryConvert_PlainStringLiteral_IsNotConditional()
    {
        Assert.True(AprxLabelExpressionParser.TryConvert("\"Leerrohr vorh.\"", out var result));
        Assert.False(result!.IsConditional);
    }

    [Fact]
    public void TryConvert_StringLiteralWithEscapedQuote_UnescapesIt()
    {
        // VB escapes a literal quote inside a string literal as "" (doubled).
        var result = Convert("\"5\"\" Rohr\"");

        Assert.Equal("5\" Rohr", result);
    }

    // -----------------------------------------------------------------------
    // Concatenation: "&" and "+", [Field] references, vbNewLine/vbTab
    // -----------------------------------------------------------------------

    [Fact]
    public void TryConvert_AmpersandConcatenation_KeepsFieldPlaceholder()
    {
        var result = Convert("\"lt. SAP: \" & [LABEL_SAP]");

        Assert.Equal("lt. SAP: [LABEL_SAP]", result);
    }

    [Fact]
    public void TryConvert_PlusConcatenation_IsTreatedLikeAmpersand()
    {
        var result = Convert("[SCHIEBERTYP] +[STATNR]");

        Assert.Equal("[SCHIEBERTYP][STATNR]", result);
    }

    [Fact]
    public void TryConvert_MixedAmpersandAndPlusConcatenation_BothWork()
    {
        var result = Convert("[A] & \" - \" + [B]");

        Assert.Equal("[A] - [B]", result);
    }

    [Fact]
    public void TryConvert_VbNewLine_BecomesLineBreak()
    {
        var result = Convert("[A] & vbNewLine & [B]");

        Assert.Equal($"[A]{Environment.NewLine}[B]", result);
    }

    [Theory]
    [InlineData("vbCrLf")]
    [InlineData("vbLf")]
    [InlineData("vbCr")]
    public void TryConvert_OtherNewLineConstants_AlsoBecomeLineBreaks(string constant)
    {
        var result = Convert($"[A] & {constant} & [B]");

        Assert.Equal($"[A]{Environment.NewLine}[B]", result);
    }

    [Fact]
    public void TryConvert_VbTab_BecomesTabCharacter()
    {
        var result = Convert("[A] & vbTab & [B]");

        Assert.Equal("[A]\t[B]", result);
    }

    // -----------------------------------------------------------------------
    // ArcGIS Pro rich-text tag stripping
    // -----------------------------------------------------------------------

    [Fact]
    public void TryConvert_ClrTagWithAttributes_IsStripped()
    {
        var result = Convert("\"<CLR red='255' green='0' blue='0'>\" & [SA4] & \"</CLR>\"");

        Assert.Equal("[SA4]", result);
    }

    [Theory]
    [InlineData("BOL")]
    [InlineData("ITA")]
    [InlineData("UND")]
    public void TryConvert_OtherKnownFormattingTags_AreAlsoStripped(string tag)
    {
        var result = Convert($"\"<{tag}>\" & [A] & \"</{tag}>\"");

        Assert.Equal("[A]", result);
    }

    [Fact]
    public void TryConvert_TagSurroundedByOtherText_KeepsSurroundingTextRemovesOnlyTag()
    {
        var result = Convert("\"prefix <CLR red='0'>\" & [A] & \"</CLR> suffix\"");

        Assert.Equal("prefix [A] suffix", result);
    }

    // -----------------------------------------------------------------------
    // round(...) -> gView's existing "[Field:Format]" placeholder syntax
    // -----------------------------------------------------------------------

    [Fact]
    public void TryConvert_RoundOfFloatCast_ProducesFixedPointFormatPlaceholder()
    {
        var result = Convert("round(float([H]), 2)");

        Assert.Equal("[H:F2]", result);
    }

    [Fact]
    public void TryConvert_RoundOfBareField_ProducesFormatPlaceholder()
    {
        var result = Convert("round([H], 0)");

        Assert.Equal("[H:F0]", result);
    }

    [Theory]
    [InlineData("cdbl")]
    [InlineData("cdec")]
    [InlineData("csng")]
    [InlineData("val")]
    [InlineData("cint")]
    [InlineData("clng")]
    public void TryConvert_RoundOfOtherNumericCasts_AllRecognized(string castFunction)
    {
        var result = Convert($"round({castFunction}([LEN]), 3)");

        Assert.Equal("[LEN:F3]", result);
    }

    [Fact]
    public void TryConvert_RoundInsideConcatenation_KeepsSurroundingText()
    {
        var result = Convert("\"H: \" & round(float([H]), 2) & \" m\"");

        Assert.Equal("H: [H:F2] m", result);
    }

    // -----------------------------------------------------------------------
    // Function wrapper with no If at all (single unconditional assignment)
    // -----------------------------------------------------------------------

    [Fact]
    public void TryConvert_UnconditionalFunction_ReducesToConcatenation()
    {
        var result = Convert("Function F([A],[B])\nF = [A] & \": \" & [B]\nEnd Function");

        Assert.Equal("[A]: [B]", result);
    }

    [Fact]
    public void TryConvert_UnconditionalFunction_IsNotConditional()
    {
        Assert.True(AprxLabelExpressionParser.TryConvert(
            "Function F([A])\nF = [A]\nEnd Function", out var result));
        Assert.False(result!.IsConditional);
    }
}
