using gView.GraphicsEngine.Abstraction;
using System;

namespace gView.Framework.Data
{
    public interface IRasterPaintContext : IDisposable
    {
        IBitmap Bitmap { get; }
    }
}
