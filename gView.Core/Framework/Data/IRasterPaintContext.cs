using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Data
{
    public interface IRasterPaintContext : IDisposable
    {
        IBitmap Bitmap { get; }
    }
}
