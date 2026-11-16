using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.NetworkModel.Config;
using gView.Framework.NetworkModel.Module;

namespace gView.Carto.Razor.Components.Dialogs.Models;

/// <summary>What a layer is in the network, as a working copy for the layer settings dialog.</summary>
public enum NetworkLayerRole { None, Node, Edge, Shards }

/// <summary>
/// The network settings of one layer, copied from the <see cref="NetworkModule"/> when the layer settings dialog
/// opens and written back only on Apply.
/// </summary>
public class NetworkLayerSettings
{
    public required NetworkModule Module { get; init; }
    public required int LayerId { get; init; }

    /// <summary>The role the geometry allows: Node (points), Edge (lines), Shards (polygons).</summary>
    public required NetworkLayerRole PossibleRole { get; init; }

    public bool Participates { get; set; }

    public NodeLayerConfig Node { get; set; } = new();
    public EdgeLayerConfig Edge { get; set; } = new();
    public ShardConfig Shards { get; set; } = new();

    public IReadOnlyList<FacetConfig> Facets => Module.Config.Facets;
    public IReadOnlyList<WeightConfig> Weights => Module.Config.Weights;

    /// <summary>The settings of a layer, or null when its geometry cannot take part in a network.</summary>
    public static NetworkLayerSettings? Create(NetworkModule? module, ILayer? layer)
    {
        if (module is null || layer is not IFeatureLayer featureLayer || featureLayer.FeatureClass is null)
        {
            return null;
        }

        var role = featureLayer.FeatureClass.GeometryType switch
        {
            GeometryType.Point or GeometryType.Multipoint => NetworkLayerRole.Node,
            GeometryType.Polyline => NetworkLayerRole.Edge,
            GeometryType.Polygon => NetworkLayerRole.Shards,
            _ => NetworkLayerRole.None
        };
        if (role == NetworkLayerRole.None)
        {
            return null;
        }

        var settings = new NetworkLayerSettings { Module = module, LayerId = layer.ID, PossibleRole = role };
        switch (role)
        {
            case NetworkLayerRole.Node when module.GetNodeLayer(layer.ID) is { } node:
                settings.Participates = true;
                settings.Node = NetworkModule.Clone(node);
                break;
            case NetworkLayerRole.Edge when module.GetEdgeLayer(layer.ID) is { } edge:
                settings.Participates = true;
                settings.Edge = NetworkModule.Clone(edge);
                break;
            case NetworkLayerRole.Shards when module.IsShardLayer(layer.ID):
                settings.Participates = true;
                settings.Shards = NetworkModule.Clone(module.Config.Shards!);
                break;
        }

        // new or outdated templates: one per facet
        if (settings.Node.Type.Count == 0) settings.Node.Type = module.DefaultTypeTemplates();
        if (settings.Edge.Type.Count == 0) settings.Edge.Type = module.DefaultTypeTemplates();
        Align(settings.Node.Type, module.Config.Facets.Count);
        Align(settings.Edge.Type, module.Config.Facets.Count);

        if (string.IsNullOrEmpty(settings.Shards.Field)
            && featureLayer.FeatureClass.Fields?.ToEnumerable().FirstOrDefault(f => f.name.Equals("E_TP", StringComparison.OrdinalIgnoreCase)) is { } eTp)
        {
            settings.Shards.Field = eTp.name;
        }

        return settings;
    }

    /// <summary>Writes the settings back to the module.</summary>
    public void Apply()
    {
        switch (PossibleRole)
        {
            case NetworkLayerRole.Node:
                Module.SetNodeLayer(LayerId, Participates ? Node : null);
                break;
            case NetworkLayerRole.Edge:
                Module.SetEdgeLayer(LayerId, Participates ? Edge : null);
                break;
            case NetworkLayerRole.Shards:
                Module.SetShardLayer(LayerId, Participates ? Shards : null);
                break;
        }
    }

    private static void Align(List<string> templates, int count)
    {
        while (templates.Count < count) templates.Add("");
        if (templates.Count > count) templates.RemoveRange(count, templates.Count - count);
    }
}
