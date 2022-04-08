using gView.Framework.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.FDB
{

    public interface IFDBDatabase : IFeatureDatabase3
    {
        Task<bool> SetSpatialIndexBounds(string FCName, string TreeType, IEnvelope Bounds, double SpatialRatio, int maxPerNode, int maxLevels);
        Task<ISpatialIndexDef> SpatialIndexDef(string dsName);
        Task<bool> ShrinkSpatialIndex(string fcName, List<long> NIDs);
        Task<bool> SetFeatureclassExtent(string fcName, IEnvelope envelope);
        Task<bool> CalculateExtent(string fcName);
    }
}
