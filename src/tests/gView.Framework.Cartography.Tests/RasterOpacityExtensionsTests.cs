using gView.Framework.Cartography.Extensions;

namespace gView.Framework.Cartography.Tests;

public class RasterOpacityExtensionsTests
{
    [Theory]
    [InlineData(100f, 1f)]    // Opacity 100 %: no transparency
    [InlineData(50f, 0.5f)]
    [InlineData(10f, 0.1f)]
    [InlineData(0f, 1f)]      // not set: ignored, drawn opaque
    [InlineData(-5f, 1f)]
    [InlineData(250f, 1f)]    // clamped
    public void ToDrawingOpacity_PercentToCanvasOpacity(float layerOpacity, float expected)
    {
        float opacity = layerOpacity.ToDrawingOpacity();

        Assert.Equal(expected, opacity, 5);
    }
}
