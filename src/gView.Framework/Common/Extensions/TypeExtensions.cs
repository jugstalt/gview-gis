using System;

namespace gView.Framework.Common.Extensions;

public static class TypeExtensions
{
    static internal bool IsValidPluginInterfaceType(this Type type)
        => type.ToString().StartsWith("gview.framework.", StringComparison.OrdinalIgnoreCase)
        || type.ToString().StartsWith("gview.carto.core.abstraction.", StringComparison.OrdinalIgnoreCase);

    static public bool IsTypeOrNullableType(this Type t, Type candidate)
            => t == candidate || Nullable.GetUnderlyingType(t) == candidate;
}
