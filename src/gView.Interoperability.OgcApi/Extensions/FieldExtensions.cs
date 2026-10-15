using gView.Framework.Core.Data;

namespace gView.Interoperability.OgcApi.Extensions;

internal static class FieldExtensions
{
    public static bool IsIdOrGeometryField(this IField field, IFeatureClass featureClass)
        => field.type is FieldType.ID or FieldType.Shape or FieldType.GEOMETRY or FieldType.GEOGRAPHY
           || field.name == featureClass.IDFieldName
           || field.name == featureClass.ShapeFieldName;

    public static bool IsStringType(this IField field)
        => field.type is FieldType.String or FieldType.NString or FieldType.character;

    public static bool IsIntegerType(this IField field)
        => field.type is FieldType.integer or FieldType.smallinteger or FieldType.biginteger;

    public static bool IsFloatingPointType(this IField field)
        => field.type is FieldType.Float or FieldType.Double;

    /// <summary>
    /// Types that can be filtered with a provider independent SQL literal.
    /// Date/boolean literals differ between the data sources and are not supported (yet).
    /// </summary>
    public static bool IsQueryableType(this IField field)
        => field.IsStringType() || field.IsIntegerType() || field.IsFloatingPointType();

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
