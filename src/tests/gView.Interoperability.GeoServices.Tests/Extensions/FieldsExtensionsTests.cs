using gView.Framework.Data.Extensions;
using gView.Interoperability.GeoServices.Extensions;

namespace gView.Interoperability.GeoServices.Tests.Extensions;

public class FieldsExtensionsTests
{
    [Fact]
    public void CheckAllowedFunctions_KnownFields_ReturnsTableSpelling()
    {
        var result = "objectid, name ,SHAPE.STArea()".FieldsNames().CheckAllowedFunctions(FakeFeatureClass.Default(), false).ToArray();

        Assert.Equal(["OBJECTID", "NAME", "SHAPE.STArea()"], result);
    }

    [Fact]
    public void CheckAllowedFunctions_MapServerStar_ReturnsStar()
    {
        Assert.Equal(["*"], "*".FieldsNames().CheckAllowedFunctions(FakeFeatureClass.Default(), false));
    }

    [Fact]
    public void CheckAllowedFunctions_FeatureServerStar_ReturnsFieldsWithoutFunctions()
    {
        var result = "*".FieldsNames().CheckAllowedFunctions(FakeFeatureClass.Default(), true).ToArray();

        Assert.Contains("NAME", result);
        Assert.DoesNotContain("SHAPE.STArea()", result);
    }

    [Theory]
    [InlineData("PASSWORD")]
    [InlineData("SHAPE.STLength()")]
    [InlineData("a;DROP\tTABLE\tx")]                 // tab instead of space
    [InlineData("(select password from users)")]
    [InlineData("NAME\nFROM")]
    public void CheckAllowedFunctions_UnknownFieldOrFunction_Throws(string outFields)
    {
        Assert.Throws<SqlDangerousStatementExceptions>(
            () => outFields.FieldsNames().CheckAllowedFunctions(FakeFeatureClass.Default(), false).ToArray());
    }
}
