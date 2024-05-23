using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.Framework.Core.Data
{
    public interface IFeatureDataset : IDataset
    {
        Task<IEnvelope> Envelope();

        Task<ISpatialReference> GetSpatialReference();
        void SetSpatialReference(ISpatialReference sRef);
    }
}