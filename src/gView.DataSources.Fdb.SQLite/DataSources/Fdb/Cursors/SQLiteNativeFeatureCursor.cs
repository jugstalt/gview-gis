using gView.DataSources.GeoPackage;
using gView.DataSources.SpatiaLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.OGC;
using gView.Framework.OGC.WKT;
using System;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.SQLite.Cursors
{
    /// <summary>
    /// Reads an FDB feature class whose <c>FDB_SHAPE</c> column is a SpatiaLite / GeoPackage
    /// geometry blob. The coarse spatial restriction is delegated to the SQLite R-Tree
    /// (a plain <c>&lt;id&gt; IN (SELECT ... FROM rtree/idx ...)</c> sub-select).
    /// <list type="bullet">
    ///   <item><b>SpatiaLite</b>: geometry read as WKB via <c>ST_AsBinary(...)</c>, precise relation
    ///     via <c>ST_Intersects</c> in SQL (needs mod_spatialite).</item>
    ///   <item><b>GeoPackage</b>: the raw GPB blob is read and decoded in managed code
    ///     (<see cref="GpkgGeometry"/>); the precise relation is checked per row in managed code -
    ///     no mod_spatialite.</item>
    /// </list>
    /// <c>WHERE</c> / <c>ORDER BY</c> / paging use <see cref="SqliteSelectBuilder"/>, like the
    /// classic SQLite FDB cursors.
    /// </summary>
    internal sealed class SQLiteNativeFeatureCursor : SQLiteFeatureCursorBase
    {
        private const string ShapeAlias = "temp_geometry";

        private readonly SpatiaLiteFlavor _flavor;
        private readonly ISpatialFilter _preciseFilter;   // GeoPackage: exact relation checked per row

        // When _preciseFilter is set, LIMIT/OFFSET cannot be pushed to SQL - the R-Tree only
        // narrows to bbox candidates, so a SQL-level LIMIT would truncate the candidate set
        // before the geometry post-filter drops the false positives, silently under-returning
        // (and never signalling "more results available"). Enforced here instead, same as
        // SQLiteSpatialFeatureCursor does for the classic BinaryTree index.
        private readonly int _limit;
        private readonly int _beginRecord;
        private int _returned;
        private int _skipped;

        private SQLiteNativeFeatureCursor(
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations,
            SpatiaLiteFlavor flavor, ISpatialFilter preciseFilter, int limit, int beginRecord)
            : base(geomDef, toSRef, datumTransformations)
        {
            _flavor = flavor;
            _preciseFilter = preciseFilter;
            _limit = limit;
            _beginRecord = beginRecord;
        }

        private bool GeoPackage => _flavor == SpatiaLiteFlavor.GeoPackage;

        protected override string ShapeColumn => ShapeAlias;

        protected override IGeometry DecodeShape(byte[] bytes)
            => OGC.WKBToGeometry(GeoPackage ? GpkgGeometry.ToWkb(bytes) : bytes);

        protected override bool PassesGeometryFilter(IGeometry shape)
            => _preciseFilter?.Geometry == null
               || gView.Framework.Geometry.SpatialRelation.Check(_preciseFilter, shape);

        protected override void PrepareConnection(SQLiteConnection connection)
        {
            if (GeoPackage)
            {
                return; // plain SQLite - gView handles the GPB blob + R-Tree
            }

            SpatiaLiteNative.LoadInto(connection);
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string tableName, string rtreeTableName, IFeatureClass fc, IQueryFilter filter,
            SpatiaLiteFlavor flavor, int srid, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            filter ??= new QueryFilter();

            // GeoPackage cannot run ST_Intersects in SQL -> keep the filter for the per-row check
            ISpatialFilter preciseFilter = NeedsPreciseRowFilter(flavor, filter) ? (ISpatialFilter)filter : null;

            var cursor = new SQLiteNativeFeatureCursor(
                fc, toSRef, datumTransformations, flavor, preciseFilter, filter.Limit, filter.BeginRecord);

            if (String.IsNullOrEmpty(filter.SubFields) || filter.SubFields == "*")
            {
                filter = (IQueryFilter)filter.Clone();
                filter.SubFields = "";
                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    filter.AddField(field.name);
                }
            }
            filter.AddField("FDB_SHAPE");

            string selectFrom = new StringBuilder("SELECT ")
                .Append(BuildFieldList(filter.SubFields, flavor))
                .Append(" FROM ")
                .Append(tableName)
                .ToString();

            string spatialWhere = BuildSpatialWhere(filter as ISpatialFilter, flavor, rtreeTableName, srid);
            string userWhere = (filter is IRowIDFilter ridf) ? ridf.RowIDWhereClause : filter.WhereClause;

            var builder = new SqliteSelectBuilder(selectFrom)
                .WhereAnd(spatialWhere, userWhere)
                .OrderBy(filter.OrderBy);

            // Paging can only be pushed to SQL when no row is dropped afterwards by the precise
            // geometry test (see the field comments above) - otherwise it is enforced in NextFeature.
            if (preciseFilter == null)
            {
                builder.Page(filter.Limit, filter.BeginRecord);
            }

            string commandText = builder.Build();

            await cursor.OpenReaderAsync(connectionString, commandText);
            return cursor;
        }

        public override async Task<IFeature> NextFeature()
        {
            // No precise post-filter -> SQL already applied ORDER BY / LIMIT / OFFSET.
            if (_preciseFilter == null || (_limit <= 0 && _beginRecord <= 1))
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

        private static string BuildFieldList(string subFields, SpatiaLiteFlavor flavor)
        {
            var fieldNames = new StringBuilder();

            foreach (string raw in subFields.Split(' '))
            {
                string bare = raw.Trim().Trim('[', ']', '"');
                if (bare.Length == 0)
                {
                    continue;
                }

                if (fieldNames.Length > 0)
                {
                    fieldNames.Append(',');
                }

                if (String.Equals(bare, "FDB_SHAPE", StringComparison.OrdinalIgnoreCase))
                {
                    if (flavor == SpatiaLiteFlavor.GeoPackage)
                    {
                        fieldNames.Append("\"FDB_SHAPE\" as ").Append(ShapeAlias);   // raw GPB blob
                    }
                    else
                    {
                        fieldNames.Append("ST_AsBinary(\"FDB_SHAPE\") as ").Append(ShapeAlias);
                    }
                }
                else
                {
                    fieldNames.Append('[').Append(bare).Append(']');
                }
            }

            return fieldNames.ToString();
        }

        /// <summary>
        /// True when <paramref name="filter"/>'s spatial relation needs the per-row managed check
        /// (<see cref="PassesGeometryFilter"/>) because it cannot be evaluated exactly in SQL:
        /// GeoPackage has no in-database <c>ST_Intersects</c>, only the R-Tree bbox pre-filter, so
        /// any relation other than a plain bbox test (<c>MapEnvelopeIntersects</c>) - or "no spatial
        /// filter at all" - needs it. SpatiaLite (mod_spatialite) evaluates every relation in SQL.
        /// </summary>
        internal static bool NeedsPreciseRowFilter(SpatiaLiteFlavor flavor, IQueryFilter filter)
            => flavor == SpatiaLiteFlavor.GeoPackage
               && filter is ISpatialFilter sf && sf.Geometry != null
               && sf.SpatialRelation != spatialRelation.SpatialRelationMapEnvelopeIntersects;

        /// <summary>
        /// <c>SELECT count(&lt;idColumn&gt;) FROM &lt;tableName&gt; WHERE ...</c> for a filter whose
        /// spatial relation (if any) <see cref="NeedsPreciseRowFilter"/> says can be evaluated fully
        /// in SQL - so the caller can COUNT directly instead of paying for a full cursor scan that
        /// decodes and discards every geometry.
        /// </summary>
        internal static string BuildCountCommandText(
            string tableName, string rtreeTableName, IQueryFilter filter, SpatiaLiteFlavor flavor, int srid, string idColumn)
        {
            string spatialWhere = BuildSpatialWhere(filter as ISpatialFilter, flavor, rtreeTableName, srid);
            string userWhere = (filter is IRowIDFilter ridf) ? ridf.RowIDWhereClause : filter?.WhereClause;

            string selectFrom = $"SELECT count([{idColumn}]) FROM {tableName}";

            return new SqliteSelectBuilder(selectFrom)
                .WhereAnd(spatialWhere, userWhere)
                .Build();
        }

        private static string BuildSpatialWhere(ISpatialFilter sFilter, SpatiaLiteFlavor flavor, string rtreeTableName, int srid)
        {
            if (sFilter?.Geometry == null)
            {
                return String.Empty;
            }

            IEnvelope env = sFilter.Geometry.Envelope;

            var sb = new StringBuilder(SpatiaLiteSchema.SpatialIndexPredicate(
                flavor, rtreeTableName, "FDB_SHAPE", "FDB_OID",
                env.MinX, env.MinY, env.MaxX, env.MaxY, srid));

            if (sFilter.SpatialRelation != spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                // empty for GeoPackage (no in-db ST_Intersects) - the cursor does the exact test
                string precise = SpatiaLiteSchema.IntersectsPredicate(flavor, "\"FDB_SHAPE\"", WKT.ToWKT(sFilter.Geometry), srid);
                if (!String.IsNullOrEmpty(precise))
                {
                    sb.Append(" AND ").Append(precise);
                }
            }

            return sb.ToString();
        }
    }
}
