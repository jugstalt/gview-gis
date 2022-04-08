using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Carto.Rendering.UI
{
    internal class Helper
    {
        static public void AlterSymbolColor(ISymbol symbol, ArgbColor from, ArgbColor to, double fac)
        {
            ArgbColor col = AlterColor(from, to, fac);

            if (symbol is IBrushColor)
            {
                ((IBrushColor)symbol).FillColor = col;
            }

            if (symbol is IFontColor)
            {
                ((IFontColor)symbol).FontColor = col;
            }

            if (symbol is IPenColor)
            {
                ((IPenColor)symbol).PenColor = col;
            }
        }

        static public ArgbColor AlterColor(ArgbColor from, ArgbColor to, double fac)
        {
            if (fac < 0.0)
            {
                return from;
            }

            if (fac > 1.0)
            {
                return to;
            }

            try
            {
                return ArgbColor.FromArgb(
                    from.A + (int)((double)(to.A - from.A) * fac),
                    from.R + (int)((double)(to.R - from.R) * fac),
                    from.G + (int)((double)(to.G - from.G) * fac),
                    from.B + (int)((double)(to.B - from.B) * fac));
            }
            catch
            {
                return ArgbColor.Transparent;
            }
        }
    }
}
