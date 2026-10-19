using System.Text.Json.Serialization;

namespace gView.Interoperability.OgcApi.Models;

public class Link
{
    public Link() { }

    public Link(string href, string rel, string? type = null, string? title = null)
    {
        Href = href;
        Rel = rel;
        Type = type;
        Title = title;
    }

    [JsonPropertyName("href")]
    public string Href { get; set; } = "";

    [JsonPropertyName("rel")]
    public string Rel { get; set; } = "";

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class LandingPage
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("links")]
    public List<Link> Links { get; set; } = new();
}

public class ConformanceDeclaration
{
    [JsonPropertyName("conformsTo")]
    public string[] ConformsTo { get; set; } = [];
}

public class Collections
{
    [JsonPropertyName("links")]
    public List<Link> Links { get; set; } = new();

    [JsonPropertyName("collections")]
    public List<Collection> Items { get; set; } = new();

    /// <summary>
    /// Part 2: CRS supported by all collections
    /// </summary>
    [JsonPropertyName("crs")]
    public string[]? Crs { get; set; }
}

public class Collection
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("links")]
    public List<Link> Links { get; set; } = new();

    [JsonPropertyName("extent")]
    public Extent? Extent { get; set; }

    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; } = "feature";

    [JsonPropertyName("crs")]
    public string[]? Crs { get; set; }

    [JsonPropertyName("storageCrs")]
    public string? StorageCrs { get; set; }
}

public class Extent
{
    [JsonPropertyName("spatial")]
    public SpatialExtent? Spatial { get; set; }
}

public class SpatialExtent
{
    [JsonPropertyName("bbox")]
    public double[][] BBox { get; set; } = [];

    [JsonPropertyName("crs")]
    public string Crs { get; set; } = CrsReference.Crs84Uri;
}

public class ExceptionResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Not an OGC resource: the server-wide list (/ogcapi) that links to the landing pages of the services.
/// </summary>
public class ServiceList
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "gView OGC API";

    [JsonPropertyName("links")]
    public List<Link> Links { get; set; } = new();

    [JsonPropertyName("services")]
    public List<ServiceListItem> Services { get; set; } = new();
}

public class ServiceListItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("folder")]
    public string? Folder { get; set; }

    [JsonPropertyName("links")]
    public List<Link> Links { get; set; } = new();
}
