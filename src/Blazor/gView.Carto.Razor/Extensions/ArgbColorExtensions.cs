using gView.GraphicsEngine;

namespace gView.Carto.Razor.Extensions;
internal static class ArgbColorExtensions
{
    static public ArgbColor AlterTo(this ArgbColor from, ArgbColor to, float fac)
    {
        if (fac < 0.0f)
        {
            return from;
        }

        if (fac > 1.0f)
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
