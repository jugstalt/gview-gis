using System;
using System.Security.Cryptography;
using System.Text;

namespace gView.Server.Services.Security.TwoFactor;

/// <summary>
/// Time-based one-time passwords (RFC 6238) with the parameters every authenticator
/// app understands: HMAC-SHA1, 6 digits, 30 second steps, 160 bit secrets.
/// </summary>
internal static class Totp
{
    public const int Digits = 6;
    public const int StepSeconds = 30;
    public const int SecretBytes = 20;

    /// <summary>
    /// Accepted steps before and after the current one (clock drift of the phone).
    /// </summary>
    public const int Drift = 1;

    private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static byte[] NewSecret() => RandomNumberGenerator.GetBytes(SecretBytes);

    public static long TimeStep(DateTime utcNow)
        => (long)Math.Floor((utcNow - DateTime.UnixEpoch).TotalSeconds / StepSeconds);

    public static string Code(byte[] secret, long step, int digits = Digits)
    {
        Span<byte> counter = stackalloc byte[8];
        for (int i = 7; i >= 0; i--)
        {
            counter[i] = (byte)(step & 0xff);
            step >>= 8;
        }

        var hash = HMACSHA1.HashData(secret, counter);
        int offset = hash[^1] & 0x0f;
        int binary = ((hash[offset] & 0x7f) << 24)
                   | (hash[offset + 1] << 16)
                   | (hash[offset + 2] << 8)
                   | hash[offset + 3];

        int modulo = 1;
        for (int i = 0; i < digits; i++)
        {
            modulo *= 10;
        }

        return (binary % modulo).ToString().PadLeft(digits, '0');
    }

    /// <summary>
    /// Checks a code against the current step ± <see cref="Drift"/>.
    /// A code of a step that is not newer than <paramref name="lastUsedStep"/> is a replay.
    /// </summary>
    /// <param name="step">the matching step (also set for a replay)</param>
    public static TotpResult Verify(byte[] secret, string code, DateTime utcNow, long lastUsedStep, out long step)
    {
        step = 0;
        code = NormalizeCode(code);

        if (code.Length != Digits)
        {
            return TotpResult.Invalid;
        }

        long current = TimeStep(utcNow);
        for (long candidate = current - Drift; candidate <= current + Drift; candidate++)
        {
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(Code(secret, candidate)),
                    Encoding.ASCII.GetBytes(code)))
            {
                step = candidate;
                return candidate <= lastUsedStep ? TotpResult.Replay : TotpResult.Valid;
            }
        }

        return TotpResult.Invalid;
    }

    /// <summary>
    /// The otpauth:// uri for the QR code of the authenticator app.
    /// </summary>
    public static string OtpAuthUri(byte[] secret, string issuer, string account)
    {
        var label = Uri.EscapeDataString(issuer) + ":" + Uri.EscapeDataString(account);

        return $"otpauth://totp/{label}?secret={ToBase32(secret)}&issuer={Uri.EscapeDataString(issuer)}"
             + $"&algorithm=SHA1&digits={Digits}&period={StepSeconds}";
    }

    public static string NormalizeCode(string code)
    {
        var sb = new StringBuilder();
        foreach (var c in code ?? String.Empty)
        {
            if (char.IsDigit(c))
            {
                sb.Append(c);
            }
            else if (!char.IsWhiteSpace(c) && c != '-')
            {
                return String.Empty;
            }
        }
        return sb.ToString();
    }

    #region Base32 (RFC 4648, no padding)

    public static string ToBase32(byte[] data)
    {
        var sb = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0, bits = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                sb.Append(Base32Chars[(buffer >> (bits - 5)) & 0x1f]);
                bits -= 5;
            }
        }
        if (bits > 0)
        {
            sb.Append(Base32Chars[(buffer << (5 - bits)) & 0x1f]);
        }

        return sb.ToString();
    }

    public static byte[] FromBase32(string text)
    {
        text = (text ?? String.Empty).Replace(" ", "").Replace("-", "").TrimEnd('=').ToUpperInvariant();

        var result = new byte[text.Length * 5 / 8];
        int buffer = 0, bits = 0, index = 0;

        foreach (var c in text)
        {
            int value = Base32Chars.IndexOf(c);
            if (value < 0)
            {
                throw new FormatException("Invalid base32 character");
            }

            buffer = (buffer << 5) | value;
            bits += 5;
            if (bits >= 8)
            {
                result[index++] = (byte)((buffer >> (bits - 8)) & 0xff);
                bits -= 8;
            }
        }

        return result;
    }

    #endregion
}

internal enum TotpResult
{
    Invalid,
    Valid,
    Replay
}
