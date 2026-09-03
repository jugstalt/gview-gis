using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite.Cursors
{
    /// <summary>
    /// Plain attribute query: no spatial filter, no NID set. <c>ORDER BY</c> / <c>LIMIT</c> /
    /// <c>OFFSET</c> are evaluated by SQLite; the cursor just streams the rows.
    /// </summary>
    internal sealed class SQLitePlainFeatureCursor : SQLiteFeatureCursorBase
    {
        private SQLitePlainFeatureCursor(IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef, toSRef, datumTransformations)
        {
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string sql, string where, string orderBy, int limit, int beginRecord,
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            var cursor = new SQLitePlainFeatureCursor(geomDef, toSRef, datumTransformations);

            string commandText = new SqliteSelectBuilder(sql)
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
