using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class FontStyleExtensions
    {
        static public SKFontStyle ToSKFontStyle(this FontStyle fontStyle)
        {
            switch(fontStyle)
            {
                case FontStyle.Bold:
                    return SKFontStyle.Bold;
                case FontStyle.Italic:
                    return SKFontStyle.Italic;
                case FontStyle.Bold | FontStyle.Italic:
                    return SKFontStyle.BoldItalic;
                default:
                    return SKFontStyle.Normal;
            }
        }
    }
}
