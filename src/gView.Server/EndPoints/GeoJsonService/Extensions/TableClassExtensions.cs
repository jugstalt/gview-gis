#nullable enable

using gView.Framework.Core.Data;
using gView.Interoperability.Extensions.Cql2;
using gView.Interoperability.Extensions.Extensions;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class TableClassExtensions
{
    /// <summary>
    /// The SQL where clause of a CQL2 filter for this table: only fields of a filterable type (unknown or
    /// other fields => 400), literals in the SQL dialect of the data source. The result needs no SQL injection
    /// check: the translator writes every literal itself.
    /// </summary>
    static public string Cql2ToWhereClause(this ITableClass tableClass, CqlPredicate predicate)
    {
        var dialect = (tableClass as ISupportsSqlDialect)?.SqlDialect ?? SqlDialect.Unknown;

        IField? Field(string name)
            => tableClass.Fields?.FindField(name) is IField field && field.IsQueryableType(dialect)
                ? field
                : null;

        return new CqlSqlTranslator(Field, dialect).Translate(predicate);
    }
}
