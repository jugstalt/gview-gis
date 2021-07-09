using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.GdiPlus.Extensions
{
    static class SmothingModeExtensions
    {
        public static System.Drawing.Drawing2D.SmoothingMode ToGdiSmoothingMode(this SmoothingMode mode)
        {
            switch(mode)
            {
                case SmoothingMode.AntiAlias:
                    return System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                case SmoothingMode.None:
                    return System.Drawing.Drawing2D.SmoothingMode.None;
                default:
                    return System.Drawing.Drawing2D.SmoothingMode.Default;
            }
        }

        public static SmoothingMode ToSmoothingMode(this System.Drawing.Drawing2D.SmoothingMode mode)
        {
            switch (mode)
            {
                case System.Drawing.Drawing2D.SmoothingMode.AntiAlias:
                    return SmoothingMode.AntiAlias;
                case System.Drawing.Drawing2D.SmoothingMode.None:
                    return SmoothingMode.None;
                default:
                    return SmoothingMode.Default;
            }
        }
    }
}
