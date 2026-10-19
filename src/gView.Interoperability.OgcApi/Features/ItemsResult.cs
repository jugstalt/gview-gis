using gView.Interoperability.OgcApi.GeoJson;
using gView.Interoperability.OgcApi.Models;
using System.Text.Json;

namespace gView.Interoperability.OgcApi.Features;

/// <summary>
/// Result of an items request: one page of features plus its links
/// </summary>
public sealed class ItemsResult
{
    internal ItemsResult(OgcCollectionLayer collection, ItemsQuery query, FeaturePage page, List<Link> links, bool swapAxes)
    {
        Collection = collection;
        Query = query;
        Page = page;
        Links = links;
        SwapAxes = swapAxes;
    }

    public OgcCollectionLayer Collection { get; }
    public ItemsQuery Query { get; }
    public FeaturePage Page { get; }
    public List<Link> Links { get; }
    public bool SwapAxes { get; }
    public DateTime TimeStamp { get; } = DateTime.UtcNow;

    public CrsReference Crs => Query.Crs;

    public byte[] ToGeoJson()
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms, GeoJsonWriter.WriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "FeatureCollection");

            writer.WritePropertyName("features");
            writer.WriteStartArray();
            foreach (var feature in Page.Features)
            {
                GeoJsonWriter.WriteFeature(writer, feature, SwapAxes);
            }
            writer.WriteEndArray();

            GeoJsonWriter.WriteLinks(writer, Links);
            writer.WriteString("timeStamp", TimeStamp.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteNumber("numberReturned", Page.Features.Count);
            writer.WriteEndObject();
        }

        return ms.ToArray();
    }
}

/// <summary>
/// Result of a single item request
/// </summary>
public sealed class ItemResult
{
    internal ItemResult(OgcCollectionLayer collection, OgcFeature feature, CrsReference crs, List<Link> links, bool swapAxes)
    {
        Collection = collection;
        Feature = feature;
        Crs = crs;
        Links = links;
        SwapAxes = swapAxes;
    }

    public OgcCollectionLayer Collection { get; }
    public OgcFeature Feature { get; }
    public CrsReference Crs { get; }
    public List<Link> Links { get; }
    public bool SwapAxes { get; }

    public byte[] ToGeoJson()
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms, GeoJsonWriter.WriterOptions))
        {
            GeoJsonWriter.WriteFeature(writer, Feature, SwapAxes, Links);
        }

        return ms.ToArray();
    }
}
