using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
