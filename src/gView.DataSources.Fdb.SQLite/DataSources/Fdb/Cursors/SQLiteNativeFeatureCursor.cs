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
    /// geometry blob. The spatial restriction is delegated to the SQLite R-Tree (a plain
    /// <c>&lt;id&gt; IN (SELECT ... FROM rtree/idx ...)</c> sub-select), the geometry is read as WKB
    /// via <c>ST_AsBinary(...)</c> and decoded with <see cref="OGC.WKBToGeometry"/>.
    /// <c>WHERE</c> / <c>ORDER BY</c> / paging use <see cref="SqliteSelectBuilder"/>, like the
    /// classic SQLite FDB cursors.
    /// </summary>
    internal sealed class SQLiteNativeFeatureCursor : SQLiteFeatureCursorBase
    {
        private const string ShapeAlias = "temp_geometry";

        private readonly SpatiaLiteFlavor _flavor;

        private SQLiteNativeFeatureCursor(
            IGeometryDef geomDef, ISpatialReference toSRef, IDatumTransformations datumTransformations, SpatiaLiteFlavor flavor)
            : base(geomDef, toSRef, datumTransformations)
        {
            _flavor = flavor;
        }

        protected override string ShapeColumn => ShapeAlias;

        protected override IGeometry DecodeShape(byte[] bytes) => OGC.WKBToGeometry(bytes);

        protected override void PrepareConnection(SQLiteConnection connection)
        {
            SpatiaLiteNative.LoadInto(connection);

            if (_flavor == SpatiaLiteFlavor.GeoPackage)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT EnableGpkgAmphibiousMode()";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "PRAGMA trusted_schema = ON";
                cmd.ExecuteNonQuery();
            }
        }

        public static async Task<IFeatureCursor> Create(
            string connectionString, string tableName, string rtreeTableName, IFeatureClass fc, IQueryFilter filter,
            SpatiaLiteFlavor flavor, int srid, ISpatialReference toSRef, IDatumTransformations datumTransformations)
        {
            var cursor = new SQLiteNativeFeatureCursor(fc, toSRef, datumTransformations, flavor);

            filter ??= new QueryFilter();

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
                    fieldNames.Append("ST_AsBinary(")
                              .Append(SpatiaLiteSchema.GeometryReadExpression(flavor, "\"FDB_SHAPE\""))
                              .Append(") as ").Append(ShapeAlias);
                }
                else
                {
                    fieldNames.Append('[').Append(bare).Append(']');
                }
            }

            return fieldNames.ToString();
        }

        private static string BuildSpatialWhere(ISpatialFilter sFilter, SpatiaLiteFlavor flavor, string tableName, int srid)
        {
            if (sFilter?.Geometry == null)
            {
                return String.Empty;
            }

            IEnvelope env = sFilter.Geometry.Envelope;

            var sb = new StringBuilder(SpatiaLiteSchema.SpatialIndexPredicate(
                flavor, tableName, "FDB_SHAPE", "FDB_OID",
                env.MinX, env.MinY, env.MaxX, env.MaxY, srid));

            if (sFilter.SpatialRelation != spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                sb.Append(" AND ")
                  .Append(SpatiaLiteSchema.IntersectsPredicate(flavor, "\"FDB_SHAPE\"", WKT.ToWKT(sFilter.Geometry), srid));
            }

            return sb.ToString();
        }
    }
}
