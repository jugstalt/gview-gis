using gView.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.GeoJson
{
    class GeoJsonFeatureCursor : IFeatureCursor
    {
        private readonly IFeature[] _features;
        private readonly IQueryFilter _filter;
        private int pos = 0;

        public GeoJsonFeatureCursor(IEnumerable<IFeature> features, IQueryFilter filter)
        {
            _features = features?.ToArray();
            _filter = filter;
        }

        #region IFeatureCursor

        public void Dispose()
        {
            
        }

        public Task<IFeature> NextFeature()
        {
            if (_features == null || _features.Length >= pos)
                return Task.FromResult<IFeature>(null);

            var feature = _features[pos++];

            // ToDO: Check Filter

            return Task.FromResult<IFeature>(feature);
        }

        #endregion
    }
}
