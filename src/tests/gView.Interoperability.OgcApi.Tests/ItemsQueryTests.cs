using gView.Framework.Core.Data;
using gView.Framework.Data;
using gView.Interoperability.OgcApi.Cql2;
using gView.Interoperability.OgcApi.Features;

namespace gView.Interoperability.OgcApi.Tests;

public class ItemsQueryTests
{
    [Fact]
    public void ParseBBox_FourNumbers_ReturnsThem()
    {
        var bbox = ItemsQuery.ParseBBox("13.5,46.5,15.25,47.0");

        Assert.Equal([13.5, 46.5, 15.25, 47.0], bbox);
    }

    [Fact]
    public void ParseBBox_SixNumbers_IgnoresVerticalExtent()
    {
        var bbox = ItemsQuery.ParseBBox("1,2,100,3,4,200");

        Assert.Equal([1.0, 2.0, 3.0, 4.0], bbox);
    }

    [Theory]
    [InlineData("1,2,3")]
    [InlineData("1,2,3,a")]
    [InlineData("1,2,3,4,5")]
    [InlineData("")]
    public void ParseBBox_Invalid_ThrowsBadRequest(string bbox)
    {
        var ex = Assert.Throws<OgcApiException>(() => ItemsQuery.ParseBBox(bbox));

        Assert.Equal(400, ex.StatusCode);
    }

    private static string PropertyFilterSql(IField field, string value, SqlDialect dialect = SqlDialect.PostgreSql)
        => new CqlSqlTranslator(name => name == field.name ? field : null, dialect)
            .Translate(ItemsQuery.PropertyFilterPredicate(field, value));

    [Theory]
    [InlineData("Graz", "NAME = 'Graz'")]
    [InlineData("O'Brien", "NAME = 'O''Brien'")]
    [InlineData("Gr*", "NAME LIKE 'Gr%'")]
    [InlineData("50%*", @"NAME LIKE '50\%%' ESCAPE '\'")]
    public void PropertyFilter_StringField_EscapesAndSupportsWildcard(string value, string expected)
    {
        var field = new Field("NAME", FieldType.String);

        Assert.Equal(expected, PropertyFilterSql(field, value));
    }

    [Fact]
    public void PropertyFilter_IntegerField_WritesNumber()
    {
        Assert.Equal("EW = 1200", PropertyFilterSql(new Field("EW", FieldType.integer), "1200"));
    }

    [Fact]
    public void PropertyFilter_DoubleField_WritesInvariantNumber()
    {
        Assert.Equal("AREA = 12.5", PropertyFilterSql(new Field("AREA", FieldType.Double), "12.5"));
    }

    [Theory]
    [InlineData(SqlDialect.PostgreSql, "ACTIVE = TRUE")]
    [InlineData(SqlDialect.SqlServer, "ACTIVE = 1")]
    [InlineData(SqlDialect.SQLite, "ACTIVE = 1")]
    public void PropertyFilter_BooleanField_WritesDialectLiteral(SqlDialect dialect, string expected)
    {
        Assert.Equal(expected, PropertyFilterSql(new Field("ACTIVE", FieldType.boolean), "true", dialect));
    }

    [Fact]
    public void PropertyFilter_DateField_ComparesDates()
    {
        Assert.Equal("date(D) = date('2024-05-17')", PropertyFilterSql(new Field("D", FieldType.Date), "2024-05-17", SqlDialect.SQLite));
    }

    [Theory]
    [InlineData(FieldType.integer, "12 or 1=1")]
    [InlineData(FieldType.integer, "1.5")]
    [InlineData(FieldType.Double, "abc")]
    [InlineData(FieldType.boolean, "yes")]
    [InlineData(FieldType.Date, "17.05.2024")]
    public void PropertyFilter_InvalidValue_ThrowsBadRequest(FieldType fieldType, string value)
    {
        var field = new Field("F", fieldType);

        var ex = Assert.Throws<OgcApiException>(() => ItemsQuery.PropertyFilterPredicate(field, value));
        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [InlineData("2024-01-01T00:00:00Z")]
    [InlineData("2024-01-01T00:00:00Z/2024-12-31T00:00:00Z")]
    [InlineData("../2024-12-31T00:00:00Z")]
    [InlineData("2024-01-01T00:00:00Z/..")]
    [InlineData("2024-01-01/")]
    public void ValidateDatetime_ValidValues_DoesNotThrow(string datetime)
    {
        ItemsQuery.ValidateDatetime(datetime);
    }

    [Theory]
    [InlineData("yesterday")]
    [InlineData("../..")]
    [InlineData("2024-01-01/2024-02-01/2024-03-01")]
    public void ValidateDatetime_InvalidValues_ThrowsBadRequest(string datetime)
    {
        var ex = Assert.Throws<OgcApiException>(() => ItemsQuery.ValidateDatetime(datetime));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void CombineWhereClauses_SkipsEmptyAndWrapsMultiple()
    {
        Assert.Equal("", FeatureQuery.CombineWhereClauses(null, " "));
        Assert.Equal("A=1", FeatureQuery.CombineWhereClauses("A=1", ""));
        Assert.Equal("(A=1) AND (B=2)", FeatureQuery.CombineWhereClauses("A=1", "B=2"));
    }
}
