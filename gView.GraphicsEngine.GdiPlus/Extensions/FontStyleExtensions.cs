using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class FontStyleExtensions
    {
        static public System.Drawing.FontStyle ToGdiFontStyle(this FontStyle fontStyle)
        {
            switch(fontStyle)
            {
                case FontStyle.Regular:
                    return System.Drawing.FontStyle.Regular;
                case FontStyle.Bold:
                    return System.Drawing.FontStyle.Bold;
                case FontStyle.Italic:
                    return System.Drawing.FontStyle.Italic;
                case FontStyle.Strikeout:
                    return System.Drawing.FontStyle.Strikeout;
                case FontStyle.Underline:
                    return System.Drawing.FontStyle.Underline;
            }

            return System.Drawing.FontStyle.Regular;
        }
    }
}
