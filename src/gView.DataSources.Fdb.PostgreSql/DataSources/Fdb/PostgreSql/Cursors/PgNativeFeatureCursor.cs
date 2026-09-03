using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.OGC;
using gView.Framework.OGC.WKT;
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.PostgreSql.Cursors
{
    /// <summary>
    /// Reads an FDB feature class whose <c>FDB_SHAPE</c> column is a PostGIS <c>geometry</c>.
    /// The spatial restriction is delegated to PostGIS (GiST index): <c>&amp;&amp;</c> against a
    /// bounding envelope for the map-draw relation, <c>ST_Intersects</c> otherwise. The geometry is
    /// fetched as WKB via <c>ST_AsBinary</c> and decoded with <see cref="OGC.WKBToGeometry"/>.
    /// </summary>
    internal sealed class PgNativeFeatureCursor : FeatureCursor
    {
        private static readonly IFormatProvider _inv = CultureInfo.InvariantCulture;

        private readonly DbProviderFactory _factory = pgFDB._dbProviderFactory;
        private DbConnection _connection;
        private DbCommand _command;
        private DbDataReader _reader;

        private PgNativeFeatureCursor(IFeatureClass fc, IQueryFilter filter)
            : base(fc, fc?.SpatialReference, filter?.FeatureSpatialReference, filter?.DatumTransformations)
        {
        }

        public static async Task<IFeatureCursor> Create(string connectionString, IFeatureClass fc, IQueryFilter filter, int srid)
        {
            var cursor = new PgNativeFeatureCursor(fc, filter);

            if (filter == null)
            {
                filter = new QueryFilter();
            }

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

            string where = BuildSpatialWhere(filter as ISpatialFilter, srid);
            string userWhere = (filter is IRowIDFilter ridf) ? ridf.RowIDWhereClause : filter.WhereClause;

            var fieldNames = new StringBuilder();
            foreach (string raw in filter.SubFields.Split(' '))
            {
                string fieldName = raw.Trim().Trim('"');
                if (fieldName.Length == 0)
                {
                    continue;
                }

                if (fieldNames.Length > 0)
                {
                    fieldNames.Append(",");
                }

                if (String.Equals(fieldName, "FDB_SHAPE", StringComparison.OrdinalIgnoreCase))
                {
                    fieldNames.Append("ST_AsBinary(\"FDB_SHAPE\") as temp_geometry");
                }
                else
                {
                    fieldNames.Append("\"").Append(fieldName).Append("\"");
                }
            }

            string tabName = fc is pgFeatureClass pgFc ? pgFc.DbTableName : "FC_" + fc.Name;

            var sql = new StringBuilder();
            sql.Append("SELECT ").Append(fieldNames).Append(" FROM ").Append(tabName);
            if (where.Length > 0 && !String.IsNullOrEmpty(userWhere))
            {
                sql.Append(" WHERE ").Append(where).Append(" AND (").Append(userWhere).Append(")");
            }
            else if (where.Length > 0)
            {
                sql.Append(" WHERE ").Append(where);
            }
            else if (!String.IsNullOrEmpty(userWhere))
            {
                sql.Append(" WHERE ").Append(userWhere);
            }
            if (!String.IsNullOrEmpty(filter.OrderBy))
            {
                sql.Append(" ORDER BY ").Append(filter.OrderBy);
            }
            if (filter.Limit > 0)
            {
                sql.Append(" LIMIT ").Append(filter.Limit);
            }
            if (filter.BeginRecord > 1)
            {
                sql.Append(" OFFSET ").Append(filter.BeginRecord - 1);
            }

            try
            {
                cursor._connection = cursor._factory.CreateConnection();
                cursor._connection.ConnectionString = connectionString;
                await cursor._connection.OpenAsync();

                cursor._command = cursor._factory.CreateCommand();
                cursor._command.Connection = cursor._connection;
                cursor._command.CommandText = sql.ToString();

                cursor._reader = await cursor._command.ExecuteReaderAsync(CommandBehavior.Default);
            }
            catch (Exception ex)
            {
                cursor._lastException = ex.Message;
                cursor.Dispose();
            }

            return cursor;
        }

        private string _lastException;

        private static string BuildSpatialWhere(ISpatialFilter sFilter, int srid)
        {
            if (sFilter?.Geometry == null)
            {
                return String.Empty;
            }

            if (sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
            {
                IEnvelope env = sFilter.Geometry.Envelope;
                return "\"FDB_SHAPE\" && ST_MakeEnvelope("
                    + env.MinX.ToString(_inv) + "," + env.MinY.ToString(_inv) + ","
                    + env.MaxX.ToString(_inv) + "," + env.MaxY.ToString(_inv) + "," + srid + ")";
            }

            return "ST_Intersects(\"FDB_SHAPE\", ST_GeomFromText('" + WKT.ToWKT(sFilter.Geometry) + "'," + srid + "))";
        }

        public override async Task<IFeature> NextFeature()
        {
            if (_reader == null || !await _reader.ReadAsync())
            {
                Dispose();
                return null;
            }

            var feature = new Feature();
            for (int i = 0; i < _reader.FieldCount; i++)
            {
                string name = _reader.GetName(i);
                object obj = _reader.GetValue(i);

                if (name == "temp_geometry" && obj != DBNull.Value)
                {
                    feature.Shape = OGC.WKBToGeometry((byte[])obj);
                }
                else
                {
                    var fv = new FieldValue(name, obj);
                    feature.Fields.Add(fv);
                    if (fv.Name == "FDB_OID")
                    {
                        feature.OID = Convert.ToInt32(obj);
                    }
                }
            }

            Transform(feature);
            return feature;
        }

        public override void Dispose()
        {
            base.Dispose();

            try { _reader?.Close(); } catch { }
            _reader = null;

            try { _command?.Dispose(); } catch { }
            _command = null;

            if (_connection != null)
            {
                try
                {
                    if (_connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                }
                catch { }
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
