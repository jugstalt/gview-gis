using gView.Carto.Core.Abstraction;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using System.Threading.Tasks;

namespace gView.Carto.Core.Extensions;

static public class CartoDocumentExtensions
{
    static public string LayerTocName(this ICartoDocument? document, ILayer layer)
    {
        var tocElement = document?.Map?.TOC?.GetTocElementByLayerId(layer.ID);

        if (tocElement is not null)
        {
            return tocElement.Name;
        }

        return layer.Title;
    }

    static public void SetHighlightLayer(
            this ICartoDocument? document, 
            ILayer layer,
            IQueryFilter? filter
        )
    {
        foreach(var element in document?.Map?.MapElements ?? [])
        {
            if (element is IFeatureHighlighting featureHighlighting)
            {
                featureHighlighting.FeatureHighlightFilter = 
                    layer == element 
                        ? filter
                        : null;
            }
        }
    }
}
