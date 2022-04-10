using gView.Framework.Geometry;

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
