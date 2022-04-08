using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Framework.FDB
{
    public interface ISpatialIndexDef
    {
        GeometryFieldType GeometryType { get; }
        IEnvelope SpatialIndexBounds { get; }
        double SplitRatio { get; }
        int MaxPerNode { get; }
        int Levels { get; }
        ISpatialReference SpatialReference { get; }
        bool ProjectTo(ISpatialReference sRef);
    }
}
