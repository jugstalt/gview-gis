using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class LineCapExtensions
    {
        static public SKStrokeCap ToSKStrokeCap(this LineCap lineCap)
        {
            switch(lineCap)
            {
                case LineCap.Round:
                    return SKStrokeCap.Round;
                case LineCap.Square:
                    return SKStrokeCap.Square;
                default:
                    return SKStrokeCap.Butt;
            }
        }

        static public LineCap ToLineCap(this SKStrokeCap strokeCap)
        {
            switch(strokeCap)
            {
                case SKStrokeCap.Round:
                    return LineCap.Round;
                case SKStrokeCap.Square:
                    return LineCap.Square;
                default:
                    return LineCap.Flat;
            }
        }
    }
}
