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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.GeoPackage
{
    /// <summary>
    /// Editable OGC feature datasource for stand-alone <b>GeoPackage</b> (<c>*.gpkg</c>) files.
    /// <para>
    /// Completely <c>mod_spatialite</c>-free: the GeoPackage geometry (GPB) blob is read / written
    /// in managed code (<see cref="GpkgGeometry"/>), the metadata and the R-Tree spatial index are
    /// plain SQL (<see cref="GeoPackageSchema"/>, no triggers), and the precise spatial relation is
    /// re-checked per row by <c>OgcSpatialFeatureCursor</c>. Read / insert / update / delete run
    /// through the shared <see cref="OgcSpatialDataset"/> pipeline.
    /// </para>
    /// </summary>
    [UseDatasetNameCase(DatasetNameCase.ignore)]
    [RegisterPlugIn("35c6d28f-7c69-4639-b8c9-26e64762da00")]
    public class GeoPackageDataset : OgcSpatialDataset, IPlugInDependencies, IFileFeatureDatabase
    {
        private static readonly IFormatProvider _inv = CultureInfo.InvariantCulture;

        private string _filename = String.Empty;

        // table (lower-case) -> geometry column name, and table (lower-case) -> has a usable R-Tree.
        private readonly Dictionary<string, string> _geometryColumns = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _rtreeTables = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _rtreeManaged = new(StringComparer.OrdinalIgnoreCase);

        public override DbProviderFactory ProviderFactory => SQLiteFactory.Instance;

        protected override OgcSpatialDataset CreateInstance() => new GeoPackageDataset();

        public override string DatasetGroupName => "GeoPackage";

        #region Open / connection string

        protected override string ModifyConnectionString(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            connectionString = connectionString.Trim();
            _filename = FilePathOf(connectionString);

            return connectionString.IndexOf('=') < 0
                ? $"Data Source={connectionString}"
                : connectionString;
        }

        public override async Task<bool> Open()
        {
            if (String.IsNullOrEmpty(_filename) || !File.Exists(_filename))
            {
                LastErrorMessage = $"GeoPackage file not found: '{_filename}'";
                return false;
            }

            try
            {
                using (var connection = await OpenConnectionAsync())
                {
                    if (!await IsGeoPackageAsync(connection))
                    {
                        LastErrorMessage =
                            $"'{_filename}' is not a GeoPackage database " +
                             "(PRAGMA application_id is not 'GPKG' and there is no 'gpkg_contents' table).";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }

            return await base.Open();
        }

        private static async Task<bool> IsGeoPackageAsync(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA application_id";
                if (Convert.ToInt64(await command.ExecuteScalarAsync()) == GeoPackageSchema.ApplicationId)
                {
                    return true;
                }

                command.CommandText =
                    "SELECT count(*) FROM sqlite_master WHERE type='table' AND lower(name)='gpkg_contents'";
                return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
            }
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
                    meta = await ReadGeoPackageMetaAsync(connection);
                    await ReadRTreeIndexAsync(connection);
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

        private async Task<DataTable> ReadGeoPackageMetaAsync(DbConnection connection)
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
            _geometryColumns.Clear();

            foreach (DataRow row in raw.Rows)
            {
                string tableName = GetString(row, "f_table_name");
                string geomColumn = GetString(row, "f_geometry_column");

                var newRow = result.NewRow();
                newRow["f_table_name"] = tableName;
                newRow["f_geometry_column"] = geomColumn;
                newRow["type"] = (GetString(row, "geometry_type_name") ?? "GEOMETRY").ToUpperInvariant();
                newRow["coord_dimension"] = ToInt(GetValue(row, "z")) >= 1 ? 3 : 2;
                newRow["srid"] = ToInt(GetValue(row, "srs_id"));
                result.Rows.Add(newRow);

                if (!String.IsNullOrEmpty(tableName) && !String.IsNullOrEmpty(geomColumn))
                {
                    _geometryColumns[tableName] = geomColumn;
                }
            }

            return result;
        }

        private async Task ReadRTreeIndexAsync(DbConnection connection)
        {
            _rtreeTables.Clear();
            _rtreeManaged.Clear();

            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT name, type FROM sqlite_master WHERE type IN ('table','trigger') AND name LIKE 'rtree\\_%' ESCAPE '\\'";

                var triggers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var tables = new List<string>();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string name = reader.GetString(0);
                        string type = reader.GetString(1);
                        if (type == "table")
                        {
                            tables.Add(name);
                        }
                        else
                        {
                            triggers.Add(name);
                        }
                    }
                }

                foreach (var (tableLower, geomColumn) in EnumerateGeometryColumns())
                {
                    string rtree = GeoPackageSchema.RTreeTable(tableLower, geomColumn);
                    bool hasTable = tables.Exists(t => String.Equals(t, rtree, StringComparison.OrdinalIgnoreCase));
                    if (!hasTable)
                    {
                        continue;
                    }

                    _rtreeTables.Add(tableLower);

                    // A GeoPackage authored by QGIS / GDAL ships INSERT/UPDATE/DELETE triggers that
                    // keep its R-Tree current; gView must then NOT also maintain it (double rows).
                    // Our own GeoPackages are trigger-free, so gView owns the R-Tree.
                    bool hasTriggers = false;
                    foreach (var trg in triggers)
                    {
                        if (trg.StartsWith(rtree + "_", StringComparison.OrdinalIgnoreCase))
                        {
                            hasTriggers = true;
                            break;
                        }
                    }

                    _rtreeManaged[tableLower] = !hasTriggers;
                }
            }
        }

        private IEnumerable<(string tableLower, string geomColumn)> EnumerateGeometryColumns()
        {
            foreach (var kvp in _geometryColumns)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Schema (create database / feature class)

        /// <summary>
        /// Creates a new, empty GeoPackage file (<see cref="GeoPackageSchema.EnsureBaseTables"/>).
        /// The instance is left pointing at the new file so <see cref="CreateFeatureClass"/> can be
        /// called straight away.
        /// </summary>
        public override bool Create(string name)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={name}"))
                {
                    connection.Open();
                    GeoPackageSchema.EnsureBaseTables(connection);
                }

                _filename = name;
                _connectionString = $"Data Source={name}";
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
            switch (geomDef.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                case GeometryType.Polyline:
                case GeometryType.Polygon:
                    break;
                default:
                    LastErrorMessage = $"Geometry type '{geomDef.GeometryType}' is not supported.";
                    return -1;
            }

            int srid = geomDef.SpatialReference?.EpsgCode ?? 0;
            const string geomColumn = "geom";

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
                using (var connection = (SQLiteConnection)await OpenConnectionAsync())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"CREATE TABLE {DbTableName(fcname)} ({columns})";
                    await command.ExecuteNonQueryAsync();

                    GeoPackageSchema.EnsureSrs(connection, srid);
                    GeoPackageSchema.RegisterFeatureTable(
                        connection, fcname, geomColumn, geomDef.GeometryType, srid, hasZ: false, hasM: false);
                    GeoPackageSchema.CreateRTree(connection, fcname, geomColumn);
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
                using (var connection = (SQLiteConnection)await OpenConnectionAsync())
                using (var command = connection.CreateCommand())
                {
                    string geomColumn = await ResolveGeometryColumnAsync(connection, name);

                    GeoPackageSchema.DropRTree(connection, name, geomColumn);
                    GeoPackageSchema.Unregister(connection, name, geomColumn);

                    command.CommandText = $"DROP TABLE IF EXISTS {DbTableName(name)}";
                    await command.ExecuteNonQueryAsync();
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

        private async Task<string> ResolveGeometryColumnAsync(DbConnection connection, string tableName)
        {
            if (_geometryColumns.TryGetValue(tableName, out var cached) && !String.IsNullOrEmpty(cached))
            {
                return cached;
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    $"SELECT column_name FROM gpkg_geometry_columns WHERE lower(table_name) = lower('{Escape(tableName)}') LIMIT 1";
                var result = await command.ExecuteScalarAsync();
                if (result is string s && !String.IsNullOrEmpty(s))
                {
                    return s;
                }
            }

            return "geom";
        }

        private static string Escape(string value) => (value ?? String.Empty).Replace("'", "''");

        #endregion

        #region SQL dialect

        public override string DbTableName(string tableName)
            => "\"" + (tableName ?? String.Empty).Replace("\"", "\"\"") + "\"";

        public override string SelectReadSchema(string tableName)
            => base.SelectReadSchema(tableName) + " LIMIT 0";

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
            => "; SELECT last_insert_rowid()";

        public override string CaseInsensitivLikeOperator => "like";

        protected override bool DbImplementsTransactions => true;

        #endregion

        #region Geometry serialization (managed GPB, no mod_spatialite)

        protected override IGeometry DecodeShape(byte[] raw)
            => OGC.WKBToGeometry(GpkgGeometry.ToWkb(raw));

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

            var wkb = OGC.GeometryToWKB(shape, 0, OGC.WkbByteOrder.Ndr);
            return GpkgGeometry.ToGpb(wkb, srid, shape.Envelope);
        }

        /// <summary>
        /// The GPB blob is bound ready-made by <see cref="ShapeParameterValue"/>; the column value
        /// is just the parameter (no <c>GeomFromWKB</c> / <c>AsGPB</c> - those need mod_spatialite).
        /// </summary>
        protected override string InsertShapeParameterExpression(OgcSpatialFeatureclass featureClass, IGeometry shape)
            => String.Empty;

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
                        $"SELECT min_x, min_y, max_x, max_y FROM gpkg_contents WHERE lower(table_name) = lower('{Escape(fc.Name)}')";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync() && !reader.IsDBNull(0) && !reader.IsDBNull(3))
                        {
                            double minX = reader.GetDouble(0), minY = reader.GetDouble(1);
                            double maxX = reader.GetDouble(2), maxY = reader.GetDouble(3);
                            if (maxX >= minX && maxY >= minY)
                            {
                                return new Envelope(minX, minY, maxX, maxY);
                            }
                        }
                    }

                    // fall back to the R-Tree extent
                    string geomColumn = await ResolveGeometryColumnAsync(connection, fc.Name);
                    string rtree = GeoPackageSchema.RTreeTable(fc.Name, geomColumn);
                    if (await TableExistsAsync(connection, rtree))
                    {
                        command.CommandText =
                            $"SELECT min(minx), min(miny), max(maxx), max(maxy) FROM {DbTableName(rtree)}";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync() && !reader.IsDBNull(0))
                            {
                                return new Envelope(
                                    reader.GetDouble(0), reader.GetDouble(1),
                                    reader.GetDouble(2), reader.GetDouble(3));
                            }
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

        #region R-Tree maintenance (gView-owned, no SQL triggers)

        private bool RTreeManaged(string tableName)
            => _rtreeManaged.TryGetValue(tableName, out var managed) && managed;

        protected override async Task AfterInsertAsync(OgcSpatialFeatureclass fClass, IFeature feature,
                                                       DbConnection connection, DbTransaction transaction)
        {
            if (feature?.Shape == null || !RTreeManaged(fClass.Name))
            {
                return;
            }

            string geomColumn = _geometryColumns.TryGetValue(fClass.Name, out var gc) ? gc : fClass.ShapeFieldName;
            var env = feature.Shape.Envelope;

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "SELECT last_insert_rowid()";
                long id = Convert.ToInt64(await command.ExecuteScalarAsync());

                command.CommandText =
                    $"INSERT INTO {DbTableName(GeoPackageSchema.RTreeTable(fClass.Name, geomColumn))} " +
                    "(id, minx, maxx, miny, maxy) VALUES (@id, @minx, @maxx, @miny, @maxy)";
                AddEnvelopeParameters(command, id, env);
                await command.ExecuteNonQueryAsync();
            }
        }

        protected override async Task AfterUpdateAsync(OgcSpatialFeatureclass fClass, IFeature feature,
                                                       DbConnection connection, DbTransaction transaction)
        {
            if (feature?.Shape == null || !RTreeManaged(fClass.Name))
            {
                return;
            }

            string geomColumn = _geometryColumns.TryGetValue(fClass.Name, out var gc) ? gc : fClass.ShapeFieldName;
            var env = feature.Shape.Envelope;
            string rtree = DbTableName(GeoPackageSchema.RTreeTable(fClass.Name, geomColumn));

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText =
                    $"DELETE FROM {rtree} WHERE id = @id; " +
                    $"INSERT INTO {rtree} (id, minx, maxx, miny, maxy) VALUES (@id, @minx, @maxx, @miny, @maxy)";
                AddEnvelopeParameters(command, feature.OID, env);
                await command.ExecuteNonQueryAsync();
            }
        }

        protected override async Task BeforeDeleteAsync(OgcSpatialFeatureclass fClass, string where,
                                                        DbConnection connection, DbTransaction transaction)
        {
            if (!RTreeManaged(fClass.Name))
            {
                return;
            }

            string geomColumn = _geometryColumns.TryGetValue(fClass.Name, out var gc) ? gc : fClass.ShapeFieldName;
            string rtree = DbTableName(GeoPackageSchema.RTreeTable(fClass.Name, geomColumn));

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText =
                    $"DELETE FROM {rtree} WHERE id IN (" +
                    $"SELECT {DbColumnName(fClass.IDFieldName)} FROM {DbTableName(fClass.Name)}" +
                    (String.IsNullOrEmpty(where) ? "" : $" WHERE {where}") + ")";
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void AddEnvelopeParameters(DbCommand command, long id, IEnvelope env)
        {
            void Add(string name, object value)
            {
                var p = command.CreateParameter();
                p.ParameterName = name;
                p.Value = value;
                command.Parameters.Add(p);
            }

            Add("@id", id);
            Add("@minx", env.MinX);
            Add("@maxx", env.MaxX);
            Add("@miny", env.MinY);
            Add("@maxy", env.MaxY);
        }

        #endregion

        #region SelectCommand (GeoPackage - raw GPB blob + R-Tree MBR pre-filter)

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
                string geomColumn = DbColumnName(fc.ShapeFieldName);
                IEnvelope env = spatialFilter.Geometry.Envelope;

                where.Append($"{geomColumn} IS NOT NULL");

                if (_rtreeTables.Contains(fc.Name))
                {
                    string rtreeGeomColumn = _geometryColumns.TryGetValue(fc.Name, out var gc) ? gc : fc.ShapeFieldName;
                    string rtree = DbTableName(GeoPackageSchema.RTreeTable(fc.Name, rtreeGeomColumn));
                    where.Append(
                        $" AND {DbColumnName(fc.IDFieldName)} IN (SELECT id FROM {rtree} WHERE " +
                        $"minx <= {env.MaxX.ToString(_inv)} AND maxx >= {env.MinX.ToString(_inv)} AND " +
                        $"miny <= {env.MaxY.ToString(_inv)} AND maxy >= {env.MinY.ToString(_inv)})");
                }

                // The precise spatial relation (Intersects, Within, ...) is re-checked per row in
                // managed code by OgcSpatialFeatureCursor - there is no in-database ST_Intersects
                // without mod_spatialite.

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
                        // raw GeoPackage geometry (GPB) blob - decoded in managed code
                        // (see DecodeShape / GpkgGeometry.ToWkb)
                        fieldNames.Append($"{DbColumnName(fc.ShapeFieldName)} as temp_geometry");
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

        // A GeoPackage needs no native library - only the managed SQLite provider.
        public static bool HasUnsolvedDependenciesStatic => SQLiteFactory.Instance == null;

        #endregion

        #region IFileFeatureDatabase

        public string DatabaseName => "GeoPackage";

        public int MaxFieldNameLength => 0;

        public bool IsFolderBased => false;

        public bool Flush(IFeatureClass fc) => true;

        public override Task<int> CreateDataset(string name, ISpatialReference sRef)
            => Task.FromResult(Create(FilePathOf(name)) ? 0 : -1);

        public override async Task<bool> Open(string name)
        {
            await SetConnectionString(name);
            return await Open();
        }

        async Task<IFeatureDataset> IFeatureDatabase.GetDataset(string name)
        {
            var path = FilePathOf(name);

            if (!String.IsNullOrEmpty(path) && !File.Exists(path) && !Create(path))
            {
                return null;
            }

            var dataset = new GeoPackageDataset();
            await dataset.SetConnectionString(name);

            return await dataset.Open() ? dataset : null;
        }

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

        private static async Task<bool> TableExistsAsync(DbConnection connection, string name)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    $"SELECT count(*) FROM sqlite_master WHERE type IN ('table','view') AND lower(name) = lower('{Escape(name)}')";
                return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
            }
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

        #endregion
    }
}
