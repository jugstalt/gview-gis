using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Types;
using SpatialNET.Network;
using SpatialNET.Network.Building;

namespace gView.Framework.NetworkModel.Build;

/// <summary>
/// The answers to SpatialNET's connection callbacks (ADR 0055) from the config: the connect rules in order, then with
/// <see cref="NetworkConfig.SeparateTiers"/> the tier's sources for line ends and the tiers themselves (the first facet),
/// then for splits the defaults <see cref="NetworkConfig.NodesOnLine"/> and <see cref="NetworkConfig.LineEndsOnLine"/>,
/// otherwise everything connects. Resolved once the types of the build are known; the builder asks once per pair of types.
/// </summary>
internal sealed class NetworkConnectRules
{
    private readonly bool _separateTiers;
    private readonly ConnectionDecision _nodesOnLine, _lineEndsOnLine;
    private readonly List<(ConnectAt At, TypeSet Nodes, TypeSet Edges, ConnectionDecision Decision, int Index, string Name)> _rules = [];
    private readonly Dictionary<int, TypeSet> _sourcesOfTier = [];
    private readonly Dictionary<int, int> _helperOfTier = [];
    private readonly TypeCatalog _types;

    private NetworkConnectRules(TypeCatalog types, NetworkConfig config)
    {
        _types = types;
        _separateTiers = config.SeparateTiers;
        _nodesOnLine = Decision(config.NodesOnLine);
        _lineEndsOnLine = Decision(config.LineEndsOnLine);
    }

    /// <summary>Whether the config asks for any callback; without, the build connects everything as before.</summary>
    public static bool Needed(NetworkConfig config)
        => config.SeparateTiers || config.ConnectRules.Any(r => r.Enabled)
           || config.NodesOnLine != ConnectAction.Connect || config.LineEndsOnLine != ConnectAction.Connect;

    private static ConnectionDecision Decision(ConnectAction action) => action switch
    {
        ConnectAction.Ignore => ConnectionDecision.Ignore,
        ConnectAction.Report => ConnectionDecision.Report,
        ConnectAction.ConnectAndReport => ConnectionDecision.ConnectAndReport,
        _ => ConnectionDecision.Connect,
    };

    /// <summary>
    /// The options with the callbacks the config asks for; they ask the rules the function gives, which must be created
    /// and resolved before the build starts (the types are complete only after every feature was added).
    /// </summary>
    public static NetworkBuildOptions Attach(NetworkBuildOptions options, NetworkConfig config, Func<NetworkConnectRules?> rules)
    {
        if (!Needed(config))
        {
            return options;
        }
        NetworkConnectRules Rules() => rules() ?? throw new InvalidOperationException("The connect rules were not resolved before the build.");
        return options with
        {
            HelperNodeType = config.SeparateTiers ? edge => Rules().HelperNodeType(edge) : null,
            Partition = config.SeparateTiers ? node => Rules().Partition(node) : null,
            Connect = (node, edge) => Rules().Connect(node, edge),
            Split = (node, edge) => Rules().Split(node, edge),
        };
    }

    /// <summary>
    /// Creates the helper types of the tiers (with separate tiers) and resolves the rules and the sources of the tiers;
    /// call after every feature was read and before the type registry is made. Throws an <see cref="InvalidDataException"/>
    /// naming the faulty rule.
    /// </summary>
    public static NetworkConnectRules Create(NetworkConfig config, TypeCatalog types)
    {
        var rules = new NetworkConnectRules(types, config);
        if (config.SeparateTiers)
        {
            foreach (int tier in types.UsedEdgeTiers())
            {
                rules._helperOfTier[tier] = types.ResolveHelper(tier);
            }
        }
        return rules;
    }

