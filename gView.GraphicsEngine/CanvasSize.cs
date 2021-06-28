using gView.GraphicsEngine.Generics;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    public struct CanvasSize
    {
        public CanvasSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
