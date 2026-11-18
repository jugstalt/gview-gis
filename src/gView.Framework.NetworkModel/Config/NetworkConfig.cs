using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Framework.NetworkModel.Config;

/// <summary>
/// The definition of a network: which layers of a map take part, how their features become typed nodes and edges,
/// and which polygons assign the shards. The single source of truth (JSON), a map may later embed it.
/// </summary>
public class NetworkConfig
{
    /// <summary>The name of the network.</summary>
    public string Name { get; set; } = "network";

    /// <summary>The map (mxl) holding the layers; relative to the config file.</summary>
    public string? Map { get; set; }

    /// <summary>The spatial reference the network is built in, e.g. "epsg:31256". Empty for the map's spatial reference. Must be metric.</summary>
    public string? Srs { get; set; }

    /// <summary>Line ends and points closer than this are joined into one node (units of the srs).</summary>
    public double Tolerance { get; set; } = 0.01;

    /// <summary>What happens to points of different node layers within the tolerance of each other.</summary>
    public NodeConflictMode NodeConflicts { get; set; } = NodeConflictMode.Error;

    /// <summary>The size of the grid cells for nodes outside every shard polygon.</summary>
    public double FallbackShardCellSize { get; set; } = 2000;

    /// <summary>The facets of a type, e.g. Ebene, Objektart, Bauart. Values not listed are numbered automatically.</summary>
    public List<FacetConfig> Facets { get; set; } = [];

    /// <summary>The weight definitions; the length is always there.</summary>
    public List<WeightConfig> Weights { get; set; } = [];

    /// <summary>Point layers that become real nodes.</summary>
    public List<NodeLayerConfig> Nodes { get; set; } = [];

    /// <summary>Line layers that become edges.</summary>
    public List<EdgeLayerConfig> Edges { get; set; } = [];

    /// <summary>The polygon layer that assigns shards.</summary>
    public ShardConfig? Shards { get; set; }

    /// <summary>The validation rules; written into network.json on build, checked by gView.Cmd -u validate.</summary>
    public List<NetworkRuleConfig> Rules { get; set; } = [];

    /// <summary>The default sources of supply and outage, a node pattern, e.g. [*,Trafo,*]; written into network.json on build.</summary>
    public string? SupplySources { get; set; }

    /// <summary>The default customers (sinks) of supply and outage, a node pattern, e.g. [*,Kunde,*]; written into network.json on build.</summary>
    public string? Customers { get; set; }

    /// <summary>
    /// Where the built network is kept, relative to the map (mxl). Empty: {map name}.network next to the map.
    /// Build writes there by default, Carto and the server search there.
    /// </summary>
    public string? Directory { get; set; }

