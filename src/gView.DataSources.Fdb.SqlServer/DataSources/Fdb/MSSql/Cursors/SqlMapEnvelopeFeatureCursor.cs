using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.MSSql.Cursors
{
    /// <summary>
    /// <c>SpatialRelationMapEnvelopeIntersects</c> - the query fired on every map draw / label
    /// pass. The whole NID set becomes a single <c>WHERE</c>; no paging, and <c>ORDER BY</c> only
    /// when the layer actually configured one (no forced tiebreaker). The coarse spatial-index
    /// (NID) selection is refined per row via <see cref="PassesGeometryFilter"/>.
    /// </summary>
    internal sealed class SqlMapEnvelopeFeatureCursor : SqlFeatureCursorBase
    {
        private readonly ISpatialFilter _spatialFilter;

        private SqlMapEnvelopeFeatureCursor(
            ISpatialFilter spatialFilter, IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef, toSRef, datumTransformations)
        {
            _spatialFilter = spatialFilter;
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string sql, string where, string orderBy, bool nolock, List<long> nids, ISpatialFilter spatialFilter,
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            var cursor = new SqlMapEnvelopeFeatureCursor(spatialFilter, geomDef, toSRef, datumTransformations);

            string nidClause = nids != null ? FdbNidWhereClause.Build(nids, "FDB_NID") : null;

            string commandText = new SqlServerSelectBuilder(sql)
                .WhereAnd(nidClause, where)
                .OrderBy(orderBy)
                .AppendRawIf(nolock, " WITH (NOLOCK)")
                .Build();

            await cursor.OpenReaderAsync(connectionString, commandText);
            return cursor;
        }

        protected override bool PassesGeometryFilter(IGeometry shape)
            => _spatialFilter?.Geometry == null
               || gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, shape);

        public override Task<IFeature> NextFeature() => NextRawFeatureAsync();
    }
}
