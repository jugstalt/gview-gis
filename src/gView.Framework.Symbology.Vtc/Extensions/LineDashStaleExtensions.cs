using gView.GraphicsEngine;

namespace gView.Framework.Symbology.Vtc.Extensions;

static internal class LineDashStaleExtensions
{
    static public LineDashStyle ToDashStale(this float[]? array)
    {
        if (array == null || array.Length == 0 || array.Length == 1)
            return LineDashStyle.Solid;

        if (array.Length == 2)
        {
            return (array[0], array[1]) switch
            {
                ( <= 1, <= 1) => LineDashStyle.Dot,
                _ => LineDashStyle.Dash
            };
        }

        if (array.Length == 3) return LineDashStyle.DashDot;

        // array.lenth > 3
        return LineDashStyle.DashDotDot;
    }
}
