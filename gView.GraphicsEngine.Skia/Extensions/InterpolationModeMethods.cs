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
            switch(interpolation)
            {
                case InterpolationMode.NearestNeighbor:
                    return SKFilterQuality.None;
                case InterpolationMode.Bilinear:
                    return SKFilterQuality.Low;
                case InterpolationMode.HighQualityBilinear:
                    return SKFilterQuality.Medium;
                case InterpolationMode.HighQualityBicubic:
                    return SKFilterQuality.High;
            }

            return SKFilterQuality.Medium;
        }

        static public InterpolationMode ToInterpolationMode(this SKFilterQuality interpolation)
        {
            switch (interpolation)
            {
                case SKFilterQuality.None:
                    return InterpolationMode.NearestNeighbor;
                case SKFilterQuality.Low:
                    return InterpolationMode.Bilinear;
                case SKFilterQuality.Medium:
                    return InterpolationMode.HighQualityBilinear;
                case SKFilterQuality.High:
                    return InterpolationMode.HighQualityBicubic;
            }

            return InterpolationMode.HighQualityBilinear;
        }
    }
}
