using gView.GraphicsEngine;

namespace gView.GraphicsEngine.Tests;

/// <summary>
/// Tests for <see cref="ArgbColor.FromString"/>/<see cref="ArgbColor.TryFromString"/> - in
/// particular the notations added for gView's <c>ColorExpression</c> feature (bare "R,G,B"/
/// "R,G,B,A" on a 0-255 scale, the "rgb:R,G,B" alias, and "cmyk(...)"), alongside regression
/// coverage for every notation that already worked before (hex, rgb()/rgba(), hsl()/hsla(), named
/// colors) - this method is also used elsewhere in the project (VTC/Mapbox-style color
/// expressions), so nothing here may regress.
/// </summary>
public class ArgbColorFromStringTests
{
    // -----------------------------------------------------------------------
    // New: bare "R,G,B" / "R,G,B,A" (0-255 for every component, alpha optional -> 255)
    // -----------------------------------------------------------------------

    [Fact]
    public void PlainRgb_ThreeValues_AlphaDefaultsTo255()
    {
        var color = ArgbColor.FromString("255,0,128");

        Assert.Equal(255, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(128, color.B);
    }

    [Fact]
    public void PlainRgba_FourValues_AlphaOn0To255Scale()
    {
        // Deliberately NOT the 0-1 scale rgba(...) uses - all 4 components share the same 0-255
        // scale here.
        var color = ArgbColor.FromString("10,20,30,128");

        Assert.Equal(128, color.A);
        Assert.Equal(10, color.R);
        Assert.Equal(20, color.G);
        Assert.Equal(30, color.B);
    }

    [Fact]
    public void RgbColonPrefix_IsAnAliasForPlainRgb()
    {
        var viaColon = ArgbColor.FromString("rgb:255,0,197");
        var viaPlain = ArgbColor.FromString("255,0,197");

        Assert.Equal(viaPlain, viaColon);
    }

    // -----------------------------------------------------------------------
    // New: cmyk(c,m,y,k), each 0-100
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("cmyk(0,0,0,0)", 255, 255, 255)]  // no ink -> white
    [InlineData("cmyk(0,0,0,100)", 0, 0, 0)]      // full black
    [InlineData("cmyk(100,0,0,0)", 0, 255, 255)]  // full cyan -> no red, full green/blue
    [InlineData("cmyk(0,100,0,0)", 255, 0, 255)]  // full magenta -> no green
    [InlineData("cmyk(0,0,100,0)", 255, 255, 0)]  // full yellow -> no blue
    public void Cmyk_ConvertsToExpectedRgb(string input, int r, int g, int b)
    {
        var color = ArgbColor.FromString(input);

        Assert.Equal(255, color.A);
        Assert.Equal(r, color.R);
        Assert.Equal(g, color.G);
        Assert.Equal(b, color.B);
    }

    // -----------------------------------------------------------------------
    // Regressions: every notation that already worked must keep working unchanged
    // -----------------------------------------------------------------------

    [Fact]
    public void HexLongForm_StillWorks()
    {
        var color = ArgbColor.FromString("#ff0000");

        Assert.Equal(255, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void HexShortForm_StillWorks()
    {
        var color = ArgbColor.FromString("#f00");

        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void RgbFunctionForm_StillWorks()
    {
        var color = ArgbColor.FromString("rgb(255,0,197)");

        Assert.Equal(255, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(197, color.B);
    }

    [Fact]
    public void RgbaFunctionForm_AlphaStaysOnZeroToOneScale()
    {
        var color = ArgbColor.FromString("rgba(255,0,0,0.5)");

        Assert.Equal(128, color.A); // 0.5 * 255 = 127.5, Convert.ToInt32's banker's rounding -> 128
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void HslFunctionForm_StillWorks()
    {
        var color = ArgbColor.FromString("hsl(0,100%,50%)"); // pure red

        Assert.Equal(255, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void NamedColor_StillWorks()
    {
        var color = ArgbColor.FromString("red");

        Assert.Equal(255, color.A);
        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }

    [Fact]
    public void TryFromString_UnknownFormat_ReturnsFalse()
    {
        var ok = ArgbColor.TryFromString("not-a-color", out _);

        Assert.False(ok);
    }

    [Fact]
    public void TryFromString_ValidFormat_ReturnsTrue()
    {
        var ok = ArgbColor.TryFromString("255,0,0", out var color);

        Assert.True(ok);
        Assert.Equal(255, color.R);
    }
}
