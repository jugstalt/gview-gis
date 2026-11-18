using gView.Cmd.Core.Abstraction;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Query;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace gView.Cmd.Network.Lib;

/// <summary>
/// Searches on a built network with <see cref="NetworkQueryService"/>: trace, reach, path, supply, outage, info, nearest, validate.
/// Nodes are given as Layer#OID, raw id, type pattern ([*,Trafo,*]) or @x,y[~radius].
/// </summary>
internal static class NetworkAnalysisCommands
{
    public static Task<bool> Run(string utility, IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        var directory = Required(parameters, "network");
        var stopwatch = Stopwatch.StartNew();
        var graph = new FileNetworkGraph(directory) { MaxLoadedShards = 0 };
        logger?.LogLine($"Network: {graph.Metadata.Name}, {graph.Metadata.NodeCount} nodes, {graph.Metadata.EdgeCount} edges, {graph.Metadata.Shards.Count} shards");

        var outPath = Optional(parameters, "out");
        var rulesPath = Optional(parameters, "rules");
        var maxCost = Optional(parameters, "max-cost");
        var request = new NetworkQueryRequest
        {
            Start = Optional(parameters, "start"),
            From = Optional(parameters, "from"),
            To = Optional(parameters, "to"),
            Sources = Optional(parameters, "sources"),
            Sinks = Optional(parameters, "sinks"),
            Node = Optional(parameters, "node"),
            Filter = Optional(parameters, "filter"),
            Block = Optional(parameters, "block"),
            Unblock = Optional(parameters, "unblock"),
            Disable = Optional(parameters, "disable"),
            X = Number(parameters, "x"),
            Y = Number(parameters, "y"),
            Radius = Number(parameters, "radius"),
            Count = Number(parameters, "count") is double count ? (int)count : null,
            Weight = Optional(parameters, "weight"),
            MaxCost = string.IsNullOrEmpty(maxCost) ? null : double.Parse(maxCost.Replace(',', '.'), CultureInfo.InvariantCulture),
            Rules = string.IsNullOrEmpty(rulesPath) ? null : LoadRules(rulesPath),
            RuleIds = Optional(parameters, "rule"),
            WithEdges = !string.IsNullOrEmpty(outPath),
        };

        var service = new NetworkQueryService(graph);
        var result = service.Run(utility, request, new SyncProgress<string>(line => logger?.LogLine(line)));
        foreach (var line in result.Messages)
        {
            logger?.LogLine(line);
        }

        if (!string.IsNullOrEmpty(outPath) && utility != "info")
        {
            NetworkResultWriter.Write(outPath, result);
            logger?.LogLine(utility == "validate"
                ? $"Written: {outPath} ({result.Violations.Count} violations)"
                : $"Written: {outPath} ({result.Nodes.Count(n => n.AliasOf.Length == 0)} nodes, {result.Nodes.Count(n => n.AliasOf.Length > 0)} merged points{(result.Edges.Count > 0 ? $", {result.Edges.Count} edges" : "")})");
        }

        logger?.LogLine($"Shards loaded: {graph.LoadedShardCount} of {graph.Metadata.Shards.Count}, {stopwatch.ElapsedMilliseconds} ms");
        return Task.FromResult(result.Success);
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

    private static string? Optional(IDictionary<string, object> parameters, string name)
        => parameters.TryGetValue(name, out var value) ? value?.ToString() : null;

    private static double? Number(IDictionary<string, object> parameters, string name)
    {
        var value = Optional(parameters, name);
        return string.IsNullOrWhiteSpace(value) ? null : double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
    }

    private static string Required(IDictionary<string, object> parameters, string name)
    {
        var value = Optional(parameters, name);
        return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"-{name} is required.") : value;
    }
}
