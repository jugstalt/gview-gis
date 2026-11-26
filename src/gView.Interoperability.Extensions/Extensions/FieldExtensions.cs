using gView.Framework.Core.Data;

namespace gView.Interoperability.Extensions.Extensions;

/// <summary>
/// Field types as the filters (CQL2, queryables) of OGC API and GeoJson Services see them
/// </summary>
public static class FieldExtensions
{
    public static bool IsStringType(this IField field)
        => field.type is FieldType.String or FieldType.NString or FieldType.character;

    public static bool IsIntegerType(this IField field)
        => field.type is FieldType.integer or FieldType.smallinteger or FieldType.biginteger;

    public static bool IsFloatingPointType(this IField field)
        => field.type is FieldType.Float or FieldType.Double;

    /// <summary>
    /// Types that can be filtered. Date/boolean literals differ between the databases,
    /// so these fields are only queryable if the SQL dialect of the data source is known.
    /// </summary>
    public static bool IsQueryableType(this IField field, SqlDialect dialect)
        => field.IsStringType() || field.IsIntegerType() || field.IsFloatingPointType()
           || (dialect != SqlDialect.Unknown && field.type is FieldType.Date or FieldType.boolean);

    /// <summary>
    /// The object id and the geometry are not written as (attribute) properties
    /// </summary>
    public static bool IsIdOrGeometryField(this IField field, IFeatureClass featureClass)
        => field.type is FieldType.ID or FieldType.Shape or FieldType.GEOMETRY or FieldType.GEOGRAPHY
           || field.name == featureClass.IDFieldName
           || field.name == featureClass.ShapeFieldName;
}
