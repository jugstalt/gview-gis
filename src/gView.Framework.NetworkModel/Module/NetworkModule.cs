using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Extensions;
using gView.Framework.NetworkModel.Graph;
using gView.Framework.NetworkModel.Store;
using System.Text.Json;

namespace gView.Framework.NetworkModel.Module;

/// <summary>
/// Keeps the network config of a map inside the map document (mxl), with layers referenced by their id.
/// Edited in Carto (module settings and the "Network" page of the layer settings), exported as JSON for gView.Cmd.
/// </summary>
[RegisterPlugIn(PluginGuid, PluginUsage.Server)]
public class NetworkModule : IMapApplicationModule, IPersistable
{
    public const string PluginGuid = "6C1F0E5A-3B7D-4C59-9A2E-8D4F1B6E2C71";

    public IMap? Map { get; private set; }

    public NetworkConfig Config { get; private set; } = CreateDefaultConfig();

    /// <summary>Whether a config was read from a document (then <see cref="OnCreate"/> keeps it).</summary>
    private bool _loaded;

    /// <summary>Whether anything was configured; an empty module is not written into the map document.</summary>
    public bool IsConfigured => Config.Nodes.Count > 0 || Config.Edges.Count > 0 || Config.Shards is not null;

    public static NetworkConfig CreateDefaultConfig() => new()
    {
        Facets =
        [
            new FacetConfig { Name = "Ebene" },
            new FacetConfig { Name = "Objektart" },
            new FacetConfig { Name = "Bauart" },
        ]
    };

    #region IMapApplicationModule

    public void OnCreate(object hook)
    {
        if (hook is IMap map)
        {
            Map = map;
            // the server loads a module before it calls OnCreate: keep a config read from the document
            if (!_loaded)
            {
                Config = CreateDefaultConfig();
                Config.Name = map.Name;
            }
        }
    }

    #endregion

    #region Layers

    public NodeLayerConfig? GetNodeLayer(int layerId) => Config.Nodes.FirstOrDefault(n => n.LayerId == layerId);

    public EdgeLayerConfig? GetEdgeLayer(int layerId) => Config.Edges.FirstOrDefault(e => e.LayerId == layerId);

    public bool IsShardLayer(int layerId) => Config.Shards?.LayerId == layerId;

    /// <summary>Sets or (with null) removes the node settings of a layer.</summary>
    public void SetNodeLayer(int layerId, NodeLayerConfig? settings)
    {
        Config.Nodes.RemoveAll(n => n.LayerId == layerId);
        if (settings is not null)
        {
            settings.LayerId = layerId;
            settings.Layer = LayerPath(layerId) ?? settings.Layer;
            Config.Nodes.Add(settings);
        }
    }

    /// <summary>Sets or (with null) removes the edge settings of a layer.</summary>
    public void SetEdgeLayer(int layerId, EdgeLayerConfig? settings)
    {
        Config.Edges.RemoveAll(e => e.LayerId == layerId);
        if (settings is not null)
        {
            settings.LayerId = layerId;
            settings.Layer = LayerPath(layerId) ?? settings.Layer;
            Config.Edges.Add(settings);
        }
    }

    /// <summary>Makes the layer the shard layer, or (with null) removes it when it is the shard layer.</summary>
    public void SetShardLayer(int layerId, ShardConfig? settings)
    {
        if (settings is not null)
        {
            settings.LayerId = layerId;
            settings.Layer = LayerPath(layerId) ?? settings.Layer;
            Config.Shards = settings;
        }
        else if (IsShardLayer(layerId))
        {
            Config.Shards = null;
        }
    }

    /// <summary>
    /// A type template for every facet: {group-name} (the group of the layer: one group per tier, the first facet is the
    /// tier), {layer-name}, then empty.
    /// </summary>
    public List<string> DefaultTypeTemplates()
        => Config.Facets.Select((_, i) => i switch { 0 => "{group-name}", 1 => "{layer-name}", _ => "" }).ToList();

    /// <summary>Adds every point and line layer that is not configured yet; returns how many were added.</summary>
    public int AddAllLayers()
    {
        if (Map is null)
        {
            return 0;
        }

        int added = 0;
        foreach (var layer in MapLayers.FeatureLayers(Map))
        {
            if (GetNodeLayer(layer.Id) is not null || GetEdgeLayer(layer.Id) is not null)
            {
                continue;
            }
            switch (layer.Layer.FeatureClass.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    SetNodeLayer(layer.Id, new NodeLayerConfig { Type = DefaultTypeTemplates() });
                    added++;
                    break;
                case GeometryType.Polyline:
                    SetEdgeLayer(layer.Id, new EdgeLayerConfig { Type = DefaultTypeTemplates() });
                    added++;
                    break;
            }
        }
        return added;
    }

