using gView.Framework.Carto;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace gView.Symbology.Framework.Symbology.Extensions
{
    static public class DisplayExtensions
    {
        const int LegendItemSymbolWidth = 20;
        const int LegendItemSymbolHeight = 20;

        static public bool IsLegendItemSymbol(this IDisplay display)
        {
            return display.iWidth == LegendItemSymbolWidth &&
                   display.iHeight == LegendItemSymbolHeight;
        }

        static public Rectangle ToLegendItemSymbolRect(this Rectangle rect)
        {
            rect.X = rect.Y = 0;
            rect.Width = LegendItemSymbolWidth;
            rect.Height = LegendItemSymbolHeight;

            return rect;
        }
    }
}
