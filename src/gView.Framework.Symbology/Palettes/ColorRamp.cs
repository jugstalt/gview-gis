#nullable enable

using gView.GraphicsEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Symbology.Palettes;

public static class ColorRamp
{
    /// <summary>
    /// Returns <paramref name="count"/> colours evenly spread along the stops,
    /// interpolated linearly in RGB(A). The first and last colour are the first and last stop.
    /// A single colour is taken from the middle of the ramp.
    /// </summary>
    public static ArgbColor[] Interpolate(IReadOnlyList<ArgbColor> stops, int count)
    {
        ArgumentNullException.ThrowIfNull(stops);
        if (stops.Count == 0)
        {
            throw new ArgumentException("At least one colour stop is required", nameof(stops));
        }

        if (count <= 0)
        {
            return [];
        }

        if (stops.Count == 1)
        {
            return Enumerable.Repeat(stops[0], count).ToArray();
        }

        var colors = new ArgbColor[count];
        for (int i = 0; i < count; i++)
        {
            double t = count == 1 ? 0.5 : (double)i / (count - 1);
            colors[i] = At(stops, t);
        }

        return colors;
    }

    /// <summary>
    /// Returns <paramref name="count"/> colours taken in order from <paramref name="colors"/>,
    /// starting again at the first colour when the list is used up.
    /// </summary>
    public static ArgbColor[] Cycle(IReadOnlyList<ArgbColor> colors, int count)
    {
        ArgumentNullException.ThrowIfNull(colors);
        if (colors.Count == 0)
        {
            throw new ArgumentException("At least one colour is required", nameof(colors));
        }

        if (count <= 0)
        {
            return [];
        }

        return Enumerable.Range(0, count)
                         .Select(i => colors[i % colors.Count])
                         .ToArray();
    }

    /// <summary>
    /// Returns <paramref name="count"/> opaque random colours.
    /// </summary>
    public static ArgbColor[] Random(int count, Random? random = null)
    {
        random ??= System.Random.Shared;

        return Enumerable.Range(0, Math.Max(count, 0))
                         .Select(_ => ArgbColor.FromArgb(255, random.Next(256), random.Next(256), random.Next(256)))
                         .ToArray();
    }

    private static ArgbColor At(IReadOnlyList<ArgbColor> stops, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);

        double position = t * (stops.Count - 1);
        int index = Math.Min((int)Math.Floor(position), stops.Count - 2);
        double fraction = position - index;

        var from = stops[index];
        var to = stops[index + 1];

        return ArgbColor.FromArgb(
            Lerp(from.A, to.A, fraction),
            Lerp(from.R, to.R, fraction),
            Lerp(from.G, to.G, fraction),
            Lerp(from.B, to.B, fraction));
    }

    private static int Lerp(byte from, byte to, double fraction)
        => (int)Math.Round(from + (to - from) * fraction);
}
