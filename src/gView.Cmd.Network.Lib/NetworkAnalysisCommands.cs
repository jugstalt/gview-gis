using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Validation;
using SpatialNET.Network;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace gView.Cmd.Network.Lib;

/// <summary>
/// Searches on a built network: trace, reach, path, supply, info.
/// Nodes are given as Layer#OID, raw id, or type pattern ([*,Trafo,*]).
/// </summary>
internal static class NetworkAnalysisCommands
{
    public static Task<bool> Run(string utility, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var directory = Required(parameters, "network");
        var stopwatch = Stopwatch.StartNew();
        var graph = new FileNetworkGraph(directory);
        logger?.LogLine($"Network: {graph.Metadata.Name}, {graph.Metadata.NodeCount} nodes, {graph.Metadata.EdgeCount} edges, {graph.Metadata.Shards.Count} shards");

        bool result = utility switch
        {
            "trace" => Trace(graph, parameters, logger),
            "reach" => Reach(graph, parameters, logger),
            "path" => Path(graph, parameters, logger),
            "supply" => Supply(graph, parameters, logger),
            "info" => Info(graph, parameters, logger),
            "validate" => Validate(graph, parameters, logger),
            _ => throw new ArgumentException($"Unknown utility {utility}")
        };

        logger?.LogLine($"Shards loaded: {graph.LoadedShardCount} of {graph.Metadata.Shards.Count}, {stopwatch.ElapsedMilliseconds} ms");
        return Task.FromResult(result);
    }

    #region Commands

    private static bool Trace(FileNetworkGraph graph, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var sources = graph.ResolveNodes(Required(parameters, "start"));
        logger?.LogLine($"Trace from {sources.Count} source(s)");

        var options = Options(graph, parameters);
        var hits = graph.Trace(sources, options).ToList();

        foreach (var group in hits.GroupBy(h => h.Kind).OrderBy(g => g.Key))
        {
            logger?.LogLine($"  {group.Key,-12} {group.Count()}");
        }
        LogTypes(graph, hits.Where(h => h.Kind == TraceHitKind.Reached).Select(h => h.TypeId), logger);

        var edges = !string.IsNullOrEmpty(Optional(parameters, "out")) ? graph.TraceEdges(sources, options).Select(e => e.Edge.Id).Distinct().ToList() : [];
        Write(graph, parameters, logger,
            hits.Select(h => new Row(h.Node, h.TypeId, h.Cost, h.Kind.ToString(), graph.NodeLabel(h.Source))),
            edges);
        return true;
    }

    private static bool Reach(FileNetworkGraph graph, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var starts = graph.ResolveNodes(Required(parameters, "start"));
        var options = Options(graph, parameters);
        logger?.LogLine($"Reach from {starts.Count} start(s), max cost {options.MaxCost}, weight {options.Weight.Name}");

        var reached = graph.Reach(starts, options).ToList();
        logger?.LogLine($"  reached {reached.Count} node(s), max cost {(reached.Count > 0 ? reached.Max(r => r.Cost) : 0):0.###}");
        LogTypes(graph, reached.Select(r => r.TypeId), logger);

        Write(graph, parameters, logger,
            reached.Select(r => new Row(r.Node, r.TypeId, r.Cost, r.Blocked ? "Blocked" : r.IsStart ? "Start" : "Reached", graph.NodeLabel(r.Start))),
            reached.Where(r => !r.IsStart).Select(r => r.Via).Distinct().ToList());
        return true;
    }

    private static bool Path(FileNetworkGraph graph, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var from = graph.ResolveNode(Required(parameters, "from"));
        var to = graph.ResolveNode(Required(parameters, "to"));
        var options = Options(graph, parameters);

        var path = graph.ShortestPath(from, to, options);
        if (path is null)
        {
            logger?.LogLine($"No path from {graph.NodeLabel(from)} to {graph.NodeLabel(to)}.");
            return false;
        }

        logger?.LogLine($"Path {graph.NodeLabel(from)} -> {graph.NodeLabel(to)}: {path.Steps.Count} edge(s), cost {path.Cost:0.###} ({options.Weight.Name})");
        double cost = 0;
        foreach (var step in path.Steps)
        {
            cost += step.Cost;
            logger?.LogLine($"  {cost,10:0.###}  {graph.EdgeLabel(step.Edge),-50} -> {graph.NodeLabel(step.To)}");
        }

        var nodes = new List<Row> { new(from, NodeType(graph, from), 0, "From", "") };
        cost = 0;
        foreach (var step in path.Steps)
        {
            cost += step.Cost;
            nodes.Add(new Row(step.To, NodeType(graph, step.To), cost, step.To == to ? "To" : "Via", ""));
        }
        Write(graph, parameters, logger, nodes, path.Steps.Select(s => s.Edge).ToList());
        return true;
    }

