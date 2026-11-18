using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Query;

namespace gView.Carto.Razor.Components.Tools.Context;

/// <summary>What a click into the map sets.</summary>
public enum NetworkSearchPick
{
    None,
    Start,
    Target,
    /// <summary>A click takes the polygon feature under it as the area, Ctrl-drag a box.</summary>
    Area
}

/// <summary>A node or edge picked in the map: its reference for the search and a readable label.</summary>
public sealed record NetworkPickedItem(string Reference, string Label);

/// <summary>The state of the network search tool (SpatialNET network of the map, see NetworkModule).</summary>
public class NetworkSearchContext
{
    /// <summary>The operations of the tool; nearest is what a click into the map does anyway.</summary>
    public static readonly IReadOnlyList<string> Operations = NetworkQueryService.Operations.Where(o => o != "nearest").ToList();

    /// <summary>trace | reach | path | supply | outage | info | validate</summary>
    public string Operation { get; set; } = "trace";

    public NetworkSearchPick Pick { get; set; } = NetworkSearchPick.Start;

    /// <summary>
    /// The start node(s): trace/reach start, path from, supply sources, info node (Layer#OID, id, type pattern or @x,y);
    /// outage: the edges to switch off (Layer#OID/part).
    /// </summary>
    public string? Start { get; set; }

    /// <summary>The label of the picked start (e.g. "end of Verbinder#12, Leitung#3456" for an edge end).</summary>
    public string? StartLabel { get; set; }

    /// <summary>The target: path to, supply sinks.</summary>
    public string? Target { get; set; }

    /// <summary>The label of the picked target.</summary>
    public string? TargetLabel { get; set; }

    /// <summary>The start nodes (or outage edges) picked in the map, Ctrl-click adds or removes one; empty when typed.</summary>
    public List<NetworkPickedItem> StartItems { get; } = [];

    /// <summary>The targets picked in the map (supply sinks with Ctrl-click).</summary>
    public List<NetworkPickedItem> TargetItems { get; } = [];

    /// <summary>outage: the sources; empty for the supply sources of the network module.</summary>
    public string? Sources { get; set; }

    /// <summary>outage: the customers; empty for the customers of the network module.</summary>
    public string? Customers { get; set; }

    /// <summary>Edges switched off for trace, reach, path and supply.</summary>
    public string? Disable { get; set; }

    /// <summary>Only the nodes inside take part (in the spatial reference of the network); stays set until removed.</summary>
    public NetworkArea? Area { get; set; }

    /// <summary>What the area is, e.g. "Stationspolygon#4711", "box", "map extent", "selection (3 polygons)".</summary>
    public string? AreaLabel { get; set; }

    /// <summary>GeoJSON export: the line ends set onto the nodes, so the file is a valid network.</summary>
    public bool SnapExport { get; set; }

    public string? Weight { get; set; }
    public string? MaxCost { get; set; }
    public string? Filter { get; set; }
    public string? Block { get; set; }
    public string? RuleIds { get; set; }

    /// <summary>The weights of the network (length first), known once the network is open.</summary>
    public IReadOnlyList<string> Weights { get; set; } = ["length"];

    public string? NetworkDirectory { get; set; }

    /// <summary>Whether the network is built, when and how big; null until known.</summary>
    public NetworkStatus? Status { get; set; }

    /// <summary>The build started from this map (running or last one).</summary>
    public NetworkBuildState? Build { get; set; }

    public NetworkQueryResult? Result { get; set; }

    public string? Error { get; set; }

    public string? Info { get; set; }

    public bool Busy { get; set; }

    /// <summary>While a run is busy: where it stands, e.g. "trace: 12,000 node(s) found".</summary>
    public string? Progress { get; set; }

    /// <summary>Raised by the tool when the state changed (result, picked node), so the panel renders again.</summary>
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();

    public bool NeedsTarget => Operation is "path" or "supply";

    /// <summary>Whether Ctrl-click may pick several starts: trace/reach starts, supply sources, outage edges.</summary>
    public bool StartAllowsMany => Operation is "trace" or "reach" or "supply" or "outage";

    /// <summary>Whether Ctrl-click may pick several targets: supply sinks.</summary>
    public bool TargetAllowsMany => Operation == "supply";

    /// <summary>outage: a click picks an edge (the branch to switch off), not a node.</summary>
    public bool PicksEdge => Operation == "outage";

    public bool NeedsStart => Operation is not "validate";
}
