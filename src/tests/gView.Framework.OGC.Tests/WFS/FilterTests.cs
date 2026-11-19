using gView.Framework.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.OGC.WFS;
using System.Xml;

namespace gView.Framework.OGC.Tests.WFS;

public class FilterTests
{
    private static XmlNode FilterNode(string innerXml)
    {
        var doc = new XmlDocument();
        doc.LoadXml($"<Filter>{innerXml}</Filter>");
        return doc.SelectSingleNode("Filter")!;
    }

    private static XmlNode OgcFilterNode(string innerXml)
    {
        var doc = new XmlDocument();
        doc.LoadXml($"<ogc:Filter xmlns:ogc=\"http://www.opengis.net/ogc\">{innerXml}</ogc:Filter>");
        return doc.DocumentElement!;
    }

    private static string Compare(string op, string property, string literal)
        => $"<{op}><PropertyName>{property}</PropertyName><Literal>{literal}</Literal></{op}>";

    private static string Where(XmlNode filter, bool withFeatureClass = true)
        => Filter.FromWFS(withFeatureClass ? FakeFeatureClass.Default() : null, filter, GmlVersion.v1).WhereClause;

    [Theory]
    [InlineData("NAME", "Graz", "NAME='Graz'")]
    [InlineData("NAME", "O'Brien", "NAME='O''Brien'")]
    [InlineData("NAME", "x' OR '1'='1", "NAME='x'' OR ''1''=''1'")]
    [InlineData("name", "Graz", "NAME='Graz'")]                     // field name with table spelling
    [InlineData("POPULATION", "1000", "POPULATION=1000")]
    [InlineData("AREA_KM2", " 12.5 ", "AREA_KM2=12.5")]
    [InlineData("AREA_KM2", "-1.5e3", "AREA_KM2=-1500")]
    [InlineData("CREATED", "2020-01-01", "CREATED='2020-01-01'")]
    [InlineData("ACTIVE", "true", "ACTIVE='true'")]
    public void FromWFS_PropertyIsEqualTo_WritesSqlLiteral(string property, string literal, string expected)
    {
        Assert.Equal(expected, Where(FilterNode(Compare("PropertyIsEqualTo", property, literal))));
    }

    [Theory]
    [InlineData("POPULATION", "1 OR 1=1")]
    [InlineData("POPULATION", "1; DROP TABLE x")]
    [InlineData("POPULATION", "abc")]
    [InlineData("AREA_KM2", "NaN")]
    [InlineData("CREATED", "yesterday' OR 1=1 --")]
    [InlineData("ACTIVE", "1=1")]
    [InlineData("PASSWORD", "x")]
    [InlineData("NAME=NAME OR NAME", "x")]
    public void FromWFS_InvalidPropertyOrLiteral_Throws(string property, string literal)
    {
        Assert.ThrowsAny<Exception>(() => Where(FilterNode(Compare("PropertyIsEqualTo", property, literal))));
    }

    [Theory]
    [InlineData("NAME", "x' OR '1'='1", "NAME='x'' OR ''1''=''1'")]
    [InlineData("VALUE", "5", "VALUE=5")]
    [InlineData("VALUE", "abc", "VALUE='abc'")]
    public void FromWFS_WithoutFeatureClass_EscapesLiterals(string property, string literal, string expected)
    {
        Assert.Equal(expected, Where(FilterNode(Compare("PropertyIsEqualTo", property, literal)), withFeatureClass: false));
    }

    [Theory]
    [InlineData("NAME OR 1=1")]
    [InlineData("(select password from users)")]
    [InlineData("a.b")]
    [InlineData("NAME--")]
    public void FromWFS_WithoutFeatureClass_InvalidPropertyName_Throws(string property)
    {
        Assert.ThrowsAny<Exception>(() => Where(FilterNode(Compare("PropertyIsEqualTo", property, "1")), withFeatureClass: false));
    }

    [Fact]
    public void FromWFS_PropertyIsLike_QuotesLiteral()
    {
        Assert.Equal("NAME like 'G''%'", Where(FilterNode(Compare("PropertyIsLike", "NAME", "G'%"))));
    }

