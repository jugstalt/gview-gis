using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data.Extensions;

internal static class StringExtensions
{
    public static bool IsJsonObjectOrArray(this string value)
    {
        value=value?.Trim();

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            return true;
        }

        if(value.StartsWith("{") && value.EndsWith("}"))
        {
            return true; 
        }

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return false;
    }
}
