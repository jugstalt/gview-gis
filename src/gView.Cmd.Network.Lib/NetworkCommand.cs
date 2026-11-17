using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Cmd.MxlUtil.Lib;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.IO;
using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Ids;
using gView.Framework.NetworkModel.Module;
using gView.Framework.NetworkModel.Store;
using SpatialNET.Network.Building;
using System.Text.Json;

namespace gView.Cmd.Network.Lib;

/// <summary>
/// SpatialNET networks:
///   -u build -config net.json [-mxl net.mxl] -output dir [-json true]
///   -u config-from-mxl -mxl net.mxl -output net.json
/// </summary>
public class NetworkCommand : ICommand
{
    public string Name => "Network";

    public string Description => "Build and search SpatialNET networks (-u build | config-from-mxl | trace | reach | path | supply | info)";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
        [
            new RequiredCommandParameter<string>("u") { Description = "build | config-from-mxl | trace | reach | path | supply | info" },
            new CommandParameter<string>("config") { Description = "network config (json); build" },
            new CommandParameter<string>("mxl") { Description = "map (mxl); for build it overrides the map of the config" },
            new CommandParameter<string>("output") { Description = "build: network directory; config-from-mxl: json file" },
            new CommandParameter<string>("json") { Description = "build: true also writes each shard as json (debugging)" },
            new CommandParameter<string>("network") { Description = "searches: the network directory" },
            new CommandParameter<string>("start") { Description = "trace, reach: nodes (Layer#OID, id or type pattern like [*,Trafo,*]; comma separated)" },
            new CommandParameter<string>("from") { Description = "path: start node" },
            new CommandParameter<string>("to") { Description = "path: end node" },
            new CommandParameter<string>("sources") { Description = "supply: source nodes" },
            new CommandParameter<string>("sinks") { Description = "supply: sink nodes" },
            new CommandParameter<string>("node") { Description = "info: the node" },
            new CommandParameter<string>("filter") { Description = "searches: only edges of these types (pattern)" },
            new CommandParameter<string>("block") { Description = "searches: nodes blocked for this search" },
            new CommandParameter<string>("unblock") { Description = "searches: nodes opened for this search" },
            new CommandParameter<string>("weight") { Description = "searches: weight name (default length)" },
            new CommandParameter<string>("max-cost") { Description = "searches: stop at this cost" },
            new CommandParameter<string>("out") { Description = "searches: result file (.csv or .geojson)" },
        ];

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        var utility = parameters.GetValue<string>("u")?.ToLowerInvariant();
        var output = parameters.GetValue<string>("output");

