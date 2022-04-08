using gView.Data.Framework.Data.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Data;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace gView.Data.Framework.Data
{
    public class DatasetCachingContext : IDatasetCachingContext
    {
        private ConcurrentBag<IDatasetCache> _caches;

        public DatasetCachingContext(IMap map)
        {
            this.Map = map;
            this._caches = new ConcurrentBag<IDatasetCache>();
        }

        public IMap Map { get; }

        public void AddCache(IDatasetCache datasetCache)
        {
            if (_caches == null)
            {
                throw new Exception("DatasetCachcontext already disposed");
            }

            if (datasetCache != null)
            {
                _caches.Add(datasetCache);
            }
        }

        public T GetCache<T>()
        {
            return (T)_caches.Where(c => c.GetType().Equals(typeof(T))).FirstOrDefault();
        }

        #region IDisposable

        public void Dispose()
        {
            foreach (var cache in _caches)
            {
                try
                {
                    cache.Dispose();
                }
                catch { }
            }

            _caches = null;
        }

        #endregion
    }
}
