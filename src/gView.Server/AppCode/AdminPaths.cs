using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace gView.Server.AppCode;

/// <summary>
/// The admin and login pages of the server (relative to the path base).
/// They get no CORS headers and the stricter security headers (no framing, CSP).
/// </summary>
internal static class AdminPaths
{
    private static readonly string[] Prefixes =
    [
        "/manage",
        "/browseservices",
        "/token",
        "/addmap",
        "/removemap",
        "/getmetadata",
        "/setmetadata"
    ];

    public static bool IsAdminPath(PathString path)
    {
        var value = path.Value ?? String.Empty;

        return Prefixes.Any(prefix =>
            value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && (value.Length == prefix.Length || value[prefix.Length] == '/'));
    }
}
