using gView.Framework.Symbology.Palettes;
using gView.GraphicsEngine;

namespace gView.Framework.Symbology.Tests;

public class ColorRampTests
{
    private static readonly ArgbColor Black = ArgbColor.FromArgb(255, 0, 0, 0);
    private static readonly ArgbColor White = ArgbColor.FromArgb(255, 255, 255, 255);

    [Fact]
    public void Interpolate_TwoStops_FirstAndLastAreTheStops()
    {
        var colors = ColorRamp.Interpolate([Black, White], 5);

        Assert.Equal(5, colors.Length);
        Assert.Equal(Black, colors[0]);
        Assert.Equal(White, colors[4]);
    }

    [Fact]
    public void Interpolate_TwoStops_MiddleIsHalfWay()
    {
        var colors = ColorRamp.Interpolate([Black, White], 3);

        Assert.Equal(128, colors[1].R);
        Assert.Equal(128, colors[1].G);
        Assert.Equal(128, colors[1].B);
    }

    [Fact]
    public void Interpolate_ThreeStops_PassesThroughMiddleStop()
    {
        var red = ArgbColor.FromArgb(255, 255, 0, 0);

        var colors = ColorRamp.Interpolate([Black, red, White], 5);

        Assert.Equal(red, colors[2]);
    }

    [Fact]
    public void Interpolate_SingleClass_TakesMiddleOfRamp()
    {
        var colors = ColorRamp.Interpolate([Black, White], 1);

        Assert.Single(colors);
        Assert.Equal(128, colors[0].R);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Interpolate_NoClasses_ReturnsEmpty(int count)
    {
        Assert.Empty(ColorRamp.Interpolate([Black, White], count));
    }

    [Fact]
    public void Cycle_MoreClassesThanColors_StartsAgain()
    {
        var colors = ColorRamp.Cycle([Black, White], 5);

        Assert.Equal([Black, White, Black, White, Black], colors);
    }

    [Fact]
    public void Random_ReturnsOpaqueColors()
    {
        var colors = ColorRamp.Random(20, new Random(42));

        Assert.Equal(20, colors.Length);
        Assert.All(colors, c => Assert.Equal(255, c.A));
    }
}
