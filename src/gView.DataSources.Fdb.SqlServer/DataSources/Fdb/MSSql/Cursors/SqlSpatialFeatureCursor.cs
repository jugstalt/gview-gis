using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.MSSql.Cursors
{
    /// <summary>
    /// Spatial query with a precise relation. One combined NID <c>WHERE</c> so <c>ORDER BY</c> is
    /// global; an <c>FDB_OID</c> tiebreaker is appended for deterministic paging. When a cursor-side
    /// geometry post-filter is active (<c>sFilter != null</c>) <c>OFFSET</c>/<c>FETCH</c> are
    /// enforced here, after the geometry check.
    /// </summary>
    internal sealed class SqlSpatialFeatureCursor : SqlFeatureCursorBase
    {
        private readonly ISpatialFilter _spatialFilter;
        private readonly int _limit;
        private readonly int _beginRecord;
        private int _returned;
        private int _skipped;

        private SqlSpatialFeatureCursor(
            ISpatialFilter spatialFilter, int limit, int beginRecord,
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef, toSRef, datumTransformations)
        {
            _spatialFilter = spatialFilter;
            _limit = limit;
            _beginRecord = beginRecord;
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string sql, string where, string orderBy, int limit, int beginRecord, bool nolock,
            List<long> nids, ISpatialFilter spatialFilter,
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            var cursor = new SqlSpatialFeatureCursor(spatialFilter, limit, beginRecord, geomDef, toSRef, datumTransformations);

            string nidClause = nids != null ? FdbNidWhereClause.Build(nids, "FDB_NID") : null;
            string orderByExpr = FdbNidWhereClause.OrderByWithOidTiebreaker(orderBy, "FDB_OID");

            var builder = new SqlServerSelectBuilder(sql)
                .WhereAnd(nidClause, where)
                .OrderBy(orderByExpr);

            // Paging can only be pushed to SQL when no row is dropped afterwards by the geometry test.
            if (spatialFilter == null)
            {
                builder.Page(limit, beginRecord);
            }

            builder.AppendRawIf(nolock, " WITH (NOLOCK)");

            await cursor.OpenReaderAsync(connectionString, builder.Build());
            return cursor;
        }

        protected override bool PassesGeometryFilter(IGeometry shape)
            => _spatialFilter == null
               || gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, shape);

        public override async Task<IFeature> NextFeature()
        {
            if (_spatialFilter == null || (_limit <= 0 && _beginRecord <= 1))
            {
                return await NextRawFeatureAsync();
            }

            while (true)
            {
                IFeature feature = await NextRawFeatureAsync();
                if (feature == null)
                {
                    return null;
                }

                if (_beginRecord > 1 && _skipped < _beginRecord - 1)
                {
                    _skipped++;
                    continue;
                }

                if (_limit > 0 && _returned >= _limit)
                {
                    Dispose();
                    return null;
                }

                _returned++;
                return feature;
            }
        }
    }
}
