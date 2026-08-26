using System.Text.Json;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx.Models;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="CimColorConverter"/>'s JSON deserialization directly (as opposed to
/// <see cref="SymbolConversionTests"/>'s HSV/CMYK/Gray conversion-math tests, which build
/// <c>CimColor</c> objects directly and never exercise JSON parsing). ArcGIS Pro exports every
/// color type - not just RGB - using the "values" array form; a real aprx's CIMHSVColor was
/// found to always look like <c>{ "type": "CIMHSVColor", "values": [H, S, V, Alpha] }</c>, which
/// the converter didn't handle for HSV/CMYK/Gray (only for RGB) - so every HSV/CMYK/Gray color
/// silently deserialized as all-zero components (black, for HSV/Gray) instead of throwing or
/// warning.
/// </summary>
public class CimColorJsonTests
{
    private static CimColor? Deserialize(string json) => JsonSerializer.Deserialize<CimColor>(json);

    [Fact]
    public void Hsv_ValuesArrayForm_IsParsedCorrectly()
    {
        // The exact shape found in a real aprx.
        var color = Assert.IsType<CimHsvColor>(Deserialize("""{"type":"CIMHSVColor","values":[111,100,59,100]}"""));

        Assert.Equal(111, color.H);
        Assert.Equal(100, color.S);
        Assert.Equal(59, color.V);
        Assert.Equal(100, color.Alpha);
    }

    [Fact]
    public void Hsv_NamedPropertyForm_IsStillParsedCorrectly()
    {
        var color = Assert.IsType<CimHsvColor>(Deserialize("""{"type":"CIMHSVColor","h":111,"s":100,"v":59,"alpha":100}"""));

        Assert.Equal(111, color.H);
        Assert.Equal(100, color.S);
        Assert.Equal(59, color.V);
        Assert.Equal(100, color.Alpha);
    }

    [Fact]
    public void Hsv_ValuesArrayForm_DefaultsAlphaTo100WhenMissing()
    {
        var color = Assert.IsType<CimHsvColor>(Deserialize("""{"type":"CIMHSVColor","values":[111,100,59]}"""));

        Assert.Equal(100, color.Alpha);
    }

    [Fact]
    public void Cmyk_ValuesArrayForm_IsParsedCorrectly()
    {
        var color = Assert.IsType<CimCmykColor>(Deserialize("""{"type":"CIMCMYKColor","values":[10,20,30,40,100]}"""));

        Assert.Equal(10, color.C);
        Assert.Equal(20, color.M);
        Assert.Equal(30, color.Y);
        Assert.Equal(40, color.K);
        Assert.Equal(100, color.Alpha);
    }

    [Fact]
    public void Cmyk_NamedPropertyForm_IsStillParsedCorrectly()
    {
        var color = Assert.IsType<CimCmykColor>(Deserialize("""{"type":"CIMCMYKColor","c":10,"m":20,"y":30,"k":40,"alpha":100}"""));

        Assert.Equal(10, color.C);
        Assert.Equal(20, color.M);
        Assert.Equal(30, color.Y);
        Assert.Equal(40, color.K);
    }

    [Fact]
    public void Gray_ValuesArrayForm_IsParsedCorrectly()
    {
        var color = Assert.IsType<CimGrayColor>(Deserialize("""{"type":"CIMGrayColor","values":[128,100]}"""));

        Assert.Equal(128, color.Level);
        Assert.Equal(100, color.Alpha);
    }

    [Fact]
    public void Gray_NamedPropertyForm_IsStillParsedCorrectly()
    {
        var color = Assert.IsType<CimGrayColor>(Deserialize("""{"type":"CIMGrayColor","level":128,"alpha":100}"""));

        Assert.Equal(128, color.Level);
    }

    [Fact]
    public void Rgb_ValuesArrayForm_StillWorks()
    {
        // Regression guard: RGB already supported "values" before this fix - make sure the
        // HSV/CMYK/Gray changes didn't disturb it.
        var color = Assert.IsType<CimRgbColor>(Deserialize("""{"type":"CIMRGBColor","values":[255,0,0,100]}"""));

        Assert.Equal(255, color.R);
        Assert.Equal(0, color.G);
        Assert.Equal(0, color.B);
    }
}
