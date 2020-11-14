using gView.Data.Framework.Data.Abstraction;
using gView.Framework.Carto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace gView.Data.Framework.Data
{
    public class DatasetCachingContext : IDisposable
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
                throw new Exception("DatasetCachcontext already disposed");

            if (datasetCache != null)
            {
                _caches.Add(datasetCache);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            foreach(var cache in _caches)
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
