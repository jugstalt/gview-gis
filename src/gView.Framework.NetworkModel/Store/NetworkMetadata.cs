using gView.Framework.NetworkModel.Config;
using SpatialNET.Network;

namespace gView.Framework.NetworkModel.Store;

/// <summary>What a built network says about itself (network.json): types, weights, feature classes and shards.</summary>
public class NetworkMetadata
{
    public int FormatVersion { get; set; } = 1;
    public string Name { get; set; } = "";
    public DateTime Created { get; set; }
    public string? Srs { get; set; }
    public double Tolerance { get; set; }
    public int NodeCount { get; set; }
    public int EdgeCount { get; set; }

    /// <summary>The number of points merged into the node of another point (aliases.idx).</summary>
    public int AliasCount { get; set; }
    public List<NetworkFacetMetadata> Facets { get; set; } = [];
    public List<NetworkTypeMetadata> Types { get; set; } = [];
    public List<NetworkWeightMetadata> Weights { get; set; } = [];
    public List<NetworkFeatureClassMetadata> FeatureClasses { get; set; } = [];
    public List<NetworkShardMetadata> Shards { get; set; } = [];

    /// <summary>Shard keys that are no numbers (e.g. E_TP as text) and the shard ids they got.</summary>
    public Dictionary<string, long> ShardKeys { get; set; } = [];

    /// <summary>The validation rules of the config at build time; gView.Cmd -u validate checks them unless -rules is given.</summary>
    public List<NetworkRuleConfig> Rules { get; set; } = [];

    /// <summary>The default sources of supply and outage (node pattern) of the config at build time.</summary>
    public string? SupplySources { get; set; }

    /// <summary>The default customers (sinks) of supply and outage (node pattern) of the config at build time.</summary>
    public string? Customers { get; set; }
}

public class NetworkFacetMetadata
{
    public string Name { get; set; } = "";
    public Dictionary<int, string> Values { get; set; } = [];
}

public class NetworkTypeMetadata
{
    public int Id { get; set; }
    public ElementKind Kind { get; set; }
    public int[] Facets { get; set; } = [];
    public string? Name { get; set; }

    /// <summary>Whether elements of the type exist in this build (unused types stay to keep ids stable).</summary>
    public bool Used { get; set; }
}

public class NetworkWeightMetadata
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public WeightKind Kind { get; set; }
    public double? Fallback { get; set; }
}

public class NetworkFeatureClassMetadata
{
    public int Index { get; set; }
    public string Layer { get; set; } = "";
    public string FeatureClass { get; set; } = "";
    public ElementKind Kind { get; set; }
}

public class NetworkShardMetadata
{
    public long Id { get; set; }
    public string? Key { get; set; }
    public int NodeCount { get; set; }
    public int EdgeCount { get; set; }
    public string File { get; set; } = "";
}
