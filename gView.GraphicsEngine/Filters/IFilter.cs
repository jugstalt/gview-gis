using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Filters
{
    public interface IFilter
    {
        IBitmap Apply(IBitmap bitmap);
        IBitmap Apply(BitmapPixelData bmData);
    }
}
