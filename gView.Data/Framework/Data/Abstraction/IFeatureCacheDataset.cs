using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Data.Framework.Data.Abstraction
{
    public interface IFeatureCacheDataset
    {
        Task<bool> InitFeatureCache(DatasetCachingContext cachingContext);
    }
}
