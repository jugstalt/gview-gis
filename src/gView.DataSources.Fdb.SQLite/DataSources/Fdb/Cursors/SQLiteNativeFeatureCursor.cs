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

        private SQLiteNativeFeatureCursor(
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations,
            SpatiaLiteFlavor flavor, ISpatialFilter preciseFilter)
            : base(geomDef, toSRef, datumTransformations)
        {
            _flavor = flavor;
            _preciseFilter = preciseFilter;
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
            ISpatialFilter preciseFilter = null;
            if (flavor == SpatiaLiteFlavor.GeoPackage
                && filter is ISpatialFilter sf && sf.Geometry != null
                && sf.SpatialRelation != spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                preciseFilter = sf;
            }

            var cursor = new SQLiteNativeFeatureCursor(fc, toSRef, datumTransformations, flavor, preciseFilter);

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

            string commandText = new SqliteSelectBuilder(selectFrom)
                .WhereAnd(spatialWhere, userWhere)
                .OrderBy(filter.OrderBy)
                .Page(filter.Limit, filter.BeginRecord)
                .Build();

            await cursor.OpenReaderAsync(connectionString, commandText);
            return cursor;
        }

        public override Task<IFeature> NextFeature() => NextRawFeatureAsync();

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
