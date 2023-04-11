using gView.Framework.Geometry;
using gView.GraphicsEngine.Abstraction;
using System;

namespace gView.Framework.Data
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
