using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class StringAlignmentExtensions
    {
        static public SKTextAlign ToSkTextAlign(this StringAlignment align)
        {
            switch(align)
            {
                case StringAlignment.Near:
                    return SKTextAlign.Left;
                case StringAlignment.Far:
                    return SKTextAlign.Right;
                default:
                    return SKTextAlign.Center;
            }
        }

        static public StringAlignment ToStringAlignment(this SKTextAlign align)
        {
            switch (align)
            {
                case SKTextAlign.Left:
                    return StringAlignment.Near;
                case SKTextAlign.Right:
                    return StringAlignment.Far;
                default:
                    return StringAlignment.Center;
            }
        }
    }
}
