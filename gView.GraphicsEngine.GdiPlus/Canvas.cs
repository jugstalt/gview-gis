using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus
{
    class Canvas : ICanvas
    {
        private Graphics _graphics;

        public Canvas(Bitmap bitmap)
        {
            _graphics = Graphics.FromImage(bitmap);
        }

        public void Dispose()
        {
            if(_graphics!=null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
        }
    }
}
