using gView.Framework.Symbology.Palettes;

namespace gView.Framework.Symbology.Tests;

public class ColorPaletteTests
{
    [Fact]
    public void All_HaveUniqueNamesAndAtLeastTwoColors()
    {
        Assert.Equal(ColorPalettes.All.Count, ColorPalettes.All.Select(p => p.Name).Distinct().Count());
        Assert.All(ColorPalettes.All, p => Assert.True(p.Colors.Count >= 2));
    }

    [Fact]
    public void Find_IgnoresCase()
    {
        Assert.Equal("Viridis", ColorPalettes.Find("viridis")?.Name);
        Assert.Null(ColorPalettes.Find("unknown"));
    }

    [Fact]
    public void GetColors_Sequential_InterpolatesToClassCount()
    {
        var palette = ColorPalettes.Find("Blues")!;

        var colors = palette.GetColors(52);

        Assert.Equal(52, colors.Length);
        Assert.Equal(palette.Colors[0], colors[0]);
        Assert.Equal(palette.Colors[^1], colors[^1]);
    }

    [Fact]
    public void GetColors_Qualitative_Cycles()
    {
        var palette = ColorPalettes.Find("Set1")!;

        var colors = palette.GetColors(palette.Colors.Count + 1);

        Assert.Equal(palette.Colors[0], colors[^1]);
    }

    [Fact]
    public void GetColors_Reverse_ReversesOrder()
    {
        var palette = ColorPalettes.Find("Reds")!;

        var colors = palette.GetColors(4, reverse: true);

        Assert.Equal(palette.Colors[^1], colors[0]);
        Assert.Equal(palette.Colors[0], colors[^1]);
    }
}
