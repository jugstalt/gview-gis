using gView.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace gView.Server.Services.Security.TwoFactor;

/// <summary>
/// One-time recovery codes for a lost authenticator: <c>xxxxx-xxxxx</c>
/// (50 random bits each), stored as SHA-256 hashes (long random secrets,
/// like url tokens) and shown only once.
/// </summary>
internal static class RecoveryCodes
{
    public const int Count = 10;

    /// <summary>
    /// From this number of remaining codes on, the admin is warned.
    /// </summary>
    public const int LowWarning = 2;

    private const string Chars = "abcdefghijklmnopqrstuvwxyz234567";

    public static string[] Generate(int count = Count)
        => Enumerable.Range(0, count).Select(_ => NewCode()).ToArray();

    public static string Hash(string code) => SecureCrypto.HashSecret(Normalize(code));

    /// <summary>
    /// The index of the hash that matches the code, or -1.
    /// </summary>
    public static int Find(IReadOnlyList<string> hashes, string code)
    {
        var normalized = Normalize(code);
        if (normalized.Length != 10 || hashes is null)
        {
            return -1;
        }

        int found = -1;
        for (int i = 0; i < hashes.Count; i++)  // no early exit: constant time per list
        {
            if (SecureCrypto.VerifySecret(normalized, hashes[i]) && found < 0)
            {
                found = i;
            }
        }
        return found;
    }

    /// <summary>
    /// Does the input look like a recovery code (and not like a 6 digit TOTP code)?
    /// </summary>
    public static bool LooksLikeRecoveryCode(string input) => Normalize(input).Length == 10;

    public static string Normalize(string code)
    {
        var sb = new StringBuilder();
        foreach (var c in (code ?? String.Empty).ToLowerInvariant())
        {
            if (Chars.Contains(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static string NewCode()
    {
        var chars = new char[10];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = Chars[RandomNumberGenerator.GetInt32(Chars.Length)];
        }
        return $"{new string(chars, 0, 5)}-{new string(chars, 5, 5)}";
    }
}
