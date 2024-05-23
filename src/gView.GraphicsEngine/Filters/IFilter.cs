using gView.GraphicsEngine.Abstraction;

namespace gView.GraphicsEngine.Filters
{
    public interface IFilter
    {
        IBitmap Apply(IBitmap bitmap);
        IBitmap Apply(BitmapPixelData bmData);
    }
}
