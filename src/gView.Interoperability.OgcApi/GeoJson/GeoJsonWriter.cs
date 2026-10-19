using gView.Framework.Core.Geometry;
using gView.Interoperability.OgcApi.Features;
using gView.Interoperability.OgcApi.Models;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace gView.Interoperability.OgcApi.GeoJson;

/// <summary>
/// Writes RFC 7946 GeoJSON. Unlike the GeoJsonService DTOs, polygons are grouped into
/// outer rings with their holes (Polygon/MultiPolygon) and rings are always closed.
/// </summary>
internal static class GeoJsonWriter
{
    /// <summary>
    /// Don't escape umlauts, apostrophes, ... (the output is JSON, never embedded in HTML)
    /// </summary>
    public static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static void WriteFeature(Utf8JsonWriter writer, OgcFeature feature, bool swapAxes, IEnumerable<Link>? links = null)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "Feature");

        if (feature.Id is not null)
        {
            writer.WritePropertyName("id");
            WriteValue(writer, feature.Id);
        }

        writer.WritePropertyName("geometry");
        WriteGeometry(writer, feature.Geometry, swapAxes);

        writer.WritePropertyName("properties");
        writer.WriteStartObject();
        foreach (var property in feature.Properties)
        {
            writer.WritePropertyName(property.Key);
            WriteValue(writer, property.Value);
        }
        writer.WriteEndObject();

        if (links is not null)
        {
            WriteLinks(writer, links);
        }

        writer.WriteEndObject();
    }

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

    public static void WriteGeometry(Utf8JsonWriter writer, IGeometry? geometry, bool swapAxes)
    {
        switch (geometry)
        {
            case IPoint point:
                writer.WriteStartObject();
                writer.WriteString("type", "Point");
                writer.WritePropertyName("coordinates");
                WritePosition(writer, point, swapAxes);
                writer.WriteEndObject();
                break;

            case IMultiPoint multiPoint when multiPoint.PointCount > 0:
                writer.WriteStartObject();
                writer.WriteString("type", "MultiPoint");
                writer.WritePropertyName("coordinates");
                WritePositions(writer, multiPoint, swapAxes, closeRing: false);
                writer.WriteEndObject();
                break;

            case IPolyline polyline when polyline.PathCount == 1:
                writer.WriteStartObject();
                writer.WriteString("type", "LineString");
                writer.WritePropertyName("coordinates");
                WritePositions(writer, polyline[0], swapAxes, closeRing: false);
                writer.WriteEndObject();
                break;

            case IPolyline polyline when polyline.PathCount > 1:
                writer.WriteStartObject();
                writer.WriteString("type", "MultiLineString");
                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();
                for (int p = 0; p < polyline.PathCount; p++)
                {
                    WritePositions(writer, polyline[p], swapAxes, closeRing: false);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;

            case IPolygon polygon when polygon.RingCount > 0:
                WritePolygon(writer, polygon, swapAxes);
                break;

            case IEnvelope envelope:
                writer.WriteStartObject();
                writer.WriteString("type", "Polygon");
                writer.WritePropertyName("coordinates");
                writer.WriteStartArray();
                writer.WriteStartArray();
                WritePosition(writer, envelope.MinX, envelope.MinY, swapAxes);
                WritePosition(writer, envelope.MaxX, envelope.MinY, swapAxes);
                WritePosition(writer, envelope.MaxX, envelope.MaxY, swapAxes);
                WritePosition(writer, envelope.MinX, envelope.MaxY, swapAxes);
                WritePosition(writer, envelope.MinX, envelope.MinY, swapAxes);
                writer.WriteEndArray();
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;

            case IAggregateGeometry aggregate when aggregate.GeometryCount > 0:
                writer.WriteStartObject();
                writer.WriteString("type", "GeometryCollection");
                writer.WritePropertyName("geometries");
                writer.WriteStartArray();
                foreach (var part in aggregate)
                {
                    WriteGeometry(writer, part, swapAxes);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;

            default:
                writer.WriteNullValue();  // null or empty geometry
                break;
        }
    }

    public static void WriteValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
            case DBNull:
                writer.WriteNullValue();
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case short s16:
                writer.WriteNumberValue(s16);
                break;
            case byte u8:
                writer.WriteNumberValue(u8);
                break;
            case sbyte s8:
                writer.WriteNumberValue(s8);
                break;
            case uint u32:
                writer.WriteNumberValue(u32);
                break;
            case ulong u64:
                writer.WriteNumberValue(u64);
                break;
            case ushort u16:
                writer.WriteNumberValue(u16);
                break;
            case decimal m:
                writer.WriteNumberValue(m);
                break;
            case double d:
                if (double.IsFinite(d)) writer.WriteNumberValue(d); else writer.WriteNullValue();
                break;
            case float f:
                if (float.IsFinite(f)) writer.WriteNumberValue(f); else writer.WriteNullValue();
                break;
            case DateTime dateTime:
                writer.WriteStringValue(dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture));
                break;
            case DateTimeOffset dateTimeOffset:
                writer.WriteStringValue(dateTimeOffset.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", CultureInfo.InvariantCulture));
                break;
            case DateOnly dateOnly:
                writer.WriteStringValue(dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                break;
            case TimeOnly timeOnly:
                writer.WriteStringValue(timeOnly.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture));
                break;
            case TimeSpan timeSpan:
                writer.WriteStringValue(timeSpan.ToString("c", CultureInfo.InvariantCulture));
                break;
            case Guid guid:
                writer.WriteStringValue(guid.ToString());
                break;
            case byte[] bytes:
                writer.WriteBase64StringValue(bytes);
                break;
            default:
                writer.WriteStringValue(Convert.ToString(value, CultureInfo.InvariantCulture));
                break;
        }
    }

    #region Helper

    private static void WritePolygon(Utf8JsonWriter writer, IPolygon polygon, bool swapAxes)
    {
        var outerRings = polygon.OuterRings().ToList();
        if (outerRings.Count == 0)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("type", outerRings.Count == 1 ? "Polygon" : "MultiPolygon");
        writer.WritePropertyName("coordinates");

        if (outerRings.Count > 1)
        {
            writer.WriteStartArray();
        }

        foreach (var outerRing in outerRings)
        {
            writer.WriteStartArray();
            WritePositions(writer, outerRing, swapAxes, closeRing: true);
            foreach (var hole in polygon.InnerRings(outerRing))
            {
                WritePositions(writer, hole, swapAxes, closeRing: true);
            }
            writer.WriteEndArray();
        }

        if (outerRings.Count > 1)
        {
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static void WritePositions(Utf8JsonWriter writer, IPointCollection points, bool swapAxes, bool closeRing)
    {
        writer.WriteStartArray();

        int count = points.PointCount;
        for (int i = 0; i < count; i++)
        {
            WritePosition(writer, points[i], swapAxes);
        }

        if (closeRing && count > 0)
        {
            var first = points[0];
            var last = points[count - 1];
            if (first.X != last.X || first.Y != last.Y)
            {
                WritePosition(writer, first, swapAxes);
            }
        }

        writer.WriteEndArray();
    }

    private static void WritePosition(Utf8JsonWriter writer, IPoint point, bool swapAxes)
        => WritePosition(writer, point.X, point.Y, swapAxes);

    private static void WritePosition(Utf8JsonWriter writer, double x, double y, bool swapAxes)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(swapAxes ? y : x);
        writer.WriteNumberValue(swapAxes ? x : y);
        writer.WriteEndArray();
    }

    #endregion
}
