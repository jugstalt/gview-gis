using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite.Cursors
{
    /// <summary>
    /// <c>SpatialRelationMapEnvelopeIntersects</c> - the query fired on every map draw / label
    /// pass. Tuned for throughput:
    /// <list type="bullet">
    ///   <item>the whole NID set becomes a single <c>WHERE</c> (one statement, one plan),</item>
    ///   <item>no paging,</item>
    ///   <item><c>ORDER BY</c> only when the layer actually configured one - no forced tiebreaker.</item>
    /// </list>
    /// The spatial-index (NID) selection is coarse - it returns every feature in a tree node that
    /// overlaps the query rectangle. <see cref="PassesGeometryFilter"/> drops the features whose
    /// own geometry does not actually intersect the query box.
    /// </summary>
    internal sealed class SQLiteMapEnvelopeFeatureCursor : SQLiteFeatureCursorBase
    {
        private readonly ISpatialFilter _spatialFilter;

        private SQLiteMapEnvelopeFeatureCursor(
            ISpatialFilter spatialFilter, IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
            : base(geomDef, toSRef, datumTransformations)
        {
            _spatialFilter = spatialFilter;
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string sql, string where, string orderBy, List<long> nids, ISpatialFilter spatialFilter,
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            var cursor = new SQLiteMapEnvelopeFeatureCursor(spatialFilter, geomDef, toSRef, datumTransformations);

            string nidClause = nids != null ? FdbNidWhereClause.Build(nids, "FDB_NID") : null;

            string commandText = new SqliteSelectBuilder(sql)
                .WhereAnd(nidClause, where)
                .OrderBy(orderBy)
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
