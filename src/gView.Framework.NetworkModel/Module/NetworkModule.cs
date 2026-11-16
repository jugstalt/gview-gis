using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.NetworkModel.Build;
using gView.Framework.NetworkModel.Config;
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
            Config = CreateDefaultConfig();
            Config.Name = map.Name;
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

    /// <summary>A type template for every facet: {group-name}, {layer-name}, then empty.</summary>
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
