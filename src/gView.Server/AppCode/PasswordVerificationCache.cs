using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace gView.Server.AppCode;

/// <summary>
/// Remembers successful password checks for a few minutes. PBKDF2 is slow on purpose,
/// but Basic auth checks the password on every request: without this cache every
/// map request of a Basic auth client would cost a full PBKDF2 run.
/// Only successful checks are cached, keyed by a hash of login file, password and
/// stored hash, so a changed password (or rehashed file) never matches an old entry.
/// </summary>
internal static class PasswordVerificationCache
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private const int MaxEntries = 10_000;

    private static readonly ConcurrentDictionary<string, DateTime> _verified = new();

    public static bool IsVerified(string loginFile, string password, string storedHash)
    {
        var key = Key(loginFile, password, storedHash);

        if (_verified.TryGetValue(key, out var expires))
        {
            if (expires > DateTime.UtcNow)
            {
                return true;
            }

            _verified.TryRemove(key, out _);
        }

        return false;
    }

    public static void SetVerified(string loginFile, string password, string storedHash)
    {
        if (_verified.Count >= MaxEntries)
        {
            _verified.Clear();
        }

        _verified[Key(loginFile, password, storedHash)] = DateTime.UtcNow.Add(Lifetime);
    }

    public static void Clear() => _verified.Clear();

    private static string Key(string loginFile, string password, string storedHash)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes($"{loginFile}\0{password}\0{storedHash}")));
}
