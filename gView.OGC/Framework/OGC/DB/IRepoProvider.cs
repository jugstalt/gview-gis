using gView.Framework.Data;
using gView.Framework.Geometry;
using System.Threading.Tasks;

namespace gView.OGC.Framework.OGC.DB
{
    public interface IRepoProvider
    {
        Task<IEnvelope> FeatureClassEnveolpe(IFeatureClass fc);

        Task<int> FeatureClassSpatialReference(IFeatureClass fc);

        Task<Fields> FeatureClassFields(IFeatureClass fc);

        Task<GeometryType> FeatureClassGeometryType(IFeatureClass fc);
    }
}
