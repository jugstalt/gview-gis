using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.OGC;
using gView.Framework.OGC.DB;
using gView.Framework.OGC.WKT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.SpatiaLite
{
    /// <summary>
    /// Editable OGC feature datasource for stand-alone SpatiaLite (<c>geometry_columns</c>
    /// + SpatiaLite geometry blob) and GeoPackage (<c>gpkg_*</c> metadata + GPKG blob)
    /// files. Read/insert/update/delete run through the shared
    /// <see cref="OgcSpatialDataset"/> pipeline; the spatial SQL comes from the bundled
    /// <c>mod_spatialite</c> extension (see <see cref="SpatiaLiteNative"/>). GeoPackage
    /// files are handled in mod_spatialite "amphibious" mode.
    /// </summary>
    [UseDatasetNameCase(DatasetNameCase.ignore)]
    [RegisterPlugIn("975bcd88-bee4-43ed-a82c-ba10e0b45500")]
    public class SpatiaLiteDataset : OgcSpatialDataset, IPlugInDependencies, IFileFeatureDatabase
    {
        private static readonly IFormatProvider _inv = CultureInfo.InvariantCulture;

        private SpatiaLiteFlavor _flavor = SpatiaLiteFlavor.SpatiaLite;
        private string _filename = String.Empty;

        public override DbProviderFactory ProviderFactory => SQLiteFactory.Instance;

        protected override OgcSpatialDataset CreateInstance() => new SpatiaLiteDataset();

        public override string DatasetGroupName => "SpatiaLite / GeoPackage";

        internal SpatiaLiteFlavor Flavor => _flavor;

        #region Open / connection string

        protected override string ModifyConnectionString(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            connectionString = connectionString.Trim();

            _filename = FilePathOf(connectionString);

            // a bare file path -> wrap it into a real connection string
            return connectionString.IndexOf('=') < 0
                ? $"Data Source={connectionString}"
                : connectionString;
        }

        public override async Task<bool> Open()
        {
            if (String.IsNullOrEmpty(_filename) || !File.Exists(_filename))
            {
                LastErrorMessage = $"SpatiaLite/GeoPackage file not found: '{_filename}'";
                return false;
            }

            if (!SpatiaLiteNative.EnsureAvailable(out var nativeError))
            {
                LastErrorMessage = $"mod_spatialite is not available: {nativeError}";
                return false;
            }

            try
            {
                using (var connection = await OpenConnectionAsync())
                {
                    var (hasGpkg, hasGeometryColumns) = await ProbeSchemaAsync(connection);

                    if (!hasGpkg && !hasGeometryColumns)
                    {
                        LastErrorMessage =
                            $"'{_filename}' is neither a SpatiaLite nor a GeoPackage database " +
                             "(no 'geometry_columns' and no 'gpkg_contents' table).";
                        return false;
                    }

                    _flavor = hasGpkg ? SpatiaLiteFlavor.GeoPackage : SpatiaLiteFlavor.SpatiaLite;
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }

            return await base.Open();
        }

        protected override async Task OnConnectionOpenedAsync(DbConnection connection)
        {
            var sqliteConnection = (SQLiteConnection)connection;

            SpatiaLiteNative.LoadInto(sqliteConnection);

            if (_flavor == SpatiaLiteFlavor.GeoPackage)
            {
                using (var command = sqliteConnection.CreateCommand())
                {
                    // amphibious mode: ST_* functions understand GPKG (GPB) blobs too.
                    command.CommandText = "SELECT EnableGpkgAmphibiousMode()";
                    await command.ExecuteNonQueryAsync();

                    // GeoPackage ships RTree / feature-count triggers that call spatial
                    // functions; without trusted_schema those inserts/updates fail with
                    // "unsafe use of ST_IsEmpty()".
                    command.CommandText = "PRAGMA trusted_schema = ON";
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private static async Task<(bool hasGpkg, bool hasGeometryColumns)> ProbeSchemaAsync(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " +
                    " (SELECT count(*) FROM sqlite_master WHERE type='table' AND lower(name)='gpkg_contents')," +
                    " (SELECT count(*) FROM sqlite_master WHERE type IN ('table','view') AND lower(name)='geometry_columns')";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return (Convert.ToInt64(reader.GetValue(0)) > 0,
                                Convert.ToInt64(reader.GetValue(1)) > 0);
                    }
                }
            }

            return (false, false);
        }

        #endregion

        #region Metadata (Elements / Element)

        public override string OgcDictionary(string ogcExpression)
        {
            if (ogcExpression.Equals("geometry_columns.f_table_schema", StringComparison.OrdinalIgnoreCase))
            {
                return String.Empty;
            }

            return base.OgcDictionary(ogcExpression);
        }

        public override async Task<List<IDatasetElement>> Elements()
        {
            if (_layers != null && _layers.Count > 0)
            {
                return _layers;
            }

            var layers = new List<IDatasetElement>();

            try
            {
                DataTable meta;
                using (var connection = await OpenConnectionAsync())
                {
                    meta = _flavor == SpatiaLiteFlavor.GeoPackage
                        ? await ReadGeoPackageMetaAsync(connection)
                        : await ReadSpatiaLiteMetaAsync(connection);
                }

                foreach (DataRow row in meta.Rows)
                {
                    var fc = await OgcSpatialFeatureclass.Create(this, row);
                    if (fc != null)
                    {
                        layers.Add(new DatasetElement(fc));
                    }
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return layers;
            }

            _layers = layers;
            return _layers;
        }

        public override async Task<IDatasetElement> Element(string title)
        {
            foreach (var element in await Elements())
            {
                if (String.Equals(element.Title, title, StringComparison.OrdinalIgnoreCase) ||
                    (element.Class != null && String.Equals(element.Class.Name, title, StringComparison.OrdinalIgnoreCase)))
                {
                    return element;
                }
            }

            return null;
        }

        private static DataTable NewMetaTable()
        {
            var table = new DataTable();
            table.Columns.Add("f_table_name", typeof(string));
            table.Columns.Add("f_geometry_column", typeof(string));
            table.Columns.Add("type", typeof(string));
            table.Columns.Add("coord_dimension", typeof(int));
            table.Columns.Add("srid", typeof(int));

            return table;
        }

        private static async Task<DataTable> ReadSpatiaLiteMetaAsync(DbConnection connection)
        {
            var raw = new DataTable();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM geometry_columns";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    raw.Load(reader);
                }
            }

            var result = NewMetaTable();

            foreach (DataRow row in raw.Rows)
            {
                var newRow = result.NewRow();
                newRow["f_table_name"] = GetString(row, "f_table_name");
                newRow["f_geometry_column"] = GetString(row, "f_geometry_column");
                newRow["type"] = SpatiaLiteGeometryTypeName(
                    HasColumn(row, "geometry_type") ? row["geometry_type"] : GetValue(row, "type"));
                newRow["coord_dimension"] = CoordDimension(GetValue(row, "coord_dimension"));
                newRow["srid"] = ToInt(GetValue(row, "srid"));
                result.Rows.Add(newRow);
            }

            return result;
        }

        private static async Task<DataTable> ReadGeoPackageMetaAsync(DbConnection connection)
        {
            var raw = new DataTable();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT c.table_name AS f_table_name, gc.column_name AS f_geometry_column, " +
                    "       gc.geometry_type_name AS geometry_type_name, gc.z AS z, gc.srs_id AS srs_id " +
                    "FROM gpkg_contents c " +
                    "JOIN gpkg_geometry_columns gc ON lower(gc.table_name) = lower(c.table_name) " +
                    "WHERE lower(c.data_type) = 'features'";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    raw.Load(reader);
                }
            }

            var result = NewMetaTable();

            foreach (DataRow row in raw.Rows)
            {
                var newRow = result.NewRow();
                newRow["f_table_name"] = GetString(row, "f_table_name");
                newRow["f_geometry_column"] = GetString(row, "f_geometry_column");
                newRow["type"] = (GetString(row, "geometry_type_name") ?? "GEOMETRY").ToUpperInvariant();
                newRow["coord_dimension"] = ToInt(GetValue(row, "z")) >= 1 ? 3 : 2;
                newRow["srid"] = ToInt(GetValue(row, "srs_id"));
                result.Rows.Add(newRow);
            }

            return result;
        }

        #endregion

        #region Schema (create database / feature class)

        /// <summary>
        /// Creates a new, empty database file. A <c>*.gpkg</c> name produces a GeoPackage
        /// (<c>gpkgCreateBaseTables()</c>), anything else a SpatiaLite database
        /// (<c>InitSpatialMetaData()</c>). The instance is left pointing at the new file so
        /// <see cref="CreateFeatureClass"/> can be called straight away.
        /// </summary>
        public override bool Create(string name)
        {
            try
            {
                if (!SpatiaLiteNative.EnsureAvailable(out var error))
                {
                    LastErrorMessage = $"mod_spatialite is not available: {error}";
                    return false;
                }

                bool asGeoPackage = name.EndsWith(".gpkg", StringComparison.OrdinalIgnoreCase);

                using (var connection = new SQLiteConnection($"Data Source={name}"))
                {
                    connection.Open();
                    SpatiaLiteNative.LoadInto(connection);

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = asGeoPackage
                            ? "SELECT gpkgCreateBaseTables()"
                            : "SELECT InitSpatialMetaData(1)";
                        command.ExecuteNonQuery();
                    }
                }

                _filename = name;
                _connectionString = $"Data Source={name}";
                _flavor = asGeoPackage ? SpatiaLiteFlavor.GeoPackage : SpatiaLiteFlavor.SpatiaLite;
                _layers = null;

                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }
        }

        public override async Task<int> CreateFeatureClass(string dsname, string fcname, IGeometryDef geomDef, IFieldCollection fields)
        {
            string geometryType;
            switch (geomDef.GeometryType)
            {
                case GeometryType.Point: geometryType = "POINT"; break;
                case GeometryType.Multipoint: geometryType = "MULTIPOINT"; break;
                case GeometryType.Polyline: geometryType = "MULTILINESTRING"; break;
                case GeometryType.Polygon: geometryType = "MULTIPOLYGON"; break;
                default:
                    LastErrorMessage = $"Geometry type '{geomDef.GeometryType}' is not supported.";
                    return -1;
            }

            int srid = geomDef.SpatialReference?.EpsgCode ?? 0;

            var columns = new StringBuilder("\"fid\" INTEGER PRIMARY KEY AUTOINCREMENT");
            foreach (IField field in fields.ToEnumerable())
            {
                if (field.type == FieldType.ID || field.type == FieldType.Shape)
                {
                    continue;
                }

                columns.Append($", {DbColumnName(field.name)} {DbDictionary(field)}");
            }

            try
            {
                using (var connection = await OpenConnectionAsync())
                using (var command = connection.CreateCommand())
                {
                    if (_flavor == SpatiaLiteFlavor.GeoPackage)
                    {
                        // SRS row must exist before the gpkg_contents FK references it
                        if (srid > 0 && !await SridExistsAsync(command, "gpkg_spatial_ref_sys", "srs_id", srid))
                        {
                            await TryExecuteAsync(command, $"SELECT gpkgInsertEpsgSRID({srid})");
                        }

                        command.CommandText = $"CREATE TABLE {DbTableName(fcname)} ({columns})";
                        await command.ExecuteNonQueryAsync();

                        // gpkg_contents row must exist before gpkgAddGeometryColumn (FK)
                        command.CommandText =
                            "INSERT INTO gpkg_contents (table_name, data_type, identifier, srs_id) " +
                            $"VALUES ('{Escape(fcname)}', 'features', '{Escape(fcname)}', {srid})";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText =
                            $"SELECT gpkgAddGeometryColumn('{Escape(fcname)}', 'geom', '{geometryType}', 0, 0, {srid})";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText = $"SELECT gpkgAddGeometryTriggers('{Escape(fcname)}', 'geom')";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText = $"SELECT gpkgAddSpatialIndex('{Escape(fcname)}', 'geom')";
                        await command.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        command.CommandText = $"CREATE TABLE {DbTableName(fcname)} ({columns})";
                        await command.ExecuteNonQueryAsync();

                        if (srid > 0 && !await SridExistsAsync(command, "spatial_ref_sys", "srid", srid))
                        {
                            await TryExecuteAsync(command, $"SELECT InsertEpsgSrid({srid})");
                        }

                        command.CommandText =
                            $"SELECT AddGeometryColumn('{Escape(fcname)}', 'geom', {srid}, '{geometryType}', 'XY')";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText = $"SELECT CreateSpatialIndex('{Escape(fcname)}', 'geom')";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                _layers = null;
                return 0;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return -1;
            }
        }

        public override async Task<bool> DeleteFeatureClass(string name)
        {
            try
            {
                using (var connection = await OpenConnectionAsync())
                using (var command = connection.CreateCommand())
                {
                    if (_flavor == SpatiaLiteFlavor.GeoPackage)
                    {
                        foreach (var suffix in new[] { "insert", "update1", "update2", "update3", "update4", "delete" })
                        {
                            await TryExecuteAsync(command, $"DROP TRIGGER IF EXISTS \"rtree_{name}_geom_{suffix}\"");
                        }

                        await TryExecuteAsync(command, $"DROP TABLE IF EXISTS \"rtree_{name}_geom\"");
                        await TryExecuteAsync(command, $"DELETE FROM gpkg_extensions WHERE lower(table_name) = lower('{Escape(name)}')");
                        await TryExecuteAsync(command, $"DELETE FROM gpkg_geometry_columns WHERE lower(table_name) = lower('{Escape(name)}')");
                        await TryExecuteAsync(command, $"DELETE FROM gpkg_contents WHERE lower(table_name) = lower('{Escape(name)}')");

                        command.CommandText = $"DROP TABLE IF EXISTS {DbTableName(name)}";
                        await command.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        foreach (var geomColumn in await GeometryColumnNamesAsync(connection, name))
                        {
                            await TryExecuteAsync(command, $"SELECT DisableSpatialIndex('{Escape(name)}', '{Escape(geomColumn)}')");
                            await TryExecuteAsync(command, $"DROP TABLE IF EXISTS \"idx_{name}_{geomColumn}\"");
                            await TryExecuteAsync(command, $"SELECT DiscardGeometryColumn('{Escape(name)}', '{Escape(geomColumn)}')");
                        }

                        command.CommandText = $"DROP TABLE IF EXISTS {DbTableName(name)}";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                _layers = null;
                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }
        }

        private static async Task<List<string>> GeometryColumnNamesAsync(DbConnection connection, string tableName)
        {
            var names = new List<string>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    $"SELECT f_geometry_column FROM geometry_columns WHERE lower(f_table_name) = lower('{Escape(tableName)}')";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        names.Add(reader.GetString(0));
                    }
                }
            }

            if (names.Count == 0)
            {
                names.Add("geom");
            }

            return names;
        }

        private static async Task TryExecuteAsync(DbCommand command, string sql)
        {
            try
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                // best effort cleanup step
            }
        }

        private static async Task<bool> SridExistsAsync(DbCommand command, string table, string column, int srid)
        {
            try
            {
                command.CommandText = $"SELECT count(*) FROM {table} WHERE {column} = {srid}";
                return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
            }
            catch
            {
                return false;
            }
        }

        private static string Escape(string value) => (value ?? String.Empty).Replace("'", "''");

        /// <summary>
        /// The geometry column as an expression usable by the spatial SQL functions.
        /// For GeoPackage the raw column holds a GPB blob that <c>MbrIntersects</c> /
        /// <c>Extent</c> cannot read directly (even in amphibious mode), so it is wrapped
        /// in <c>CastAutomagic()</c>; for SpatiaLite the plain quoted column is used.
        /// </summary>
        private string GeometryColumnExpression(string shapeFieldName)
        {
            var column = DbColumnName(shapeFieldName);

            return _flavor == SpatiaLiteFlavor.GeoPackage
                ? $"CastAutomagic({column})"
                : column;
        }

        #endregion

        #region SQL dialect

        public override string DbTableName(string tableName)
            => "\"" + (tableName ?? String.Empty).Replace("\"", "\"\"") + "\"";

        public override string SelectReadSchema(string tableName)
            => base.SelectReadSchema(tableName) + " LIMIT 0";

        /// <summary>
        /// Bind-parameter name for a field. Field names may contain characters that are
        /// invalid in a SQLite parameter token - notably <c>:</c> (OSM-style keys like
        /// <c>mtb:scale:uphill</c>), spaces or <c>-</c>. They get collapsed to <c>_</c>;
        /// the real (quoted) column name is still used on the column side, and the base
        /// Insert/Update path uses this same token on both sides so they stay in sync.
        /// </summary>
        protected override string DbParameterName(string name)
        {
            var builder = new StringBuilder(name.Length + 1);
            builder.Append('@');

            foreach (char c in name)
            {
                bool safe = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
                builder.Append(safe ? c : '_');
            }

            return builder.ToString();
        }

        public override string DbDictionary(IField field)
        {
            switch (field.type)
            {
                case FieldType.Shape:
                    return String.Empty;
                case FieldType.ID:
                    return "INTEGER PRIMARY KEY AUTOINCREMENT";
                case FieldType.smallinteger:
                case FieldType.integer:
                case FieldType.biginteger:
                case FieldType.boolean:
                    return "INTEGER";
                case FieldType.Float:
                case FieldType.Double:
                    return "REAL";
                case FieldType.Date:
                    return "DATETIME";
                case FieldType.character:
                case FieldType.String:
                    return "TEXT";
                default:
                    return "TEXT";
            }
        }

        public override string IntegerPrimaryKeyField(string tableName)
        {
            var t = (tableName ?? String.Empty).Replace("\"", "").Replace("'", "''");

            return $"SELECT name FROM pragma_table_info('{t}') " +
                   "WHERE pk = 1 AND instr(lower(type), 'int') > 0 LIMIT 1";
        }

        protected override string InsertReturnRowIdStatement(OgcSpatialFeatureclass featureClass)
        {
            // the id column is always an INTEGER PRIMARY KEY (rowid alias) - SpatiaLite tables
            // as well as GeoPackage "fid" - so last_insert_rowid() is exact and connection-local.
            return "; SELECT last_insert_rowid()";
        }

        public override string CaseInsensitivLikeOperator => "like";

        protected override bool DbImplementsTransactions => true;

        #endregion

        #region Geometry serialization

        protected override object ShapeParameterValue(OgcSpatialFeatureclass fClass,
                                                      IGeometry shape,
                                                      int srid,
                                                      StringBuilder sqlStatementHeader,
                                                      out bool AsSqlParameter)
        {
            AsSqlParameter = true;

            if (shape == null)
            {
                return null;
            }

            // Plain OGC WKB (srid 0 -> no PostGIS EWKB header); the SRID is applied by
            // GeomFromWKB(?, srid) in InsertShapeParameterExpression.
            return OGC.GeometryToWKB(shape, 0, OGC.WkbByteOrder.Ndr);
        }

        protected override string InsertShapeParameterExpression(OgcSpatialFeatureclass featureClass, IGeometry shape)
        {
            int srid = featureClass?.SpatialReference?.EpsgCode ?? 0;

            string expression = $"GeomFromWKB({{0}}, {srid})";

            if (shape is IPolygon)
            {
                expression = $"ST_MakeValid({expression})";
            }

            // SpatiaLite / GeoPackage geometry columns are strictly typed - promote to the
            // declared multi-type so a single-part geometry is accepted.
            string declared = (featureClass?.GeometryTypeString ?? String.Empty).ToUpperInvariant();
            if (declared.StartsWith("MULTIPOLYGON"))
            {
                expression = $"CastToMultiPolygon({expression})";
            }
            else if (declared.StartsWith("MULTILINESTRING"))
            {
                expression = $"CastToMultiLineString({expression})";
            }
            else if (declared.StartsWith("MULTIPOINT"))
            {
                expression = $"CastToMultiPoint({expression})";
            }

            // A GeoPackage geometry column must hold a GPB ("GP..") blob - in amphibious
            // mode the ST_* result is a SpatiaLite blob, so wrap it.
            if (_flavor == SpatiaLiteFlavor.GeoPackage)
            {
                expression = $"AsGPB({expression})";
            }

            return expression;
        }

        public override async Task<IEnvelope> FeatureClassEnvelope(IFeatureClass fc)
        {
            if (fc == null)
            {
                return new Envelope();
            }

            try
            {
                using (var connection = await OpenConnectionAsync())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        $"SELECT ST_AsBinary(Extent({GeometryColumnExpression(fc.ShapeFieldName)})) FROM {DbTableName(fc.Name)}";

                    var result = await command.ExecuteScalarAsync();
                    if (result is byte[] wkb && wkb.Length > 0)
                    {
                        var envelope = OGC.WKBToGeometry(wkb)?.Envelope;
                        if (envelope != null)
                        {
                            return envelope;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
            }

            return new Envelope(-180, -90, 180, 90);
        }

        public override bool CanEditFeatureClass(IFeatureClass fc, EditCommands command)
        {
            if (fc is not OgcSpatialFeatureclass ogcFeatureClass)
            {
                return false;
            }

            if (command == EditCommands.Delete)
            {
                return true;
            }

            var type = (ogcFeatureClass.GeometryTypeString ?? String.Empty).ToUpperInvariant();
            if (type.EndsWith("Z") || type.EndsWith("M"))
            {
                LastErrorMessage = $"Editing geometries of type '{type}' is not supported.";
                return false;
            }

            return true;
        }

        #endregion

        #region SelectCommand (SpatiaLite spatial SQL)

        public override DbCommand SelectCommand(OgcSpatialFeatureclass fc,
                                                IQueryFilter filter,
                                                out string shapeFieldName,
                                                string functionName = "",
                                                string functionField = "",
                                                string functionAlias = "")
        {
            shapeFieldName = String.Empty;

            filter.fieldPrefix = filter.fieldPostfix = "\"";

            if (filter.SubFields == "*")
            {
                filter.SubFields = "";

                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    filter.AddField(field.name);
                }
                filter.AddField(fc.IDFieldName);
                filter.AddField(fc.ShapeFieldName);
            }
            else if (!(filter is DistinctFilter) && !(filter is FunctionFilter))
            {
                filter.AddField(fc.IDFieldName);
            }

            var where = new StringBuilder();

            if (filter is ISpatialFilter spatialFilter && spatialFilter.Geometry != null)
            {
                int srid = fc.SpatialReference?.EpsgCode ?? 0;
                string geomColumn = DbColumnName(fc.ShapeFieldName);
                string geomExpr = GeometryColumnExpression(fc.ShapeFieldName);
                IEnvelope env = spatialFilter.Geometry.Envelope;

                string mbr = $"BuildMbr(" +
                             $"{env.MinX.ToString(_inv)},{env.MinY.ToString(_inv)}," +
                             $"{env.MaxX.ToString(_inv)},{env.MaxY.ToString(_inv)},{srid})";

                where.Append($"{geomColumn} IS NOT NULL AND MbrIntersects({geomExpr}, {mbr}) = 1");

                if (spatialFilter.SpatialRelation != spatialRelation.SpatialRelationMapEnvelopeIntersects)
                {
                    string wkt = WKT.ToWKT(spatialFilter.Geometry);
                    where.Append($" AND ST_Intersects({geomExpr}, GeomFromText('{wkt}', {srid})) = 1");
                }

                filter.AddField(fc.ShapeFieldName);
            }

            if (!String.IsNullOrWhiteSpace(functionName) && !String.IsNullOrWhiteSpace(functionField))
            {
                filter.SubFields = "";
                filter.AddField(functionName + "(" + filter.fieldPrefix + functionField + filter.fieldPostfix + ")");
            }

            string filterWhereClause = (filter is IRowIDFilter rowIdFilter)
                ? rowIdFilter.RowIDWhereClause
                : filter.WhereClause;

            if (where.Length == 0)
            {
                where.Append(filterWhereClause);
            }
            else if (!String.IsNullOrEmpty(filterWhereClause))
            {
                where.Append(" AND (");
                where.Append(filterWhereClause);
                where.Append(')');
            }

            var fieldNames = new StringBuilder();

            if (filter is DistinctFilter || filter is FunctionFilter)
            {
                fieldNames.Append(filter.SubFieldsAndAlias);
            }
            else
            {
                // QuerySubFields yields one entry per field (each already quoted); unlike
                // filter.SubFields.Split(' ') it does not fall apart on field names that
                // contain a space, e.g. "some name".
                foreach (string fieldName in filter.QuerySubFields)
                {
                    if (String.IsNullOrEmpty(fieldName))
                    {
                        continue;
                    }

                    if (fieldNames.Length > 0)
                    {
                        fieldNames.Append(',');
                    }

                    if (fieldName == "\"" + fc.ShapeFieldName + "\"")
                    {
                        fieldNames.Append($"ST_AsBinary({GeometryColumnExpression(fc.ShapeFieldName)}) as temp_geometry");
                        shapeFieldName = "temp_geometry";
                    }
                    else
                    {
                        fieldNames.Append(fieldName);
                    }
                }
            }

            var orderBy = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(filter.OrderBy))
            {
                orderBy.Append($" ORDER BY {filter.OrderBy}");
            }

            var limit = new StringBuilder();
            bool hasOffset = filter.BeginRecord > 1;
            if (filter.Limit > 0)
            {
                limit.Append($" LIMIT {filter.Limit}");
            }
            else if (hasOffset)
            {
                limit.Append(" LIMIT -1");
            }

            if (hasOffset)
            {
                limit.Append($" OFFSET {Math.Max(0, filter.BeginRecord - 1)}");
            }

            var sql = new StringBuilder();
            sql.Append("SELECT ");
            sql.Append(fieldNames);
            sql.Append(" FROM ");
            sql.Append(DbTableName(fc.Name));
            if (where.Length > 0)
            {
                sql.Append(" WHERE ");
                sql.Append(where);
            }
            sql.Append(orderBy);
            sql.Append(limit);

            var command = ProviderFactory.CreateCommand();
            command.CommandText = sql.ToString();

            return command;
        }

        #endregion

        #region IPlugInDependencies

        public bool HasUnsolvedDependencies() => HasUnsolvedDependenciesStatic;

        public static bool HasUnsolvedDependenciesStatic
            => SQLiteFactory.Instance == null || !SpatiaLiteNative.EnsureAvailable(out _);

        #endregion

        #region IFileFeatureDatabase

        public string DatabaseName => "SpatiaLite / GeoPackage";

        public int MaxFieldNameLength => 0; // SQLite has no practical identifier-length limit

        public bool IsFolderBased => false; // single file, not a directory of files

        public bool Flush(IFeatureClass fc) => true; // SQLite commits per transaction

        public override Task<int> CreateDataset(string name, ISpatialReference sRef)
            => Task.FromResult(Create(FilePathOf(name)) ? 0 : -1);

        /// <summary>
        /// <see cref="IDatabase.Open"/> overload used by the command parameter builders for
        /// an <see cref="IFileFeatureDatabase"/>: <paramref name="name"/> is a file path or a
        /// <c>Data Source=…</c> connection string.
        /// </summary>
        public override async Task<bool> Open(string name)
        {
            await SetConnectionString(name);
            return await Open();
        }

        /// <summary>
        /// Re-implemented for <see cref="IFileFeatureDatabase"/>: <paramref name="name"/> is a
        /// file path or a <c>Data Source=…</c> connection string. Opens it (creating an empty
        /// SpatiaLite / GeoPackage first if the file is missing) and returns a dataset bound
        /// to that file.
        /// </summary>
        async Task<IFeatureDataset> IFeatureDatabase.GetDataset(string name)
        {
            var path = FilePathOf(name);

            if (!String.IsNullOrEmpty(path) && !File.Exists(path) && !Create(path))
            {
                return null;
            }

            var dataset = new SpatiaLiteDataset();
            await dataset.SetConnectionString(name);

            return await dataset.Open() ? dataset : null;
        }

        /// <summary>Extracts the file path from a bare path or a <c>Data Source=…</c> string.</summary>
        private static string FilePathOf(string nameOrConnectionString)
        {
            if (String.IsNullOrWhiteSpace(nameOrConnectionString))
            {
                return String.Empty;
            }

            var value = nameOrConnectionString.Trim();
            if (value.IndexOf('=') < 0)
            {
                return value;
            }

            try
            {
                return new SQLiteConnectionStringBuilder(value).DataSource;
            }
            catch
            {
                return String.Empty;
            }
        }

        #endregion

        #region Helper

        private static bool HasColumn(DataRow row, string name)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (String.Equals(column.ColumnName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static object GetValue(DataRow row, string name)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (String.Equals(column.ColumnName, name, StringComparison.OrdinalIgnoreCase))
                {
                    var value = row[column];
                    return value == DBNull.Value ? null : value;
                }
            }

            return null;
        }

        private static string GetString(DataRow row, string name)
            => GetValue(row, name)?.ToString();

        private static int ToInt(object value)
        {
            if (value == null)
            {
                return 0;
            }

            try
            {
                return Convert.ToInt32(value, _inv);
            }
            catch
            {
                return int.TryParse(value.ToString(), NumberStyles.Integer, _inv, out int parsed) ? parsed : 0;
            }
        }

        private static int CoordDimension(object value)
        {
            if (value == null)
            {
                return 2;
            }

            if (value is string text)
            {
                return text.ToUpperInvariant().Contains("Z") ? 3 : 2;
            }

            return ToInt(value) >= 3 ? 3 : 2;
        }

        private static string SpatiaLiteGeometryTypeName(object value)
        {
            if (value == null)
            {
                return "GEOMETRY";
            }

            if (value is string text)
            {
                return text.Trim().ToUpperInvariant();
            }

            long code = Convert.ToInt64(value, _inv) % 1000;
            switch (code)
            {
                case 1: return "POINT";
                case 2: return "LINESTRING";
                case 3: return "POLYGON";
                case 4: return "MULTIPOINT";
                case 5: return "MULTILINESTRING";
                case 6: return "MULTIPOLYGON";
                case 7: return "GEOMETRYCOLLECTION";
                default: return "GEOMETRY";
            }
        }

        #endregion
    }
}
