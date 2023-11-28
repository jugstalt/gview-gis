using System.Collections.Generic;
using System.Threading.Tasks;
using gView.Framework.Core.Data;

namespace gView.Framework.Data.Cursors
{
    public class SimpleRasterlayerCursor : IRasterLayerCursor
    {
        private List<IRasterLayer> _layers;
        private int _pos = 0;

        public SimpleRasterlayerCursor(List<IRasterLayer> layers)
        {
            _layers = layers;
        }

        #region IRasterLayerCursor Member

        public Task<IRasterLayer> NextRasterLayer()
        {
            if (_layers == null || _pos >= _layers.Count)
            {
                return null;
            }

            return Task.FromResult<IRasterLayer>(_layers[_pos++]);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {

        }

        #endregion
    }
}
