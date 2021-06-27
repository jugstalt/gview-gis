using gView.GraphicsEngine.Generics;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    public class CanvasRectangleF : CanvasRectangleGeneric<float>
    {
        public CanvasRectangleF()
            : base(0f, 0f, 0f, 0f)
        { }

        public CanvasRectangleF(float left, float top, float width, float height)
            : base(left, top, width, height)
        {
        }

        public CanvasRectangleF(CanvasRectangleF rectangleF)
            : base(rectangleF?.Left ?? 0f, rectangleF?.Top ?? 0f,
                   rectangleF?.Width ?? 0f, rectangleF?.Height ?? 0f)
        {
        }
    }
}
