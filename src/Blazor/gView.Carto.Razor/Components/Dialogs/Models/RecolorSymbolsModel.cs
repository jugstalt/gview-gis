using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Geometry;
using gView.Framework.Symbology.Models;
using gView.Framework.Symbology.Palettes;
using gView.GraphicsEngine;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public enum RecolorMode
{
    Palette,
    Gradient,
    Random
}

public class RecolorSymbolsModel : IDialogResultItem
{
    // info (set by caller)
    public GeometryType GeometryType { get; set; }
    public int ClassCount { get; set; }
    public int SkippedCount { get; set; }
    public bool HasAllOtherValues { get; set; }
    /// <summary>Number of selected classes the dialog applies to (0 = no selection, all classes).</summary>
    public int SelectedCount { get; set; }

    // options
    public RecolorMode Mode { get; set; } = RecolorMode.Palette;
    public string PaletteName { get; set; } = ColorPalettes.All[0].Name;
    public ArgbColor Color1 { get; set; } = ArgbColor.FromArgb(255, 255, 255, 204);
    public ArgbColor Color2 { get; set; } = ArgbColor.FromArgb(255, 128, 0, 38);
    public bool Reverse { get; set; }
    public SymbolColorTarget Target { get; set; } = SymbolColorTarget.Both;
    public bool IncludeAllOtherValues { get; set; }

    public ArgbColor[] GetColors(int count)
    {
        var colors = Mode switch
        {
            RecolorMode.Random => ColorRamp.Random(count),
            RecolorMode.Gradient => ColorRamp.Interpolate([Color1, Color2], count),
            _ => (ColorPalettes.Find(PaletteName) ?? ColorPalettes.All[0]).GetColors(count)
        };

        if (Reverse && Mode != RecolorMode.Random)
        {
            Array.Reverse(colors);
        }

        return colors;
    }
}
