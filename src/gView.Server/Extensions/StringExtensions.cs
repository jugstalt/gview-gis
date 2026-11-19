using gView.Server.AppCode;
using System;
using System.IO;
using System.Linq;

namespace gView.Server.Extensions;

static internal class StringExtensions
{

    static public bool UserNameIsUrlToken(this string username) => username.StartsWith(Globals.UrlTokenNamePrefix);

    static public bool IsValueUrlToken(this string token) => token.UserNameIsUrlToken() && token.Contains("~");

    static public string NameOfUrlToken(this string token) =>
        token.IsValueUrlToken()
            ? token.Split('~')[0]
            : throw new Exception("Invalid url token");

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
