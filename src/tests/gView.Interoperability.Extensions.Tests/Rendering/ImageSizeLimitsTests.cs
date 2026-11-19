using gView.Framework.Core.Exceptions;
using gView.Interoperability.Extensions.Rendering;

namespace gView.Interoperability.Extensions.Tests.Rendering;

public class ImageSizeLimitsTests
{
    [Fact]
    public void Fit_WithinLimits_IsUnchanged()
    {
        var (width, height, dpi, resized) = ImageSizeLimits.Fit(800, 600, 96, 4096, 4096, 600);

        Assert.Equal((800, 600, 96.0, false), (width, height, dpi, resized));
    }

    [Fact]
    public void Fit_TooWide_ScalesDownKeepingAspectRatioAndDpi()
    {
        var (width, height, dpi, resized) = ImageSizeLimits.Fit(8192, 4096, 192, 4096, 4096, 600);

        Assert.True(resized);
        Assert.Equal(4096, width);
        Assert.Equal(2048, height);
        Assert.Equal(96.0, dpi, 6);
    }

    [Fact]
    public void Fit_TooHigh_ScalesDownByHeight()
    {
        var (width, height, _, resized) = ImageSizeLimits.Fit(1000, 10000, 96, 4096, 2000, 600);

        Assert.True(resized);
        Assert.Equal(200, width);
        Assert.Equal(2000, height);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100000, 100000)]
    [InlineData(50000, 0)]
    public void Fit_NoOrTooLargeServiceLimits_UsesAbsoluteMax(int maxWidth, int maxHeight)
    {
        var (width, height, _, resized) = ImageSizeLimits.Fit(100000, 100000, 96, maxWidth, maxHeight, 0);

        Assert.True(resized);
        Assert.Equal(ImageSizeLimits.AbsoluteMaxImageSize, width);
        Assert.Equal(ImageSizeLimits.AbsoluteMaxImageSize, height);
    }

    [Theory]
    [InlineData(100000, 600, 600)]       // default max dpi
    [InlineData(100000, 1200, 1200)]     // configured max dpi
    [InlineData(300, 0, 300)]
    [InlineData(0.5, 0, 1)]              // lower bound
    [InlineData(0, 0, 96)]               // invalid => default dpi
    [InlineData(-10, 0, 96)]
    [InlineData(double.NaN, 0, 96)]
    [InlineData(double.PositiveInfinity, 0, 96)]
    public void Fit_Dpi_IsLimited(double requestedDpi, int maxDpi, double expectedDpi)
    {
        var (_, _, dpi, resized) = ImageSizeLimits.Fit(800, 600, requestedDpi, 4096, 4096, maxDpi);

        Assert.False(resized);
        Assert.Equal(expectedDpi, dpi, 6);
    }

    [Fact]
    public void Fit_HighDpiAndTooLarge_ClampsDpiFirstThenScales()
    {
        var (width, _, dpi, _) = ImageSizeLimits.Fit(8192, 8192, 10000, 4096, 4096, 600);

        Assert.Equal(4096, width);
        Assert.Equal(300.0, dpi, 6);
    }

    [Theory]
    [InlineData(0, 600)]
    [InlineData(800, 0)]
    [InlineData(-5, 600)]
    [InlineData(800, -1)]
    public void Fit_InvalidSize_ThrowsMapServerException400(int width, int height)
    {
        var ex = Assert.Throws<MapServerException>(() => ImageSizeLimits.Fit(width, height, 96, 4096, 4096, 600));

        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [InlineData(0, ImageSizeLimits.AbsoluteMaxImageSize)]
    [InlineData(4096, 4096)]
    [InlineData(20000, ImageSizeLimits.AbsoluteMaxImageSize)]
    public void EffectiveMaxImageSize_ReturnsConfiguredOrAbsolute(int configured, int expected)
    {
        Assert.Equal(expected, ImageSizeLimits.EffectiveMaxImageSize(configured));
    }
}
