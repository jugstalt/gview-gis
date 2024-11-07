using SkiaSharp;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace gView.Blazor.Core.Extensions;

static public class StringExtensions
{
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength
            ? value
            : $"{value.Substring(0, Math.Max(1, maxLength - 3))}...";
    }

    public static string SplitCamelCase(this string str, int maxLength = 0)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var builder = new System.Text.StringBuilder();

        foreach (char c in str)
        {
            if (char.IsUpper(c) && builder.Length > 0)
            {
                builder.Append(' ');
            }
            builder.Append(c);
        }

        string result = builder.ToString();

        return maxLength > 0 ? result.TruncateWithEllipsis(maxLength) : result;
    }

    public static string ToEnvironmentVairableName(this string variable)
        => $"%{variable}%";

    static public string? AppendNonEmpty(this string? str, string seperator, params string?[] items)
    {
        if(items is null) return str;

        StringBuilder result = new StringBuilder(str);

        foreach(var item in items.Where(i=>!String.IsNullOrEmpty(str)))
        {
            if (result.Length > 0) result.Append(seperator);
            result.Append(item);
        }

        return result.ToString();
    }

    static public string GenerateSHA1(this string input)
    {
        using (var sha1 = SHA1.Create())
        {
            byte[] bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
