#nullable enable

using gView.Framework.Common.Extensions;
using gView.Framework.Core.Data;
using gView.Interoperability.Extensions.Cql2;
using gView.Interoperability.Extensions.Extensions;
using System;
using System.Linq;

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
            => tableClass.FieldByName(name) is IField field && field.IsQueryableType(dialect)
                ? field
                : null;

        return new CqlSqlTranslator(Field, dialect).Translate(predicate);
    }

    /// <summary>
    /// A field by its name, exact or case-insensitive (FieldCollection.FindField compares the alias: a field with an
    /// alias was not found by its name)
    /// </summary>
    static public IField? FieldByName(this ITableClass tableClass, string? name)
    {
        if (String.IsNullOrWhiteSpace(name) || tableClass.Fields is null)
        {
            return null;
        }
        name = name.Trim();
        var fields = tableClass.Fields.ToEnumerable().ToList();
        return fields.FirstOrDefault(f => f.name == name)
               ?? fields.FirstOrDefault(f => f.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
