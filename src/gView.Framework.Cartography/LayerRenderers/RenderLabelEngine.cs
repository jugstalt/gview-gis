using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;

namespace gView.Framework.Cartography.LayerRenderers
{
    public sealed class RenderLabelEngine
    {
        private IDisplay _display;
        private ICancelTracker _cancelTracker;
        private ILabelEngine _labelEngine;

        public RenderLabelEngine(IDisplay display, ILabelEngine labelEngine, ICancelTracker cancelTracker)
        {
            _display = display;
            _cancelTracker = cancelTracker;
            _labelEngine = labelEngine;
        }

        public void Render()
        {
            if (_display == null || _labelEngine == null)
            {
                return;
            }

            _labelEngine.Draw(_display, _cancelTracker);
            _labelEngine.Release();
        }
    }
}
