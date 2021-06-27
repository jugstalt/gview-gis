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
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
