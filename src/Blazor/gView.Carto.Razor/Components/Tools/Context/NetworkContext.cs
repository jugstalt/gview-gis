using gView.Framework.Core.Data;

namespace gView.Carto.Razor.Components.Tools.Context;
public class NetworkContext
{
    public IEnumerable<ILayer> NetworkLayers { get; set; } = [];

    public ILayer? CurrentNetworkLayer { get; set; }
}
