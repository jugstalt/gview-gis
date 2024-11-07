using System;

namespace gView.Framework.Core.Data
{
    public interface IDatasetCachingContext : IDisposable
    {
        T GetCache<T>();
    }
}
