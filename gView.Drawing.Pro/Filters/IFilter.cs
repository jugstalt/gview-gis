using gView.GraphicsEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace gView.Drawing.Pro.Filters
{
    public interface IFilter
    {
        Bitmap Apply(Bitmap bitmap);
        Bitmap Apply(BitmapData bmData);
        //Bitmap Apply(BitmapPixelData bmData);
    }
}
