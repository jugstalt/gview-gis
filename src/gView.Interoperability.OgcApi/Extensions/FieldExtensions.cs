using gView.Interoperability.Extensions.Extensions;
using gView.Framework.Core.Data;

namespace gView.Interoperability.OgcApi.Extensions;

internal static class FieldExtensions
{
    /// <summary>
    /// JSON schema type and format of a field, used for queryables and the OpenAPI schemas
    /// </summary>
    public static (string type, string? format) ToJsonSchemaType(this IField field)
        => field.type switch
        {
            _ when field.IsIntegerType() => ("integer", null),
            _ when field.IsFloatingPointType() => ("number", null),
            FieldType.boolean => ("boolean", null),
            FieldType.Date => ("string", "date-time"),
            FieldType.guid or FieldType.replicationID => ("string", "uuid"),
            FieldType.binary => ("string", "byte"),
            _ => ("string", null)
        };

    public static string Title(this IField field)
        => String.IsNullOrWhiteSpace(field.aliasname) ? field.name : field.aliasname;
}
