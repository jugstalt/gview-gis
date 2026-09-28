using System;

namespace gView.Interoperability.GeoServices.Extensions;

internal static class ObjectExtensions
{
    /// <summary>
    /// esriFieldTypeDate values are milliseconds since 1970-01-01 (UTC).
    /// DateTime values with Kind Unspecified are treated as UTC,
    /// time-only values are returned as time on 1970-01-01.
    /// Any other value is returned unchanged.
    /// </summary>
    public static object ToEsriDateValue(this object value)
        => value switch
        {
            DateTime dateTime => dateTime.Kind == DateTimeKind.Local
                ? new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds()
                : new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)).ToUnixTimeMilliseconds(),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToUnixTimeMilliseconds(),
            DateOnly dateOnly => new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero).ToUnixTimeMilliseconds(),
            TimeOnly timeOnly => (long)timeOnly.ToTimeSpan().TotalMilliseconds,
            TimeSpan timeSpan => (long)timeSpan.TotalMilliseconds,
            _ => value
        };
}
