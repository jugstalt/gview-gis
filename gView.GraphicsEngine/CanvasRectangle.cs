using gView.GraphicsEngine.Generics;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    public class CanvasRectangle : CanvasRectangleGeneric<int>
    {
        public CanvasRectangle()
            : base(0, 0, 0, 0)
        { }

        public CanvasRectangle(int left, int top, int width, int height)
            : base(left, top, width, height)
        {
        }

        public CanvasRectangle(CanvasRectangle rectangle)
            : base(rectangle?.Left ?? 0, rectangle?.Top ?? 0,
                   rectangle?.Width ?? 0, rectangle?.Height ?? 0)
        {
        }
    }
}
