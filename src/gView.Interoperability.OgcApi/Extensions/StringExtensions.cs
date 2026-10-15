namespace gView.Interoperability.OgcApi.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// Appends the (escaped) query parameters to an url
    /// </summary>
    public static string WithQuery(this string url, IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var query = String.Join("&", parameters
            .Where(p => !String.IsNullOrEmpty(p.Key))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value ?? "")}"));

        return String.IsNullOrEmpty(query)
            ? url
            : $"{url}{(url.Contains('?') ? "&" : "?")}{query}";
    }

    public static string WithFormat(this string url, string format)
        => url.WithQuery([new KeyValuePair<string, string>("f", format)]);
}
