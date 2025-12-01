using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace gView.Blazor.Core.Extensions;

static public class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());

        DescriptionAttribute? attribute = field is not null
            ? Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute
            : null;

        return attribute == null ? value.ToString() : attribute.Description;
    }

    public static bool IsOnOf(this Enum value, params Enum[] candidates)
    {
        foreach(var candidate in candidates )
        {
            if(candidate.Equals(value))
            {
                return true;
            }
        }

        return false;
    }

    public static IEnumerable<KeyValuePair<string, T>> ToKeyValuePairs<T>(bool ignoreNone = false) where T : Enum
    {
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            if (ignoreNone && (int)value == 0) continue;

            yield return new KeyValuePair<string, T>(value.ToString()!.SplitCamelCase(), (T)value);
        }
    }
}
