#nullable enable

using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace gView.Server.AppCode.Extensions;

static internal class FormCollectionExtensions
{
    private readonly static System.Globalization.NumberFormatInfo numberFormat_EnUS = new System.Globalization.CultureInfo("en-US", false).NumberFormat;

    static public T Parse<T>(this IFormCollection form, string key)
    {
        if (string.IsNullOrEmpty(form[key]))
        {
            throw new ArgumentException($"{key} is empty!");
        }

        return (T)Convert.ChangeType(
            form[key].ToString(),
            Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T),
            numberFormat_EnUS);
    }

    static public T? ParseOrDefault<T>(this IFormCollection form, string key)
    {
        if (string.IsNullOrEmpty(form[key]))
        {
            return default(T);
        }

        return (T)Convert.ChangeType(
            form[key].ToString(),
            Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T),
            numberFormat_EnUS);
    }

    static public T?[] AsArrayOrDefault<T>(this IFormCollection form, string key, T?[] defaultValue = null!)
    {
        if (String.IsNullOrEmpty(form[key]))
        {
            return defaultValue;
        }

        return form[key].ToString().Split(',')
                                   .Select(s =>
                                            typeof(T) == typeof(string)
                                                ? s.Trim()
                                                : Convert.ChangeType(s.Trim(), typeof(T)))
                                   .Cast<T>()
                                   .ToArray();
    }

    static public T[] AsArray<T>(this IFormCollection form, string key)
    {
        var array = form.AsArrayOrDefault<T>(key);

        if (array is null)
        {
            return Array.Empty<T>();
        }

        return array!;
    }

    static public T? AsObject<T>(this IFormCollection form, string key, Func<string, T> func)
    {
        if (string.IsNullOrEmpty(form[key]))
        {
            throw new ArgumentException($"{key} is empty!");
        }

        return func(form[key].ToString());
    }

    static public T? AsObjectOrDefault<T>(this IFormCollection form, string key, Func<string, T> func)
    {
        if (string.IsNullOrEmpty(form[key]))
        {
            return default(T);
        }

        return func(form[key].ToString());
    }

    static public bool IsTrue(this IFormCollection form, string key)
        => "true".Equals(form[key], StringComparison.OrdinalIgnoreCase);

    static public bool IsNotFalse(this IFormCollection form, string key)
        => "false".Equals(form[key], StringComparison.OrdinalIgnoreCase) != true;

    static public T AsEnumValue<T>(this IFormCollection form, string key)
        where T : struct
    {
        if (string.IsNullOrEmpty(form[key]))
        {
            throw new ArgumentException($"{key} is empty!");
        }

        return Enum.Parse<T>(form[key].ToString());
    }
}
