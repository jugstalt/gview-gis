using gView.GraphicsEngine.Generics;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    public class CanvasSizeF : CanvasSizeGeneric<float>
    {
        public CanvasSizeF()
            : base(0f, 0f)
        { }

        public CanvasSizeF(float width, float height)
            : base(width, height)
        { }
    }
}
