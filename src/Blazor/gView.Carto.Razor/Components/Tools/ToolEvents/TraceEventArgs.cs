using gView.Carto.Core.Models.ToolEvents;
using gView.Framework.Core.Network;

namespace gView.Carto.Razor.Components.Tools.ToolEvents;

public class TraceEventArgs(INetworkTracer tracer) : ToolEventArgs
{
    public INetworkTracer Tracer { get; } = tracer;
}
