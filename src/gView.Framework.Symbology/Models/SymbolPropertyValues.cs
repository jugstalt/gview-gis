#nullable enable

using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Symbology.Models;

/// <summary>
/// Values to apply to many symbols at once. Only properties that are not null are applied.
/// </summary>
public class SymbolPropertyValues
{
    public ArgbColor? Color { get; set; }
    /// <summary>Line width, or outline width for polygons.</summary>
    public float? Width { get; set; }
    public LineDashStyle? DashStyle { get; set; }
    /// <summary>Fill opacity of polygons in percent (0..100).</summary>
    public float? Opacity { get; set; }
    /// <summary>Point symbol size.</summary>
    public float? Size { get; set; }
    public SymbolSmoothing? Smoothing { get; set; }

    public bool IsEmpty
        => Color is null
        && Width is null
        && DashStyle is null
        && Opacity is null
        && Size is null
        && Smoothing is null;
}
