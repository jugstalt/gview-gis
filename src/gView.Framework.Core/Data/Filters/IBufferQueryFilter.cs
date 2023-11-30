using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;

namespace gView.Framework.Core.Data.Filters
{
    public interface IBufferQueryFilter : IQueryFilter
    {
        IQueryFilter RootFilter { get; }
        IFeatureClass RootFeatureClass { get; }

        double BufferDistance { get; }
        GeoUnits BufferUnits { get; }
    }
}
