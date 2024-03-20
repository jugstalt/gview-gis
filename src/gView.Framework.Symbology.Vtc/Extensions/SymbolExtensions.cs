using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;
using gView.GraphicsEngine;

namespace gView.Framework.Symbology.Vtc.Extensions;

static internal class SymbolExtensions
{
    static public void ModifyStyles(this IPointSymbol pointSymbol, Dictionary<string, IValueFunc> valueFuncs, IDisplay display, IFeature? feature)
    {
        pointSymbol.SymbolSmoothingMode =
               valueFuncs.GetValueOrDeafult(GLStyleProperties.LineOpacity, true, display, feature)
               ? SymbolSmoothing.AntiAlias
               : SymbolSmoothing.None;

        if(pointSymbol is IIconSymbol icon)
        {
            var iconName = valueFuncs.GetValueOrDeafult<string>(GLStyleProperties.IconImage, "", display, feature);
            if(iconName!=null)
            {
                icon.Filename = $"resource:{iconName}@2x";
                icon.SizeX = 32;
                icon.SizeY = 32;
            }
        }
    }

    static public void ModifyStyles(this ILineSymbol lineSymbol, Dictionary<string, IValueFunc> valueFuncs, IDisplay display, IFeature? feature)
    {
        lineSymbol.SymbolSmoothingMode =
                valueFuncs.GetValueOrDeafult(GLStyleProperties.LineOpacity, true, display, feature)
                ? SymbolSmoothing.AntiAlias
                : SymbolSmoothing.None;

        float opacity = valueFuncs.GetValueOrDeafult(GLStyleProperties.LineOpacity, 1f, display, feature);
        float[]? dashArray = valueFuncs.GetValueOrDeafult<float[]?>(GLStyleProperties.LineDashArray, null, display, feature);

        if (lineSymbol is IPenWidth penWidth)
        {
            penWidth.PenWidth = valueFuncs.GetValueOrDeafult(GLStyleProperties.LineWidth, penWidth.PenWidth, display, null);
        }

        if (lineSymbol is IPenColor penColor)
        {
            penColor.PenColor = valueFuncs.GetValueOrDeafult(GLStyleProperties.LineColor, penColor.PenColor, display, null);

            if (opacity != 1f)
            {
                penColor.PenColor = ArgbColor.FromArgb((int)(255 * opacity), penColor.PenColor);
            }
        }

        if (lineSymbol is IPenDashStyle dashStyle)
        {
            dashStyle.PenDashStyle = dashArray.ToDashStale();
        }
    }

    static public void ModifyStyles(this IFillSymbol fillSymbol, Dictionary<string, IValueFunc> valueFuncs, IDisplay display, IFeature? feature)
    {
        fillSymbol.SymbolSmoothingMode =
                valueFuncs.GetValueOrDeafult(GLStyleProperties.FillOpacity, true, display, feature)
                ? SymbolSmoothing.AntiAlias
                : SymbolSmoothing.None;

        float opacity = valueFuncs.GetValueOrDeafult(GLStyleProperties.FillOpacity, 1f, display, feature);

        if (fillSymbol is IBrushColor brushColor)
        {
            brushColor.FillColor
                = valueFuncs.GetValueOrDeafult([GLStyleProperties.FillColor, GLStyleProperties.FillExtrusionColor],
                        brushColor.FillColor,
                        display, feature);

            if (opacity != 1f)
            {
                brushColor.FillColor = ArgbColor.FromArgb((int)(255 * opacity), brushColor.FillColor);
            }
        }

        #region Outline

        if (fillSymbol is IPenWidth penWidth)
        {
            penWidth.PenWidth = valueFuncs.GetValueOrDeafult(GLStyleProperties.FillOutlineWidth, penWidth.PenWidth, display, null);
        }

        if (fillSymbol is IPenColor penColor)
        {
            penColor.PenColor = valueFuncs.GetValueOrDeafult(GLStyleProperties.FillOutlineColor, penColor.PenColor, display, null);

            float outlineOpacity = valueFuncs.GetValueOrDeafult(GLStyleProperties.FillOutlineOpacity, 1f, display, feature);
            if (outlineOpacity != 1f)
            {
                penColor.PenColor = ArgbColor.FromArgb((int)(255 * outlineOpacity), penColor.PenColor);
            }
        }

        if (fillSymbol is IPenDashStyle dashStyle)
        {
            float[]? dashArray = valueFuncs.GetValueOrDeafult<float[]?>(GLStyleProperties.FillOutlineDashArray, null, display, feature);
            dashStyle.PenDashStyle = dashArray.ToDashStale();
        }

        #endregion
    }

    static internal T GetValueOrDeafult<T>(this Dictionary<string, IValueFunc> valueFuncs, string name, T defaultValue, IDisplay display, IFeature? feature)
    {
        if (!valueFuncs.ContainsKey(name)) return defaultValue;

        return valueFuncs[name].Value<T>(display, feature) ?? defaultValue;
    }

    static internal T GetValueOrDeafult<T>(this Dictionary<string, IValueFunc> valueFuncs, string[] names, T defaultValue, IDisplay display, IFeature? feature)
    {
        foreach (var name in names)
        {
            if (valueFuncs.ContainsKey(name))
            {
                return valueFuncs[name].Value<T>(display, feature) ?? defaultValue;
            }
        }

        return defaultValue;
    }
}
