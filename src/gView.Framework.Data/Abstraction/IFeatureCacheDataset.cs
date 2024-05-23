using gView.Framework.Core.Data;
using gView.Framework.Data;
using System.Threading.Tasks;

namespace gView.Framework.Data.Abstraction
{
    public interface IFeatureCacheDataset : IFeatureDataset
    {
        Task<bool> InitFeatureCache(DatasetCachingContext cachingContext);
    }
}
