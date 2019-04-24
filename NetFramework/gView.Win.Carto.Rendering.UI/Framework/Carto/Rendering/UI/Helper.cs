using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Symbology;
using System.Drawing;

namespace gView.Framework.Carto.Rendering.UI
{
    internal class Helper
    {
        static public void AlterSymbolColor(ISymbol symbol, Color from, Color to, double fac)
        {
            Color col = AlterColor(from, to, fac);

            if (symbol is IBrushColor)
                ((IBrushColor)symbol).FillColor = col;
            if (symbol is IFontColor)
                ((IFontColor)symbol).FontColor = col;
            if (symbol is IPenColor)
                ((IPenColor)symbol).PenColor = col;
        }

        static public Color AlterColor(Color from, Color to, double fac)
        {
            if (fac < 0.0) return from;
            if (fac > 1.0) return to;

            try
            {
                return Color.FromArgb(
                    from.A + (int)((double)(to.A - from.A) * fac),
                    from.R + (int)((double)(to.R - from.R) * fac),
                    from.G + (int)((double)(to.G - from.G) * fac),
                    from.B + (int)((double)(to.B - from.B) * fac));
            }
            catch
            {
                return Color.Transparent;
            }
        }
    }
}
