using gView.Interoperability.OgcApi.Extensions;

namespace gView.Interoperability.OgcApi.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void WithQuery_EscapesKeysAndValues()
    {
        var url = "https://server/ogcapi/_/test/collections/c1/items".WithQuery(
        [
            new("bbox-crs", "http://www.opengis.net/def/crs/EPSG/0/31256"),
            new("NAME", "A&B")
        ]);

        Assert.Equal(
            "https://server/ogcapi/_/test/collections/c1/items?bbox-crs=http%3A%2F%2Fwww.opengis.net%2Fdef%2Fcrs%2FEPSG%2F0%2F31256&NAME=A%26B",
            url);
    }

    [Fact]
    public void WithFormat_AppendsToExistingQuery()
    {
        Assert.Equal("https://server/x?limit=10&f=json", "https://server/x?limit=10".WithFormat("json"));
        Assert.Equal("https://server/x?f=html", "https://server/x".WithFormat("html"));
    }
}
