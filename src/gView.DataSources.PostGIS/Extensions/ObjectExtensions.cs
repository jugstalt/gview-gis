using System;

namespace gView.DataSources.PostGIS.Extensions;

internal static class ObjectExtensions
{
    /// <summary>
    /// Npgsql (without legacy timestamp behavior) only writes a DateTime with a matching Kind:
    /// timestamptz => Utc, timestamp/date => Unspecified. time columns need a TimeSpan,
    /// timetz columns a DateTimeOffset. Date values coming from clients (e.g. GeoServices REST
    /// epoch milliseconds => Utc, parsed strings => Unspecified) are adapted here to the column type.
    /// Unspecified values are treated as UTC, the same way they are written to clients.
    /// </summary>
    public static object ToPostgresDateValue(this object value, string pgTypeName)
    {
        if (String.IsNullOrEmpty(pgTypeName))
        {
            return value;
        }

        DateTime utc;
        if (value is DateTime dateTime)
        {
            utc = dateTime.Kind switch
            {
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
                _ => dateTime
            };
        }
        else if (value is DateTimeOffset dateTimeOffset)
        {
            utc = dateTimeOffset.UtcDateTime;
        }
        else
        {
            return value;
        }

        string typeName = pgTypeName.ToLowerInvariant();

        if (typeName == "timestamptz" || typeName.StartsWith("timestamp with time zone"))
        {
            return utc;
        }
        if (typeName == "timestamp" || typeName.StartsWith("timestamp without time zone") || typeName == "date")
        {
            return DateTime.SpecifyKind(utc, DateTimeKind.Unspecified);
        }
        if (typeName == "timetz" || typeName.StartsWith("time with time zone"))
        {
            return new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Unspecified), TimeSpan.Zero);
        }
        if (typeName == "time" || typeName.StartsWith("time without time zone"))
        {
            return utc.TimeOfDay;
        }

        return value;
    }
}
