using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Core.FDB
{
    public interface ISpatialIndexDef
    {
        GeometryFieldType GeometryType { get; }

        /// <summary>How the feature class stores its geometry (proprietary blob vs. open / native).</summary>
        GeometryStorageType StorageType { get; }

        IEnvelope SpatialIndexBounds { get; }
        double SplitRatio { get; }
        int MaxPerNode { get; }
        int Levels { get; }
        ISpatialReference SpatialReference { get; }
        bool ProjectTo(ISpatialReference sRef, IDatumTransformations datumTransformations);
    }
}
