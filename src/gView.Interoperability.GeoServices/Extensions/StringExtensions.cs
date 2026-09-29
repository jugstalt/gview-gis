using System;

namespace gView.Interoperability.GeoServices.Extensions
{
    static class StringExtensions
    {
        static public string UrlEncodePassword(this string password)
        {
            if (password != null && password.IndexOfAny("+/=&".ToCharArray()) > 0)
            {
                password = System.Web.HttpUtility.UrlEncode(password);
            }

            return password;
        }

        static public string UrlEncodeWhereClause(this string whereClause)
        {
            if (String.IsNullOrWhiteSpace(whereClause))
            {
                return String.Empty;
            }

            return whereClause.Replace("%", "%25")
                      .Replace("+", "%2B")
                      .Replace("/", "%2F")
                      //.Replace(@"\", "%5C")   // Darf man nicht ersetzen!! Sonst geht beim Kunden der Filter für Usernamen nicht mehr!!!!!!
                      .Replace("&", "%26");
        }

        private static readonly string[] DateTimeFormats = new string[]
        {
            "dd.MM.yyyy HH:mm:ss",
            "dd.MM.yyyy HH:mm",
            "yyyy.MM.dd HH:mm:ss",
            "yyyy.MM.dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm"
        };

        private static readonly string[] DateFormats = new string[]
        {
            "dd.MM.yyyy",
            "yyyy.MM.dd",
            "yyyy-MM-dd"
        };

        /// <summary>
        /// Parses a date value sent by a client as string.
        /// "yyyy-MM-dd HH:mm[:ss]", "dd.MM.yyyy[ HH:mm[:ss]]", "yyyy.MM.dd[ HH:mm[:ss]]" => DateTime (Kind Unspecified)
        /// ISO 8601 with "T" (e.g. "2024-05-17T10:30:15Z", "2024-05-17T12:30:15+02:00") => DateTime (Kind Utc),
        /// without offset the value is taken as UTC.
        /// </summary>
        /// <exception cref="FormatException">if the value can't be parsed</exception>
        static public DateTime ToEsriDateTime(this string value)
        {
            value = value?.Trim() ?? String.Empty;

            if (DateTime.TryParseExact(value,
                                       value.Contains(" ") ? DateTimeFormats : DateFormats,
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None,
                                       out DateTime dateTime))
            {
                return dateTime;
            }

            if (value.Contains("T") &&
                DateTimeOffset.TryParse(value,
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.AssumeUniversal,
                                        out DateTimeOffset dateTimeOffset))
            {
                return dateTimeOffset.UtcDateTime;
            }

            throw new FormatException($"Invalid date value '{value}'. Use epoch milliseconds, ISO 8601 (yyyy-MM-ddTHH:mm:ss[Z|±HH:mm]), yyyy-MM-dd[ HH:mm[:ss]] or dd.MM.yyyy[ HH:mm[:ss]]");
        }
    }
}