    private static bool Supply(FileNetworkGraph graph, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var sources = graph.ResolveNodes(Required(parameters, "sources"));
        var sinks = graph.ResolveNodes(Required(parameters, "sinks"));
        logger?.LogLine($"Supply: {sources.Count} source(s), {sinks.Count} sink(s), {sinks.Sum(s => graph.AliasesOf(s).Count)} merged point(s) at the sinks");

        var progress = new Progress<SupplyProgress>(p =>
        {
            if (p.SourcesDone % 1000 == 0 || p.SourcesDone == p.SourcesTotal)
            {
                logger?.LogLine($"  {p.SourcesDone}/{p.SourcesTotal} sources");
            }
        });
        var supply = graph.Supply(sources, sinks, Options(graph, parameters), progress).ToList();

        int unsupplied = supply.Count(s => s.Sources.Count == 0);
        int multiple = supply.Count(s => s.Sources.Count > 1);
        logger?.LogLine($"  supplied once: {supply.Count - unsupplied - multiple}");
        logger?.LogLine($"  not supplied:  {unsupplied}");
        logger?.LogLine($"  multiple:      {multiple}");
        foreach (var sink in supply.Where(s => s.Sources.Count != 1).Take(20))
        {
            logger?.LogLine($"    {graph.NodeLabel(sink.Sink)}: {(sink.Sources.Count == 0 ? "not supplied" : string.Join(", ", sink.Sources.Select(graph.NodeLabel)))}");
        }

        Write(graph, parameters, logger,
            supply.Select(s => new Row(s.Sink, NodeType(graph, s.Sink), 0,
                s.Sources.Count == 0 ? "NotSupplied" : s.Sources.Count == 1 ? "Supplied" : "Multiple",
                string.Join(" ", s.Sources.Select(graph.NodeLabel)))),
            []);
        return true;
    }

