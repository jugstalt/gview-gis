using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.PostgreSql.Cursors
{
    /// <summary>
    /// Plain attribute query: no spatial filter, no NID set. <c>ORDER BY</c> / <c>limit</c> /
    /// <c>offset</c> are evaluated by PostgreSQL; the cursor just streams the rows.
    /// </summary>
    internal sealed class PgPlainFeatureCursor : PgFeatureCursorBase
    {
        private PgPlainFeatureCursor(IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef, toSRef, datumTransformations)
        {
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string sql, string where, string orderBy, int limit, int beginRecord,
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            var cursor = new PgPlainFeatureCursor(geomDef, toSRef, datumTransformations);

            string commandText = new PostgreSqlSelectBuilder(sql)
                .Where(where)
                .OrderBy(orderBy)
                .Page(limit, beginRecord)
                .Build();

            await cursor.OpenReaderAsync(connectionString, commandText);
            return cursor;
        }

        public override Task<IFeature> NextFeature() => NextRawFeatureAsync();
    }
}
