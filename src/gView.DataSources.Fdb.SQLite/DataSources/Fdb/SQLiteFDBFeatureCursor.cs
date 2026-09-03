using gView.DataSources.Fdb.SQLite.Cursors;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite
{
    /// <summary>
    /// Entry point kept for <see cref="SQLiteFDB.Query"/>: picks the right cursor implementation
    /// for the query shape.
    /// <list type="bullet">
    ///   <item><see cref="SQLitePlainFeatureCursor"/> - no spatial filter.</item>
    ///   <item><see cref="SQLiteMapEnvelopeFeatureCursor"/> - <c>MapEnvelopeIntersects</c>
    ///     (the map-draw hot path).</item>
    ///   <item><see cref="SQLiteSpatialFeatureCursor"/> - every other spatial relation.</item>
    /// </list>
    /// </summary>
    internal static class SQLiteFDBFeatureCursor
    {
        public static Task<IFeatureCursor> Create(
            string connString, string sql, string where, string orderby, int limit, int beginRecord,
            List<long> nids, ISpatialFilter filter, IGeometryDef geomDef,
            ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            if (filter == null)
            {
                return SQLitePlainFeatureCursor.Create(
                    connString, sql, where, orderby, limit, beginRecord, geomDef, toSRef, datumTransformations);
            }

            if (filter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                return SQLiteMapEnvelopeFeatureCursor.Create(
                    connString, sql, where, orderby, nids, filter, geomDef, toSRef, datumTransformations);
            }

            return SQLiteSpatialFeatureCursor.Create(
                connString, sql, where, orderby, limit, beginRecord, nids, filter, geomDef, toSRef, datumTransformations);
        }
    }
}
