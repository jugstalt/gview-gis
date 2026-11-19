using gView.Server.AppCode;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace gView.Server.Extensions;

static internal class StringExtensions
{

    static public bool UserNameIsUrlToken(this string username) => username.StartsWith(Globals.UrlTokenNamePrefix);

    static public bool IsValueUrlToken(this string token) => token.UserNameIsUrlToken() && token.Contains("~");

    static public string NameOfUrlToken(this string token) =>
        token.IsValueUrlToken()
            ? token.Split('~')[0]
            : throw new Exception("Invalid url token");

    private static readonly Regex SecretParameterRegex = new(
        @"(?<key>[?&](?:token|access[-_]?token|password|pwd|passwd|client[-_]?secret|secret)=)[^&\s""']*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex SecretJsonRegex = new(
        @"(?<key>""(?:token|access[-_]?token|password|pwd|passwd|client[-_]?secret|secret)""\s*:\s*"")[^""]*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex UrlTokenRegex = new(
        @"(?<key>geoservices\([^)~/\s]*~)[^)/\s]*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AuthorizationRegex = new(
        @"(?<key>\b(?:Basic|Bearer)\s+)[A-Za-z0-9\-._~+/]+=*",
        RegexOptions.Compiled);

    /// <summary>
    /// Masks secrets before a message is written to a log: token, password and secret
    /// parameters (query strings, form bodies, json), url tokens
    /// (<c>geoservices(token-name~***)</c>) and Authorization header values.
    /// </summary>
    static public string MaskSecrets(this string message)
    {
        if (String.IsNullOrEmpty(message))
        {
            return message;
        }

        message = SecretParameterRegex.Replace(message, "${key}***");
        message = SecretJsonRegex.Replace(message, "${key}***");
        message = UrlTokenRegex.Replace(message, "${key}***");
        message = AuthorizationRegex.Replace(message, "${key}***");

        return message;
    }

    /// <summary>
    /// The login file (<c>&lt;folder&gt;/&lt;username&gt;.lgn</c>) of a user, or null when the
    /// username could leave the login folder (path separators, "..", drive letters, ...).
    /// Usernames come from requests (forms, Basic auth, tokens) and must never be
    /// used as a path without this check.
    /// </summary>
    static public FileInfo ToLoginFile(this string username, string folder)
    {
        if (String.IsNullOrWhiteSpace(username)
            || String.IsNullOrWhiteSpace(folder)
            || username.Length > 256
            || username.Contains("..")
            || username.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || username.IndexOfAny(['/', '\\', ':']) >= 0)
        {
            return null;
        }

        var directory = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var fileInfo = new FileInfo(Path.Combine(directory, $"{username}.lgn"));

        if (!String.Equals(fileInfo.DirectoryName, directory, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return fileInfo;
    }
}
