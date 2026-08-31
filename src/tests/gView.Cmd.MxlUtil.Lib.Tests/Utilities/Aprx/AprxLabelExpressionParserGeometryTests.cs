using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxLabelExpressionParser"/>'s support of ArcGIS Pro's geometry-derived
/// label functions <c>Length($feature)</c>/<c>$feature.Length</c> (line length / polygon
/// perimeter) and <c>Area($feature)</c>/<c>$feature.Area</c> (polygon area), bare and wrapped in
/// <c>Round(...)</c>. These map onto the reserved pseudo-field placeholders <c>[$feature.length]</c>
/// /<c>[$feature.area]</c>, resolved at label-render time from the feature's actual geometry (see
/// <c>SimpleLabelRenderer</c>/<c>GeometryExtensions.GetLength</c>/<c>GetArea</c>) rather than from
/// an attribute field - unlike every other placeholder this parser produces, so there's nothing to
/// evaluate/round-trip through <see cref="ScriptSimulator"/> here, only the generated expression
/// text itself.
/// </summary>
public class AprxLabelExpressionParserGeometryTests
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

    // -----------------------------------------------------------------------
    // Bare forms
    // -----------------------------------------------------------------------

    [Fact]
    public void BareLengthCall_MapsToReservedPlaceholder()
    {
        Assert.Equal("[$feature.length]", Convert("Length($feature)").Expression);
    }

    [Fact]
    public void BareFeatureLengthAccess_MapsToReservedPlaceholder()
    {
        Assert.Equal("[$feature.length]", Convert("$feature.Length").Expression);
    }

    [Fact]
    public void BareAreaCall_MapsToReservedPlaceholder()
    {
        Assert.Equal("[$feature.area]", Convert("Area($feature)").Expression);
    }

    [Fact]
    public void BareFeatureAreaAccess_MapsToReservedPlaceholder()
    {
        Assert.Equal("[$feature.area]", Convert("$feature.Area").Expression);
    }

    [Fact]
    public void BareLengthCall_IsCaseInsensitive()
    {
        Assert.Equal("[$feature.length]", Convert("LENGTH($FEATURE)").Expression);
        Assert.Equal("[$feature.length]", Convert("$feature.LENGTH").Expression);
    }

    // -----------------------------------------------------------------------
    // Round(...)-wrapped forms
    // -----------------------------------------------------------------------

    [Fact]
    public void RoundOfLengthCall_MapsToReservedPlaceholderWithFormat()
    {
        Assert.Equal("[$feature.length:F1]", Convert("Round(Length($feature),1)").Expression);
    }

    [Fact]
    public void RoundOfFeatureLengthAccess_MapsToReservedPlaceholderWithFormat()
    {
        Assert.Equal("[$feature.length:F2]", Convert("Round($feature.Length,2)").Expression);
    }

    [Fact]
    public void RoundOfAreaCall_MapsToReservedPlaceholderWithFormat()
    {
        Assert.Equal("[$feature.area:F1]", Convert("Round(Area($feature),1)").Expression);
    }

    [Fact]
    public void RoundOfFeatureAreaAccess_MapsToReservedPlaceholderWithFormat()
    {
        Assert.Equal("[$feature.area:F0]", Convert("Round($feature.Area,0)").Expression);
    }

    // -----------------------------------------------------------------------
    // Combined with concatenation
    // -----------------------------------------------------------------------

    [Fact]
    public void LengthCombinedWithConcatenation_ProducesExpectedPlaceholderSequence()
    {
        var result = Convert("\"L=\" & Round(Length($feature),1) & \"m\"");

        Assert.Equal("L=[$feature.length:F1]m", result.Expression);
    }

    // -----------------------------------------------------------------------
    // Regressions
    // -----------------------------------------------------------------------

    [Fact]
    public void FeatureAccessToRealField_StillWorksAsPlainFieldReference()
    {
        // "$feature.Length"/"$feature.Area" are special-cased; any other "$feature.<Name>" must
        // still behave exactly as before (a plain field reference).
        Assert.Equal("[MELD_TXT]", Convert("$feature.MELD_TXT").Expression);
    }

    [Fact]
    public void OtherGeometryFunctionCalls_AreStillRejected()
    {
        // Only Length/Area are recognized function calls - anything else geometry-related stays
        // unsupported rather than guessed at (unlike bare identifiers, function calls aren't
        // reachable via the generic "$feature.<Name>" field-access fallback below).
        AssertRejected("Centroid($feature)");
    }

    [Fact]
    public void OtherFeatureAccessors_StillFallBackToPlainFieldReference()
    {
        // "$feature.Length"/"$feature.Area" are special-cased (see above); any other
        // "$feature.<Name>" - including one that happens to look like a geometry accessor this
        // parser doesn't know, e.g. "$feature.Centroid" - falls back to being treated as a plain
        // field reference, exactly as before this feature existed. Not addressed by this feature;
        // documented here so a future, more complete fix doesn't regress this test unnoticed.
        Assert.Equal("[Centroid]", Convert("$feature.Centroid").Expression);
    }

    [Fact]
    public void LengthCallWithNonFeatureArgument_IsRejected()
    {
        // Only a bare "$feature" argument is understood - Length of a field or a sub-property
        // isn't something this parser can reduce to a placeholder.
        AssertRejected("Length([SOME_FIELD])");
        AssertRejected("Length($feature.SubField)");
    }

    [Fact]
    public void RoundOfLengthWithNonFeatureArgument_IsRejected()
    {
        AssertRejected("Round(Length([SOME_FIELD]),1)");
    }
}
