using gView.Framework.Core.Data;
using System.Runtime.InteropServices;

namespace gView.Carto.Razor.Components.Tools.Context;

public enum NetworkContextTool
{
    None,
    SetStartNode,
    SetTargetNode,
    Trace
}

public class NetworkContext
{
    public IEnumerable<ILayer> NetworkLayers { get; set; } = [];

    public ILayer? CurrentNetworkLayer { get; set; }

    public NetworkContextTool ContextTool { get; set; }
    
    public int StartNode { get; set; }
    public int TargetNode { get; set; } 
}
