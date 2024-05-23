namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class FontStyleExtensions
    {
        static public System.Drawing.FontStyle ToGdiFontStyle(this FontStyle fontStyle)
        {
            var result = System.Drawing.FontStyle.Regular;

            if (fontStyle.HasFlag(FontStyle.Bold))
            {
                result |= System.Drawing.FontStyle.Bold;
            }

            if (fontStyle.HasFlag(FontStyle.Italic))
            {
                result |= System.Drawing.FontStyle.Italic;
            }

            if (fontStyle.HasFlag(FontStyle.Strikeout))
            {
                result |= System.Drawing.FontStyle.Strikeout;
            }

            if (fontStyle.HasFlag(FontStyle.Underline))
            {
                result |= System.Drawing.FontStyle.Underline;
            }

            return result;
        }
    }
}
