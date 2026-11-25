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

    public string Description => "Build and search SpatialNET networks (-u build | refresh-metadata | config-from-mxl | trace | reach | path | supply | outage | feeders | info | nearest | validate)";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
        [
            new RequiredCommandParameter<string>("u") { Description = "build | config-from-mxl | trace | reach | path | supply | outage | feeders | info | nearest | validate" },
            new CommandParameter<string>("config") { Description = "network config (json); build" },
            new CommandParameter<string>("mxl") { Description = "map (mxl); for build it overrides the map of the config" },
            new CommandParameter<string>("output") { Description = "build: network directory (default: \"directory\" of the config or <map>.network next to the mxl); config-from-mxl: json file" },
            new CommandParameter<string>("json") { Description = "build: true also writes each shard as json (debugging)" },
            new CommandParameter<string>("network") { Description = "searches: the network directory" },
            new CommandParameter<string>("start") { Description = "trace, reach: nodes (Layer#OID, id, type pattern like [*,Trafo,*] or @x,y[~radius]; comma separated)" },
            new CommandParameter<string>("from") { Description = "path: start node" },
            new CommandParameter<string>("to") { Description = "path: end node" },
            new CommandParameter<string>("sources") { Description = "supply, outage, feeders: source nodes (default: the sources of the tier, else supplySources of the config)" },
            new CommandParameter<string>("open-edges") { Description = "feeders: true also reports the edges no branch went along at nodes a branch went through (Open)" },
            new CommandParameter<string>("branches") { Description = "feeders: type pattern of the edges at a source that start a branch, e.g. [Niederspannung,Anlagenverbinder,*] (default: every edge)" },
            new CommandParameter<string>("sinks") { Description = "supply, outage: sink nodes (default: customers of the config)" },
            new CommandParameter<string>("disable") { Description = "searches: edges switched off (Layer#OID, Layer#OID/part, id); outage: the edges to switch off" },
            new CommandParameter<string>("x") { Description = "nearest: x in the spatial reference of the network" },
            new CommandParameter<string>("y") { Description = "nearest: y in the spatial reference of the network" },
            new CommandParameter<string>("radius") { Description = "nearest: search radius (default 10 x tolerance)" },
            new CommandParameter<string>("count") { Description = "nearest: number of nodes (default 1)" },
            new CommandParameter<string>("node") { Description = "info: the node" },
            new CommandParameter<string>("filter") { Description = "searches: only edges of these types (pattern)" },
            new CommandParameter<string>("tier") { Description = "searches, validate: only these tiers (Ebenen, names from the config, comma separated)" },
            new CommandParameter<string>("stop-at-sources") { Description = "trace with -tier: false goes on through the sources of the tier (default true)" },
            new CommandParameter<string>("block") { Description = "searches: nodes blocked for this search" },
            new CommandParameter<string>("unblock") { Description = "searches: nodes opened for this search" },
            new CommandParameter<string>("area") { Description = "searches, validate: only this area: xmin,ymin,xmax,ymax | GeoJSON/Esri JSON polygon | WKT POLYGON/MULTIPOLYGON | shard:key1,key2 (network srs)" },
            new CommandParameter<string>("area-file") { Description = "searches, validate: a file with the area (instead of -area)" },
            new CommandParameter<string>("weight") { Description = "searches: weight name (default length)" },
            new CommandParameter<string>("max-cost") { Description = "searches: stop at this cost" },
            new CommandParameter<string>("out") { Description = "searches, validate: result file (.csv or .geojson)" },
            new CommandParameter<string>("rules") { Description = "validate: rules (json array, or an object with \"rules\"); default: the rules in network.json" },
            new CommandParameter<string>("rule") { Description = "validate: check only these rule ids (comma separated)" },
        ];

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        var utility = parameters.GetValue<string>("u")?.ToLowerInvariant();
        var output = parameters.GetValue<string>("output");

        switch (utility)
        {
            case "build":
                return await Build(parameters, output, logger);
            case "refresh-metadata":
                return await RefreshMetadata(parameters, output, logger);
            case "config-from-mxl":
                return await ConfigFromMxl(parameters.GetValue<string>("mxl"), output, logger);
            case "trace":
            case "reach":
            case "path":
            case "supply":
            case "outage":
            case "feeders":
            case "info":
            case "nearest":
            case "validate":
                return await NetworkAnalysisCommands.Run(utility, parameters, logger);
            default:
                logger?.LogLine("Usage:");
                logger?.LogLine("  -u build (-config <net.json> [-mxl <map.mxl>] | -mxl <map.mxl with network from Carto>) [-output <directory>, default: directory of the config or <map>.network] [-json true]");
                logger?.LogLine("  -u refresh-metadata (-config <net.json> [-mxl <map.mxl>] | -mxl <map.mxl>) [-output <network directory>]: rewrites supplySources, customers, rules and tiers in network.json without building");
                logger?.LogLine("  -u config-from-mxl -mxl <map.mxl> -output <net.json>");
                logger?.LogLine("  -u trace  -network <dir> -start <nodes> [-filter <pattern>] [-block <nodes>] [-unblock <nodes>] [-out x.csv|x.geojson]");
                logger?.LogLine("  -u reach  -network <dir> -start <nodes> -max-cost <cost> [-weight <name>] [-out ...]");
                logger?.LogLine("  -u path   -network <dir> -from <node> -to <node> [-weight <name>] [-out ...]");
                logger?.LogLine("  -u supply -network <dir> -sources <nodes> -sinks <nodes> [-out ...]");
                logger?.LogLine("  -u outage -network <dir> -disable <edges> [-sources <nodes>] [-sinks <nodes>] [-filter <pattern>] [-out ...]");
                logger?.LogLine("  -u feeders -network <dir> [-sources <nodes>] [-branches <edge type pattern>] [-tier <tier>] [-out ...]: every edge gets its source and branch");
                logger?.LogLine("  -u info   -network <dir> -node <node>");
                logger?.LogLine("  -u nearest -network <dir> -x <x> -y <y> [-radius <r>] [-count <n>]");
                logger?.LogLine("  -u validate -network <dir> [-rules <rules.json>] [-rule <id,...>] [-out x.csv|x.geojson]");
                logger?.LogLine("  nodes: Layer#OID (Trafo#5929), node id, type pattern ([*,Trafo,*], ![*,Kunde,*]) or @x,y[~radius] (nearest node, also edge ends), comma separated");
                logger?.LogLine("  edges: Layer#OID (every part of the line), Layer#OID/part or edge id, comma separated");
                logger?.LogLine("  searches and validate take [-area <xmin,ymin,xmax,ymax | json | wkt | shard:keys> | -area-file <file>]: only the nodes inside take part");
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

        if (string.IsNullOrEmpty(output))
        {
            output = config.ResolveDirectory(mxlPath);
        }

        logger?.LogLine($"Map:    {mxlPath}");
        logger?.LogLine($"Output: {output}");

        map ??= await LoadMap(mxlPath);
        return await NetworkBuildRunner.RunAsync(
            map, config, output,
            "true".Equals(parameters.GetValue<string>("json"), StringComparison.OrdinalIgnoreCase),
            message => logger?.LogLine(message));
    }

    /// <summary>Takes supplySources, customers, rules and tiers from the config into the built network (no shards are built).</summary>
    private static async Task<bool> RefreshMetadata(IDictionary<string, object> parameters, string output, ICommandLogger? logger)
    {
        var configPath = parameters.GetValue<string>("config");
        var mxlPath = parameters.GetValue<string>("mxl");
        NetworkConfig config;
        if (string.IsNullOrEmpty(configPath))
        {
            if (string.IsNullOrEmpty(mxlPath))
            {
                throw new ArgumentException("Pass -config <net.json> or -mxl <map.mxl> with a network configured in Carto.");
            }
            var map = await LoadMap(mxlPath);
            config = NetworkModuleReader.Read(map, reader =>
            {
                var stream = new XmlStream("");
                stream.ReadStream(mxlPath);
                stream.Load("MapDocument", null, reader);
            })?.Config ?? throw new ArgumentException($"{mxlPath} has no network configured.");
        }
        else
        {
            config = NetworkConfig.Load(configPath);
            if (string.IsNullOrEmpty(mxlPath) && !string.IsNullOrEmpty(config.Map))
            {
                mxlPath = Path.IsPathRooted(config.Map) ? config.Map : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(configPath))!, config.Map));
            }
        }

        if (string.IsNullOrEmpty(output))
        {
            output = string.IsNullOrEmpty(mxlPath)
                ? throw new ArgumentException("Pass -output <network directory> (or -mxl / \"map\" in the config to find it).")
                : config.ResolveDirectory(mxlPath);
        }

        var metadata = NetworkMetadataRefresh.Refresh(config, output, out var warnings);
        logger?.LogLine($"Refreshed: {output} ({metadata.Tiers.Count} tiers, {metadata.Rules.Count} rules, sources {metadata.SupplySources ?? "-"}, customers {metadata.Customers ?? "-"})");
        foreach (var warning in warnings)
        {
            logger?.LogLine($"Warning: {warning}");
        }
        return true;
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
