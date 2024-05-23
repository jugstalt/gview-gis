using System;

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
}
