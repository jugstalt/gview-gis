using gView.Interoperability.Extensions.Features;

namespace gView.Interoperability.OgcApi.Tests;

public class CrsReferenceTests
{
    [Theory]
    [InlineData("http://www.opengis.net/def/crs/OGC/1.3/CRS84")]
    [InlineData("https://www.opengis.net/def/crs/OGC/1.3/CRS84")]
    [InlineData("[OGC:CRS84]")]
    [InlineData("CRS84")]
    public void TryParse_Crs84Variants_ReturnsCrs84(string value)
    {
        bool success = CrsReference.TryParse(value, out var crs);

        Assert.True(success);
        Assert.True(crs.IsCrs84);
        Assert.Equal(4326, crs.EpsgCode);
        Assert.Equal(CrsReference.Crs84Uri, crs.Uri);
    }

    [Theory]
    [InlineData("http://www.opengis.net/def/crs/EPSG/0/31256", 31256)]
    [InlineData("https://www.opengis.net/def/crs/EPSG/0/4326", 4326)]
    [InlineData("EPSG:3857", 3857)]
    [InlineData("[EPSG:25833]", 25833)]
    public void TryParse_EpsgVariants_ReturnsEpsgCode(string value, int expectedCode)
    {
        bool success = CrsReference.TryParse(value, out var crs);

        Assert.True(success);
        Assert.False(crs.IsCrs84);
        Assert.Equal(expectedCode, crs.EpsgCode);
        Assert.Equal($"http://www.opengis.net/def/crs/EPSG/0/{expectedCode}", crs.Uri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://www.opengis.net/def/crs/EPSG/0/abc")]
    [InlineData("http://www.opengis.net/def/crs/EPSG/0/-1")]
    [InlineData("WGS84")]
    public void TryParse_InvalidValue_ReturnsFalse(string? value)
    {
        bool success = CrsReference.TryParse(value, out _);

        Assert.False(success);
    }

    [Fact]
    public void Equals_Crs84AndEpsg4326_AreDifferent()
    {
        Assert.NotEqual(CrsReference.Crs84, CrsReference.FromEpsg(4326));
        Assert.Equal(CrsReference.FromEpsg(4326), CrsReference.FromEpsg(4326));
    }

    [Theory]
    [InlineData("urn:ogc:def:crs:EPSG::31256", 31256, CrsNotation.Urn, "urn:ogc:def:crs:EPSG::31256")]
    [InlineData("urn:ogc:def:crs:EPSG:6.6:4326", 4326, CrsNotation.Urn, "urn:ogc:def:crs:EPSG::4326")]
    [InlineData("urn:x-ogc:def:crs:EPSG:3857", 3857, CrsNotation.Urn, "urn:ogc:def:crs:EPSG::3857")]
    [InlineData("EPSG:3857", 3857, CrsNotation.EpsgShort, "EPSG:3857")]
    [InlineData("http://www.opengis.net/gml/srs/epsg.xml#4326", 4326, CrsNotation.GmlSrsUrl, "http://www.opengis.net/gml/srs/epsg.xml#4326")]
    [InlineData("http://www.opengis.net/def/crs/EPSG/0/4326", 4326, CrsNotation.Uri, "http://www.opengis.net/def/crs/EPSG/0/4326")]
    public void TryParse_KeepsNotation(string value, int epsgCode, CrsNotation notation, string name)
    {
        Assert.True(CrsReference.TryParse(value, out var crs));

        Assert.Equal(epsgCode, crs.EpsgCode);
        Assert.Equal(notation, crs.Notation);
        Assert.Equal(name, crs.Name);
        Assert.Equal(CrsReference.FromEpsg(epsgCode), crs);  // the notation is no part of equality
    }

    [Fact]
    public void TryParse_UrnCrs84()
    {
        Assert.True(CrsReference.TryParse("urn:ogc:def:crs:OGC:1.3:CRS84", out var crs));

        Assert.True(crs.IsCrs84);
        Assert.Equal(CrsReference.Crs84, crs);
    }
}