    /// <summary>Resolves the patterns of the rules and of the tier sources against the complete types of the build.</summary>
    public void Resolve(NetworkConfig config, TypeRegistry registry)
    {
        var problems = new List<string>();
        // switched off rules are left out as if deleted (the numbers in messages stay those of the list)
        foreach (var (rule, index) in config.ConnectRules.Select((r, i) => (r, i + 1)).Where(r => r.r.Enabled))
        {
            try
            {
                _rules.Add((rule.At, registry.Nodes(rule.Nodes), registry.Edges(rule.Edges), Decision(rule.Action), index, rule.DisplayName));
            }
            catch (ArgumentException ex)
            {
                problems.Add($"{NetworkConfig.ConnectRuleLabel(rule, index)} ({rule}): {ex.Message}");
            }
        }

        if (_separateTiers)
        {
            List<ResolvedTier> tiers;
            try
            {
                tiers = NetworkTierConfig.ResolveAll(config.Tiers, registry);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException(ex.Message, ex);
            }
            foreach (var tier in tiers)
            {
                try
                {
                    if (tier.Config.ResolveNodes(registry, "sources", tier.Config.Sources) is { } sources)
                    {
                        _sourcesOfTier[tier.Value] = sources;
                    }
                }
                catch (ArgumentException ex)
                {
                    problems.Add(ex.Message);
                }
            }
        }

        if (problems.Count > 0)
        {
            throw new InvalidDataException("Invalid connect rules:" + Environment.NewLine + string.Join(Environment.NewLine, problems.Select(p => "  " + p)));
        }
    }

    /// <summary>The helper type of the tier of an edge type; 0 without a tier.</summary>
    public int HelperNodeType(int edgeType)
        => _helperOfTier.TryGetValue(_types.TierOf(edgeType), out int helper) ? helper : 0;

    /// <summary>The partition of a real node: its tier.</summary>
    public int Partition(int nodeType) => _types.TierOf(nodeType);

    /// <summary>Whether a line end connects to a real node.</summary>
    public ConnectionDecision Connect(int nodeType, int edgeType)
    {
        if (Match(ConnectAt.End, nodeType, edgeType) is { } decision)
        {
            return decision;
        }
        if (_separateTiers && _sourcesOfTier.TryGetValue(_types.TierOf(edgeType), out var sources) && sources.Contains(nodeType))
        {
            return ConnectionDecision.Connect;
        }
        return ByTier(nodeType, edgeType);
    }

    /// <summary>
    /// Whether a node (real, or the helper node of a T junction) splits the inside of a line: a rule, a node of another
    /// tier (separate tiers) does not, otherwise the default for nodes or for line ends on a line.
    /// </summary>
    public ConnectionDecision Split(int nodeType, int edgeType)
    {
        if (Match(ConnectAt.Inside, nodeType, edgeType) is { } decision)
        {
            return decision;
        }
        if (ByTier(nodeType, edgeType) == ConnectionDecision.Ignore)
        {
            return ConnectionDecision.Ignore;
        }
        return _types.IsHelper(nodeType) ? _lineEndsOnLine : _nodesOnLine;
    }

    private ConnectionDecision? Match(ConnectAt at, int nodeType, int edgeType)
        => MatchRule(at, nodeType, edgeType) is int i ? _rules[i].Decision : null;

    /// <summary>The position in <see cref="_rules"/> of the first rule that matches; null for none.</summary>
    private int? MatchRule(ConnectAt at, int nodeType, int edgeType)
    {
        for (int i = 0; i < _rules.Count; i++)
        {
            var rule = _rules[i];
            if ((rule.At == ConnectAt.Any || rule.At == at) && rule.Nodes.Contains(nodeType) && rule.Edges.Contains(edgeType))
            {
                return i;
            }
        }
        return null;
    }

    public const string DefaultNodesOnLine = "default: nodes on a line";
    public const string DefaultLineEndsOnLine = "default: line ends on a line";

    /// <summary>
    /// What decided a connection the build reported (refused or reported): the connect rule (its number in the list, from 1,
    /// and its name or the rule in short), or for splits without a rule the default for nodes or for line ends on a line;
    /// null when the tiers decided, which never reports.
    /// </summary>
    public (int? Index, string Name)? DecidedBy(bool split, int nodeType, int edgeType)
    {
        if (MatchRule(split ? ConnectAt.Inside : ConnectAt.End, nodeType, edgeType) is int i)
        {
            return (_rules[i].Index, _rules[i].Name);
        }
        if (split && ByTier(nodeType, edgeType) != ConnectionDecision.Ignore)
        {
            return (null, _types.IsHelper(nodeType) ? DefaultLineEndsOnLine : DefaultNodesOnLine);
        }
        return null;
    }

    private ConnectionDecision ByTier(int nodeType, int edgeType)
    {
        if (!_separateTiers)
        {
            return ConnectionDecision.Connect;
        }
        int nodeTier = _types.TierOf(nodeType), edgeTier = _types.TierOf(edgeType);
        return nodeTier == 0 || edgeTier == 0 || nodeTier == edgeTier ? ConnectionDecision.Connect : ConnectionDecision.Ignore;
    }
}
