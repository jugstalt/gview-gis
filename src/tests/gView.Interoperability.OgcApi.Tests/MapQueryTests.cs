using gView.GraphicsEngine;
using gView.Interoperability.OgcApi.Maps;

namespace gView.Interoperability.OgcApi.Tests;

public class MapQueryTests
{
    private static readonly IReadOnlyList<CrsReference> SupportedCrs =
        [CrsReference.FromEpsg(31256), CrsReference.FromEpsg(3857), CrsReference.Crs84];

    private static MapQuery Parse(string query, string? accept = null, bool isDatasetMap = true)
    {
        var parameters = query
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => p.Length > 1 ? p[1] : "");

        return MapQuery.Parse(parameters, accept, SupportedCrs, isDatasetMap);
    }

    [Fact]
    public void Parse_Defaults()
    {
        var query = Parse("");

        Assert.Null(query.BBox);
        Assert.Equal(CrsReference.Crs84, query.BBoxCrs);
        Assert.Null(query.Crs);
        Assert.False(query.Transparent);
        Assert.Equal(ImageFormat.Png, query.Format);
        Assert.Equal(OgcApiMediaTypes.Png, query.ContentType);
        Assert.Null(query.Collections);
        Assert.Equal(25.4 / 0.28, query.Dpi, 6);
    }

    [Fact]
    public void Parse_AllParameters()
    {
        var query = Parse("bbox=1,2,3,4&bbox-crs=http://www.opengis.net/def/crs/EPSG/0/3857&crs=http://www.opengis.net/def/crs/EPSG/0/31256"
                        + "&width=800&height=600&transparent=true&bgcolor=0xFF0000&collections=c1,c3&mm-per-pixel=0.254&f=jpg");

        Assert.Equal([1.0, 2.0, 3.0, 4.0], query.BBox!);
        Assert.Equal(CrsReference.FromEpsg(3857), query.BBoxCrs);
        Assert.Equal(CrsReference.FromEpsg(31256), query.Crs);
        Assert.Equal(800, query.Width);
        Assert.Equal(600, query.Height);
        Assert.True(query.Transparent);
        Assert.Equal(ArgbColor.FromArgb(255, 255, 0, 0).ToArgb(), query.BackgroundColor.ToArgb());
        Assert.Equal(["c1", "c3"], query.Collections);
        Assert.Equal(100.0, query.Dpi, 6);
        Assert.Equal(ImageFormat.Jpeg, query.Format);
    }

    [Theory]
    [InlineData("foo=bar")]
    [InlineData("width=0")]
    [InlineData("height=abc")]
    [InlineData("transparent=yes")]
    [InlineData("bgcolor=red")]
    [InlineData("crs=http://www.opengis.net/def/crs/EPSG/0/2056")]
    [InlineData("scale-denominator=-1")]
    [InlineData("f=gif")]
    public void Parse_InvalidParameter_ThrowsBadRequest(string query)
    {
        var ex = Assert.Throws<OgcApiException>(() => Parse(query));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void Parse_CollectionsOnCollectionMap_ThrowsBadRequest()
    {
        var ex = Assert.Throws<OgcApiException>(() => Parse("collections=c1", isDatasetMap: false));

        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [InlineData("image/jpeg", ImageFormat.Jpeg)]
    [InlineData("image/png,image/jpeg", ImageFormat.Png)]
    [InlineData("*/*", ImageFormat.Png)]
    public void Parse_AcceptHeader_SelectsFormat(string accept, ImageFormat expected)
    {
        Assert.Equal(expected, Parse("", accept).Format);
    }

    [Theory]
    [InlineData("", 2000, 1000, 1024, 512)]            // default: fits 1024x768, keeps aspect
    [InlineData("", 1000, 2000, 384, 768)]
    [InlineData("width=500", 2000, 1000, 500, 250)]   // height from aspect
    [InlineData("height=300", 2000, 1000, 600, 300)]  // width from aspect
    [InlineData("width=300&height=300", 2000, 1000, 300, 300)]
    public void ImageSize_CalculatesMissingSizeFromAspect(string query, double extentWidth, double extentHeight, int expectedWidth, int expectedHeight)
    {
        var size = Parse(query).ImageSize(extentWidth, extentHeight, 1.0, 4096, 4096);

        Assert.Equal((expectedWidth, expectedHeight), size);
    }

    [Fact]
    public void ImageSize_LimitedByServiceKeepingAspect()
    {
        var size = Parse("width=4000&height=2000").ImageSize(4000, 2000, 1.0, 1000, 1000);

        Assert.Equal((1000, 500), size);
    }

    [Fact]
    public void ImageSize_ScaleDenominator_UsesMmPerPixel()
    {
        // 1:10000 with 0.28 mm/pixel => 2.8 m per pixel
        var size = Parse("scale-denominator=10000").ImageSize(2800, 1400, 1.0, 4096, 4096);

        Assert.Equal((1000, 500), size);
    }

    [Fact]
    public void ImageSize_ScaleDenominatorWithGeographicCrs_ThrowsBadRequest()
    {
        var ex = Assert.Throws<OgcApiException>(() => Parse("scale-denominator=10000").ImageSize(1, 1, null, 4096, 4096));

        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [InlineData("0x00FF00", 0, 255, 0)]
    [InlineData("#0000FF", 0, 0, 255)]
    [InlineData("FFFFFF", 255, 255, 255)]
    public void ParseColor_HexFormats(string value, int r, int g, int b)
    {
        var color = MapQuery.ParseColor(value);

        Assert.Equal(ArgbColor.FromArgb(255, r, g, b).ToArgb(), color.ToArgb());
    }
}
