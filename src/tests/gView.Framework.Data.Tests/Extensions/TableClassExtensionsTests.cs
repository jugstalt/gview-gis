using gView.Framework.Data.Extensions;

namespace gView.Framework.Data.Tests.Extensions;

public class TableClassExtensionsTests
{
    [Fact]
    public void ProjectRequestFieldNames_KnownFields_ReturnsTableSpelling()
    {
        var fc = FakeFeatureClass.Default();

        var result = fc.ProjectRequestFieldNames(["name", " POPULATION ", "*", "", "SHAPE.STArea()"]).ToArray();

        Assert.Equal(["NAME", "POPULATION", "*", "SHAPE.STArea()"], result);
    }

    [Theory]
    [InlineData("PASSWORD")]
    [InlineData("NAME,POPULATION")]
    [InlineData("SHAPE.STLength()")]
    [InlineData("(select password from users)")]
    [InlineData("a;DROP\tTABLE\tx")]
    [InlineData("NAME as x")]
    public void ProjectRequestFieldNames_UnknownField_Throws(string field)
    {
        var fc = FakeFeatureClass.Default();

        Assert.Throws<SqlDangerousStatementExceptions>(() => fc.ProjectRequestFieldNames([field]).ToArray());
    }

    [Fact]
    public void ProjectRequestFieldNames_PassThrough_IsReturnedUnchanged()
    {
        var fc = FakeFeatureClass.Default();

        Assert.Equal(["#ALL#"], fc.ProjectRequestFieldNames(["#ALL#"], "#ALL#"));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("name", "NAME")]
    [InlineData("name asc", "NAME ASC")]
    [InlineData("name DESC, objectid", "NAME DESC,OBJECTID")]
    [InlineData(" POPULATION \tdesc ", "POPULATION DESC")]
    public void ProjectRequestOrderBy_Valid_ReturnsNormalized(string? orderBy, string expected)
    {
        Assert.Equal(expected, FakeFeatureClass.Default().ProjectRequestOrderBy(orderBy));
    }

    [Theory]
    [InlineData("PASSWORD")]
    [InlineData("NAME UP")]
    [InlineData("NAME ASC NULLS")]
    [InlineData("*")]
    [InlineData("NAME,")]
    [InlineData("(CASE WHEN 1=1 THEN NAME END)")]
    [InlineData("NAME; DROP TABLE x")]
    [InlineData("NAME--")]
    public void ProjectRequestOrderBy_Invalid_Throws(string orderBy)
    {
        Assert.Throws<SqlDangerousStatementExceptions>(() => FakeFeatureClass.Default().ProjectRequestOrderBy(orderBy));
    }

    [Theory]
    [InlineData("a=1", "b=2", "(a=1) and (b=2)")]
    [InlineData("a=1\tOR b=2", "c=3", "(a=1\tOR b=2) and (c=3)")]
    [InlineData("(a=1)OR(b=2)", "c=3", "((a=1)OR(b=2)) and (c=3)")]
    [InlineData("", "c=3", "c=3")]
    [InlineData("a=1", "", "a=1")]
    public void AppendWhereClause_AlwaysParenthesizes(string filter, string append, string expected)
    {
        Assert.Equal(expected, filter.AppendWhereClause(append));
    }
}