    private static bool Info(FileNetworkGraph graph, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var id = graph.ResolveNode(Required(parameters, "node"));
        if (!graph.TryGetNode(id, out var node))
        {
            logger?.LogLine("Node not found.");
            return false;
        }
        graph.TryGetShardOf(id, out var shard);

        logger?.LogLine($"Node    {graph.NodeLabel(id)}  (id {id})");
        logger?.LogLine($"Type    {node.Node.TypeId} {graph.TypeName(node.Node.TypeId)}");
        logger?.LogLine($"At      {node.Node.Location.X.ToString("0.###", CultureInfo.InvariantCulture)}, {node.Node.Location.Y.ToString("0.###", CultureInfo.InvariantCulture)}");
        logger?.LogLine($"Shard   {shard}{ShardKey(graph, shard)}");
        logger?.LogLine($"Blocked {node.Node.Blocked}");
        var aliases = graph.AliasesOf(id);
        if (aliases.Count > 0)
        {
            logger?.LogLine($"Merged  {string.Join(", ", aliases.Select(a => $"{graph.NodeLabel(a)} ({graph.TypeName(graph.AliasTypeOf(a))})"))}");
        }

        var edges = graph.EdgesOf(id);
        logger?.LogLine($"Edges   {edges.Count}");
        foreach (var edge in edges)
        {
            var other = edge.Edge.From == id ? edge.Edge.To : edge.Edge.From;
            var otherType = NodeType(graph, other);
            logger?.LogLine($"  {graph.EdgeLabel(edge.Edge.Id),-50} {graph.TypeName(edge.Edge.TypeId),-45} {edge.Edge.Length,9:0.##} m  {(edge.Edge.Disabled ? "disabled " : "")}-> {graph.NodeLabel(other)} ({graph.TypeName(otherType)})");
        }
        return true;
    }

    /// <summary>
    /// Checks the rules of network.json (or of -rules) over the whole network; a summary per rule, the violations to -out.
    /// False (exit code 1) when a rule of severity Error is violated; warnings do not count.
    /// </summary>
    private static bool Validate(FileNetworkGraph graph, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var rulesPath = Optional(parameters, "rules");
        var rules = string.IsNullOrEmpty(rulesPath) ? graph.Metadata.Rules : LoadRules(rulesPath);
        var only = Optional(parameters, "rule");
        if (!string.IsNullOrEmpty(only))
        {
            var ids = only.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var unknown = ids.Where(id => !rules.Any(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase))).ToList();
            if (unknown.Count > 0)
            {
                throw new ArgumentException($"Unknown rule(s): {string.Join(", ", unknown)}. Rules: {string.Join(", ", rules.Select(r => r.Id))}");
            }
            rules = rules.Where(r => ids.Contains(r.Id, StringComparer.OrdinalIgnoreCase)).ToList();
        }
        if (rules.Count == 0)
        {
            logger?.LogLine(string.IsNullOrEmpty(rulesPath)
                ? "No rules in network.json: add rules to the network config (or the network module of the map) and build again, or pass -rules <json>."
                : $"No rules in {rulesPath}.");
            return false;
        }
        logger?.LogLine($"Validate: {rules.Count} rule(s) from {(string.IsNullOrEmpty(rulesPath) ? "network.json" : rulesPath)}");

        var validator = new NetworkValidator(graph, rules);
        var progress = new SyncProgress<(int Done, int Total)>(p =>
        {
            if (p.Done % 500 == 0 || p.Done == p.Total)
            {
                logger?.LogLine($"  {p.Done}/{p.Total} shards");
            }
        });

        var outPath = Optional(parameters, "out");
        var violations = new List<RuleViolation<long, long>>();
        foreach (var violation in validator.Validate(progress))
        {
            if (!string.IsNullOrEmpty(outPath))
            {
                violations.Add(violation);
            }
        }

        var summaries = validator.Summaries;
        int idWidth = Math.Max(4, summaries.Max(s => s.Rule.Id.Length));
        logger?.LogLine($"  {"rule".PadRight(idWidth)}  severity  {"checked",10}  {"violations",10}");
        foreach (var summary in summaries)
        {
            var description = string.IsNullOrEmpty(summary.Rule.Description) ? "" : "  " + summary.Rule.Description;
            logger?.LogLine($"  {summary.Rule.Id.PadRight(idWidth)}  {summary.Rule.Severity,-8}  {summary.Checked,10}  {summary.Violations,10}{description}");
        }
        long errors = summaries.Where(s => s.Rule.Severity == RuleSeverity.Error).Sum(s => s.Violations);
        long warnings = summaries.Where(s => s.Rule.Severity == RuleSeverity.Warning).Sum(s => s.Violations);
        logger?.LogLine($"Errors: {errors}, warnings: {warnings}");

        if (!string.IsNullOrEmpty(outPath))
        {
            WriteViolations(graph, outPath, violations);
            logger?.LogLine($"Written: {outPath} ({violations.Count} violations)");
        }
        return errors == 0;
    }

    /// <summary>A rules file: a JSON array of rules, or an object with "rules" (e.g. a whole network config).</summary>
    internal static List<NetworkRuleConfig> LoadRules(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var element = document.RootElement;
        if (element.ValueKind == JsonValueKind.Object)
        {
            element = element.EnumerateObject().FirstOrDefault(p => p.Name.Equals("rules", StringComparison.OrdinalIgnoreCase)).Value;
            if (element.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException($"{path}: no \"rules\" array.");
            }
        }
        return element.Deserialize<List<NetworkRuleConfig>>(NetworkConfig.JsonOptions) ?? [];
    }

    private sealed class SyncProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    #endregion

    #region Options

