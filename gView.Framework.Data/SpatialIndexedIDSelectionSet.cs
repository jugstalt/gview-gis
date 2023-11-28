using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Data
{
    public class SpatialIndexedIDSelectionSet : SpatialIndexedIDSelectionSetTemplate<int>, ISpatialIndexedIDSelectionSet
    {
        public SpatialIndexedIDSelectionSet(IEnvelope bounds)
            : base(bounds)
        {
        }
    }
}
