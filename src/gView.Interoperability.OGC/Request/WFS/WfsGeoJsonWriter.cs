#nullable enable

using gView.Interoperability.Extensions.Features;
using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace gView.Interoperability.OGC.Request.WFS;

/// <summary>
/// Writes GetFeature responses as GeoJSON (outputFormat=application/json): a FeatureCollection with
/// numberMatched, numberReturned, timeStamp and next/previous links, feature ids c{layerId}.{objectId}.
/// Coordinates are always x/y (east/north, lon/lat), a crs member names the CRS if it is not WGS84.
/// </summary>
public static class WfsGeoJsonWriter
{
    public const string ContentType = "application/json";

    public static byte[] Write(WfsQueryResult result, bool hits, CrsReference? crs, DateTime timeStamp, string? next, string? previous)
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms, GeoJsonWriter.WriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "FeatureCollection");
            if (result.NumberMatched.HasValue)
            {
                writer.WriteNumber("numberMatched", result.NumberMatched.Value);
            }
            writer.WriteNumber("numberReturned", hits ? 0 : result.NumberReturned);
            writer.WriteString("timeStamp", timeStamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
            WriteCrs(writer, crs);

            writer.WritePropertyName("features");
            writer.WriteStartArray();
            if (!hits)
            {
                foreach (var set in result.FeatureSets)
                {
                    foreach (var feature in set.Features)
                    {
                        GeoJsonWriter.WriteFeature(writer, WithResourceId(set.Collection, feature), swapAxes: false);
                    }
                }
            }
            writer.WriteEndArray();

            if (next is not null || previous is not null)
            {
                writer.WritePropertyName("links");
                writer.WriteStartArray();
                WriteLink(writer, next, "next");
                WriteLink(writer, previous, "previous");
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
        return ms.ToArray();
    }

    /// <summary>
    /// A single feature (GetFeatureById)
    /// </summary>
    public static byte[] WriteFeature(OgcCollectionLayer collection, OgcFeature feature, CrsReference? crs)
    {
        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms, GeoJsonWriter.WriterOptions))
        {
            GeoJsonWriter.WriteFeature(writer, WithResourceId(collection, feature), swapAxes: false, w => WriteCrs(w, crs));
        }
        return ms.ToArray();
    }

    /// <summary>
    /// The feature with the WFS resource id (c{layerId}.{objectId}) as id
    /// </summary>
    private static OgcFeature WithResourceId(OgcCollectionLayer collection, OgcFeature feature)
    {
        var result = new OgcFeature()
        {
            Id = $"{collection.Id}.{WfsFeatureWriter.FormatValue(feature.Id) ?? "0"}",
            Geometry = feature.Geometry
        };
        result.Properties.AddRange(feature.Properties);
        return result;
    }

    private static void WriteCrs(Utf8JsonWriter writer, CrsReference? crs)
    {
        if (crs is null || crs.EpsgCode == 4326)
        {
            return;  // RFC 7946: WGS84 lon/lat
        }

        writer.WritePropertyName("crs");
        writer.WriteStartObject();
        writer.WriteString("type", "name");
        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        writer.WriteString("name", crs.Name);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static void WriteLink(Utf8JsonWriter writer, string? href, string rel)
    {
        if (href is null)
        {
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("href", href);
        writer.WriteString("rel", rel);
        writer.WriteString("type", ContentType);
        writer.WriteEndObject();
    }
}