        switch (utility)
        {
            case "build":
                return await Build(parameters, output, logger);
            case "config-from-mxl":
                return await ConfigFromMxl(parameters.GetValue<string>("mxl"), output, logger);
            case "trace":
            case "reach":
            case "path":
            case "supply":
            case "info":
                return await NetworkAnalysisCommands.Run(utility, parameters, logger);
            default:
                logger?.LogLine("Usage:");
                logger?.LogLine("  -u build (-config <net.json> [-mxl <map.mxl>] | -mxl <map.mxl with network from Carto>) -output <directory> [-json true]");
                logger?.LogLine("  -u config-from-mxl -mxl <map.mxl> -output <net.json>");
                logger?.LogLine("  -u trace  -network <dir> -start <nodes> [-filter <pattern>] [-block <nodes>] [-unblock <nodes>] [-out x.csv|x.geojson]");
                logger?.LogLine("  -u reach  -network <dir> -start <nodes> -max-cost <cost> [-weight <name>] [-out ...]");
                logger?.LogLine("  -u path   -network <dir> -from <node> -to <node> [-weight <name>] [-out ...]");
                logger?.LogLine("  -u supply -network <dir> -sources <nodes> -sinks <nodes> [-out ...]");
                logger?.LogLine("  -u info   -network <dir> -node <node>");
                logger?.LogLine("  nodes: Layer#OID (Trafo#5929), node id, or type pattern ([*,Trafo,*]), comma separated");
                return false;
        }
    }

    private static async Task<bool> Build(IDictionary<string, object> parameters, string output, ICommandLogger? logger)
    {
        var configPath = parameters.GetValue<string>("config");
        var mxlPath = parameters.GetValue<string>("mxl");
        NetworkConfig config;
        Map map;
        if (string.IsNullOrEmpty(configPath))
        {
            // the config stored in the map by Carto (network module)
            if (string.IsNullOrEmpty(mxlPath))
            {
                throw new ArgumentException("Pass -config <net.json> or -mxl <map.mxl> with a network configured in Carto.");
            }
            map = await LoadMap(mxlPath);
            var module = NetworkModuleReader.Read(map, reader =>
            {
                var stream = new XmlStream("");
                stream.ReadStream(mxlPath);
                stream.Load("MapDocument", null, reader);
            }) ?? throw new ArgumentException($"{mxlPath} has no network configured. Configure it in Carto (layer settings, page Network) or pass -config.");
            config = module.Config;
            logger?.LogLine($"Config: network of {mxlPath}");
        }
        else
        {
            config = NetworkConfig.Load(configPath);
            map = null!;
        }

        if (string.IsNullOrEmpty(mxlPath))
        {
            if (string.IsNullOrEmpty(config.Map))
            {
                throw new ArgumentException("No map: set \"map\" in the config or pass -mxl.");
            }
            mxlPath = Path.IsPathRooted(config.Map)
                ? config.Map
                : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(configPath))!, config.Map));
        }

        logger?.LogLine($"Map:    {mxlPath}");
        logger?.LogLine($"Output: {output}");

        map ??= await LoadMap(mxlPath);
        var store = new FileNetworkStore(output, "true".Equals(parameters.GetValue<string>("json"), StringComparison.OrdinalIgnoreCase));
        var service = new NetworkBuildService(message => logger?.LogLine(message));

        NetworkBuildReport report;
        try
        {
            report = await service.BuildAsync(map, config, store);
        }
        catch (NetworkBuildException ex)
        {
            WriteWarningsAndDuplicates(service, output, logger);

            string Source(object? source) => source is FeatureRef r ? $"{service.LayerOf(r.FeatureClass)} #{r.Oid}" : source?.ToString() ?? "";

            logger?.LogLine($"The data cannot form a network: {ex.Conflicts.Count} conflict(s)");
            foreach (var group in ex.Conflicts
                .GroupBy(c => c.Kind == NodeConflictKind.RealNodesTooClose
                    ? string.Join(" / ", c.Sources.OfType<FeatureRef>().Select(r => service.LayerOf(r.FeatureClass)).Distinct().Order())
                    : c.Kind.ToString())
                .OrderByDescending(g => g.Count()))
            {
                logger?.LogLine($"  {group.Count(),6}  {group.Key}");
            }
            File.WriteAllText(Path.Combine(output, "conflicts.json"), JsonSerializer.Serialize(
                ex.Conflicts.Select(c => new { Kind = c.Kind.ToString(), c.Location.X, c.Location.Y, Sources = c.Sources.Select(Source) }),
                NetworkConfig.JsonOptions));
            logger?.LogLine($"All conflicts: {Path.Combine(output, "conflicts.json")}");
            return false;
        }

        WriteWarningsAndDuplicates(service, output, logger);
        File.Delete(Path.Combine(output, "conflicts.json")); // from an earlier failed build

        foreach (var group in report.Diagnostics.GroupBy(d => d.Kind))
        {
            logger?.LogLine($"Diagnostic {group.Key}: {group.Count()}");
        }
        File.WriteAllText(Path.Combine(output, "diagnostics.json"), JsonSerializer.Serialize(
            report.Diagnostics.Select(d => new { Kind = d.Kind.ToString(), d.Location.X, d.Location.Y, d.Message }),
            NetworkConfig.JsonOptions));

        logger?.LogLine($"Types: {report.Metadata.Types.Count(t => t.Used)} used");
        foreach (var type in report.Metadata.Types.Where(t => t.Used))
        {
            logger?.LogLine($"  {type.Id,4} {type.Kind,-4} {type.Name}");
        }

        return true;
    }

    private static void WriteWarningsAndDuplicates(NetworkBuildService service, string output, ICommandLogger? logger)
    {
        Directory.CreateDirectory(output);
        foreach (var warning in service.Warnings)
        {
            logger?.LogLine($"Warning: {warning}");
        }
        foreach (var group in service.Duplicates.GroupBy(d => d.Layer))
        {
            logger?.LogLine($"Duplicates left out {group.Key}: {group.Count()}");
        }
        File.WriteAllText(Path.Combine(output, "duplicates.json"), JsonSerializer.Serialize(service.Duplicates, NetworkConfig.JsonOptions));
    }

    private static async Task<bool> ConfigFromMxl(string mxlPath, string output, ICommandLogger? logger)
    {
        if (string.IsNullOrEmpty(mxlPath))
        {
            throw new ArgumentException("-mxl is required.");
        }

        var map = await LoadMap(mxlPath);
        var layers = MapLayers.FeatureLayers(map);

        var config = new NetworkConfig
        {
            Name = Path.GetFileNameWithoutExtension(mxlPath),
            Map = Path.GetRelativePath(Path.GetDirectoryName(Path.GetFullPath(output))!, Path.GetFullPath(mxlPath)),
            Facets =
            [
                new FacetConfig { Name = "Ebene" },
                new FacetConfig { Name = "Objektart" },
                new FacetConfig { Name = "Bauart" },
            ]
        };

        foreach (var layer in layers)
        {
            var fc = layer.Layer.FeatureClass;
            var fields = fc.Fields?.ToEnumerable().Select(f => f.name).ToArray() ?? [];
            logger?.LogLine($"{fc.GeometryType,-10} {layer.Path}");
            logger?.LogLine($"           fields: {string.Join(", ", fields)}");

            var type = new List<string> { "{group-name}", "{layer-name}", "" };
            switch (fc.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    config.Nodes.Add(new NodeLayerConfig { Layer = layer.Path, Type = type });
                    break;
                case GeometryType.Polyline:
                    config.Edges.Add(new EdgeLayerConfig { Layer = layer.Path, Type = type });
                    break;
                case GeometryType.Polygon when config.Shards is null:
                    config.Shards = new ShardConfig
                    {
                        Layer = layer.Path,
                        Field = fields.FirstOrDefault(f => f.Equals("E_TP", StringComparison.OrdinalIgnoreCase)) ?? "SHARD_FIELD"
                    };
                    break;
            }
        }

        File.WriteAllText(output, config.ToJson());
        logger?.LogLine($"Written: {output} (nodes: {config.Nodes.Count}, edges: {config.Edges.Count}, shards: {config.Shards?.Layer ?? "-"})");
        return true;
    }

    private static async Task<Map> LoadMap(string mxlPath)
    {
        var stream = new XmlStream("");
        if (!stream.ReadStream(mxlPath))
        {
            throw new FileNotFoundException($"Can't read map {mxlPath}");
        }

        var doc = new MxlDocument();
        await stream.LoadAsync("MapDocument", doc);

        var map = doc.Maps.FirstOrDefault() as Map ?? throw new InvalidDataException($"No map in {mxlPath}");
        if (map.HasErrorMessages(ErrorMessageLevel.Warning))
        {
            throw new InvalidDataException($"Can't load {mxlPath}:{Environment.NewLine}{string.Join('\n', map.ErrorMessages(ErrorMessageLevel.Warning))}");
        }
        return map;
    }
}
