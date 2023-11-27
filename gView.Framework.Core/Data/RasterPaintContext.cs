using gView.Framework.Core.Geometry;
using gView.GraphicsEngine.Abstraction;

namespace gView.Framework.Core.Data
{
    public class RasterPaintContext : IRasterPaintContext
    {
        private IBitmap _bitmap;

        public RasterPaintContext(IBitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public IBitmap Bitmap => _bitmap;

        public virtual void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }

    public class RasterPaintContext2 : RasterPaintContext, IRasterPointContext2
    {
        public RasterPaintContext2(IBitmap bitmap)
            : base(bitmap)
        {
        }

        public IPoint PicPoint1 { get; set; }
        public IPoint PicPoint2 { get; set; }
        public IPoint PicPoint3 { get; set; }
    }
}
