using gView.Framework.OGC.WFS;

namespace gView.Framework.OGC.Tests.WFS;

public class WFSParameterDescriptorTests
{
    [Fact]
    public void ParseKvpFilter_FilterXml_ReturnsFilterElement()
    {
        var filter = WFSParameterDescriptor.ParseKvpFilter(
            "<Filter><PropertyIsEqualTo><PropertyName>NAME</PropertyName><Literal>Graz</Literal></PropertyIsEqualTo></Filter>");

        Assert.Equal("Filter", filter.Name);
        Assert.Equal("NAME", filter.SelectSingleNode("PropertyIsEqualTo/PropertyName")!.InnerText);
    }

    [Fact]
    public void ParseKvpFilter_OgcNamespace_ReturnsFilterElement()
    {
        var filter = WFSParameterDescriptor.ParseKvpFilter(
            "<ogc:Filter xmlns:ogc=\"http://www.opengis.net/ogc\"><ogc:PropertyIsNull><ogc:PropertyName>NAME</ogc:PropertyName></ogc:PropertyIsNull></ogc:Filter>");

        Assert.Equal("Filter", filter.LocalName);
        Assert.Equal("http://www.opengis.net/ogc", filter.NamespaceURI);
    }

    [Theory]
    [InlineData("http://127.0.0.1:1/filter.xml")]          // must not be requested (SSRF)
    [InlineData("file:///c:/windows/win.ini")]
    [InlineData("C:\\Windows\\win.ini")]                    // must not be read from disk
    [InlineData("/etc/passwd")]
    [InlineData("")]
    [InlineData("<NotAFilter/>")]
    public void ParseKvpFilter_UrlPathOrOtherXml_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => WFSParameterDescriptor.ParseKvpFilter(value));
    }

    [Fact]
    public void ParseKvpFilter_ExternalEntity_IsNotResolved()
    {
        const string xml =
            "<?xml version=\"1.0\"?><!DOCTYPE Filter [<!ENTITY x SYSTEM \"file:///c:/windows/win.ini\">]>" +
            "<Filter><PropertyIsEqualTo><PropertyName>NAME</PropertyName><Literal>&x;</Literal></PropertyIsEqualTo></Filter>";

        var exception = Record.Exception(() =>
        {
            var filter = WFSParameterDescriptor.ParseKvpFilter(xml);
            Assert.Equal(String.Empty, filter.SelectSingleNode("PropertyIsEqualTo/Literal")!.InnerText);
        });

        Assert.True(exception is null or ArgumentException, exception?.ToString());
    }
}
