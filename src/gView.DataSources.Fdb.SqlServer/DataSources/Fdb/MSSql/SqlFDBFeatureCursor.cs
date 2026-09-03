using gView.DataSources.Fdb.MSSql.Cursors;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.MSSql
{
    /// <summary>
    /// Entry point kept for <c>SqlFDB.Query</c>: picks the right cursor implementation for the
    /// query shape.
    /// <list type="bullet">
    ///   <item><see cref="SqlPlainFeatureCursor"/> - no spatial filter.</item>
    ///   <item><see cref="SqlMapEnvelopeFeatureCursor"/> - <c>MapEnvelopeIntersects</c>
    ///     (the map-draw hot path).</item>
    ///   <item><see cref="SqlSpatialFeatureCursor"/> - every other spatial relation.</item>
    /// </list>
    /// </summary>
    internal static class SqlFDBFeatureCursor
    {
        public static Task<IFeatureCursor> Create(
            string connString, string sql, string where, string orderBy, int limit, int beginRecord, bool nolock,
            List<long> nids, ISpatialFilter filter, IGeometryDef geomDef,
            ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            if (filter == null)
            {
                return SqlPlainFeatureCursor.Create(
                    connString, sql, where, orderBy, limit, beginRecord, nolock, geomDef, toSRef, datumTransformations);
            }

            if (filter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                return SqlMapEnvelopeFeatureCursor.Create(
                    connString, sql, where, orderBy, nolock, nids, filter, geomDef, toSRef, datumTransformations);
            }

            return SqlSpatialFeatureCursor.Create(
                connString, sql, where, orderBy, limit, beginRecord, nolock, nids, filter, geomDef, toSRef, datumTransformations);
        }
    }
}
