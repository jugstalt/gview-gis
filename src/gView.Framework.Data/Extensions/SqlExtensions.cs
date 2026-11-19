using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Data.Sql;
using System;

namespace gView.Framework.Data.Extensions
{
    static public class SqlExtensions
    {
        #region Sql Injection

        /// <summary>
        /// Checks a where clause from a public request against the SQL token allowlist
        /// (<see cref="SqlWhereClauseValidator"/>). If <paramref name="tableClass"/> is set,
        /// identifiers must be fields of the table. Returns the unchanged where clause.
        /// </summary>
        static public string CheckWhereClauseForSqlInjection(this string where, ITableClass tableClass = null)
        {
            SqlWhereClauseValidator.Validate(where, tableClass);

            return where;
        }

        #endregion
    }

    /// <summary>
    /// SQL from a public request was rejected. Derived from <see cref="MapServerException"/>
    /// so the services return the message (not the SQL) to the client with status 400.
    /// </summary>
    public class SqlDangerousStatementExceptions : MapServerException
    {
        public SqlDangerousStatementExceptions(string message)
            : base(message, 400)
        {

        }
    }
}