    /// <summary>Resizes the type templates of every layer after the facets changed.</summary>
    public void AlignTypeTemplates()
    {
        foreach (var layer in Config.Nodes.Cast<LayerConfig>().Concat(Config.Edges))
        {
            while (layer.Type.Count < Config.Facets.Count)
            {
                layer.Type.Add("");
            }
            if (layer.Type.Count > Config.Facets.Count)
            {
                layer.Type.RemoveRange(Config.Facets.Count, layer.Type.Count - Config.Facets.Count);
            }
        }
    }

    private string? LayerPath(int layerId)
        => Map is null ? null : MapLayers.FeatureLayers(Map).FirstOrDefault(l => l.Id == layerId)?.Path;

    #endregion

    #region Build

    /// <summary>The build started from Carto (running or the last one); null when none was started.</summary>
    public NetworkBuildState? BuildState { get; private set; }

    public NetworkStatus GetStatus(string? mapPath)
    {
        if (string.IsNullOrEmpty(mapPath))
        {
            return NetworkStatus.NotBuilt(null);
        }
        var directory = Config.ResolveDirectory(mapPath);
        NetworkMetadata? metadata = null;
        try
        {
            if (File.Exists(Path.Combine(directory, FileNetworkStore.MetadataFile)))
            {
                metadata = new FileNetworkStore(directory).ReadMetadata();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { }

        if (metadata is null)
        {
            return NetworkStatus.NotBuilt(directory);
        }

        // the build time is written in UTC: compare in UTC, show in local time
        var created = AsUtc(metadata.Created);
        string? reason = null;
        if (File.Exists(mapPath) && File.GetLastWriteTimeUtc(mapPath) > created)
        {
            reason = "The map was saved after the build.";
        }
        return new(directory, true, created.ToLocalTime(), metadata.NodeCount, metadata.EdgeCount, metadata.Shards.Count, reason is not null, reason);
    }

    /// <summary>A time of network.json in UTC; one without a kind was written as UTC (DateTime.UtcNow).</summary>
    internal static DateTime AsUtc(DateTime time) => time.Kind switch
    {
        DateTimeKind.Utc => time,
        DateTimeKind.Local => time.ToUniversalTime(),
        _ => DateTime.SpecifyKind(time, DateTimeKind.Utc),
    };

    /// <summary>The known values of every facet (fixed values of the config, values of a built network) for pickers.</summary>
    public List<List<string>> FacetValueSuggestions(string? mapPath)
    {
        NetworkMetadata? metadata = null;
        try
        {
            var directory = string.IsNullOrEmpty(mapPath) ? null : Config.ResolveDirectory(mapPath);
            if (directory is not null && File.Exists(Path.Combine(directory, FileNetworkStore.MetadataFile)))
            {
                metadata = new FileNetworkStore(directory).ReadMetadata();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { }

        return Config.Facets.Select(facet =>
        {
            var values = facet.Values.Values.AsEnumerable();
            var built = metadata?.Facets.FirstOrDefault(f => f.Name.Equals(facet.Name, StringComparison.OrdinalIgnoreCase));
            if (built is not null)
            {
                values = values.Concat(built.Values.Values);
            }
            return values.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToList();
        }).ToList();
    }

    /// <summary>
    /// Writes supply sources, customers, rules and tiers into the built network (network.json) without building the shards.
    /// Throws when the config or its tier patterns are invalid or the network is not built.
    /// </summary>
    public NetworkMetadata RefreshMetadata(string mapPath)
        => NetworkMetadataRefresh.Refresh(Clone(Config), Config.ResolveDirectory(mapPath));

    /// <summary>
    /// Checks the tier patterns against the types of the built network; the problems, empty when all are fine.
    /// Says so when there is no built network to check against.
    /// </summary>
    public List<string> CheckTiers(string? mapPath)
    {
        var problems = new List<string>();
        try
        {
            Config.ValidateTiersAndRules();
        }
        catch (InvalidDataException ex)
        {
            problems.Add(ex.Message);
        }
        var directory = string.IsNullOrEmpty(mapPath) ? null : Config.ResolveDirectory(mapPath);
        NetworkMetadata? metadata = directory is not null && File.Exists(Path.Combine(directory, FileNetworkStore.MetadataFile))
            ? new FileNetworkStore(directory).ReadMetadata()
            : null;
        if (metadata is null)
        {
            problems.Add("The network is not built yet: the patterns can only be checked against its types.");
            return problems;
        }
        var types = metadata.ToTypeRegistry();
        try
        {
            Config.ResolveTiers(types);
        }
        catch (InvalidDataException ex)
        {
            problems.Add(ex.Message);
        }
        try
        {
            Config.ResolveRules(types);
        }
        catch (InvalidDataException ex)
        {
            problems.Add(ex.Message);
        }
        foreach (var (rule, index) in Config.ConnectRules.Select((r, i) => (r, i + 1)))
        {
            try
            {
                types.Nodes(rule.Nodes);
                types.Edges(rule.Edges);
            }
            catch (ArgumentException ex)
            {
                problems.Add($"Connect rule {index} ({rule}): {ex.Message}");
            }
        }
        return problems;
    }

    /// <summary>Builds the network of the map in the background; false when a build is already running.</summary>
    public bool StartBuild(string mapPath)
    {
        if (Map is null)
        {
            throw new InvalidOperationException("The module is not attached to a map.");
        }
        if (string.IsNullOrEmpty(mapPath))
        {
            throw new InvalidOperationException("Save the map first: the network is built next to it.");
        }
        if (BuildState is { Running: true })
        {
            return false;
        }

        Config.Validate();
        var map = Map;
        var config = Clone(Config); // later edits must not change a running build
        var directory = config.ResolveDirectory(mapPath);
        var state = new NetworkBuildState();
        BuildState = state;

        _ = Task.Run(async () =>
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(directory)!);
                state.AddLine($"Network directory: {directory}");
                state.Succeeded = await NetworkBuildRunner.RunAsync(map, config, directory, false, state.AddLine);
                NetworkGraphCache.Shared.Invalidate(directory);
            }
            catch (Exception ex)
            {
                state.Error = ex.Message;
                state.AddLine($"Error: {ex.Message}");
                state.Succeeded = false;
            }
            finally
            {
                state.Finished = DateTime.Now;
                state.Notify();
            }
        });
        return true;
    }

    #endregion

    #region JSON

    /// <summary>The config as JSON for gView.Cmd: layer paths refreshed from the map, ids kept.</summary>
    public string ExportJson(string? mapPath = null)
    {
        var config = Clone(Config);
        config.Map = mapPath;
        foreach (var layer in config.Nodes.Cast<LayerConfig>().Concat(config.Edges))
        {
            if (layer.LayerId is int id)
            {
                layer.Layer = LayerPath(id) ?? layer.Layer;
            }
        }
        if (config.Shards?.LayerId is int shardId)
        {
            config.Shards.Layer = LayerPath(shardId) ?? config.Shards.Layer;
        }
        return config.ToJson();
    }

    /// <summary>Replaces the config by a JSON config; layers are resolved to ids in this map. Returns the problems.</summary>
    public IReadOnlyList<string> ImportJson(string json)
    {
        var config = JsonSerializer.Deserialize<NetworkConfig>(json, NetworkConfig.JsonOptions)
                     ?? throw new InvalidDataException("Empty network config.");
        var problems = new List<string>();
        var layers = Map is null ? [] : MapLayers.FeatureLayers(Map);

        bool Resolve(string reference, int? layerId, Action<int> setId)
        {
            try
            {
                setId(MapLayers.Find(layers, layerId, reference).Id);
                return true;
            }
            catch (InvalidDataException ex)
            {
                problems.Add(ex.Message);
                return false;
            }
        }

        config.Nodes.RemoveAll(n => !Resolve(n.Layer, n.LayerId, id => n.LayerId = id));
        config.Edges.RemoveAll(e => !Resolve(e.Layer, e.LayerId, id => e.LayerId = id));
        if (config.Shards is not null && !Resolve(config.Shards.Layer, config.Shards.LayerId, id => config.Shards.LayerId = id))
        {
            config.Shards = null;
        }

        config.Map = null;
        Config = config;
        AlignTypeTemplates();
        return problems;
    }

    /// <summary>The rules as a JSON array (the format of gView.Cmd -u validate -rules).</summary>
    public string ExportRulesJson() => JsonSerializer.Serialize(Config.Rules, NetworkConfig.JsonOptions);

    /// <summary>Replaces the rules by a JSON array of rules, or the "rules" of a network config.</summary>
    public int ImportRulesJson(string json)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var element = document.RootElement;
        if (element.ValueKind == JsonValueKind.Object)
        {
            element = element.EnumerateObject().FirstOrDefault(p => p.Name.Equals("rules", StringComparison.OrdinalIgnoreCase)).Value;
        }
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("No rules: expected a JSON array of rules or an object with \"rules\".");
        }
        Config.Rules = element.Deserialize<List<NetworkRuleConfig>>(NetworkConfig.JsonOptions) ?? [];
        return Config.Rules.Count;
    }

    public static T Clone<T>(T value)
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, NetworkConfig.JsonOptions), NetworkConfig.JsonOptions)!;

    #endregion

    #region IPersistable

    public void Load(IPersistStream stream)
    {
        var json = stream?.Load("config", null) as string;
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }
        try
        {
            Config = JsonSerializer.Deserialize<NetworkConfig>(json, NetworkConfig.JsonOptions) ?? Config;
            _loaded = true;
        }
        catch
        {
            // a broken config must not stop the map from loading
        }
    }

    public void Save(IPersistStream stream)
    {
        if (stream is null)
        {
            return;
        }
        // refresh the layer paths for readers of the mxl; the ids decide
        var json = ExportJson();
        stream.Save("config", json);
    }

    #endregion
}
