using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.GraphicsEngine.Extensions
{
    public static class CanvasSizeExtensions
    {
        public static CanvasSizeF AddPadding(this CanvasSizeF sizeF, IFont font)
        {
            if (font == null)
            {
                return sizeF;
            }

            if (Current.Engine.MeasuresTextWithPadding == false)
            {
                var fontPixelSize = font.Size;

                switch (font.Unit)
                {
                    case GraphicsUnit.Point:
                        fontPixelSize *= Current.Engine.ScreenDpi / 72f;
                        break;
                }

                sizeF.Width += fontPixelSize * 0.4f;
                sizeF.Height += fontPixelSize * 0.2f;
            }

            return sizeF;
        }
    }
}
