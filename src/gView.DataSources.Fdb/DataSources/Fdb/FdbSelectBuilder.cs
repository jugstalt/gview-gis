using System;
using System.Text;

namespace gView.DataSources.Fdb
{
    /// <summary>
    /// Fluent builder for the <c>SELECT</c> statements the FDB feature cursors assemble. Backed by a
    /// single <see cref="StringBuilder"/> (no intermediate strings, no boxing of the numeric parts)
    /// and only appends a clause when it actually has content.
    /// <para>
    /// Everything except paging is dialect-independent and lives here. <c>LIMIT</c> / <c>OFFSET</c>
    /// differ per database, so <see cref="Page"/> is implemented by
    /// <see cref="SqliteSelectBuilder"/>, <see cref="PostgreSqlSelectBuilder"/> and
    /// <see cref="SqlServerSelectBuilder"/>.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// string sql = new SqliteSelectBuilder("SELECT * FROM FC_x")
    ///     .WhereAnd(nidClause, userWhere)
    ///     .OrderBy(orderByExpr)
    ///     .Page(limit, beginRecord)
    ///     .Build();
    /// </code>
    /// </example>
    public abstract class FdbSelectBuilder
    {
        protected readonly StringBuilder Sql;

        /// <summary>Whether an <c>ORDER BY</c> clause has been appended (SQL Server paging needs one).</summary>
        protected bool HasOrderBy;

        /// <param name="selectFrom">The <c>SELECT ... FROM ...</c> head to append clauses to.</param>
        protected FdbSelectBuilder(string selectFrom)
        {
            Sql = new StringBuilder(selectFrom ?? String.Empty, (selectFrom?.Length ?? 0) + 96);
        }

        /// <summary>Appends <c> WHERE {clause}</c> when <paramref name="clause"/> is non-empty.</summary>
        public FdbSelectBuilder Where(string clause)
        {
            if (!String.IsNullOrEmpty(clause))
            {
                Sql.Append(" WHERE ").Append(clause);
            }
            return this;
        }

        /// <summary>
        /// Appends <c> WHERE {first} AND ({second})</c>, tolerating either side being empty: with
        /// only one non-empty side that side is used verbatim, with none nothing is appended.
        /// <paramref name="first"/> (typically the NID predicate) is expected to be already
        /// self-contained / parenthesized.
        /// </summary>
        public FdbSelectBuilder WhereAnd(string first, string second)
        {
            string combined =
                String.IsNullOrEmpty(first) ? second
                : String.IsNullOrEmpty(second) ? first
                : $"{first} AND ({second})";

            return Where(combined);
        }

        /// <summary>Appends <c> ORDER BY {clause}</c> when <paramref name="clause"/> is non-whitespace.</summary>
        public FdbSelectBuilder OrderBy(string clause)
        {
            if (!String.IsNullOrWhiteSpace(clause))
            {
                Sql.Append(" ORDER BY ").Append(clause);
                HasOrderBy = true;
            }
            return this;
        }

        /// <summary>Appends a raw fragment verbatim (e.g. <c> WITH (NOLOCK)</c>) when non-empty.</summary>
        public FdbSelectBuilder AppendRaw(string sqlFragment)
        {
            if (!String.IsNullOrEmpty(sqlFragment))
            {
                Sql.Append(sqlFragment);
            }
            return this;
        }

        /// <summary>Appends a raw fragment verbatim only when <paramref name="condition"/> holds.</summary>
        public FdbSelectBuilder AppendRawIf(bool condition, string sqlFragment)
            => condition ? AppendRaw(sqlFragment) : this;

        /// <summary>
        /// Appends the dialect's paging clause so the statement returns the rows
        /// <c>[beginRecord-1 .. beginRecord-1+limit)</c>. <paramref name="limit"/> &lt;= 0 means
        /// "no upper bound"; <paramref name="beginRecord"/> &lt;= 1 means "start at the first row".
        /// Nothing is appended when both are at their defaults.
        /// </summary>
        public abstract FdbSelectBuilder Page(int limit, int beginRecord);

        /// <summary>1-based <c>beginRecord</c> -&gt; 0-based row offset.</summary>
        protected static int ToOffset(int beginRecord) => Math.Max(0, beginRecord - 1);

        public string Build() => Sql.ToString();

        public override string ToString() => Sql.ToString();
    }

    /// <summary>
    /// SQLite paging: <c> LIMIT n [OFFSET m]</c>. An <c>OFFSET</c> is only valid together with a
    /// <c>LIMIT</c>, so an offset-only page uses the <c>LIMIT -1</c> ("no limit") sentinel.
    /// </summary>
    public sealed class SqliteSelectBuilder : FdbSelectBuilder
    {
        public SqliteSelectBuilder(string selectFrom) : base(selectFrom) { }

        public override FdbSelectBuilder Page(int limit, int beginRecord)
        {
            int offset = ToOffset(beginRecord);
            if (limit <= 0 && offset == 0)
            {
                return this;
            }

            Sql.Append(" LIMIT ").Append(limit > 0 ? limit : -1);
            if (offset > 0)
            {
                Sql.Append(" OFFSET ").Append(offset);
            }
            return this;
        }
    }

    /// <summary>PostgreSQL paging: <c> LIMIT n</c> and <c> OFFSET m</c> are independent clauses.</summary>
    public sealed class PostgreSqlSelectBuilder : FdbSelectBuilder
    {
        public PostgreSqlSelectBuilder(string selectFrom) : base(selectFrom) { }

        public override FdbSelectBuilder Page(int limit, int beginRecord)
        {
            if (limit > 0)
            {
                Sql.Append(" LIMIT ").Append(limit);
            }

            int offset = ToOffset(beginRecord);
            if (offset > 0)
            {
                Sql.Append(" OFFSET ").Append(offset);
            }
            return this;
        }
    }

    /// <summary>
    /// SQL Server paging: there is no <c>LIMIT</c>; it is
    /// <c> OFFSET m ROWS [FETCH NEXT n ROWS ONLY]</c>, which is only valid after an <c>ORDER BY</c>.
    /// One on <c>FDB_OID</c> (the FDB primary key) is added when the caller supplied none.
    /// </summary>
    public sealed class SqlServerSelectBuilder : FdbSelectBuilder
    {
        public SqlServerSelectBuilder(string selectFrom) : base(selectFrom) { }

        public override FdbSelectBuilder Page(int limit, int beginRecord)
        {
            int offset = ToOffset(beginRecord);
            if (limit <= 0 && offset == 0)
            {
                return this;
            }

            if (!HasOrderBy)
            {
                OrderBy("FDB_OID");
            }

            Sql.Append(" OFFSET ").Append(offset).Append(" ROWS");
            if (limit > 0)
            {
                Sql.Append(" FETCH NEXT ").Append(limit).Append(" ROWS ONLY");
            }
            return this;
        }
    }
}
