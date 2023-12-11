using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Common.Extensions;
static public class EnumerableExtensions
{
    static public bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        => items is null || items.Count() == 0;
}