    private static TraversalOptions<long, long> Options(FileNetworkGraph graph, IDictionary<string, object> parameters)
    {
        var options = new TraversalOptions<long, long>();

        var weightName = Optional(parameters, "weight");
        if (!string.IsNullOrEmpty(weightName) && !weightName.Equals("length", StringComparison.OrdinalIgnoreCase))
        {
            var weight = graph.Metadata.Weights.FirstOrDefault(w => w.Name.Equals(weightName, StringComparison.OrdinalIgnoreCase))
                         ?? throw new ArgumentException($"Unknown weight '{weightName}'. Weights: length, {string.Join(", ", graph.Metadata.Weights.Select(w => w.Name))}");
            options = options with { Weight = new WeightDefinition(weight.Id, weight.Name, weight.Kind, weight.Fallback) };
        }

        var maxCost = Optional(parameters, "max-cost");
        if (!string.IsNullOrEmpty(maxCost))
        {
            options = options with { MaxCost = double.Parse(maxCost.Replace(',', '.'), CultureInfo.InvariantCulture) };
        }

        var filter = Optional(parameters, "filter");
        if (!string.IsNullOrEmpty(filter))
        {
            options = options with { EdgeTypes = graph.Types.Edges(filter) };
        }

        var block = Optional(parameters, "block");
        var unblock = Optional(parameters, "unblock");
        if (!string.IsNullOrEmpty(block) || !string.IsNullOrEmpty(unblock))
        {
            var state = new NetworkState<long, long>();
            foreach (var id in string.IsNullOrEmpty(block) ? [] : graph.ResolveNodes(block))
            {
                state.Block(id);
            }
            foreach (var id in string.IsNullOrEmpty(unblock) ? [] : graph.ResolveNodes(unblock))
            {
                state.Unblock(id);
            }
            options = options with { State = state };
        }

        return options;
    }

    private static string? Optional(IDictionary<string, object> parameters, string name)
        => parameters.TryGetValue(name, out var value) ? value?.ToString() : null;

