using gView.Framework.Core.Geometry;
using gView.GraphicsEngine.Abstraction;
using System;

namespace gView.Framework.Core.Data
{
    public interface IRasterPaintContext : IDisposable
    {
        IBitmap Bitmap { get; }
    }

    public interface IRasterPointContext2 : IRasterPaintContext
    {
        IPoint PicPoint1 { get; }
        IPoint PicPoint2 { get; }
        IPoint PicPoint3 { get; }
    }
}
