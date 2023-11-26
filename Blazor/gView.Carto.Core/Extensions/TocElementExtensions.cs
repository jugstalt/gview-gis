using gView.Framework.Data;
using gView.Framework.Symbology;
using gView.Framework.UI;
using System.Collections.Generic;
using System.Linq;
using static gView.Framework.Network.Algorthm.RoadBook;

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
            foreach (IFeatureLayer featureLayer in tocElement.Layers.Where(l => l is IFeatureLayer fLayer && fLayer.FeatureRenderer?.Symbols != null))
            {
                items.AddRange(featureLayer.FeatureRenderer.Symbols);
            }
        }

        return items;
    }

    static public bool SetLegendItemSymbol(this ITocElement tocElement, ISymbol symbol, ISymbol newSymbol)
    {
        if (tocElement?.Layers is not null)
        {
            foreach (var legendGroup in tocElement.Layers.Where(l => l is IFeatureLayer fLayer && fLayer.FeatureRenderer is ILegendGroup)  
                                                         .Select(l=> ((IFeatureLayer)l).FeatureRenderer as ILegendGroup))
            {
                if(legendGroup == null)
                {
                    continue;
                }
                for(int i=0;i<legendGroup.LegendItemCount;i++)
                {
                    var legendItem = legendGroup.LegendItem(i);
                    if (legendItem == symbol)
                    {
                        legendGroup.SetSymbol(legendItem, newSymbol);

                        return true;
                    }
                }
            }
        }

        return false;
    }
}
