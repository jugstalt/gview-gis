using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data
{
    public interface IDatasetCachingContext : IDisposable
    {
        T GetCache<T>();
    }
}
