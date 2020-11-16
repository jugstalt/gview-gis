using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Data.Framework.Data.Abstraction
{
    public interface IFeatureCacheDataset : IFeatureDataset
    {
        Task<bool> InitFeatureCache(DatasetCachingContext cachingContext);
    }
}
