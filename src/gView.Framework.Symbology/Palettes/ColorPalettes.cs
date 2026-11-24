#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Symbology.Palettes;

/// <summary>
/// Predefined colour palettes for classified renderers.
/// Viridis: matplotlib colormap (CC0).
/// All others: ColorBrewer 2.0 by Cynthia A. Brewer, Pennsylvania State University (Apache License 2.0).
/// </summary>
public static class ColorPalettes
{
    public static readonly IReadOnlyList<ColorPalette> All =
    [
        // Sequential
        new ColorPalette("Viridis", ColorPaletteType.Sequential,
            "440154", "472d7b", "3b528b", "2c728e", "21918c", "28ae80", "5ec962", "addc30", "fde725"),
        new ColorPalette("Blues", ColorPaletteType.Sequential,
            "f7fbff", "deebf7", "c6dbef", "9ecae1", "6baed6", "4292c6", "2171b5", "08519c", "08306b"),
        new ColorPalette("Greens", ColorPaletteType.Sequential,
            "f7fcf5", "e5f5e0", "c7e9c0", "a1d99b", "74c476", "41ab5d", "238b45", "006d2c", "00441b"),
        new ColorPalette("Reds", ColorPaletteType.Sequential,
            "fff5f0", "fee0d2", "fcbba1", "fc9272", "fb6a4a", "ef3b2c", "cb181d", "a50f15", "67000d"),
        new ColorPalette("YlOrRd", ColorPaletteType.Sequential,
            "ffffcc", "ffeda0", "fed976", "feb24c", "fd8d3c", "fc4e2a", "e31a1c", "bd0026", "800026"),

        // Diverging
        new ColorPalette("RdYlGn", ColorPaletteType.Diverging,
            "a50026", "d73027", "f46d43", "fdae61", "fee08b", "ffffbf", "d9ef8b", "a6d96a", "66bd63", "1a9850", "006837"),
        new ColorPalette("RdBu", ColorPaletteType.Diverging,
            "67001f", "b2182b", "d6604d", "f4a582", "fddbc7", "f7f7f7", "d1e5f0", "92c5de", "4393c3", "2166ac", "053061"),

        // Qualitative
        new ColorPalette("Set1", ColorPaletteType.Qualitative,
            "e41a1c", "377eb8", "4daf4a", "984ea3", "ff7f00", "ffff33", "a65628", "f781bf", "999999"),
        new ColorPalette("Set3", ColorPaletteType.Qualitative,
            "8dd3c7", "ffffb3", "bebada", "fb8072", "80b1d3", "fdb462", "b3de69", "fccde5", "d9d9d9", "bc80bd", "ccebc5", "ffed6f"),
        new ColorPalette("Paired", ColorPaletteType.Qualitative,
            "a6cee3", "1f78b4", "b2df8a", "33a02c", "fb9a99", "e31a1c", "fdbf6f", "ff7f00", "cab2d6", "6a3d9a", "ffff99", "b15928"),
    ];

    public static ColorPalette? Find(string? name)
        => All.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
