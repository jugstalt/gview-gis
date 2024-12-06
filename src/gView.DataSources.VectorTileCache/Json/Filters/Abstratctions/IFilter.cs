using gView.Framework.Core.Data;

namespace gView.DataSources.VectorTileCache.Json.Filters.Abstratctions;

public interface IFilter
{
    bool Test(IFeature feature);
}
