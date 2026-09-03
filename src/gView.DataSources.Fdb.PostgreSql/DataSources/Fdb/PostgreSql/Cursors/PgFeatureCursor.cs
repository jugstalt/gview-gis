using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.PostgreSql.Cursors
{
    /// <summary>
    /// Entry point kept for <c>pgFDB.Query</c>: picks the right cursor implementation for the
    /// query shape.
    /// <list type="bullet">
    ///   <item><see cref="PgPlainFeatureCursor"/> - no spatial filter.</item>
    ///   <item><see cref="PgMapEnvelopeFeatureCursor"/> - <c>MapEnvelopeIntersects</c>
    ///     (the map-draw hot path).</item>
    ///   <item><see cref="PgSpatialFeatureCursor"/> - every other spatial relation.</item>
    /// </list>
    /// </summary>
    internal static class PgFeatureCursor
    {
        public static Task<IFeatureCursor> Create(
            string connString, string sql, string where, string orderBy, int limit, int beginRecord,
            bool nolock, List<long> nids, ISpatialFilter filter, IGeometryDef geomDef,
            ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            if (filter == null)
            {
                return PgPlainFeatureCursor.Create(
                    connString, sql, where, orderBy, limit, beginRecord, geomDef, toSRef, datumTransformations);
            }

            if (filter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                return PgMapEnvelopeFeatureCursor.Create(
                    connString, sql, where, orderBy, nids, filter, geomDef, toSRef, datumTransformations);
            }

            return PgSpatialFeatureCursor.Create(
                connString, sql, where, orderBy, limit, beginRecord, nids, filter, geomDef, toSRef, datumTransformations);
        }
    }
}
