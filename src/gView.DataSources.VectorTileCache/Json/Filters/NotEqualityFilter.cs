using gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;
using gView.Framework.Core.Data;

namespace gView.DataSources.VectorTileCache.Json.Filters;

class NotEqualityFilter : IFilter
{
    private IFilter _filter;

    public NotEqualityFilter(IFilter filter)
    {
        _filter = filter;
    }

    public bool Test(IFeature feature)
    {
        return _filter.Test(feature) == false;
    }
}

