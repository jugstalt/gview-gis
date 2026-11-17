using Xunit;
using gView.Framework.NetworkModel.Templates;

namespace gView.Framework.NetworkModel.Tests;

public class ValueTemplateTests
{
    private static readonly string[] Groups = ["Strom", "Niederspannung"];

    [Theory]
    [InlineData("Kasten", "Kasten")]
    [InlineData("{layer-name}", "Kasten NS")]
    [InlineData("{group-name}", "Niederspannung")]
    [InlineData("{group-name:1}", "Strom")]
    [InlineData("{group-name:2}", "Niederspannung")]
    [InlineData("{group-name:3}", "")]
    [InlineData("[TYP]", "KVS")]
    [InlineData("[TYP]-[BAUART]", "KVS-Freistehend")]
    [InlineData("{group-name} [TYP]", "Niederspannung KVS")]
    [InlineData("[LEER]", "")]
    [InlineData("", "")]
    public void Evaluate_Placeholders_GiveExpectedText(string template, string expected)
    {
        var parsed = ValueTemplate.Parse(template, "Kasten NS", Groups);

        var value = parsed.Evaluate(field => field switch { "TYP" => " KVS ", "BAUART" => "Freistehend", _ => null });

        Assert.Equal(expected, value);
    }

    [Fact]
    public void Parse_Fields_AreListed()
    {
        var parsed = ValueTemplate.Parse("[A]{layer-name}[B]", "L", Groups);

        Assert.Equal(["A", "B"], parsed.FieldNames);
        Assert.False(parsed.IsEmpty);
        Assert.True(ValueTemplate.Parse(null, "L", Groups).IsEmpty);
    }

    [Theory]
    [InlineData("[TYP")]
    [InlineData("{layer-name")]
    [InlineData("{unknown}")]
    [InlineData("{group-name:0}")]
    public void Parse_Invalid_Throws(string template)
    {
        Assert.Throws<FormatException>(() => ValueTemplate.Parse(template, "L", Groups));
    }
}
