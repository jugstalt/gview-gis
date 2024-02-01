using gView.Framework.Core.Data;
using gView.Framework.Core.Network;
using System.Runtime.InteropServices;

namespace gView.Carto.Razor.Components.Tools.Context;

public enum NetworkContextTool
{
    None,
    SetStartNode,
    SetTargetNode,
    RemoveElements,
    Trace
}

public class NetworkContext
{
    public IEnumerable<ILayer> NetworkLayers { get; set; } = [];

    public ILayer? CurrentNetworkLayer { get; set; }

    public NetworkContextTool ContextTool { get; set; } = NetworkContextTool.SetStartNode;
    
    public int StartNode { get; set; }
    public int TargetNode { get; set; } 

    public WeightApplying WeightApplying { get; set; }
    public IGraphWeight? GraphWeight { get; set; }
}
