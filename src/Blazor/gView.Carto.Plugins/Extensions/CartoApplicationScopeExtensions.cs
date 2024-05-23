using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.Data;
using gView.Framework.Core.Network;

namespace gView.Carto.Plugins.Extensions;
static public class CartoApplicationScopeExtensions
{
    //static public CartoApplicationScopeService ToCartoScopeService(this IApplicationScope appScope)
    //    => appScope is CartoApplicationScopeService ?
    //        (CartoApplicationScopeService)appScope :
    //        throw new Exception("AppScope is not an Service. Appliation Service not registered correctly");

    static public bool HasNetworkClasses(this ICartoApplicationScopeService scope)
        => scope.Document?.Map?.MapElements?.Any(e => e.Class is INetworkFeatureClass) == true;

    static public IEnumerable<ILayer> NetworkLayers(this ICartoApplicationScopeService scope)
        => scope.Document?.Map?.MapElements?
            .Where(e => e is ILayer && e.Class is INetworkFeatureClass)
            .Select(e => (ILayer)e) ?? [];
}
