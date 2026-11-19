using gView.Framework.Cartography.VectorTiles;
using gView.GraphicsEngine;

namespace gView.Framework.Cartography.VectorTiles.Tests;

public class GlyphPbfBuilderTests
{
    [Theory]
    [InlineData("Arial", FontStyle.Regular, "Arial-Regular")]
    [InlineData("Open Sans", FontStyle.Bold, "Open_Sans-Bold")]
    [InlineData("Arial", FontStyle.Bold | FontStyle.Italic, "Arial-Bold__Italic")]
    [InlineData("../evil", FontStyle.Regular, "___evil-Regular")]
    public void CacheName_IsASafeFolderName(string family, FontStyle style, string expected)
    {
        Assert.Equal(expected, GlyphPbfBuilder.CacheName(family, style));
    }
}
