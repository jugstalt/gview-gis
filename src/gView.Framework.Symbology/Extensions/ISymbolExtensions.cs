#nullable enable

using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Models;
using gView.Framework.Symbology.UI.Abstractions;
using gView.GraphicsEngine;
using System;
using System.Linq;

namespace gView.Framework.Symbology.Extensions;

public static class ISymbolExtensions
{
    public static GeometryType ToGeometryType(this ISymbol symbol)
        => symbol switch
        {
            ISymbolCollection collection 
                => collection.Symbols.FirstOrDefault()?.Symbol?.ToGeometryType() ?? GeometryType.Unknown,IFillSymbol => GeometryType.Polygon,
            ILineSymbol => GeometryType.Polyline,
            IPointSymbol => GeometryType.Point,
            ITextSymbol => GeometryType.Point,
            _ => GeometryType.Unknown
        };

    public static ISymbol AddLegendLabel(this ISymbol symbol, string legendLabel)
    {
        if(symbol is ILegendItem legendItem)
        {
            symbol.LegendLabel = legendLabel;
        }

        return symbol;
    }

    /// <summary>
    /// True for simple symbols (the ones that offer quick properties) that bulk edits can change.
    /// Symbol collections and other complex symbols return false and are skipped.
    /// </summary>
    public static bool CanBulkEdit(this ISymbol? symbol)
        => symbol is IQuickSymolPropertyProvider
        && symbol is not ISymbolCollection
        && symbol.ToGeometryType() != GeometryType.Unknown;

    /// <summary>
    /// Replaces the RGB part of the symbol colour and keeps each colour's alpha.
    /// Polygons colour fill and/or outline (<paramref name="target"/>),
    /// lines their pen and points their fill or font colour.
    /// </summary>
    /// <returns>false, if the symbol can't be bulk edited</returns>
    public static bool ApplyColor(this ISymbol symbol, ArgbColor color, SymbolColorTarget target = SymbolColorTarget.Both)
    {
        if (!symbol.CanBulkEdit())
        {
            return false;
        }

        switch (symbol.ToGeometryType())
        {
            case GeometryType.Polygon:
                if (target.HasFlag(SymbolColorTarget.Fill) && symbol is IBrushColor fill)
                {
                    fill.FillColor = ArgbColor.FromArgb(fill.FillColor.A, color);
                }
                if (target.HasFlag(SymbolColorTarget.Outline) && symbol is IPenColor outline)
                {
                    outline.PenColor = ArgbColor.FromArgb(outline.PenColor.A, color);
                }
                break;
            case GeometryType.Polyline:
                if (symbol is IPenColor pen)
                {
                    pen.PenColor = ArgbColor.FromArgb(pen.PenColor.A, color);
                }
                break;
            case GeometryType.Point:
                if (symbol is IBrushColor brush)
                {
                    brush.FillColor = ArgbColor.FromArgb(brush.FillColor.A, color);
                }
                else if (symbol is IFontColor font)
                {
                    font.FontColor = ArgbColor.FromArgb(font.FontColor.A, color);
                }
                break;
        }

        return true;
    }

    /// <summary>
    /// Applies every value that is set. A colour is applied like <see cref="ApplyColor"/> with fill and outline.
    /// </summary>
    /// <returns>false, if the symbol can't be bulk edited</returns>
    public static bool ApplyProperties(this ISymbol symbol, SymbolPropertyValues values)
    {
        if (!symbol.CanBulkEdit())
        {
            return false;
        }

        if (values.Color.HasValue)
        {
            symbol.ApplyColor(values.Color.Value, SymbolColorTarget.Both);
        }
        if (values.Width.HasValue && symbol is IPenWidth penWidth)
        {
            penWidth.PenWidth = values.Width.Value;
        }
        if (values.DashStyle.HasValue && symbol is IPenDashStyle penDashStyle)
        {
            penDashStyle.PenDashStyle = values.DashStyle.Value;
        }
        if (values.Opacity.HasValue
            && symbol.ToGeometryType() == GeometryType.Polygon
            && symbol is IBrushColor brushColor)
        {
            int alpha = (int)Math.Round(Math.Clamp(values.Opacity.Value, 0f, 100f) * 2.55f);
            brushColor.FillColor = ArgbColor.FromArgb(alpha, brushColor.FillColor);
        }
        if (values.Size.HasValue && symbol is ISymbolSize symbolSize)
        {
            symbolSize.SymbolSize = values.Size.Value;
        }
        if (values.Smoothing.HasValue)
        {
            symbol.SymbolSmoothingMode = values.Smoothing.Value;
        }

        return true;
    }

    /// <summary>
    /// Current values of a bulk editable symbol (start values for the bulk edit dialog).
    /// </summary>
    public static SymbolPropertyValues ReadPropertyValues(this ISymbol? symbol)
    {
        var values = new SymbolPropertyValues();

        if (!symbol.CanBulkEdit())
        {
            return values;
        }

        values.Color = symbol switch
        {
            IBrushColor brush when brush.FillColor.A > 0 => ArgbColor.FromArgb(255, brush.FillColor),
            IPenColor pen => ArgbColor.FromArgb(255, pen.PenColor),
            IFontColor font => ArgbColor.FromArgb(255, font.FontColor),
            _ => null
        };
        values.Width = (symbol as IPenWidth)?.PenWidth;
        values.DashStyle = (symbol as IPenDashStyle)?.PenDashStyle;
        values.Opacity = symbol is IBrushColor fill ? (float)Math.Round(fill.FillColor.A / 2.55f) : null;
        values.Size = (symbol as ISymbolSize)?.SymbolSize;
        values.Smoothing = symbol!.SymbolSmoothingMode;

        return values;
    }
}
