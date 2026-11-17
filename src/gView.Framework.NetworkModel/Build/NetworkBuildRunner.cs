using gView.Framework.Core.Carto;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Store;
using SpatialNET.Network.Building;
using System.Text;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Build;

/// <summary>
/// Builds a network into a directory and writes its reports next to it, the same for gView.Cmd and the server:
/// duplicates.json, merged.json, diagnostics.json (or conflicts.json when the data cannot form a network) and build.log.
/// </summary>
public static class NetworkBuildRunner
{
    public const string LogFile = "build.log";

    public static readonly IReadOnlyList<string> ReportFiles = ["build.log", "duplicates.json", "merged.json", "diagnostics.json", "conflicts.json"];

    /// <summary>True when the network was built; false when the data has conflicts (see conflicts.json).</summary>
    public static async Task<bool> RunAsync(IMap map, NetworkConfig config, string output, bool writeJson, Action<string>? log, CancellationToken cancellationToken = default)
    {
        var lines = new StringBuilder();
        void Log(string line)
        {
            lines.AppendLine($"{DateTime.Now:HH:mm:ss} {line}");
            log?.Invoke(line);
        }

        Log($"Build {config.Name} -> {output}");
        var store = new FileNetworkStore(output, writeJson);
        var service = new NetworkBuildService(Log);
        try
        {
            NetworkBuildReport report;
            try
            {
                report = await service.BuildAsync(map, config, store, cancellationToken);
            }
            catch (NetworkBuildException ex)
            {
                WriteWarningsAndDuplicates(service, output, Log);

                string Source(object? source) => source is FeatureRef r ? $"{service.LayerOf(r.FeatureClass)} #{r.Oid}" : source?.ToString() ?? "";

                Log($"The data cannot form a network: {ex.Conflicts.Count} conflict(s)");
                foreach (var group in ex.Conflicts
                    .GroupBy(c => c.Kind == NodeConflictKind.RealNodesTooClose
                        ? string.Join(" / ", c.Sources.OfType<FeatureRef>().Select(r => service.LayerOf(r.FeatureClass)).Distinct().Order())
                        : c.Kind.ToString())
                    .OrderByDescending(g => g.Count()))
                {
                    Log($"  {group.Count(),6}  {group.Key}");
                }
                File.WriteAllText(Path.Combine(output, "conflicts.json"), JsonSerializer.Serialize(
                    ex.Conflicts.Select(c => new { Kind = c.Kind.ToString(), c.Location.X, c.Location.Y, Sources = c.Sources.Select(Source) }),
                    NetworkConfig.JsonOptions));
                Log($"All conflicts: {Path.Combine(output, "conflicts.json")}");
                return false;
            }

            WriteWarningsAndDuplicates(service, output, Log);
            File.Delete(Path.Combine(output, "conflicts.json")); // from an earlier failed build

            foreach (var group in report.Diagnostics.GroupBy(d => d.Kind))
            {
                Log($"Diagnostic {group.Key}: {group.Count()}");
            }
            File.WriteAllText(Path.Combine(output, "diagnostics.json"), JsonSerializer.Serialize(
                report.Diagnostics.Select(d => new { Kind = d.Kind.ToString(), d.Location.X, d.Location.Y, d.Message }),
                NetworkConfig.JsonOptions));

            Log($"Types: {report.Metadata.Types.Count(t => t.Used)} used");
            foreach (var type in report.Metadata.Types.Where(t => t.Used))
            {
                Log($"  {type.Id,4} {type.Kind,-4} {type.Name}");
            }
            Log($"Built: {report.Metadata.NodeCount} nodes, {report.Metadata.EdgeCount} edges, {report.Metadata.Shards.Count} shards");
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log($"Error: {ex.Message}");
            throw;
        }
        finally
        {
            try
            {
                Directory.CreateDirectory(output);
                File.WriteAllText(Path.Combine(output, LogFile), lines.ToString());
            }
            catch (IOException) { }
        }
    }

    private static void WriteWarningsAndDuplicates(NetworkBuildService service, string output, Action<string> log)
    {
        Directory.CreateDirectory(output);
        foreach (var warning in service.Warnings)
        {
            log($"Warning: {warning}");
        }
        foreach (var group in service.Duplicates.GroupBy(d => d.Layer))
        {
            log($"Duplicates left out {group.Key}: {group.Count()}");
        }
        File.WriteAllText(Path.Combine(output, "duplicates.json"), JsonSerializer.Serialize(service.Duplicates, NetworkConfig.JsonOptions));

        if (service.Merged.Count > 0)
        {
            log($"Merged into the point of a layer higher in the TOC: {service.Merged.Count}");
            foreach (var group in service.Merged.GroupBy(m => $"{m.MergedLayer} -> {m.KeptLayer}").OrderByDescending(g => g.Count()))
            {
                log($"  {group.Count(),6}  {group.Key}");
            }
        }
        File.WriteAllText(Path.Combine(output, "merged.json"), JsonSerializer.Serialize(service.Merged, NetworkConfig.JsonOptions));
    }
}
