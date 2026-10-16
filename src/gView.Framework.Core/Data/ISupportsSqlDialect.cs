namespace gView.Framework.Core.Data
{
    public enum SqlDialect
    {
        Unknown = 0,
        PostgreSql = 1,
        SqlServer = 2,
        SQLite = 3
    }

    /// <summary>
    /// Table classes that evaluate <see cref="Filters.IQueryFilter.WhereClause"/> in a known SQL dialect.
    /// Clients (e.g. the OGC API CQL2 filter) use it to write date and boolean literals the database understands.
    /// </summary>
    public interface ISupportsSqlDialect
    {
        SqlDialect SqlDialect { get; }
    }
}
