using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;

namespace gView.Framework.NetworkModel.Build;

/// <summary>A feature layer of a map with its id, name and group path in the TOC.</summary>
public sealed record MapLayer(IFeatureLayer Layer, string Name, IReadOnlyList<string> GroupPath)
{
    public int Id => Layer.ID;

    public string Path => GroupPath.Count == 0 ? Name : $"{string.Join('/', GroupPath)}/{Name}";
}

public static class MapLayers
{
    /// <summary>All feature layers of the map.</summary>
    public static IReadOnlyList<MapLayer> FeatureLayers(IMap map)
    {
        var result = new List<MapLayer>();
        foreach (var element in map.MapElements)
        {
            if (element is not IFeatureLayer featureLayer || featureLayer.FeatureClass is null)
            {
                continue;
            }

            var tocElement = map.TOC?.GetTOCElement(featureLayer);
            var name = tocElement?.Name ?? featureLayer.Title ?? featureLayer.FeatureClass.Name;

            var groups = new List<string>();
            for (var group = tocElement?.ParentGroup; group is not null; group = group.ParentGroup)
            {
                groups.Insert(0, group.Name);
            }

            result.Add(new MapLayer(featureLayer, name, groups));
        }
        return result;
    }

    /// <summary>Finds a layer by its id, else by its TOC path ("Group/Layer") or, if unique, by its name.</summary>
    public static MapLayer Find(IReadOnlyList<MapLayer> layers, int? layerId, string? reference)
    {
        if (layerId is int id)
        {
            var byId = layers.FirstOrDefault(l => l.Id == id);
            if (byId is not null)
            {
                return byId;
            }
            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new InvalidDataException($"Layer with id {id} not found in the map.");
            }
        }

        return Find(layers, reference ?? "");
    }

    /// <summary>Finds a layer by its TOC path ("Group/Layer") or, if unique, by its name.</summary>
    public static MapLayer Find(IReadOnlyList<MapLayer> layers, string reference)
    {
        var byPath = layers.Where(l => l.Path.Equals(reference, StringComparison.OrdinalIgnoreCase)).ToList();
        if (byPath.Count == 1)
        {
            return byPath[0];
        }

        var byName = layers.Where(l => l.Name.Equals(reference, StringComparison.OrdinalIgnoreCase)).ToList();
        return byName.Count switch
        {
            1 => byName[0],
            0 => throw new InvalidDataException($"Layer '{reference}' not found in the map."),
            _ => throw new InvalidDataException($"Layer name '{reference}' is ambiguous, use the TOC path: {string.Join(", ", byName.Select(l => l.Path))}")
        };
    }
}
