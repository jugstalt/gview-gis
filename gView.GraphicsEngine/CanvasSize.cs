using gView.GraphicsEngine.Generics;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    public class CanvasSize : CanvasSizeGeneric<int>
    {
        public CanvasSize()
            : base(0,0)
        { }

        public CanvasSize(int width, int height)
            : base(width, height)
        { }
    }
}
