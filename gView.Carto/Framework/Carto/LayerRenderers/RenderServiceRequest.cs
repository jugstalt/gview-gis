using gView.Framework.Core.Data;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Carto.LayerRenderers
{
    public sealed class RenderServiceRequest
    {
        private Map _map;
        private IWebServiceLayer _layer;
        private int _order = 0;
        private DateTime _startTime, _finishTime;

        public delegate void RequestThreadFinished(RenderServiceRequest sender, bool succeeded, GeorefBitmap image, int order);
        public RequestThreadFinished finish = null;

        public RenderServiceRequest(Map map, IWebServiceLayer layer, int order)
        {
            _map = map;
            _layer = layer;
            _order = order;
        }

        public DateTime StartTime
        {
            get { return _startTime; }
        }
        public DateTime FinishTime
        {
            get { return _finishTime; }
        }
        public IWebServiceLayer WebServiceLayer
        {
            get { return _layer; }
        }

        // Thread
        async public Task ImageRequest()
        {
            _startTime = DateTime.Now;

            if (_layer == null || _layer.WebServiceClass == null)
            {
                _finishTime = DateTime.Now;
                if (finish != null)
                {
                    finish(this, false, null, 0);
                }

                return;
            }

            if (await _layer.WebServiceClass.MapRequest(_map.Display))
            {
                _finishTime = DateTime.Now;
                if (finish != null)
                {
                    finish(this, true, _layer.WebServiceClass.Image, _order);
                }
            }
            else
            {
                _finishTime = DateTime.Now;
                if (finish != null)
                {
                    finish(this, false, null, _order);
                }
            }
        }
    }
}
