namespace gView.WebApps.Extensions;

static public class StringExtensions
{
    static public string ToAppTitle(this string str)
    {
        if(str.Contains("/carto", StringComparison.OrdinalIgnoreCase))
        {
            return "gView.Carto";
        }

        if (str.Contains("/explorer", StringComparison.OrdinalIgnoreCase))
        {
            return "gView.DataExplorer";
        }

        return "gView.WebApps";
    }

    /// <summary>
    /// Returns the return url as a path relative to the app base (e.g. "carto") when it is a local path,
    /// otherwise "" (no open redirects: no scheme, no host, no "//" or "\" tricks)
    /// </summary>
    static public string ToLocalReturnPath(this string? returnUrl, string pathBase = "")
    {
        if (String.IsNullOrWhiteSpace(returnUrl)
            || returnUrl.Contains('\\')
            || returnUrl.Contains("//")
            || returnUrl.Contains(':')
            || returnUrl.Any(char.IsControl)
            || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            return "";
        }

        var path = returnUrl.TrimStart('/');

        // "ReturnUrl" from the cookie handler contains the path base: /gview/login?... => login?...
        var basePath = pathBase.Trim('/');
        if (basePath.Length > 0 && path.StartsWith($"{basePath}/", StringComparison.OrdinalIgnoreCase))
        {
            path = path.Substring(basePath.Length + 1);
        }

        return path;
    }
}
