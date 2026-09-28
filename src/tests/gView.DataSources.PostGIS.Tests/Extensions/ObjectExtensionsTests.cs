using gView.DataSources.PostGIS.Extensions;

namespace gView.DataSources.PostGIS.Tests.Extensions;

public class ObjectExtensionsTests
{
    private static readonly DateTime Utc = new DateTime(2024, 5, 17, 10, 30, 15, DateTimeKind.Utc);

    [Theory]
    [InlineData("timestamp with time zone")]
    [InlineData("timestamptz")]
    public void ToPostgresDateValue_TimestampTz_ReturnsUtcDateTime(string pgType)
    {
        var unspecified = DateTime.SpecifyKind(Utc, DateTimeKind.Unspecified);

        var result = unspecified.ToPostgresDateValue(pgType);

        var dateTime = Assert.IsType<DateTime>(result);
        Assert.Equal(DateTimeKind.Utc, dateTime.Kind);
        Assert.Equal(Utc, dateTime);
    }

    [Theory]
    [InlineData("timestamp without time zone")]
    [InlineData("timestamp")]
    [InlineData("date")]
    public void ToPostgresDateValue_TimestampOrDate_ReturnsUnspecifiedWithSameClockTime(string pgType)
    {
        var result = ((object)Utc).ToPostgresDateValue(pgType);

        var dateTime = Assert.IsType<DateTime>(result);
        Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);
        Assert.Equal(Utc.Ticks, dateTime.Ticks);
    }

    [Fact]
    public void ToPostgresDateValue_Time_ReturnsTimeOfDay()
    {
        var result = ((object)Utc).ToPostgresDateValue("time without time zone");

        Assert.Equal(new TimeSpan(10, 30, 15), Assert.IsType<TimeSpan>(result));
    }

    [Fact]
    public void ToPostgresDateValue_TimeTz_ReturnsDateTimeOffsetWithZeroOffset()
    {
        var result = ((object)Utc).ToPostgresDateValue("time with time zone");

        var dateTimeOffset = Assert.IsType<DateTimeOffset>(result);
        Assert.Equal(TimeSpan.Zero, dateTimeOffset.Offset);
        Assert.Equal(Utc.TimeOfDay, dateTimeOffset.TimeOfDay);
    }

    [Fact]
    public void ToPostgresDateValue_DateTimeOffset_IsConvertedToUtc()
    {
        var value = new DateTimeOffset(2024, 5, 17, 12, 30, 15, TimeSpan.FromHours(2));

        var result = ((object)value).ToPostgresDateValue("timestamp with time zone");

        Assert.Equal(Utc, Assert.IsType<DateTime>(result));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("character varying")]
    public void ToPostgresDateValue_UnknownColumnType_ReturnsValueUnchanged(string? pgType)
    {
        var result = ((object)Utc).ToPostgresDateValue(pgType!);

        Assert.Equal(Utc, result);
    }

    [Fact]
    public void ToPostgresDateValue_NonDateValue_ReturnsValueUnchanged()
    {
        var result = ((object)"2024-05-17").ToPostgresDateValue("timestamp");

        Assert.Equal("2024-05-17", result);
    }
}
