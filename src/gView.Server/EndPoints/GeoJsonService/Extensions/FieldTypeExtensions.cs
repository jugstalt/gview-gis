using gView.Framework.Core.Data;
using gView.GeoJsonService.DTOs;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class FieldTypeExtensions
{
    /// <summary>The JSON type of a field (before, only the names that match one of PropertyType were mapped)</summary>
    static public PropertyType ToPropertyType(this FieldType type)
        => type switch
        {
            FieldType.ID or FieldType.integer or FieldType.smallinteger or FieldType.biginteger => PropertyType.Integer,
            FieldType.Float or FieldType.Double => PropertyType.Float,
            FieldType.boolean => PropertyType.Boolean,
            FieldType.Date => PropertyType.Date,
            _ => PropertyType.String
        };
}
