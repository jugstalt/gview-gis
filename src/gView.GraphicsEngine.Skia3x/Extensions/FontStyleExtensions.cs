using SkiaSharp;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class FontStyleExtensions
    {
        static public SKFontStyle ToSKFontStyle(this FontStyle fontStyle)
        {
            if (fontStyle.HasFlag(FontStyle.Bold) &&
               fontStyle.HasFlag(FontStyle.Italic))
            {
                return SKFontStyle.BoldItalic;
            }

            if (fontStyle.HasFlag(FontStyle.Bold))
            {
                return SKFontStyle.Bold;
            }

            if (fontStyle.HasFlag(FontStyle.Italic))
            {
                return SKFontStyle.Italic;
            }

            return SKFontStyle.Normal;
        }
    }
}
