using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class InterpolationModeMethods
    {
        static public SKFilterQuality ToSKFilterQuality(this InterpolationMode interpolation)
        {
            switch (interpolation)
            {
                case InterpolationMode.Low:
                    return SKFilterQuality.Low;
                case InterpolationMode.NearestNeighbor:
                    return SKFilterQuality.None;
                case InterpolationMode.Bilinear:
                case InterpolationMode.HighQualityBilinear:
                    return SKFilterQuality.Medium;
                case InterpolationMode.Bicubic:
                case InterpolationMode.HighQualityBicubic:
                    return SKFilterQuality.High;
            }

            return SKFilterQuality.None;
        }
    }
}
