using gView.Interoperability.Extensions.Cql2;

namespace gView.Interoperability.Extensions.Tests.Cql2;

public class CqlJsonParserTests
{
    [Theory]
    [InlineData("""{"op":"=","args":[{"property":"NAME"},"Wien"]}""", "NAME = 'Wien'")]
    [InlineData("""{"op":"and","args":[{"op":">","args":[{"property":"EW"},1000]},{"op":"<","args":[{"property":"EW"},5000]}]}""", "EW > 1000 AND EW < 5000")]
    [InlineData("""{"op":"or","args":[{"op":"=","args":[{"property":"A"},1]},{"op":"=","args":[{"property":"A"},2]},{"op":"=","args":[{"property":"A"},3]}]}""", "A = 1 OR A = 2 OR A = 3")]
    [InlineData("""{"op":"not","args":[{"op":"isNull","args":[{"property":"NAME"}]}]}""", "NOT NAME IS NULL")]
    [InlineData("""{"op":"like","args":[{"property":"NAME"},"W%"]}""", "NAME LIKE 'W%'")]
    [InlineData("""{"op":"between","args":[{"property":"EW"},1,10]}""", "EW BETWEEN 1 AND 10")]
    [InlineData("""{"op":"in","args":[{"property":"EW"},[1,2,3]]}""", "EW IN (1, 2, 3)")]
    [InlineData("""{"op":"=","args":[{"casei":{"property":"NAME"}},{"casei":"wien"}]}""", "CASEI(NAME) = CASEI('wien')")]
    [InlineData("""{"op":">=","args":[{"property":"CREATED"},{"date":"2024-01-31"}]}""", "CREATED >= DATE('2024-01-31')")]
    [InlineData("true", "TRUE")]
    public void Parse_GivesTheSameTreeAsCql2Text(string json, string text)
    {
        AssertSameTree(CqlTextParser.Parse(text), CqlJsonParser.Parse(json));
    }

    // records compare lists by reference: IN lists element by element
    private static void AssertSameTree(CqlPredicate expected, CqlPredicate actual)
    {
        if (expected is CqlIn expectedIn && actual is CqlIn actualIn)
        {
            Assert.Equal(expectedIn with { List = [] }, actualIn with { List = [] });
            Assert.Equal(expectedIn.List, actualIn.List);
            return;
        }
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("""{"args":[]}""")]
    [InlineData("""{"op":"=","args":[{"property":"A"}]}""")]
    [InlineData("""{"op":"s_intersects","args":[{"property":"geom"},{"type":"Point","coordinates":[1,2]}]}""")]
    [InlineData("""{"op":"in","args":[{"property":"A"},1]}""")]
    [InlineData("""{"op":"=","args":[{"property":"A"},{"date":"31.01.2024"}]}""")]
    public void Parse_Invalid_IsBadRequest(string json)
    {
        var ex = Assert.Throws<CqlException>(() => CqlJsonParser.Parse(json));
        Assert.Equal(400, ex.StatusCode);
    }
}
