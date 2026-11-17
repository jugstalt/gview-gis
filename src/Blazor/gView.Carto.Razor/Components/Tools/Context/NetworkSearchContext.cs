using gView.Framework.NetworkModel.Query;

namespace gView.Carto.Razor.Components.Tools.Context;

/// <summary>What a click into the map sets.</summary>
public enum NetworkSearchPick
{
    None,
    Start,
    Target
}

/// <summary>The state of the network search tool (SpatialNET network of the map, see NetworkModule).</summary>
public class NetworkSearchContext
{
    public static readonly IReadOnlyList<string> Operations = NetworkQueryService.Operations;

    /// <summary>trace | reach | path | supply | info | validate</summary>
    public string Operation { get; set; } = "trace";

    public NetworkSearchPick Pick { get; set; } = NetworkSearchPick.Start;

    /// <summary>The start node(s): trace/reach start, path from, supply sources, info node (Layer#OID, id or type pattern).</summary>
    public string? Start { get; set; }

    /// <summary>The target: path to, supply sinks.</summary>
    public string? Target { get; set; }

    public string? Weight { get; set; }
    public string? MaxCost { get; set; }
    public string? Filter { get; set; }
    public string? Block { get; set; }
    public string? RuleIds { get; set; }

    /// <summary>The weights of the network (length first), known once the network is open.</summary>
    public IReadOnlyList<string> Weights { get; set; } = ["length"];

    public string? NetworkDirectory { get; set; }

    public NetworkQueryResult? Result { get; set; }

    public string? Error { get; set; }

    public string? Info { get; set; }

    public bool Busy { get; set; }

    /// <summary>Raised by the tool when the state changed (result, picked node), so the panel renders again.</summary>
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();

    public bool NeedsTarget => Operation is "path" or "supply";

    public bool NeedsStart => Operation is not "validate";
}
