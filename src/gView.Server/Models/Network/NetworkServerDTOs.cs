using gView.Interoperability.GeoServices.Rest.Reflection;
using System.Text.Json.Serialization;

namespace gView.Server.Models.Network;

/// <summary>The NetworkServer resource of a map service (geoservices/rest/services/.../networkserver).</summary>
[ServiceMethod("Trace", "trace")]
[ServiceMethod("Reach", "reach")]
[ServiceMethod("Path", "path")]
[ServiceMethod("Supply", "supply")]
[ServiceMethod("Outage", "outage")]
[ServiceMethod("Info", "info")]
[ServiceMethod("Nearest", "nearest")]
[ServiceMethod("Validate", "validate")]
public class JsonNetworkServerDTO
{
    [JsonPropertyName("currentVersion")]
    public double CurrentVersion { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("created")]
    public string Created { get; set; } = "";

    [JsonPropertyName("srs")]
    public string? Srs { get; set; }

    [JsonPropertyName("nodeCount")]
    public int NodeCount { get; set; }

    [JsonPropertyName("edgeCount")]
    public int EdgeCount { get; set; }

    [JsonPropertyName("mergedPointCount")]
    public int MergedPointCount { get; set; }

    [JsonPropertyName("shardCount")]
    public int ShardCount { get; set; }

    [JsonPropertyName("operations")]
    public string[] Operations { get; set; } = [];

    [JsonPropertyName("nodeLayers")]
    public string[] NodeLayers { get; set; } = [];

    [JsonPropertyName("edgeLayers")]
    public string[] EdgeLayers { get; set; } = [];

    [JsonPropertyName("weights")]
    public string[] Weights { get; set; } = [];

    [JsonPropertyName("types")]
    public TypeInfo[] Types { get; set; } = [];

    [JsonPropertyName("rules")]
    public RuleInfo[] Rules { get; set; } = [];

    [JsonPropertyName("tiers")]
    public TierInfo[] Tiers { get; set; } = [];

    public class TypeInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("kind")]
        public string Kind { get; set; } = "";

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class TierInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; } = "";
    }

    public class RuleInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("severity")]
        public string Severity { get; set; } = "";

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}

#region Forms (shown when an operation is called without parameters)

public class JsonNetworkSearchOptionsDTO
{
    [JsonPropertyName("filter")]
    public string Filter { get; set; } = "";

    /// <summary>Only these tiers (Ebenen, comma separated names; see the tiers of the NetworkServer resource).</summary>
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = "";

    /// <summary>trace with a tier: stop at the sources of the tier (default true).</summary>
    [JsonPropertyName("stopAtSources")]
    public bool StopAtSources { get; set; } = true;

    [JsonPropertyName("block")]
    public string Block { get; set; } = "";

    [JsonPropertyName("unblock")]
    public string Unblock { get; set; } = "";

    [JsonPropertyName("disable")]
    public string Disable { get; set; } = "";

    /// <summary>Only the nodes inside take part: xmin,ymin,xmax,ymax, GeoJSON/Esri JSON polygon, WKT or shard:key1,key2 (in inSR).</summary>
    [JsonPropertyName("area")]
    [FormInput(FormInputAttribute.InputTypes.TextBox)]
    public string Area { get; set; } = "";

    [JsonPropertyName("weight")]
    public string Weight { get; set; } = "";

    [JsonPropertyName("maxCost")]
    public string MaxCost { get; set; } = "";

    [JsonPropertyName("geometry")]
    public bool Geometry { get; set; } = true;

    [JsonPropertyName("snap")]
    public bool Snap { get; set; }

    [JsonPropertyName("attributes")]
    public bool Attributes { get; set; }

    [JsonPropertyName("outFields")]
    public string OutFields { get; set; } = "*";

    [JsonPropertyName("inSR")]
    public string InSR { get; set; } = "";

    [JsonPropertyName("outSR")]
    public string OutSR { get; set; } = "";

    [JsonPropertyName("f")]
    [FormInput(Values = new[] { "pjson", "json", "geojson", "csv" })]
    public string F { get; set; } = "pjson";
}

public class JsonNetworkTraceDTO : JsonNetworkSearchOptionsDTO
{
    [JsonPropertyName("start")]
    public string Start { get; set; } = "";
}

public class JsonNetworkReachDTO : JsonNetworkTraceDTO { }

public class JsonNetworkPathDTO : JsonNetworkSearchOptionsDTO
{
    [JsonPropertyName("from")]
    public string From { get; set; } = "";

    [JsonPropertyName("to")]
    public string To { get; set; } = "";
}

public class JsonNetworkSupplyDTO : JsonNetworkSearchOptionsDTO
{
    [JsonPropertyName("sources")]
    public string Sources { get; set; } = "";

    [JsonPropertyName("sinks")]
    public string Sinks { get; set; } = "";
}

/// <summary>outage: the edges to switch off are "disable"; sources and sinks default to the config of the network.</summary>
public class JsonNetworkOutageDTO : JsonNetworkSearchOptionsDTO
{
    [JsonPropertyName("sources")]
    public string Sources { get; set; } = "";

    [JsonPropertyName("sinks")]
    public string Sinks { get; set; } = "";
}

public class JsonNetworkInfoDTO
{
    [JsonPropertyName("node")]
    public string Node { get; set; } = "";

    [JsonPropertyName("inSR")]
    public string InSR { get; set; } = "";

    [JsonPropertyName("f")]
    [FormInput(Values = new[] { "pjson", "json" })]
    public string F { get; set; } = "pjson";
}

public class JsonNetworkNearestDTO
{
    [JsonPropertyName("x")]
    public string X { get; set; } = "";

    [JsonPropertyName("y")]
    public string Y { get; set; } = "";

    [JsonPropertyName("radius")]
    public string Radius { get; set; } = "";

    [JsonPropertyName("count")]
    public string Count { get; set; } = "1";

    [JsonPropertyName("inSR")]
    public string InSR { get; set; } = "";

    [JsonPropertyName("outSR")]
    public string OutSR { get; set; } = "";

    [JsonPropertyName("f")]
    [FormInput(Values = new[] { "pjson", "json", "geojson", "csv" })]
    public string F { get; set; } = "pjson";
}

public class JsonNetworkValidateDTO
{
    [JsonPropertyName("rules")]
    public string Rules { get; set; } = "";

    /// <summary>Only the nodes inside and the edges with an end inside are checked (in inSR).</summary>
    [JsonPropertyName("area")]
    [FormInput(FormInputAttribute.InputTypes.TextBox)]
    public string Area { get; set; } = "";

    [JsonPropertyName("inSR")]
    public string InSR { get; set; } = "";

    [JsonPropertyName("geometry")]
    public bool Geometry { get; set; } = true;

    [JsonPropertyName("attributes")]
    public bool Attributes { get; set; }

    [JsonPropertyName("outFields")]
    public string OutFields { get; set; } = "*";

    [JsonPropertyName("outSR")]
    public string OutSR { get; set; } = "";

    [JsonPropertyName("f")]
    [FormInput(Values = new[] { "pjson", "json", "geojson", "csv" })]
    public string F { get; set; } = "pjson";
}

#endregion
