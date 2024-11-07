using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Data
{
    public class SpatialIndexedGlobalIDSelectionSet : SpatialIndexedIDSelectionSetTemplate<long>, ISpatialIndexedGlobalIDSelectionSet
    {
        public SpatialIndexedGlobalIDSelectionSet(IEnvelope bounds)
            : base(bounds)
        {
        }
    }
}
