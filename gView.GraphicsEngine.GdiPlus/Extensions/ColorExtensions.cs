using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class ColorExtensions
    {
        static public Color ToGdiColor(this ArgbColor color)
        {
            return Color.FromArgb(color.ToArgb());
        }

        static public ArgbColor ToArgbColor(this Color color)
        {
            return ArgbColor.FromArgb(color.ToArgb());
        }
    }
}
