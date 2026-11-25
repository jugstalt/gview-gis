#nullable enable

using gView.Framework.Cartography.Rendering;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Symbology;
using gView.GeoJsonService.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class FeatureRendererExtensions
{
    /// <summary>
    /// The renderer in a simple form: single, categories, classes; any other renderer as "other" with its legend
    /// items (label and image)
    /// </summary>
    static public LayerRenderer? ToLayerRenderer(this IFeatureRenderer? renderer, IMap? map)
    {
        switch (renderer)
        {
            case null:
                return null;

            case SimpleRenderer simple:
                return new LayerRenderer()
                {
                    Type = "single",
                    Classes = [new RendererClass() { Label = (simple.Symbol as ILegendItem)?.LegendLabel, Symbol = simple.Symbol.ToSymbolInfo(map) }]
                };

            case ValueMapRenderer valueMap when valueMap.Keys is not null:
                return new LayerRenderer()
                {
                    Type = "categories",
                    Field = valueMap.ValueField,
                    Classes = valueMap.Keys
                        .Where(key => key != ValueMapRenderer.AllOtherValuesKey)
                        .Select(key => new RendererClass()
                        {
                            Value = key,
                            Label = (valueMap[key] as ILegendItem)?.LegendLabel ?? key,
                            Symbol = valueMap[key].ToSymbolInfo(map)
                        })
                        .ToArray(),
                    Default = valueMap.DefaultSymbol is ISymbol defaultSymbol
                        ? new RendererClass() { Label = (defaultSymbol as ILegendItem)?.LegendLabel, Symbol = defaultSymbol.ToSymbolInfo(map) }
                        : null
                };

            case QuantityRenderer quantity:
                return new LayerRenderer()
                {
                    Type = "classes",
                    Field = quantity.ValueField,
                    Classes = (quantity.QuantityClasses ?? [])
                        .Where(c => c is not null)
                        .OrderBy(c => c.Min)
                        .Select(c => new RendererClass()
                        {
                            Min = c.Min,
                            Max = c.Max,
                            Label = (c.Symbol as ILegendItem)?.LegendLabel,
                            Symbol = c.Symbol.ToSymbolInfo(map)
                        })
                        .ToArray(),
                    Default = quantity.DefaultSymbol is ISymbol quantityDefault
                        ? new RendererClass() { Label = (quantityDefault as ILegendItem)?.LegendLabel, Symbol = quantityDefault.ToSymbolInfo(map) }
                        : null
                };

            case ILegendGroup legendGroup:
                var classes = new List<RendererClass>();
                for (int i = 0; i < legendGroup.LegendItemCount; i++)
                {
                    var item = legendGroup.LegendItem(i);
                    classes.Add(new RendererClass() { Label = item?.LegendLabel, Symbol = (item as ISymbol).ToSymbolInfo(map) });
                }
                return new LayerRenderer() { Type = "other", Classes = classes };

            default:
                return new LayerRenderer() { Type = "other" };
        }
    }
}