    private static string Required(IDictionary<string, object> parameters, string name)
    {
        var value = Optional(parameters, name);
        return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"-{name} is required.") : value;
    }

    #endregion

    #region Output

    private readonly record struct Row(long Node, int TypeId, double Cost, string Kind, string Source);

    private static int NodeType(FileNetworkGraph graph, long id) => graph.TryGetNode(id, out var n) ? n.Node.TypeId : 0;

    private static string ShardKey(FileNetworkGraph graph, long shard)
    {
        var key = graph.Metadata.Shards.FirstOrDefault(s => s.Id == shard)?.Key;
        return key is null ? "" : $" ({key})";
    }

    private static void LogTypes(FileNetworkGraph graph, IEnumerable<int> typeIds, ICommandLogger? logger)
    {
        foreach (var group in typeIds.GroupBy(t => t).OrderByDescending(g => g.Count()))
        {
            logger?.LogLine($"    {group.Count(),8}  {graph.TypeName(group.Key)}");
        }
    }

    /// <summary>-out x.csv (one row per node) or -out x.geojson (nodes as points, edges as straight lines).</summary>
    private static void Write(FileNetworkGraph graph, IDictionary<string, object> parameters, ICommandLogger? logger, IEnumerable<Row> rows, IReadOnlyList<long> edgeIds)
    {
        var path = Optional(parameters, "out");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        // a node with merged points (customers drawn on a box) is followed by one row per merged point
        var list = new List<(Row Row, long Id, string Label, int TypeId, string AliasOf)>();
        foreach (var row in rows)
        {
            list.Add((row, row.Node, graph.NodeLabel(row.Node), row.TypeId, ""));
            foreach (var alias in graph.AliasesOf(row.Node))
            {
                list.Add((row, alias, graph.NodeLabel(alias), graph.AliasTypeOf(alias), graph.NodeLabel(row.Node)));
            }
        }
        if (path.EndsWith(".geojson", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            var features = new List<object>();
            foreach (var (row, id, label, typeId, aliasOf) in list)
            {
                if (graph.TryGetNode(row.Node, out var node))
                {
                    features.Add(new
                    {
                        type = "Feature",
                        geometry = new { type = "Point", coordinates = new[] { node.Node.Location.X, node.Node.Location.Y } },
                        properties = new { kind = row.Kind, id = id.ToString(), node = label, type = graph.TypeName(typeId), cost = row.Cost, source = row.Source, alias_of = aliasOf }
                    });
                }
            }
            foreach (var edgeId in edgeIds)
            {
                if (graph.TryGetEdge(edgeId, out var edge)
                    && graph.TryGetNode(edge.Edge.From, out var from)
                    && graph.TryGetNode(edge.Edge.To, out var to))
                {
                    features.Add(new
                    {
                        type = "Feature",
                        geometry = new { type = "LineString", coordinates = new[] { new[] { from.Node.Location.X, from.Node.Location.Y }, new[] { to.Node.Location.X, to.Node.Location.Y } } },
                        properties = new { kind = "Edge", id = edgeId.ToString(), edge = graph.EdgeLabel(edgeId), type = graph.TypeName(edge.Edge.TypeId), length = edge.Edge.Length }
                    });
                }
            }
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                type = "FeatureCollection",
                crs = graph.Metadata.Srs is null ? null : new { type = "name", properties = new { name = graph.Metadata.Srs } },
                features
            }));
        }
        else
        {
            var sb = new StringBuilder("id;node;type;kind;cost;source;alias_of\n");
            foreach (var (row, id, label, typeId, aliasOf) in list)
            {
                sb.Append(id).Append(';').Append(label).Append(';').Append(graph.TypeName(typeId)).Append(';')
                  .Append(row.Kind).Append(';').Append(row.Cost.ToString("0.###", CultureInfo.InvariantCulture)).Append(';').Append(row.Source).Append(';')
                  .Append(aliasOf).Append('\n');
            }
            File.WriteAllText(path, sb.ToString());
        }
        logger?.LogLine($"Written: {path} ({list.Count(r => r.AliasOf.Length == 0)} nodes, {list.Count(r => r.AliasOf.Length > 0)} merged points{(edgeIds.Count > 0 ? $", {edgeIds.Count} edges" : "")})");
    }

    /// <summary>One row (or point) per violation: the node with its merged points (aliases), the edge for edge rules.</summary>
    private static void WriteViolations(FileNetworkGraph graph, string path, IReadOnlyList<RuleViolation<long, long>> violations)
    {
        string Aliases(long node) => string.Join(";", graph.AliasesOf(node).Select(graph.NodeLabel));
        string Edge(RuleViolation<long, long> v) => v.Kind == ElementKind.Edge ? graph.EdgeLabel(v.Edge) : "";

        if (path.EndsWith(".geojson", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                type = "FeatureCollection",
                crs = graph.Metadata.Srs is null ? null : new { type = "name", properties = new { name = graph.Metadata.Srs } },
                features = violations.Select(v => new
                {
                    type = "Feature",
                    geometry = new { type = "Point", coordinates = new[] { v.Location.X, v.Location.Y } },
                    properties = new
                    {
                        rule = v.RuleId,
                        severity = v.Severity.ToString(),
                        kind = v.Kind.ToString(),
                        node = graph.NodeLabel(v.Node),
                        edge = Edge(v),
                        type = graph.TypeName(v.TypeId),
                        count = v.Count,
                        min = v.Min,
                        max = v.Max,
                        aliases = Aliases(v.Node)
                    }
                })
            }));
            return;
        }

        var sb = new StringBuilder("rule;severity;kind;node;edge;type;count;min;max;x;y;aliases\n");
        foreach (var v in violations)
        {
            sb.Append(v.RuleId).Append(';').Append(v.Severity).Append(';').Append(v.Kind).Append(';')
              .Append(graph.NodeLabel(v.Node)).Append(';').Append(Edge(v)).Append(';').Append(graph.TypeName(v.TypeId)).Append(';')
              .Append(v.Count).Append(';').Append(v.Min).Append(';').Append(v.Max?.ToString(CultureInfo.InvariantCulture) ?? "").Append(';')
              .Append(v.Location.X.ToString("0.###", CultureInfo.InvariantCulture)).Append(';')
              .Append(v.Location.Y.ToString("0.###", CultureInfo.InvariantCulture)).Append(';')
              .Append(Aliases(v.Node)).Append('\n');
        }
        File.WriteAllText(path, sb.ToString());
    }

    #endregion
}
