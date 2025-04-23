using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.FDB
{
    public interface ISpatialIndexDef
    {
        GeometryFieldType GeometryType { get; }
        IEnvelope SpatialIndexBounds { get; }
        double SplitRatio { get; }
        int MaxPerNode { get; }
        int Levels { get; }
        ISpatialReference SpatialReference { get; }
        bool ProjectTo(ISpatialReference sRef, IDatumTransformations datumTransformations);
    }
}
