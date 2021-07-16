using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Filters
{
    // Never change the numbers
    public enum FilterImplementations
    {
        Default = 0,
        GrayscaleBT709 = 1,
        GrayscaleRMY = 2,
        GrayscaleY = 3,
        Channel_Red = 4,
        Channel_Green = 5,
        Channel_Blue = 6,
        //Channel_Alpha = 7 
    }
}
