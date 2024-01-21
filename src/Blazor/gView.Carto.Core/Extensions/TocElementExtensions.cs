using gView.Blazor.Core.Extensions;
using gView.Framework.Core.Data;
using gView.Framework.Core.Symbology;
using gView.Framework.Core.UI;
using System.Collections.Generic;
using System.Linq;

namespace gView.Carto.Core.Extensions;

static public class TocElementExtensions
{
    static public bool HasLegendItems(this ITocElement tocElement)
        => tocElement?.Layers?.Any(l =>
                l is IFeatureLayer fLayer
                && fLayer.FeatureRenderer?.Symbols?.Any() == true) == true;

    static public IEnumerable<ISymbol> GetLegendItems(this ITocElement tocElement)
    {
        List<ISymbol> items = new List<ISymbol>();

        if (tocElement?.Layers is not null)
        {
            foreach (IFeatureLayer featureLayer in tocElement.Layers.Where(l => l is IFeatureLayer fLayer && fLayer.LabelRenderer is ILegendGroup))
            {
                var legendGroup = (ILegendGroup)featureLayer.LabelRenderer;
                for(int i=0;i<legendGroup.LegendItemCount;i++)
                {
                    var symbol = legendGroup.LegendItem(i) as ISymbol;
                    if(symbol is not null && symbol is not ITextSymbol)
                    {
                        items.Add(symbol);
                    }
                }
            }

            foreach (IFeatureLayer featureLayer in tocElement.Layers.Where(l => l is IFeatureLayer fLayer && fLayer.FeatureRenderer?.Symbols != null))
            {
                items.AddRange(featureLayer.FeatureRenderer.Symbols);
            }
        }

        return items.Where(item => item is not null).ToArray();
    }

    static public bool SetLegendItemSymbol(this ITocElement tocElement, ISymbol symbol, ISymbol newSymbol)
    {
        if (tocElement?.Layers is not null)
        {
            foreach (var legendGroup in tocElement.Layers.Where(l => l is IFeatureLayer fLayer && fLayer.LabelRenderer is ILegendGroup)
                                                         .Select(l => ((IFeatureLayer)l).LabelRenderer as ILegendGroup))
            {
                if(SetLegendItemSymbol(legendGroup, symbol, newSymbol))
                {
                    return true;
                }
            }

            foreach (var legendGroup in tocElement.Layers.Where(l => l is IFeatureLayer fLayer && fLayer.FeatureRenderer is ILegendGroup)
                                                         .Select(l => ((IFeatureLayer)l).FeatureRenderer as ILegendGroup))
            {
                if(SetLegendItemSymbol(legendGroup, symbol, newSymbol))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static private bool SetLegendItemSymbol(this ILegendGroup? legendGroup, ISymbol symbol, ISymbol newSymbol)
    {
        if (legendGroup == null)
        {
            return false;
        }
        for (int i = 0; i < legendGroup.LegendItemCount; i++)
        {
            var legendItem = legendGroup.LegendItem(i);
            if (legendItem == symbol)
            {
                legendGroup.SetSymbol(legendItem, newSymbol);

                return true;
            }
        }

        return false;
    }

    static public bool IsGroupElement(this ITocElement tocElement)
        => tocElement.ElementType == TocElementType.ClosedGroup
           || tocElement.ElementType == TocElementType.OpenedGroup;

    static public bool IsQueryable(this ITocElement tocElement)
        => tocElement.CollectQueryableLayers().Any();

    static public IEnumerable<ILayer> CollectQueryableLayers(this ITocElement tocElement, List<ILayer>? appendTo = null)
    {
        appendTo ??= new();

        if (tocElement?.IsGroupElement() == true)
        {
            var childTocElements = tocElement.TOC.GetChildElements(tocElement);
            foreach (var childTocElement in childTocElements?.Where(e => !e.IsGroupElement()) ?? [])
            {
                childTocElement.CollectQueryableLayers(appendTo);
            }
        }
        else
        {
            foreach (var layer in tocElement?.Layers ?? [])
            {
                if (layer?.Class is IFeatureClass || layer?.Class is IRasterClass)
                {
                    appendTo.Add(layer);
                }
            }
        }

        return appendTo;
    }

    static public string FullPath(this ITocElement? tocElement,
                                  string rootName = "",
                                  char separator = '/')
        =>  tocElement is null 
            ? rootName
            : $"{tocElement.ParentGroup.FullPath(rootName, separator)}{separator}{tocElement.Name}";
}
