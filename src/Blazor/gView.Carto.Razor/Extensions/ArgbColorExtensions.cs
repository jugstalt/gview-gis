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

    static public string ToCssHex(this ArgbColor color)
        => $"#{color.R:x2}{color.G:x2}{color.B:x2}";

    /// <summary>
    /// CSS linear-gradient (left to right) for a colour bar. With <paramref name="hardStops"/>
    /// every colour gets an equally wide band (qualitative palettes).
    /// </summary>
    static public string ToCssLinearGradient(this IReadOnlyList<ArgbColor> colors, bool hardStops = false)
    {
        if (colors.Count == 0)
        {
            return "transparent";
        }
        if (colors.Count == 1)
        {
            return colors[0].ToCssHex();
        }

        var stops = hardStops
            ? colors.Select((c, i) => FormattableString.Invariant(
                $"{c.ToCssHex()} {100.0 * i / colors.Count:0.##}% {100.0 * (i + 1) / colors.Count:0.##}%"))
            : colors.Select(c => c.ToCssHex());

        return $"linear-gradient(to right, {String.Join(", ", stops)})";
    }
}
