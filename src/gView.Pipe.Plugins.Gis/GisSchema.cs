using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Pipe.Abstraction;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

/// <summary>
/// The schema of a feature class as item (gview-pipe ADR 0044): neutral type names instead of gView's enums,
/// <c>{ name, geometryType, srs, idField, shapeField, fields: [ { name, type, size, precision, nullable } ] }</c>.
/// </summary>
static internal class GisSchema
{
    static private readonly Dictionary<FieldType, string> TypeNames = new()
    {
        [FieldType.boolean] = "boolean",
        [FieldType.biginteger] = "bigInteger",
        [FieldType.integer] = "integer",
        [FieldType.smallinteger] = "smallInteger",
        [FieldType.Float] = "float",
        [FieldType.Double] = "double",
        [FieldType.String] = "text",
        [FieldType.NString] = "text",
        [FieldType.character] = "text",
        [FieldType.Date] = "date",
        [FieldType.guid] = "guid",
        [FieldType.replicationID] = "guid",
        [FieldType.binary] = "binary",
        [FieldType.unknown] = "text"
    };

    static private readonly Dictionary<string, FieldType> FieldTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["boolean"] = FieldType.boolean,
        ["bigInteger"] = FieldType.biginteger,
        ["integer"] = FieldType.integer,
        ["smallInteger"] = FieldType.smallinteger,
        ["float"] = FieldType.Float,
        ["double"] = FieldType.Double,
        ["text"] = FieldType.String,
        ["date"] = FieldType.Date,
        ["guid"] = FieldType.guid,
        ["binary"] = FieldType.binary
    };

    static private readonly Dictionary<GeometryType, string> GeometryNames = new()
    {
        [GeometryType.Point] = "point",
        [GeometryType.Multipoint] = "multipoint",
        [GeometryType.Polyline] = "polyline",
        [GeometryType.Polygon] = "polygon",
        [GeometryType.Aggregate] = "aggregate",
        [GeometryType.Envelope] = "envelope",
        [GeometryType.Network] = "network",
        [GeometryType.Unknown] = "unknown"
    };

    /// <summary>Text for a text field without size: gView's databases need one.</summary>
    public const int DefaultTextSize = 255;

    static public string GeometryName(GeometryType type) => GeometryNames.GetValueOrDefault(type, "unknown");

    static public JsonObject ToItem(IFeatureClass featureClass)
    {
        var fields = new JsonArray();
        foreach (var field in featureClass.Fields.ToEnumerable())
        {
            if (field.type is FieldType.ID or FieldType.Shape or FieldType.GEOMETRY or FieldType.GEOGRAPHY
                || field.name.Equals(featureClass.IDFieldName, StringComparison.OrdinalIgnoreCase)
                || field.name.Equals(featureClass.ShapeFieldName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var json = new JsonObject { ["name"] = field.name, ["type"] = TypeNames.GetValueOrDefault(field.type, "text") };
            if (field.size > 0)
            {
                json["size"] = field.size;
            }
            if (field.precision > 0)
            {
                json["precision"] = field.precision;
            }
            json["nullable"] = field.IsNullable;
            fields.Add(json);
        }

        return new JsonObject
        {
            ["name"] = featureClass.Name,
            ["geometryType"] = GeometryName(featureClass.GeometryType),
            ["srs"] = GisSpatialReferences.Epsg(featureClass),
            ["idField"] = featureClass.IDFieldName,
            ["shapeField"] = featureClass.ShapeFieldName,
            ["fields"] = fields
        };
    }

    /// <summary>Geometry type and fields of a schema item for <c>IFeatureDatabase.CreateFeatureClass</c>.</summary>
    static public (GeometryType GeometryType, FieldCollection Fields) FromItem(JsonObject schema)
    {
        var geometryName = Text(schema["geometryType"]) ?? "unknown";
        var geometryType = GeometryNames.FirstOrDefault(g => g.Value.Equals(geometryName, StringComparison.OrdinalIgnoreCase));
        if (geometryType.Value is null || geometryType.Key is GeometryType.Unknown or GeometryType.Envelope or GeometryType.Network)
        {
            throw new PipeException($"The schema needs a geometryType of point, multipoint, polyline, polygon or aggregate, not '{geometryName}'");
        }

        var idField = Text(schema["idField"]);
        var shapeField = Text(schema["shapeField"]);
        var fields = new FieldCollection();
        foreach (var node in schema["fields"] as JsonArray ?? [])
        {
            if (node is not JsonObject field || Text(field["name"]) is not { Length: > 0 } name)
            {
                throw new PipeException("Every field of the schema needs a name");
            }
            // the target makes its own id and geometry columns
            if (name.Equals(idField, StringComparison.OrdinalIgnoreCase) || name.Equals(shapeField, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var typeName = Text(field["type"]) ?? "text";
            if (!FieldTypes.TryGetValue(typeName, out var type))
            {
                throw new PipeException($"Unknown type '{typeName}' of field '{name}'; use {string.Join(", ", FieldTypes.Keys)}");
            }

            var size = Number(field["size"]);
            if (type == FieldType.String && size <= 0)
            {
                size = DefaultTextSize;
            }
            fields.Add(new Field(name, type, Math.Max(size, 0), Math.Max(Number(field["precision"]), 0))
            {
                IsNullable = field["nullable"] is not JsonValue nullable || nullable.GetValueKind() != JsonValueKind.False
            });
        }

        return (geometryType.Key, fields);
    }

    static private string? Text(JsonNode? node)
        => node is JsonValue value && value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>().Trim() : null;

    static private int Number(JsonNode? node)
        => node is JsonValue value && value.GetValueKind() == JsonValueKind.Number && int.TryParse(value.ToJsonString(), out var number) ? number : 0;
}
