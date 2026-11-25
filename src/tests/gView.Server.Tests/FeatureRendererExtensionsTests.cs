using gView.Framework.Cartography.Rendering;
using gView.Framework.Symbology;
using gView.GraphicsEngine;
using gView.Server.EndPoints.GeoJsonService.Extensions;

namespace gView.Server.Tests;

public class FeatureRendererExtensionsTests
{
    [Fact]
    public void Simple_IsSingleWithColor()
    {
        var renderer = new SimpleRenderer() { Symbol = new SimpleLineSymbol() { Color = ArgbColor.FromArgb(255, 200, 10, 20), Width = 3 } };

        var layerRenderer = renderer.ToLayerRenderer(null)!;

        Assert.Equal("single", layerRenderer.Type);
        var symbol = Assert.Single(layerRenderer.Classes!).Symbol!;
        Assert.Equal("line", symbol.Type);
        Assert.Equal([200, 10, 20, 255], symbol.Color!);
        Assert.False(string.IsNullOrEmpty(symbol.LegendImageBase64));
        Assert.Equal(3, symbol.Width);
    }

    [Fact]
    public void ValueMap_IsCategoriesPerValue()
    {
        var renderer = new ValueMapRenderer() { ValueField = "TYP" };
        renderer["A"] = new SimpleFillSymbol() { Color = ArgbColor.FromArgb(255, 1, 2, 3) };
        renderer["B"] = new SimpleFillSymbol() { Color = ArgbColor.FromArgb(255, 4, 5, 6) };

        var layerRenderer = renderer.ToLayerRenderer(null)!;

        Assert.Equal("categories", layerRenderer.Type);
        Assert.Equal("TYP", layerRenderer.Field);
        Assert.Equal(["A", "B"], layerRenderer.Classes!.Select(c => c.Value));
        Assert.Equal([4, 5, 6, 255], layerRenderer.Classes!.Last().Symbol!.Color!);
        Assert.Equal("fill", layerRenderer.Classes!.First().Symbol!.Type);
    }

    [Fact]
    public void NoRenderer_IsNull()
        => Assert.Null(((Framework.Core.Carto.IFeatureRenderer?)null).ToLayerRenderer(null));
}
