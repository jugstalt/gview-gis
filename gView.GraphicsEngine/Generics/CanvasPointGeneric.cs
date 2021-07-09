using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Generics
{
    public class CanvasPointGeneric<T>
    {
        protected CanvasPointGeneric(T x, T y)
        {
            this.X = x;
            this.Y = y;
        }
        public T X { get; set; }
        public T Y { get; set; }
    }
}
