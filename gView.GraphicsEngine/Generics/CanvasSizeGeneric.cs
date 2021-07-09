using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Generics
{
    public class CanvasSizeGeneric<T>
    {
        protected CanvasSizeGeneric(T width, T height)
        {
            this.Width = width;
            this.Height = height;
        }

        public T Width { get; set; }
        public T Height { get; set; }
    }
}
