using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Common.Extensions;

internal static class TypeExtensions
{
    static public bool IsValidPluginInterfaceType(this Type type)
        => type.ToString().StartsWith("gview.framework.", StringComparison.OrdinalIgnoreCase)
        || type.ToString().StartsWith("gview.carto.core.abstraction.", StringComparison.OrdinalIgnoreCase);
}
