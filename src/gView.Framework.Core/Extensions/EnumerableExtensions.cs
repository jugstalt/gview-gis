using gView.Framework.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Core.Extensions;
public static class EnumerableExtensions
{
    static public IEnumerable<T> GetLayersInGroup<T>(this IEnumerable<IDatasetElement> elements, int? groupLayerId = null, bool recursive = false) where T : ILayer
    {
        if (elements is null)
        {
            return [];
        }

        List<T> result = new List<T>();

        foreach (var layer in elements.Where(e => e is ILayer layer && (layer.GroupLayer?.ID == groupLayerId)))
        {
            if (recursive && layer is IGroupLayer groupLayer)
            {
                result.AddRange(GetLayersInGroup<T>(elements, groupLayer.ID, recursive));
            }
            else if (typeof(T).IsAssignableFrom(layer.GetType()))
            {
                result.Add((T)layer);
            }
        }

        return result;
    }
}
