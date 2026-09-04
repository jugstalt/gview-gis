using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.OGC;
using gView.Framework.OGC.WKT;
using System;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.MSSql.Cursors
{
    /// <summary>
    /// Reads an FDB feature class whose <c>FDB_SHAPE</c> column is a SQL Server <c>geometry</c> /
    /// <c>geography</c>. The spatial restriction is delegated to SQL Server (GEOMETRY_GRID /
    /// GEOGRAPHY_GRID index): <c>.Filter(...)</c> for the map-draw relation, <c>.STIntersects(...) = 1</c>
    /// otherwise. The geometry is fetched as WKB via <c>FDB_SHAPE.STAsBinary() AS temp_geometry</c>
    /// and decoded with <see cref="OGC.WKBToGeometry"/>. <c>WHERE</c> / <c>ORDER BY</c> / paging are
    /// assembled with <see cref="SqlServerSelectBuilder"/>, like the classic SQL Server FDB cursors
    /// (which also gives this path the <c>OFFSET ... FETCH</c> paging the old cursor lacked).
    /// </summary>
    internal sealed class SqlNativeFeatureCursor : SqlFeatureCursorBase
    {
        private const string ShapeAlias = "temp_geometry";

        private SqlNativeFeatureCursor(IFeatureClass fc, IQueryFilter filter)
            : base(fc, filter?.FeatureSpatialReference, filter?.DatumTransformations)
        {
        }

        protected override string ShapeColumn => ShapeAlias;

        protected override IGeometry DecodeShape(byte[] bytes) => OGC.WKBToGeometry(bytes);

        public static async Task<IFeatureCursor> Create(
            string connectionString, IFeatureClass fc, IQueryFilter filter, GeometryFieldType geometryType)
        {
            var cursor = new SqlNativeFeatureCursor(fc, filter);

            filter ??= new QueryFilter();

            // geography needs a geographic SRID (4326); geometry uses the feature class' EPSG (0 if unknown)
            int srid = geometryType == GeometryFieldType.MsGeography
                ? 4326
                : (fc.SpatialReference?.EpsgCode ?? 0);

            if (filter.SubFields == "*")
            {
                filter = (IQueryFilter)filter.Clone();
                filter.SubFields = "";
                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    filter.AddField(field.name);
                }
            }
            filter.AddField(fc.ShapeFieldName);

            string tabName = fc is SqlFDBFeatureClass sqlFc ? sqlFc.DbTableName : "FC_" + fc.Name;
            string selectFrom = new StringBuilder("SELECT ")
                .Append(BuildFieldList(filter.SubFields, fc.ShapeFieldName))
                .Append(" FROM ")
                .Append(tabName)
                .ToString();

            string spatialWhere = BuildSpatialWhere(filter as ISpatialFilter, fc.ShapeFieldName, geometryType, srid);
            string userWhere = (filter is IRowIDFilter ridf) ? ridf.RowIDWhereClause : filter.WhereClause;

            string commandText = new SqlServerSelectBuilder(selectFrom)
                .WhereAnd(spatialWhere, userWhere)
                .OrderBy(filter.OrderBy)
                .Page(filter.Limit, filter.BeginRecord)
                .Build();

            await cursor.OpenReaderAsync(connectionString, commandText);
            return cursor;
        }

        public override Task<IFeature> NextFeature() => NextRawFeatureAsync();

        private static string BuildFieldList(string subFields, string shapeFieldName)
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

                if (String.Equals(bare, shapeFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    fieldNames.Append('[').Append(shapeFieldName).Append("].STAsBinary() as ").Append(ShapeAlias);
                }
                else
                {
                    fieldNames.Append('[').Append(bare).Append(']');
                }
            }

            return fieldNames.ToString();
        }

        private static string BuildSpatialWhere(
            ISpatialFilter sFilter, string shapeFieldName, GeometryFieldType geometryType, int srid)
        {
            if (sFilter?.Geometry == null)
            {
                return String.Empty;
            }

            string typePrefix = geometryType == GeometryFieldType.MsGeography ? "geography" : "geometry";

            bool mapEnvelope =
                sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects &&
                sFilter.Geometry is IEnvelope;

            return new StringBuilder()
                .Append('[').Append(shapeFieldName).Append(']')
                .Append(mapEnvelope ? ".Filter(" : ".STIntersects(")
                .Append(typePrefix).Append("::STGeomFromText('")
                .Append(WKT.ToWKT(sFilter.Geometry)).Append("',")
                .Append(srid).Append("))=1")
                .ToString();
        }
    }
}
