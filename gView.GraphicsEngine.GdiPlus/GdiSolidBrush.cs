using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System.Drawing;

namespace gView.GraphicsEngine.GdiPlus
{
    class GdiSolidBrush : IBrush
    {
        private SolidBrush _brush;

        public GdiSolidBrush(ArgbColor color)
        {
            _brush = new SolidBrush(color.ToGdiColor());
        }

        public object EngineElement => _brush;

        public ArgbColor Color
        {
            get { return _brush.Color.ToArgbColor(); }
            set { _brush.Color = value.ToGdiColor(); }
        }

        public void Dispose()
        {
            if (_brush != null)
            {
                _brush.Dispose();
                _brush = null;
            }
        }
    }
}
