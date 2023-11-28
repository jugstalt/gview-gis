using System.Collections.Generic;
using System.Threading.Tasks;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;

namespace gView.Framework.Data.Cursors
{
    public class SimpleFeatureCursor : IFeatureCursor
    {
        private List<IFeature> _features;
        private int _pos = 0;

        public SimpleFeatureCursor(List<IFeature> features)
        {
            _features = features;
        }

        #region IFeatureCursor Member

        public Task<IFeature> NextFeature()
        {
            if (_features == null || _pos >= _features.Count)
            {
                return Task.FromResult<IFeature>(null);
            }

            return Task.FromResult<IFeature>(_features[_pos++]);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }
}
