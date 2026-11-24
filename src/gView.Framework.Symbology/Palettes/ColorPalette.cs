#nullable enable

using gView.GraphicsEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Symbology.Palettes;

public sealed class ColorPalette
{
    public ColorPalette(string name, ColorPaletteType type, params string[] hexColors)
    {
        if (hexColors.Length == 0)
        {
            throw new ArgumentException("A palette needs at least one colour", nameof(hexColors));
        }

        Name = name;
        Type = type;
        Colors = hexColors.Select(ArgbColor.FromHexString).ToArray();
    }

    public string Name { get; }
    public ColorPaletteType Type { get; }
    public IReadOnlyList<ArgbColor> Colors { get; }

    /// <summary>
    /// Colours for <paramref name="count"/> classes: sequential and diverging palettes
    /// are interpolated, qualitative palettes are repeated cyclically.
    /// </summary>
    public ArgbColor[] GetColors(int count, bool reverse = false)
    {
        var colors = Type == ColorPaletteType.Qualitative
            ? ColorRamp.Cycle(Colors, count)
            : ColorRamp.Interpolate(Colors, count);

        if (reverse)
        {
            Array.Reverse(colors);
        }

        return colors;
    }

    public override string ToString() => Name;
}
