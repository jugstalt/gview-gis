using gView.Interoperability.GeoServices.Extensions;

namespace gView.Interoperability.GeoServices.Tests.Extensions;

public class StringExtensionsTests
{
    private static readonly DateTime Utc = new DateTime(2024, 5, 17, 10, 30, 15, DateTimeKind.Utc);

    [Theory]
    [InlineData("2024-05-17T10:30:15Z")]
    [InlineData("2024-05-17T10:30:15.000Z")]
    [InlineData("2024-05-17T12:30:15+02:00")]
    [InlineData("2024-05-17T10:30:15")]
    public void ToEsriDateTime_Iso8601_ReturnsUtcDateTime(string value)
    {
        var result = value.ToEsriDateTime();

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(Utc, result);
    }

    [Theory]
    [InlineData("2024-05-17 10:30:15")]
    [InlineData("17.05.2024 10:30:15")]
    [InlineData("2024.05.17 10:30:15")]
    public void ToEsriDateTime_DateTimeFormats_ReturnsUnspecifiedDateTime(string value)
    {
        var result = value.ToEsriDateTime();

        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
        Assert.Equal(Utc.Ticks, result.Ticks);
    }

    [Theory]
    [InlineData("2024-05-17")]
    [InlineData("17.05.2024")]
    [InlineData("2024.05.17")]
    public void ToEsriDateTime_DateFormats_ReturnsMidnight(string value)
    {
        var result = value.ToEsriDateTime();

        Assert.Equal(new DateTime(2024, 5, 17), result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a date")]
    [InlineData("05/17/2024")]
    [InlineData("2024-05-17Tbad")]
    public void ToEsriDateTime_InvalidValue_ThrowsFormatException(string value)
    {
        Assert.Throws<FormatException>(() => value.ToEsriDateTime());
    }
}
