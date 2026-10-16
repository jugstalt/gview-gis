using gView.Interoperability.OgcApi.Cql2;

namespace gView.Interoperability.OgcApi.Tests;

public class CqlTextParserTests
{
    [Fact]
    public void Parse_Comparison_ReturnsComparison()
    {
        var predicate = CqlTextParser.Parse("NAME = 'Graz'");

        Assert.Equal(new CqlComparison("=", new CqlProperty("NAME"), new CqlString("Graz")), predicate);
    }

    [Fact]
    public void Parse_AndBindsStrongerThanOr()
    {
        var predicate = CqlTextParser.Parse("a = 1 OR b = 2 AND c = 3");

        var or = Assert.IsType<CqlOr>(predicate);
        Assert.IsType<CqlComparison>(or.Left);
        Assert.IsType<CqlAnd>(or.Right);
    }

    [Fact]
    public void Parse_ParenthesesAndNot()
    {
        var predicate = CqlTextParser.Parse("NOT (a = 1 OR b <> 2)");

        var not = Assert.IsType<CqlNot>(predicate);
        Assert.IsType<CqlOr>(not.Predicate);
    }

    [Theory]
    [InlineData("a=-5", -5.0)]
    [InlineData("a = 1.5e3", 1500.0)]
    [InlineData("a = .5", 0.5)]
    public void Parse_Numbers(string cql, double expected)
    {
        var comparison = Assert.IsType<CqlComparison>(CqlTextParser.Parse(cql));

        Assert.Equal(new CqlNumber(expected), comparison.Right);
    }

    [Fact]
    public void Parse_QuotedIdentifierAndEscapedString()
    {
        var comparison = Assert.IsType<CqlComparison>(CqlTextParser.Parse("\"Gemeinde Name\" = 'O''Brien'"));

        Assert.Equal(new CqlProperty("Gemeinde Name"), comparison.Left);
        Assert.Equal(new CqlString("O'Brien"), comparison.Right);
    }

    [Fact]
    public void Parse_KeywordsAreCaseInsensitive()
    {
        var predicate = CqlTextParser.Parse("a is not null and b not like 'x%'");

        var and = Assert.IsType<CqlAnd>(predicate);
        Assert.Equal(new CqlIsNull(new CqlProperty("a"), true), and.Left);
        Assert.Equal(new CqlLike(new CqlProperty("b"), new CqlString("x%"), true), and.Right);
    }

    [Fact]
    public void Parse_BetweenAndIn()
    {
        var and = Assert.IsType<CqlAnd>(CqlTextParser.Parse("ew BETWEEN 100 AND 200 AND bez NOT IN ('A', 'B')"));

        var between = Assert.IsType<CqlBetween>(and.Left);
        Assert.Equal(new CqlNumber(100), between.Lower);
        Assert.Equal(new CqlNumber(200), between.Upper);

        var @in = Assert.IsType<CqlIn>(and.Right);
        Assert.True(@in.Negated);
        Assert.Equal(2, @in.List.Count);
    }

    [Fact]
    public void Parse_FunctionsCaseiDateTimestamp()
    {
        var and = Assert.IsType<CqlAnd>(CqlTextParser.Parse("CASEI(name) = casei('graz') AND d > DATE('2024-01-31') AND t < TIMESTAMP('2024-01-31T10:00:00+02:00')"));

        var casei = Assert.IsType<CqlComparison>(Assert.IsType<CqlAnd>(and.Left).Left);
        Assert.Equal(new CqlCaseInsensitive(new CqlProperty("name")), casei.Left);

        var date = Assert.IsType<CqlComparison>(Assert.IsType<CqlAnd>(and.Left).Right);
        Assert.Equal(new CqlDate(new DateOnly(2024, 1, 31)), date.Right);

        var timestamp = Assert.IsType<CqlComparison>(and.Right);
        Assert.Equal(new CqlTimestamp(new DateTime(2024, 1, 31, 8, 0, 0, DateTimeKind.Utc)), timestamp.Right);
    }

    [Fact]
    public void Parse_BooleanLiteralPredicate()
    {
        Assert.Equal(new CqlBooleanPredicate(true), CqlTextParser.Parse("TRUE"));
        Assert.Equal(new CqlComparison("=", new CqlProperty("flag"), new CqlBoolean(false)), CqlTextParser.Parse("flag = false"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a =")]
    [InlineData("a = 'x")]
    [InlineData("a 'x'")]
    [InlineData("(a = 1")]
    [InlineData("a = 1)")]
    [InlineData("a = 1; DROP TABLE x")]
    [InlineData("S_INTERSECTS(geom, POINT(1 2))")]
    [InlineData("d = DATE('31.01.2024')")]
    [InlineData("t = TIMESTAMP('2024-01-31')")]
    [InlineData("AND = 1")]
    public void Parse_InvalidFilter_ThrowsBadRequest(string cql)
    {
        var ex = Assert.Throws<OgcApiException>(() => CqlTextParser.Parse(cql));

        Assert.Equal(400, ex.StatusCode);
    }
}
