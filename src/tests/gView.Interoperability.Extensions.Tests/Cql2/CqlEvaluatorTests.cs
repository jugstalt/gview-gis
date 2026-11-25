using gView.Interoperability.Extensions.Cql2;

namespace gView.Interoperability.Extensions.Tests.Cql2;

public class CqlEvaluatorTests
{
    private static readonly Dictionary<string, object?> Row = new()
    {
        ["LAND"] = "Wien",
        ["count"] = 23L,
        ["avg_EW"] = 81234.5,
        ["NOTHING"] = null
    };

    private static bool Evaluate(string cql)
        => CqlEvaluator.Evaluate(CqlTextParser.Parse(cql), name => Row.TryGetValue(name, out var value)
                ? value
                : throw new CqlException($"unknown {name}"));

    [Theory]
    [InlineData("count > 20", true)]
    [InlineData("count >= 23 AND avg_EW < 100000", true)]
    [InlineData("count = 23.0", true)]
    [InlineData("LAND = 'Wien'", true)]
    [InlineData("LAND <> 'Wien' OR count < 10", false)]
    [InlineData("NOT (count > 20)", false)]
    [InlineData("LAND LIKE 'W%'", true)]
    [InlineData("LAND LIKE 'w%'", false)]
    [InlineData("CASEI(LAND) LIKE CASEI('w%')", true)]
    [InlineData("count BETWEEN 20 AND 30", true)]
    [InlineData("count NOT BETWEEN 20 AND 30", false)]
    [InlineData("LAND IN ('Tirol', 'Wien')", true)]
    [InlineData("LAND NOT IN ('Tirol', 'Wien')", false)]
    [InlineData("NOTHING IS NULL", true)]
    [InlineData("NOTHING = 1", false)]
    [InlineData("NOTHING <> 1", false)]
    public void Evaluate_Row(string cql, bool expected)
        => Assert.Equal(expected, Evaluate(cql));

    [Fact]
    public void UnknownProperty_Throws()
        => Assert.Throws<CqlException>(() => Evaluate("NOPE = 1"));
}
