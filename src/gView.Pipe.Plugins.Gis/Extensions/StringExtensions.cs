using System.Globalization;

namespace gView.Pipe.Plugins.Gis.Extensions;

internal static class StringExtensions
{
    /// <summary>Null for null, empty and white space, otherwise the trimmed text.</summary>
    public static string? TrimmedOrNull(this string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>The number after the last colon of an srs like <c>EPSG:31256</c> (or the whole text without a colon); null when there is none.</summary>
    public static int? ToEpsgCode(this string? srs)
    {
        if (string.IsNullOrEmpty(srs))
        {
            return null;
        }
        var code = srs[(srs.LastIndexOf(':') + 1)..];
        return int.TryParse(code, NumberStyles.Integer, CultureInfo.InvariantCulture, out var epsg) ? epsg : null;
    }
}
