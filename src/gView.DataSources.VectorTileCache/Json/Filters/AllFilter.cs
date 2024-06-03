using gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;
using gView.Framework.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace gView.DataSources.VectorTileCache.Json.Filters;

class AllFilter : IFilter
{
    private IEnumerable<IFilter> Filters;

    public AllFilter(IEnumerable<IFilter> filters)
    {
        Filters = filters;
    }

    public bool Test(IFeature feature)
    {
        return Filters.All(filter => filter.Test(feature));
    }
}