    /// <summary>The network directory for a map document (see <see cref="Directory"/>).</summary>
    public string ResolveDirectory(string mapPath)
    {
        var mapDirectory = Path.GetDirectoryName(Path.GetFullPath(mapPath))!;
        if (string.IsNullOrWhiteSpace(Directory))
        {
            return Path.Combine(mapDirectory, Path.GetFileNameWithoutExtension(mapPath) + ".network");
        }
        return Path.GetFullPath(Path.IsPathRooted(Directory) ? Directory : Path.Combine(mapDirectory, Directory));
    }

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static NetworkConfig Load(string path)
        => JsonSerializer.Deserialize<NetworkConfig>(File.ReadAllText(path), JsonOptions)
           ?? throw new InvalidDataException($"Empty network config: {path}");

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public void Validate()
    {
        if (Facets.Count == 0)
        {
            throw new InvalidDataException("A network config needs at least one facet.");
        }
        if (!(Tolerance > 0))
        {
            throw new InvalidDataException("The tolerance must be greater than 0.");
        }
        if (Nodes.Count == 0 && Edges.Count == 0)
        {
            throw new InvalidDataException("A network config needs node or edge layers.");
        }
        foreach (var layer in Nodes.Cast<LayerConfig>().Concat(Edges))
        {
            if (string.IsNullOrWhiteSpace(layer.Layer) && layer.LayerId is null)
            {
                throw new InvalidDataException("Every node and edge entry needs a layer.");
            }
            if (layer.Type.Count != Facets.Count)
            {
                throw new InvalidDataException($"Layer '{layer.Layer}': the type has {layer.Type.Count} templates, but there are {Facets.Count} facets.");
            }
        }
        foreach (var edge in Edges)
        {
            foreach (var weightName in edge.Weights.Keys)
            {
                if (!Weights.Any(w => w.Name.Equals(weightName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidDataException($"Layer '{edge.Layer}': unknown weight '{weightName}'.");
                }
            }
        }
        var duplicateRule = Rules.GroupBy(r => r.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1 || string.IsNullOrWhiteSpace(g.Key));
        if (duplicateRule is not null)
        {
            throw new InvalidDataException(string.IsNullOrWhiteSpace(duplicateRule.Key) ? "Every rule needs an id." : $"The rule id '{duplicateRule.Key}' is used twice.");
        }
        if (Shards is not null && ((string.IsNullOrWhiteSpace(Shards.Layer) && Shards.LayerId is null) || string.IsNullOrWhiteSpace(Shards.Field)))
        {
            throw new InvalidDataException("The shards need a layer and a field.");
        }
    }
}

/// <summary>What happens to points of different node layers within the tolerance of each other (SpatialNET ADR 0054).</summary>
public enum NodeConflictMode
{
    /// <summary>A data error: the build stops and lists every conflict (ADR 0050).</summary>
    Error,

    /// <summary>
    /// The point of the layer higher in the TOC stays (the lowest object id within a layer); the others are merged into it
    /// and kept as aliases, so Layer#OID still finds their node.
    /// </summary>
    PriorityByToc,
}

public class FacetConfig
{
    public string Name { get; set; } = "";

    /// <summary>Predefined values (number → text); they win over automatic numbering.</summary>
    public Dictionary<int, string> Values { get; set; } = [];
}

public class WeightConfig
{
    public string Name { get; set; } = "";
    public SpatialNET.Network.WeightKind Kind { get; set; } = SpatialNET.Network.WeightKind.PerLength;
    public double? Fallback { get; set; }
}

public abstract class LayerConfig
{
    /// <summary>The layer in the map: its TOC path "Group/Layer" or its name, if unique.</summary>
    public string Layer { get; set; } = "";

    /// <summary>The id of the layer in the map; wins over <see cref="Layer"/> (survives renaming and moving in the TOC).</summary>
    public int? LayerId { get; set; }

    /// <summary>Optional where clause; only matching features take part.</summary>
    public string? Where { get; set; }

    /// <summary>
    /// One template per facet: a fixed text, [FIELD], {layer-name}, {group-name} or {group-name:n} (n-th group from the top), combinable.
    /// Empty means "not set".
    /// </summary>
    public List<string> Type { get; set; } = [];
}

public class NodeLayerConfig : LayerConfig
{
    /// <summary>The normal switching state: the node is blocked when the field has one of the values.</summary>
    public FieldValuesConfig? Blocked { get; set; }

    /// <summary>
    /// Points of this layer within the tolerance of each other are duplicates; the lowest object id takes part,
    /// the others are left out and reported. On by default.
    /// </summary>
    public bool RemoveDuplicates { get; set; } = true;
}

public class EdgeLayerConfig : LayerConfig
{
    /// <summary>A surveyed length template, e.g. [AUFMASS]; empty for the geometric length.</summary>
    public string? Length { get; set; }

    /// <summary>both, forward, backward, or a template that yields one of these.</summary>
    public string? Direction { get; set; }

    /// <summary>The normal switching state: the edge is disabled when the field has one of the values.</summary>
    public FieldValuesConfig? Disabled { get; set; }

    /// <summary>Weight name → template of its value (a number or [FIELD]).</summary>
    public Dictionary<string, string> Weights { get; set; } = [];
}

public class FieldValuesConfig
{
    public string Field { get; set; } = "";
    public List<string> Values { get; set; } = [];

    public bool Matches(string? value)
        => value is not null && Values.Any(v => v.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
}

public class ShardConfig
{
    public string Layer { get; set; } = "";
    public int? LayerId { get; set; }
    public string? Where { get; set; }

    /// <summary>The field holding the shard key, e.g. E_TP.</summary>
    public string Field { get; set; } = "";
}
