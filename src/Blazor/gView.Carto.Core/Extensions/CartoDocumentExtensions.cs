using gView.Carto.Core.Abstractions;
using gView.Framework.Core.Data;

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
}
