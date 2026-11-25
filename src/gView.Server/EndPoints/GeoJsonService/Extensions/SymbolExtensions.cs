#nullable enable

using gView.Framework.Cartography.UI;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology;
using gView.Framework.Symbology.Extensions;
using gView.GeoJsonService.DTOs;
using gView.GraphicsEngine;
using System;
using System.IO;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class SymbolExtensions
{
    /// <summary>
    /// The main properties of a symbol for clients that draw features themselves, and its legend image
    /// (as in the legend endpoint) for every symbol, also the ones that have no simple form
    /// </summary>
    static public SymbolInfo? ToSymbolInfo(this ISymbol? symbol, IMap? map)
    {
        if (symbol is null)
        {
            return null;
        }

        var info = symbol switch
        {
            SimplePointSymbol point => new SymbolInfo()
            {
                Type = "point",
                Color = point.Color.ToRgba(),
                OutlineColor = point.OutlineColor.ToRgba(),
                Width = point.OutlineWidth,
                Size = point.Size
            },
            TrueTypeMarkerSymbol marker => new SymbolInfo()
            {
                Type = "point",
                Color = marker.FontColor.ToRgba(),
                Size = marker.Font?.Size
            },
            SimpleLineSymbol line => new SymbolInfo()
            {
                Type = "line",
                Color = line.Color.ToRgba(),
                Width = line.Width
            },
            SimpleFillSymbol fill => new SymbolInfo()
            {
                Type = "fill",
                Color = fill.Color.ToRgba(),
                OutlineColor = fill.OutlineColor.ToRgba(),
                Width = fill.OutlineWidth
            },
            ITextSymbol => new SymbolInfo() { Type = "text" },
            _ => new SymbolInfo() { Type = "other" }
        };

        info.LegendImageBase64 = symbol.LegendImageBase64(map);
        return info;
    }

    static private string? LegendImageBase64(this ISymbol symbol, IMap? map)
    {
        try
        {
            var rect = new CanvasRectangle().ToLegendItemSymbolRect();
            using var bitmap = Current.Engine.CreateBitmap(rect.Width, rect.Height);
            using (var canvas = bitmap.CreateCanvas())
            {
                new SymbolPreview(map).Draw(canvas, rect, symbol);
            }

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return null;  // a symbol that can't be drawn has no image, the rest of the layer info stays
        }
    }

    static private int[] ToRgba(this ArgbColor color) => [color.R, color.G, color.B, color.A];
}
