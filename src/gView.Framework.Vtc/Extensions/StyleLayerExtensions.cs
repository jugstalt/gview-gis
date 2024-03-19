using gView.DataSources.VectorTileCache.Json.Styles;
using gView.Framework.Symbology;
using gView.Framework.Symbology.Vtc;
using gView.GraphicsEngine;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.Vtc.Extensions;

static internal class StyleLayerExtensions
{
    static public PaintSymbol ToPaintSymbol(this StyleLayer styleLayer)
    {
        if (styleLayer?.Paint == null)
        {
            return new PaintSymbol();
        }

        var paintSymbol = new PaintSymbol(
                styleLayer.Type?.ToLower() switch
                {
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
                                ? new SimpleFillSymbol() { OutlineSymbol = new SimpleLineSymbol() }
                                : new SimpleFillSymbol() { OutlineSymbol = null },
                    "fill-extrusion" => new SimpleFillSymbol() { OutlineSymbol = null },
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
}
