using gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;
using gView.Framework.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace gView.DataSources.VectorTileCache.Json.Filters;

class AnyFilter : IFilter
{
    private IEnumerable<IFilter> Filters;

    public AnyFilter(IEnumerable<IFilter> filters)
    {
        Filters = filters;
    }

    public bool Test(IFeature feature)
    {
        return Filters.Any(filter => filter.Test(feature));
    }
}
