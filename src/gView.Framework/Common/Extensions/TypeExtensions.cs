using gView.Framework.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Common.Extensions;

public static class TypeExtensions
{
    static internal bool IsValidPluginInterfaceType(this Type type)
        => type.ToString().StartsWith("gview.framework.", StringComparison.OrdinalIgnoreCase)
        || type.ToString().StartsWith("gview.carto.core.abstraction.", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The gView plugin interfaces a plugin type implements (without IPlugInDependencies)
    /// </summary>
    static internal IEnumerable<Type> PluginInterfaces(this Type type)
        => type.GetInterfaces()
               .Where(i => i.IsValidPluginInterfaceType() && i != typeof(IPlugInDependencies));

    static public bool IsTypeOrNullableType(this Type t, Type candidate)
            => t == candidate || Nullable.GetUnderlyingType(t) == candidate;
}
