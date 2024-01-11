using gView.Framework.Core.Carto;
using System.Linq;

namespace gView.Blazor.Core.Extensions;

static public class MapExtensions
{
    static public bool IsEmpty(this IMap map)
        => map?.Datasets == null || map?.Datasets.Any() == false;
}
