namespace gView.Framework.Symbology.Palettes;

public enum ColorPaletteType
{
    /// <summary>Ordered from light to dark (or along a perceptual ramp); interpolated to any class count.</summary>
    Sequential,
    /// <summary>Two ramps meeting in a neutral middle; interpolated to any class count.</summary>
    Diverging,
    /// <summary>Distinct colours without order; repeated cyclically when there are more classes than colours.</summary>
    Qualitative
}
