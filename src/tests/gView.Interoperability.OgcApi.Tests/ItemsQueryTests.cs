using gView.Framework.Core.Data;
using gView.Framework.Data;
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

    [Theory]
    [InlineData("Graz", "NAME='Graz'")]
    [InlineData("O'Brien", "NAME='O''Brien'")]
    [InlineData("Gr*", "NAME LIKE 'Gr%'")]
    public void BuildFieldClause_StringField_EscapesAndSupportsWildcard(string value, string expected)
    {
        var field = new Field("NAME", FieldType.String);

        Assert.Equal(expected, ItemsQuery.BuildFieldClause(field, value));
    }

    [Fact]
    public void BuildFieldClause_IntegerField_WritesNumber()
    {
        var field = new Field("EW", FieldType.integer);

        Assert.Equal("EW=1200", ItemsQuery.BuildFieldClause(field, "1200"));
    }

    [Fact]
    public void BuildFieldClause_DoubleField_WritesInvariantNumber()
    {
        var field = new Field("AREA", FieldType.Double);

        Assert.Equal("AREA=12.5", ItemsQuery.BuildFieldClause(field, "12.5"));
    }

    [Theory]
    [InlineData(FieldType.integer, "12 or 1=1")]
    [InlineData(FieldType.integer, "1.5")]
    [InlineData(FieldType.Double, "abc")]
    public void BuildFieldClause_InvalidNumber_ThrowsBadRequest(FieldType fieldType, string value)
    {
        var field = new Field("F", fieldType);

        var ex = Assert.Throws<OgcApiException>(() => ItemsQuery.BuildFieldClause(field, value));
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
