using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Abstraction
{
    public interface IGraphicsEngine
    {
        IBitmap CreateBitmap(int width, int height);
        IBitmap CreateBitmap(int width, int height, PixelFormat format);
        IBitmap CreateBitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0);

        IPen CreatePen(ArgbColor color, float width);
        IBrush CreateSolidBrush(ArgbColor color);
    }
}
