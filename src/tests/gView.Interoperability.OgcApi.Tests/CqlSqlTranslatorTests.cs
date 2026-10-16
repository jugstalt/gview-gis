using gView.Framework.Core.Data;
using gView.Framework.Data;
using gView.Interoperability.OgcApi.Cql2;

namespace gView.Interoperability.OgcApi.Tests;

public class CqlSqlTranslatorTests
{
    private static readonly IField[] Fields =
    [
        new Field("NAME", FieldType.String),
        new Field("BEZIRK", FieldType.String),
        new Field("EW", FieldType.integer),
        new Field("AREA", FieldType.Double),
        new Field("ACTIVE", FieldType.boolean),
        new Field("CREATED", FieldType.Date)
    ];

    private static string ToSql(string cql, SqlDialect dialect = SqlDialect.PostgreSql)
        => new CqlSqlTranslator(name => Fields.FirstOrDefault(f => f.name == name), dialect)
            .Translate(CqlTextParser.Parse(cql));

    [Theory]
    [InlineData("NAME = 'Graz'", "NAME = 'Graz'")]
    [InlineData("EW >= 1000 AND AREA < 2.5", "(EW >= 1000 AND AREA < 2.5)")]
    [InlineData("NOT (EW = 1 OR EW = 2)", "NOT ((EW = 1 OR EW = 2))")]
    [InlineData("NAME = BEZIRK", "NAME = BEZIRK")]
    [InlineData("NAME IS NOT NULL", "NAME IS NOT NULL")]
    [InlineData("NAME NOT LIKE 'Gr%'", "NAME NOT LIKE 'Gr%'")]
    [InlineData("EW BETWEEN 10 AND 20", "EW BETWEEN 10 AND 20")]
    [InlineData("NAME IN ('A', 'B''C')", "NAME IN ('A', 'B''C')")]
    [InlineData("CASEI(NAME) LIKE CASEI('graz%')", "UPPER(NAME) LIKE UPPER('graz%')")]
    [InlineData("TRUE", "1=1")]
    public void Translate_BasicPredicates(string cql, string expectedSql)
    {
        Assert.Equal(expectedSql, ToSql(cql));
    }

    [Fact]
    public void Translate_LikeWithEscapedWildcard_AddsEscapeClause()
    {
        Assert.Equal(@"NAME LIKE '100\%' ESCAPE '\'", ToSql(@"NAME LIKE '100\%'"));
    }

    [Theory]
    [InlineData(SqlDialect.PostgreSql, "ACTIVE = TRUE")]
    [InlineData(SqlDialect.SqlServer, "ACTIVE = 1")]
    [InlineData(SqlDialect.SQLite, "ACTIVE = 1")]
    public void Translate_Boolean_PerDialect(SqlDialect dialect, string expectedSql)
    {
        Assert.Equal(expectedSql, ToSql("ACTIVE = TRUE", dialect));
    }

    [Theory]
    [InlineData(SqlDialect.PostgreSql, "CREATED > CAST('2024-05-17 08:30:00' AS timestamp)")]
    [InlineData(SqlDialect.SqlServer, "CREATED > CONVERT(datetime2, '2024-05-17T08:30:00', 126)")]
    [InlineData(SqlDialect.SQLite, "datetime(CREATED) > datetime('2024-05-17 08:30:00')")]
    public void Translate_Timestamp_PerDialectInUtc(SqlDialect dialect, string expectedSql)
    {
        Assert.Equal(expectedSql, ToSql("CREATED > TIMESTAMP('2024-05-17T10:30:00+02:00')", dialect));
    }

    [Theory]
    [InlineData(SqlDialect.PostgreSql, "CAST(CREATED AS date) = CAST('2024-05-17' AS date)")]
    [InlineData(SqlDialect.SqlServer, "CAST(CREATED AS date) = CAST('2024-05-17' AS date)")]
    [InlineData(SqlDialect.SQLite, "date(CREATED) = date('2024-05-17')")]
    public void Translate_Date_ComparesDates(SqlDialect dialect, string expectedSql)
    {
        Assert.Equal(expectedSql, ToSql("CREATED = DATE('2024-05-17')", dialect));
    }

    [Theory]
    [InlineData("ACTIVE = TRUE")]
    [InlineData("CREATED > DATE('2024-01-01')")]
    public void Translate_DateOrBoolean_UnknownDialect_ThrowsBadRequest(string cql)
    {
        var ex = Assert.Throws<OgcApiException>(() => ToSql(cql, SqlDialect.Unknown));

        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [InlineData("UNKNOWN = 1")]
    [InlineData("NAME = 1")]
    [InlineData("EW = 'x'")]
    [InlineData("EW LIKE '1%'")]
    [InlineData("NAME BETWEEN 'a' AND 'b'")]
    [InlineData("CASEI(EW) = 1")]
    [InlineData("CREATED = '2024-01-01'")]
    [InlineData("NAME LIKE BEZIRK")]
    [InlineData("1 IS NULL")]
    public void Translate_InvalidOrMistypedFilter_ThrowsBadRequest(string cql)
    {
        var ex = Assert.Throws<OgcApiException>(() => ToSql(cql));

        Assert.Equal(400, ex.StatusCode);
    }
}
