using gView.Framework.Core.Exceptions;
using gView.Framework.OGC.WMS;
using System.Text;

namespace gView.Framework.OGC.Tests.WMS;

public class WMSParameterDescriptorTests
{
    private static string[] GetFeatureInfoParameters(string infoFormat) =>
    [
        "SERVICE=WMS",
        "VERSION=1.1.1",
        "REQUEST=GetFeatureInfo",
        "SRS=EPSG:4326",
        "FORMAT=image/png",
        "LAYERS=0",
        "QUERY_LAYERS=0",
        "QUERYLAYERS=0",
        "STYLES=",
        "WIDTH=100",
        "HEIGHT=100",
        "BBOX=0,0,10,10",
        "X=50",
        "Y=50",
        $"INFO_FORMAT={infoFormat}"
    ];

    [Theory]
    [InlineData("xsl/default.xsl")]
    [InlineData("xsl/my_info-format.v2.xsl")]
    [InlineData("xsl/a.xsl")]
    public void IsValidInfoFormatXsl_FileNameInXslFolder_ReturnsTrue(string value)
    {
        Assert.True(WMSParameterDescriptor.IsValidInfoFormatXsl(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("xsl/")]
    [InlineData("xsl/.xsl")]
    [InlineData("xsl/../../../secret.xsl")]
    [InlineData("xsl/..xsl")]
    [InlineData("xsl/a..b.xsl")]
    [InlineData("xsl/sub/file.xsl")]
    [InlineData("xsl/..\\..\\file.xsl")]
    [InlineData("xsl/c:/windows/file.xsl")]
    [InlineData("xsl//etc/file.xsl")]
    [InlineData("xsl/file.xml")]
    [InlineData("xsl/file.xsl\n")]
    [InlineData("xsl/file.xsl%00")]
    [InlineData("xsl/File.xsl")]
    [InlineData("file.xsl")]
    [InlineData("xsl/http:file.xsl")]
    public void IsValidInfoFormatXsl_PathOrInvalidName_ReturnsFalse(string? value)
    {
        Assert.False(WMSParameterDescriptor.IsValidInfoFormatXsl(value!));
    }

    [Fact]
    public void GetInfoFormatXslPath_ValidValue_ReturnsPathInsideGetFeatureInfoFolder()
    {
        var etcPath = Path.Combine(Path.GetTempPath(), "gview-etc");
        var root = Path.GetFullPath(Path.Combine(etcPath, "wms", "getfeatureinfo")) + Path.DirectorySeparatorChar;

        var path = WMSParameterDescriptor.GetInfoFormatXslPath(etcPath, "xsl/default.xsl");

        Assert.Equal(Path.Combine(root, "xsl", "default.xsl"), path);
    }

    [Theory]
    [InlineData("xsl/../../../secret.xsl")]
    [InlineData("xsl/..\\..\\secret.xsl")]
    [InlineData("xsl/c:/windows/secret.xsl")]
    [InlineData("xsl//etc/secret.xsl")]
    public void GetInfoFormatXslPath_Traversal_ThrowsMapServerException(string value)
    {
        var etcPath = Path.Combine(Path.GetTempPath(), "gview-etc");

        Assert.Throws<MapServerException>(() => WMSParameterDescriptor.GetInfoFormatXslPath(etcPath, value));
    }

    [Fact]
    public void ParseParameters_ValidInfoFormatXsl_SetsInfoFormatXsl()
    {
        var descriptor = new WMSParameterDescriptor();

        var result = descriptor.ParseParameters(GetFeatureInfoParameters("XSL/Default.xsl"));

        Assert.True(result.data is null, result.data is null ? null : Encoding.UTF8.GetString(result.data));
        Assert.Equal(WMSInfoFormat.xsl, descriptor.InfoFormat);
        Assert.Equal("xsl/default.xsl", descriptor.InfoFormatXsl);
    }

    [Theory]
    [InlineData("xsl/../../../../windows/win.ini")]
    [InlineData("xsl/..%2F..%2Ffile.xsl")]
    [InlineData("xsl/sub/file.xsl")]
    public void ParseParameters_InvalidInfoFormatXsl_ReturnsErrorWithoutValue(string value)
    {
        var descriptor = new WMSParameterDescriptor();

        var result = descriptor.ParseParameters(GetFeatureInfoParameters(value));

        Assert.NotNull(result.data);
        Assert.NotEqual(WMSInfoFormat.xsl, descriptor.InfoFormat);
        Assert.Equal(String.Empty, descriptor.InfoFormatXsl);
        var message = Encoding.UTF8.GetString(result.data);
        Assert.Contains("INFO_FORMAT", message);
        Assert.DoesNotContain(value, message);
    }
}
