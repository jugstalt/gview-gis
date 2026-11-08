using gView.Framework.Cartography.VectorTiles;

namespace gView.Framework.Cartography.VectorTiles.Tests;

public class MvtLayerNamesTests
{
    [Theory]
    [InlineData("Straßen", "strassen")]
    [InlineData("Gebäude (Fläche)", "gebaeude_flaeche")]
    [InlineData("  Gruppe/Layer 1 ", "gruppe_layer_1")]
    [InlineData("___", "layer")]
    [InlineData(null, "layer")]
    public void Normalize_GivenName_ReturnsLowercaseAsciiIdentifier(string? name, string expected)
    {
        var result = MvtLayerNames.Normalize(name);

        Assert.Equal(expected, result);
    }
}
