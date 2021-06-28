using gView.GraphicsEngine.Generics;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine
{
    public struct CanvasRectangle
    { 
        public CanvasRectangle(int left, int top, int width, int height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }

        public CanvasRectangle(CanvasRectangle rectangle)
        {
            this.Left = rectangle.Left;
            this.Top = rectangle.Top;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
        }

        public int Left { get; set; }
        public int Top { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
