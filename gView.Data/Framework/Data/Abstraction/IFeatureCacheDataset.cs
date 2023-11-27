using gView.Framework.Core.Data;
using System.Threading.Tasks;

namespace gView.Data.Framework.Data.Abstraction
{
    public interface IFeatureCacheDataset : IFeatureDataset
    {
        Task<bool> InitFeatureCache(DatasetCachingContext cachingContext);
    }
}
