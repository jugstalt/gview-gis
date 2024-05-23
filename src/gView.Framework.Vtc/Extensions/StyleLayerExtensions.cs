using gView.DataSources.VectorTileCache.Json.GLStyles;
using gView.Framework.Core.Carto;
using gView.Framework.Symbology;
using gView.Framework.Symbology.Vtc;
using gView.GraphicsEngine;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.Vtc.Extensions;

static internal class StyleLayerExtensions
{
    static public PaintSymbol ToPaintSymbol(this GLStyleLayer styleLayer, IMap map)
    {
        if (styleLayer?.Paint is null
            && styleLayer?.Layout is null)
        {
            return new PaintSymbol();
        }

        var paintSymbol = new PaintSymbol(
                styleLayer.Type?.ToLower() switch
                {
                    "symbol" when styleLayer.Layout?.IconImage != null =>
                        new RasterMarkerSymbol(),
                    _ => null
                },
                styleLayer.Type?.ToLower() switch
                {
                    "line" => new SimpleLineSymbol(),
                    _ => null,
                },
                styleLayer.Type?.ToLower() switch
                {
                    "fill" =>
                        styleLayer.Paint.FillOutlineColor != null
                        && styleLayer.Paint.FillOutlineWidth != null
                                ? new SimpleFillSymbol() { FillColor = ArgbColor.Transparent, OutlineSymbol = new SimpleLineSymbol() }
                                : new SimpleFillSymbol() { FillColor = ArgbColor.Transparent, OutlineSymbol = null },
                    "fill-extrusion" => new SimpleFillSymbol() { FillColor = ArgbColor.Transparent, OutlineSymbol = null },
                    _ => null
                },
                styleLayer.Type?.ToLower() switch
                {
                    "symbol" => styleLayer.Paint?.TextHaloColor switch
                    {
                        null => new SimpleTextSymbol(),
                        _ => new GlowingTextSymbol()
                        {
                            GlowingColor = styleLayer.Paint.TextHaloColor.Value.ToFuncValueOfType<ArgbColor>()
                        }
                    },
                    _ => null,
                }
            );

        if (styleLayer.Layout is not null)
        {
            foreach (var layoutPropertyInfo in styleLayer.Layout.GetType().GetProperties())
            {
                var jsonName = layoutPropertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonName != null && layoutPropertyInfo.PropertyType == typeof(JsonElement?))
                {
                    var value = (JsonElement?)layoutPropertyInfo.GetValue(styleLayer.Layout, null);

                    var func = value.ToValueFunc();
                    if (func != null)
                    {
                        paintSymbol.AddValueFunc(jsonName.Name, func);
                    }
                }
            }
        }

        if (styleLayer.Paint is not null)
        {
            foreach (var paintPropertyInfo in styleLayer.Paint.GetType().GetProperties())
            {
                var jsonName = paintPropertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonName != null && paintPropertyInfo.PropertyType == typeof(JsonElement?))
                {
                    var value = (JsonElement?)paintPropertyInfo.GetValue(styleLayer.Paint, null);

                    var func = value.ToValueFunc();
                    if (func != null)
                    {
                        paintSymbol.AddValueFunc(jsonName.Name, func);
                    }
                }
            }
        }

        return paintSymbol;
    }

    #region Helper

    static private CanvasSizeF GetResourceSize(this IMap map, string resouceName)
    {
        try
        {
            var iconData = map.ResourceContainer[resouceName];
            if(iconData is not null)
            {
                using var bm = Current.Engine.CreateBitmap(new MemoryStream(iconData));

                return new CanvasSizeF(bm.Width, bm.Height);
            }
        }
        catch
        {

        }

        return new(0, 0);
    }

    #endregion
}
