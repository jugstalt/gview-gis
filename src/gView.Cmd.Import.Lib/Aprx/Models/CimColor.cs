using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Cmd.Import.Aprx.Models;

/// <summary>
/// Base class for CIM color types.
/// Deserialized via <see cref="CimColorConverter"/> which handles both the
/// named-property format and the <c>values</c> array format used by ArcGIS Pro.
/// </summary>
[JsonConverter(typeof(CimColorConverter))]
internal class CimColor
{
    /// <summary>Alpha value 0–100 (ESRI uses 0–100, not 0–255).</summary>
    public double Alpha { get; set; } = 100;

    /// <summary>Converts the ESRI alpha (0–100) to a byte value (0–255).</summary>
    public byte AlphaByte => (byte)(Alpha / 100.0 * 255);
}

internal class CimRgbColor : CimColor
{
    public double R { get; set; }
    public double G { get; set; }
    public double B { get; set; }
}

internal class CimCmykColor : CimColor
{
    public double C { get; set; }
    public double M { get; set; }
    public double Y { get; set; }
    public double K { get; set; }
}

internal class CimGrayColor : CimColor
{
    public double Level { get; set; }
}

internal class CimHsvColor : CimColor
{
    public double H { get; set; }
    public double S { get; set; }
    public double V { get; set; }
}

/// <summary>
/// Deserializes any CIM color object, handling both named properties and the
/// <c>values</c> array format: [R, G, B, Alpha(0-100)] used by ArcGIS Pro.
/// </summary>
internal sealed class CimColorConverter : JsonConverter<CimColor>
{
    public override CimColor? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Buffer the entire object so we can read "type" first
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var type = root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;

        return type switch
        {
            "CIMRGBColor" => ReadRgb(root),
            "CIMCMYKColor" => ReadCmyk(root),
            "CIMGrayColor" => ReadGray(root),
            "CIMHSVColor" => ReadHsv(root),
            _ => ReadBase(root)
        };
    }

    private static CimRgbColor ReadRgb(JsonElement root)
    {
        var color = new CimRgbColor();

        // ArcGIS Pro stores RGB as a "values" array: [R, G, B, Alpha]
        if (root.TryGetProperty("values", out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            var vals = arr.EnumerateArray().Select(v => v.GetDouble()).ToArray();
            if (vals.Length >= 1) color.R = vals[0];
            if (vals.Length >= 2) color.G = vals[1];
            if (vals.Length >= 3) color.B = vals[2];
            if (vals.Length >= 4) color.Alpha = vals[3];
        }
        else
        {
            // Fallback: named properties
            if (root.TryGetProperty("r", out var r)) color.R = r.GetDouble();
            if (root.TryGetProperty("g", out var g)) color.G = g.GetDouble();
            if (root.TryGetProperty("b", out var b)) color.B = b.GetDouble();
            ReadAlpha(root, color);
        }

        return color;
    }

    private static CimCmykColor ReadCmyk(JsonElement root)
    {
        var color = new CimCmykColor();
        if (root.TryGetProperty("c", out var c)) color.C = c.GetDouble();
        if (root.TryGetProperty("m", out var m)) color.M = m.GetDouble();
        if (root.TryGetProperty("y", out var y)) color.Y = y.GetDouble();
        if (root.TryGetProperty("k", out var k)) color.K = k.GetDouble();
        ReadAlpha(root, color);
        return color;
    }

    private static CimGrayColor ReadGray(JsonElement root)
    {
        var color = new CimGrayColor();
        if (root.TryGetProperty("level", out var l)) color.Level = l.GetDouble();
        ReadAlpha(root, color);
        return color;
    }

    private static CimHsvColor ReadHsv(JsonElement root)
    {
        var color = new CimHsvColor();
        if (root.TryGetProperty("h", out var h)) color.H = h.GetDouble();
        if (root.TryGetProperty("s", out var s)) color.S = s.GetDouble();
        if (root.TryGetProperty("v", out var v)) color.V = v.GetDouble();
        ReadAlpha(root, color);
        return color;
    }

    private static CimColor ReadBase(JsonElement root)
    {
        var color = new CimColor();
        ReadAlpha(root, color);
        return color;
    }

    private static void ReadAlpha(JsonElement root, CimColor color)
    {
        if (root.TryGetProperty("alpha", out var a))
            color.Alpha = a.GetDouble();
    }

    public override void Write(Utf8JsonWriter writer, CimColor value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        switch (value)
        {
            case CimRgbColor rgb:
                writer.WriteString("type", "CIMRGBColor");
                writer.WriteStartArray("values");
                writer.WriteNumberValue(rgb.R);
                writer.WriteNumberValue(rgb.G);
                writer.WriteNumberValue(rgb.B);
                writer.WriteNumberValue(rgb.Alpha);
                writer.WriteEndArray();
                break;
            case CimCmykColor cmyk:
                writer.WriteString("type", "CIMCMYKColor");
                writer.WriteNumber("c", cmyk.C);
                writer.WriteNumber("m", cmyk.M);
                writer.WriteNumber("y", cmyk.Y);
                writer.WriteNumber("k", cmyk.K);
                writer.WriteNumber("alpha", cmyk.Alpha);
                break;
            case CimGrayColor gray:
                writer.WriteString("type", "CIMGrayColor");
                writer.WriteNumber("level", gray.Level);
                writer.WriteNumber("alpha", gray.Alpha);
                break;
            case CimHsvColor hsv:
                writer.WriteString("type", "CIMHSVColor");
                writer.WriteNumber("h", hsv.H);
                writer.WriteNumber("s", hsv.S);
                writer.WriteNumber("v", hsv.V);
                writer.WriteNumber("alpha", hsv.Alpha);
                break;
            default:
                writer.WriteString("type", "CIMColor");
                writer.WriteNumber("alpha", value.Alpha);
                break;
        }
        writer.WriteEndObject();
    }
}
