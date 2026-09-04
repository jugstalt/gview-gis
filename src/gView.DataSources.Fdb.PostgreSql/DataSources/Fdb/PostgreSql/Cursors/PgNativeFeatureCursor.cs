using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.OGC;
using gView.Framework.OGC.WKT;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.PostgreSql.Cursors
{
    /// <summary>
    /// Reads an FDB feature class whose <c>FDB_SHAPE</c> column is a PostGIS <c>geometry</c>.
    /// The spatial restriction is delegated to PostGIS (GiST index): <c>&amp;&amp;</c> against a
    /// bounding envelope for the map-draw relation, <c>ST_Intersects</c> otherwise. The geometry is
    /// fetched as WKB via <c>ST_AsBinary(...) AS temp_geometry</c> and decoded with
    /// <see cref="OGC.WKBToGeometry"/>. <c>WHERE</c> / <c>ORDER BY</c> / paging are assembled with
    /// <see cref="PostgreSqlSelectBuilder"/>, like the classic PostgreSQL FDB cursors.
    /// </summary>
    internal sealed class PgNativeFeatureCursor : PgFeatureCursorBase
    {
        private const string ShapeAlias = "temp_geometry";
        private static readonly IFormatProvider _inv = CultureInfo.InvariantCulture;

        private PgNativeFeatureCursor(IFeatureClass fc, IQueryFilter filter)
            : base(fc, filter?.FeatureSpatialReference, filter?.DatumTransformations)
        {
        }

        protected override string ShapeColumn => ShapeAlias;

        protected override IGeometry DecodeShape(byte[] bytes) => OGC.WKBToGeometry(bytes);

        public static async Task<IFeatureCursor> Create(string connectionString, IFeatureClass fc, IQueryFilter filter, int srid)
        {
            var cursor = new PgNativeFeatureCursor(fc, filter);

            filter ??= new QueryFilter();

            // expand "*" so the shape column can be replaced by ST_AsBinary(...)
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

            string tabName = fc is pgFeatureClass pgFc ? pgFc.DbTableName : "FC_" + fc.Name;
            string selectFrom = new StringBuilder("SELECT ")
                .Append(BuildFieldList(filter.SubFields))
                .Append(" FROM ")
                .Append(tabName)
                .ToString();

            string spatialWhere = BuildSpatialWhere(filter as ISpatialFilter, srid);
            string userWhere = (filter is IRowIDFilter ridf) ? ridf.RowIDWhereClause : filter.WhereClause;

            string commandText = new PostgreSqlSelectBuilder(selectFrom)
                .WhereAnd(spatialWhere, userWhere)
                .OrderBy(filter.OrderBy)
                .Page(filter.Limit, filter.BeginRecord)
                .Build();

            await cursor.OpenReaderAsync(connectionString, commandText);
            return cursor;
        }

        public override Task<IFeature> NextFeature() => NextRawFeatureAsync();

        private static string BuildFieldList(string subFields)
        {
            var fieldNames = new StringBuilder();

            foreach (string raw in subFields.Split(' '))
            {
                string fieldName = raw.Trim().Trim('"');
                if (fieldName.Length == 0)
                {
                    continue;
                }

                if (fieldNames.Length > 0)
                {
                    fieldNames.Append(',');
                }

                if (String.Equals(fieldName, "FDB_SHAPE", StringComparison.OrdinalIgnoreCase))
                {
                    fieldNames.Append("ST_AsBinary(\"FDB_SHAPE\") as ").Append(ShapeAlias);
                }
                else
                {
                    fieldNames.Append('"').Append(fieldName).Append('"');
                }
            }

            return fieldNames.ToString();
        }

        private static string BuildSpatialWhere(ISpatialFilter sFilter, int srid)
        {
            if (sFilter?.Geometry == null)
            {
                return String.Empty;
            }

            var sb = new StringBuilder();

            if (sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                IEnvelope env = sFilter.Geometry.Envelope;
                sb.Append("\"FDB_SHAPE\" && ST_MakeEnvelope(")
                  .Append(env.MinX.ToString(_inv)).Append(',')
                  .Append(env.MinY.ToString(_inv)).Append(',')
                  .Append(env.MaxX.ToString(_inv)).Append(',')
                  .Append(env.MaxY.ToString(_inv)).Append(',')
                  .Append(srid).Append(')');
            }
            else
            {
                sb.Append("ST_Intersects(\"FDB_SHAPE\", ST_GeomFromText('")
                  .Append(WKT.ToWKT(sFilter.Geometry)).Append("',")
                  .Append(srid).Append("))");
            }

            return sb.ToString();
        }
    }
}
