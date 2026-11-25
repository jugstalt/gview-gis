namespace gView.Pipe.Plugins.Gis.Tests;

public class NetworkSupplyTests
{
    [Theory]
    [InlineData("Transformer#4711 Transformer#4712", 4711L, "4711,4712")]
    [InlineData("Transformer#4711 Box#4711", 4711L, "4711")]
    [InlineData("Low Voltage/House Connection#7 Low Voltage/Box#8", 7L, "7,8")]
    [InlineData("Line#12/0", 12L, "12")]
    [InlineData("", null, null)]
    [InlineData(null, null, null)]
    public void NodeLabels_FirstOidAndOidsText(string? text, long? firstOid, string? oidsText)
    {
        var labels = new NodeLabels(text);

        Assert.Equal(firstOid, labels.FirstOid);
        Assert.Equal(oidsText, labels.OidsText);
    }

    [Theory]
    [InlineData(new[] { "Edge", "Multiple" }, "Multiple")]
    [InlineData(new[] { "Open", "Open" }, "Open")]
    [InlineData(new[] { "Open", "Edge" }, "Edge")]
    [InlineData(new string[0], "Edge")]
    public void SupplyKind_Of_TheParts(string[] parts, string kind)
    {
        Assert.Equal(kind, SupplyKind.Of(parts));
    }
}