    [Fact]
    public void FromWFS_PropertyIsBetween_WritesBetween()
    {
        var filter = FilterNode(
            "<PropertyIsBetween><PropertyName>POPULATION</PropertyName>" +
            "<LowerBoundary><Literal>10</Literal></LowerBoundary>" +
            "<UpperBoundary><Literal>20</Literal></UpperBoundary></PropertyIsBetween>");

        Assert.Equal("POPULATION BETWEEN 10 AND 20", Where(filter));
    }

    [Fact]
    public void FromWFS_PropertyIsBetween_InvalidBoundary_Throws()
    {
        var filter = FilterNode(
            "<PropertyIsBetween><PropertyName>POPULATION</PropertyName>" +
            "<LowerBoundary><Literal>10</Literal></LowerBoundary>" +
            "<UpperBoundary><Literal>20 OR 1=1</Literal></UpperBoundary></PropertyIsBetween>");

        Assert.ThrowsAny<Exception>(() => Where(filter));
    }

    [Fact]
    public void FromWFS_PropertyIsNull_WritesIsNull()
    {
        Assert.Equal("NAME IS NULL", Where(FilterNode("<PropertyIsNull><PropertyName>NAME</PropertyName></PropertyIsNull>")));
    }

    [Fact]
    public void FromWFS_Not_KeepsNegation()
    {
        var filter = FilterNode($"<Not>{Compare("PropertyIsEqualTo", "NAME", "Graz")}</Not>");

        Assert.Equal("NOT (NAME='Graz')", Where(filter));
    }

    [Fact]
    public void FromWFS_AndOr_CombinesClauses()
    {
        var filter = FilterNode(
            "<And>" + Compare("PropertyIsEqualTo", "NAME", "Graz") +
            "<Or>" + Compare("PropertyIsGreaterThan", "POPULATION", "10") + Compare("PropertyIsLessThan", "AREA_KM2", "5") + "</Or>" +
            "</And>");

        Assert.Equal("NAME='Graz' AND (POPULATION>10 OR AREA_KM2<5)", Where(filter));
    }

    [Fact]
    public void FromWFS_OgcNamespace_EscapesLiteral()
    {
        var filter = OgcFilterNode(
            "<ogc:PropertyIsEqualTo><ogc:PropertyName>NAME</ogc:PropertyName><ogc:Literal>O'Brien</ogc:Literal></ogc:PropertyIsEqualTo>");

        Assert.Equal("NAME='O''Brien'", Where(filter));
    }

    private static XmlNode SortBy(string property, string? order)
    {
        var doc = new XmlDocument();
        doc.LoadXml($"<SortBy><SortProperty><PropertyName>{property}</PropertyName>{(order != null ? $"<SortOrder>{order}</SortOrder>" : "")}</SortProperty></SortBy>");
        return doc.DocumentElement!;
    }

    [Theory]
    [InlineData("name", null, "NAME")]
    [InlineData("NAME", "DESC", "NAME DESC")]
    [InlineData("POPULATION", "asc", "POPULATION ASC")]
    [InlineData("POPULATION", "D", "POPULATION DESC")]
    public void AppendSortBy_Valid_SetsOrderBy(string property, string? order, string expected)
    {
        var filter = new QueryFilter();

        Filter.AppendSortBy(FakeFeatureClass.Default(), filter, SortBy(property, order));

        Assert.Equal(expected, filter.OrderBy);
    }

    [Theory]
    [InlineData("PASSWORD", null)]
    [InlineData("NAME; DROP TABLE x", null)]
    [InlineData("(CASE WHEN 1=1 THEN NAME END)", null)]
    [InlineData("NAME", "DESC; DROP TABLE x")]
    [InlineData("NAME", "UP")]
    public void AppendSortBy_Invalid_Throws(string property, string? order)
    {
        Assert.ThrowsAny<Exception>(() => Filter.AppendSortBy(FakeFeatureClass.Default(), new QueryFilter(), SortBy(property, order)));
    }

    [Fact]
    public void AppendSortBy_WithoutFeatureClass_OnlySimpleIdentifiers()
    {
        var filter = new QueryFilter();
        Filter.AppendSortBy(filter, SortBy("NAME", "DESC"));
        Assert.Equal("NAME DESC", filter.OrderBy);

        Assert.ThrowsAny<Exception>(() => Filter.AppendSortBy(new QueryFilter(), SortBy("NAME,(SELECT 1)", null)));
    }
}
