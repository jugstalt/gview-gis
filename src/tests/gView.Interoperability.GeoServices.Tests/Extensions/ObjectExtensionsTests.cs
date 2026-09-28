using gView.Interoperability.GeoServices.Extensions;

namespace gView.Interoperability.GeoServices.Tests.Extensions;

public class ObjectExtensionsTests
{
    // 2024-05-17T10:30:15Z
    private const long ExpectedMilliseconds = 1715941815000;

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ToEsriDateValue_DateTime_ReturnsEpochMilliseconds(DateTimeKind kind)
    {
        var value = new DateTime(2024, 5, 17, 10, 30, 15, kind);

        var result = ((object)value).ToEsriDateValue();

        Assert.Equal(ExpectedMilliseconds, result);
    }

    [Fact]
    public void ToEsriDateValue_DateTimeOffset_ReturnsEpochMilliseconds()
    {
        var value = new DateTimeOffset(2024, 5, 17, 12, 30, 15, TimeSpan.FromHours(2));

        var result = ((object)value).ToEsriDateValue();

        Assert.Equal(ExpectedMilliseconds, result);
    }

    [Fact]
    public void ToEsriDateValue_DateOnly_ReturnsMidnightUtc()
    {
        var result = ((object)new DateOnly(2024, 5, 17)).ToEsriDateValue();

        Assert.Equal(ExpectedMilliseconds - (10 * 3600 + 30 * 60 + 15) * 1000L, result);
    }

    [Fact]
    public void ToEsriDateValue_TimeSpan_ReturnsTimeOn19700101()
    {
        var result = ((object)new TimeSpan(10, 30, 15)).ToEsriDateValue();

        Assert.Equal((10 * 3600 + 30 * 60 + 15) * 1000L, result);
    }

    [Fact]
    public void ToEsriDateValue_TimeOnly_ReturnsTimeOn19700101()
    {
        var result = ((object)new TimeOnly(10, 30, 15)).ToEsriDateValue();

        Assert.Equal((10 * 3600 + 30 * 60 + 15) * 1000L, result);
    }

    [Fact]
    public void ToEsriDateValue_OtherValues_AreReturnedUnchanged()
    {
        Assert.Equal("text", ((object)"text").ToEsriDateValue());
        Assert.Equal(42L, ((object)42L).ToEsriDateValue());
        Assert.Null(((object?)null)!.ToEsriDateValue());
    }
}
