using gView.Framework.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace gView.Framework.Db.Extensions
{
    static public class CommandExtensions
    {
        static public void SetCustomCursorTimeout(this DbCommand command)
        {
            if (command != null && Globals.CustomCursorTimeoutSeconds >= 0)
            {
                command.CommandTimeout = Globals.CustomCursorTimeoutSeconds;
            }
        }

        /// <summary>
        /// Runs an INSERT that returns the new row id as a single scalar
        /// (<paramref name="idSelectSql"/> is appended to the command text, eg
        /// <c>" RETURNING id"</c> or <c>"; SELECT SCOPE_IDENTITY()"</c>) and writes that id
        /// back to <see cref="IOID.OID"/> of <paramref name="feature"/>.
        /// </summary>
        static public async Task ExecuteInsertAndApplyId(this DbCommand command, string idSelectSql, IFeature feature)
        {
            command.CommandText += idSelectSql;

            ApplyOid(await command.ExecuteScalarAsync(), feature);
        }

        /// <summary>
        /// Runs a (possibly multi-row) INSERT that returns the new row ids as a result set
        /// (<paramref name="idSelectSql"/> is appended to the command text, eg
        /// <c>" RETURNING id"</c>) and writes the ids back to <see cref="IOID.OID"/> of
        /// <paramref name="features"/> in order (1:1 with the inserted rows).
        /// </summary>
        static public async Task ExecuteInsertAndApplyIds(this DbCommand command, string idSelectSql, IReadOnlyList<IFeature> features)
        {
            command.CommandText += idSelectSql;

            using var reader = await command.ExecuteReaderAsync();
            for (int i = 0; i < features.Count && await reader.ReadAsync(); i++)
            {
                ApplyOid(reader.GetValue(0), features[i]);
            }
        }

        static private void ApplyOid(object idValue, IFeature feature)
        {
            if (idValue is not null && idValue != DBNull.Value && feature is Row row)
            {
                row.OID = Convert.ToInt32(idValue);
            }
        }
    }
}
