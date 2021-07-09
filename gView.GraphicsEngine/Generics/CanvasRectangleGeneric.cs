using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Generics
{
    public class CanvasRectangleGeneric<T>
    {
        protected CanvasRectangleGeneric(T left, T top, T width, T height)
        {

        }

        public T Left { get; set; }
        public T Top { get; set; }

        public T Width { get; set; }
        public T Height { get; set; }
    }
}
