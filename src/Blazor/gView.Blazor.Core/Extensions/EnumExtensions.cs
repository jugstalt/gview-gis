using System;
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
}
