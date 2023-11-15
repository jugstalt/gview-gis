using gView.Framework.Data;
using gView.Framework.Geometry;

namespace gView.Blazor.Core.Extensions;

static public class LayerExtensions
{
    static public ISpatialReference? GetSpatialReference(this ILayer layer)
        => layer switch
        {
            IFeatureLayer fLayer => fLayer.FeatureClass?.SpatialReference,
            IRasterLayer rLayer => rLayer.RasterClass?.SpatialReference,
            IWebServiceLayer wLayer => wLayer.WebServiceClass?.SpatialReference,
            _ => null
        };

    static public bool ImplementsSpatialReference(this ILayer layer)
        => layer switch
        {
            IFeatureLayer => true,
            IRasterLayer => true,
            IWebServiceLayer => true,
            _ => false
        };
}
