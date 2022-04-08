using gView.GraphicsEngine.Abstraction;

namespace gView.Framework.Data
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
}
