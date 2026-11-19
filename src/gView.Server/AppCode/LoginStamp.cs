using gView.Server.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace gView.Server.AppCode;

/// <summary>
/// The security stamp of a login: a hash of its login file (<c>&lt;user&gt;.lgn</c>, the
/// password hash) and an optional <c>&lt;user&gt;.stamp</c> file. Every token issued for
/// a login carries the stamp it had at login time; a token whose stamp is no longer
/// current is rejected. So changing the password or "sign out everywhere" (a new
/// random .stamp file) ends all sessions and tokens of that user.
/// The current stamps are cached for a few seconds (tokens are checked on every request).
/// </summary>
internal static class LoginStamp
{
    private static readonly TimeSpan CacheTime = TimeSpan.FromSeconds(10);
    private static readonly ConcurrentDictionary<string, (string stamp, DateTime expires)> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The stamp of a login file, read from disk; null if the login does not exist.
    /// </summary>
    public static string Compute(FileInfo loginFile)
    {
        loginFile.Refresh();
        if (!loginFile.Exists)
        {
            return null;
        }

        var stampFile = StampFile(loginFile);
        var content = File.ReadAllText(loginFile.FullName) + "\n" + (stampFile.Exists ? File.ReadAllText(stampFile.FullName) : String.Empty);

        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(content)))
                      .Replace('+', '-').Replace('/', '_')
                      .Substring(0, 22);
    }

    /// <summary>
    /// The current stamp of a user in a login folder (cached); null if unknown.
    /// </summary>
    public static string Current(string folder, string username)
    {
        var loginFile = username?.ToLoginFile(folder);
        if (loginFile is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        if (_cache.TryGetValue(loginFile.FullName, out var cached) && cached.expires > now)
        {
            return cached.stamp;
        }

        var stamp = Compute(loginFile);
        _cache[loginFile.FullName] = (stamp, now.Add(CacheTime));

        return stamp;
    }

    /// <summary>
    /// Invalidates every token of the user ("sign out everywhere").
    /// </summary>
    public static void Renew(string folder, string username)
    {
        var loginFile = username?.ToLoginFile(folder);
        if (loginFile is null || !loginFile.Exists)
        {
            return;
        }

        File.WriteAllText(StampFile(loginFile).FullName, Convert.ToHexString(RandomNumberGenerator.GetBytes(16)));
        Invalidate(loginFile);
    }

    /// <summary>
    /// Removes the stamp file of a deleted login.
    /// </summary>
    public static void Delete(FileInfo loginFile)
    {
        var stampFile = StampFile(loginFile);
        if (stampFile.Exists)
        {
            stampFile.Delete();
        }
        Invalidate(loginFile);
    }

    /// <summary>
    /// Forget the cached stamp (the login file changed on this instance).
    /// Other instances notice the change after the cache time.
    /// </summary>
    public static void Invalidate(FileInfo loginFile) => _cache.TryRemove(loginFile.FullName, out _);

    public static void ClearCache() => _cache.Clear();

    private static FileInfo StampFile(FileInfo loginFile)
        => new FileInfo(Path.ChangeExtension(loginFile.FullName, ".stamp"));
}
