using gView.Interoperability.OgcApi.Models;
using System.Text.Json;

namespace gView.Interoperability.OgcApi.GeoJson;

/// <summary>
/// The OGC API parts of GeoJSON (links); features and geometries are written by the shared <see cref="GeoJsonWriter"/>
/// </summary>
internal static class OgcGeoJsonWriter
{
    public static void WriteFeature(Utf8JsonWriter writer, OgcFeature feature, bool swapAxes, IEnumerable<Link>? links)
        => GeoJsonWriter.WriteFeature(writer, feature, swapAxes, links is null ? null : w => WriteLinks(w, links));

    public static void WriteLinks(Utf8JsonWriter writer, IEnumerable<Link> links)
    {
        writer.WritePropertyName("links");
        writer.WriteStartArray();
        foreach (var link in links)
        {
            writer.WriteStartObject();
            writer.WriteString("href", link.Href);
            writer.WriteString("rel", link.Rel);
            if (!String.IsNullOrEmpty(link.Type))
            {
                writer.WriteString("type", link.Type);
            }
            if (!String.IsNullOrEmpty(link.Title))
            {
                writer.WriteString("title", link.Title);
            }
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }
}
