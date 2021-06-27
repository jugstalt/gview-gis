using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus
{
    class GdiPen : IPen
    {
        private Pen _pen;

        public GdiPen(ArgbColor color, float width)
        {
            _pen = new Pen(color.ToGdiColor(), width);
        }

        public void Dispose()
        {
            if (_pen != null)
            {
                _pen.Dispose();
                _pen = null;
            }
        }
    }
}
