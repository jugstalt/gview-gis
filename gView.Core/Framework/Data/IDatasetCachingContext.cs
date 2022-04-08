using System;

namespace gView.Framework.Data
{
    public interface IDatasetCachingContext : IDisposable
    {
        T GetCache<T>();
    }
}
