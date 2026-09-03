using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.MSSql.Cursors
{
    /// <summary>
    /// Plain attribute query: no spatial filter, no NID set. <c>ORDER BY</c> and
    /// <c>OFFSET ... FETCH NEXT</c> are evaluated by SQL Server; the cursor just streams the rows.
    /// </summary>
    internal sealed class SqlPlainFeatureCursor : SqlFeatureCursorBase
    {
        private SqlPlainFeatureCursor(IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef, toSRef, datumTransformations)
        {
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string sql, string where, string orderBy, int limit, int beginRecord, bool nolock,
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            var cursor = new SqlPlainFeatureCursor(geomDef, toSRef, datumTransformations);

            // Page() adds an ORDER BY FDB_OID itself when paging is requested without one.
            string commandText = new SqlServerSelectBuilder(sql)
                .Where(where)
                .OrderBy(orderBy)
                .Page(limit, beginRecord)
                .AppendRawIf(nolock, " WITH (NOLOCK)")
                .Build();

            await cursor.OpenReaderAsync(connectionString, commandText);
            return cursor;
        }

        public override Task<IFeature> NextFeature() => NextRawFeatureAsync();
    }
}
