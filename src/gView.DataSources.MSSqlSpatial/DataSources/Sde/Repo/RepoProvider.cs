using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.OGC.DB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo
{
    public class RepoProvider : IRepoProvider
    {
        private string _connectionString;
        private Version _sqlServerVersion = new Version(0, 0, 0, 0);
        private Version _sdeVersion = new Version(0, 0, 0, 0);

        async public Task<bool> Init(string connectionString)
        {
            _connectionString = connectionString;
            return await Refresh();
        }

        async private Task<bool> Refresh()
        {
            if (SdeLayers.Count != 0)  // DoTo: Refresh if older than 1 min...
            {
                return true;
            }

            SdeLayers.Clear();
            SdeSpatialReferences.Clear();
            SdeColumns.Clear();
            SdeGeometryColumns.Clear();
            SdeLayerMultiversionViewNames.Clear();

            var providerFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;

            string sdeSchemaName = TableSchemaName("sde_layers");
            if (String.IsNullOrWhiteSpace(sdeSchemaName))
            {
                throw new Exception("Can't determine sde db-schema");
            }

            await DetermineSqlServerVersion();
            await DetermineSdeVersion();

            using (DbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                await connection.OpenAsync();

                var command = providerFactory.CreateCommand();
                command.Connection = connection;


                command.CommandText = $"select * from {sdeSchemaName}.sde_table_registry";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string mv_view_name = reader["imv_view_name"]?.ToString();
                        if (!String.IsNullOrWhiteSpace(mv_view_name))
                        {
                            SdeLayerMultiversionViewNames[($"{reader["owner"]?.ToString()}.{reader["table_name"]?.ToString()}").ToLower()] = mv_view_name;
                        }
                    }
                }

                command.CommandText = $"select * from {sdeSchemaName}.sde_layers";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        SdeLayers.Add(new SdeLayer(reader, SdeLayerMultiversionViewNames));
                    }
                }

                command.CommandText = $"select * from {sdeSchemaName}.sde_spatial_references";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        SdeSpatialReferences.Add(new SdeSpatialReference(reader));
                    }
                }

                command.CommandText = $"select * from {sdeSchemaName}.sde_column_registry";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        SdeColumns.Add(new SdeColumn(reader));
                    }
                }

                command.CommandText = $"select * from {sdeSchemaName}.sde_geometry_columns";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        SdeGeometryColumns.Add(new SdeGeometryColumn(reader));
                    }
                }

                return true;
            }
        }

        private List<SdeLayer> SdeLayers = new List<SdeLayer>();
        private List<SdeSpatialReference> SdeSpatialReferences = new List<SdeSpatialReference>();
        private List<SdeColumn> SdeColumns = new List<SdeColumn>();
        private List<SdeGeometryColumn> SdeGeometryColumns = new List<SdeGeometryColumn>();
        private Dictionary<string, string> SdeLayerMultiversionViewNames = new Dictionary<string, string>();

        public Version SqlServerVersion => _sqlServerVersion;
        public Version SdeVersion => _sdeVersion;

        public IEnumerable<SdeLayer> Layers => this.SdeLayers.ToArray();

        public string TableSchemaName(string dbName)
        {
            var providerFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;

            using (DbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();

                var command = providerFactory.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select SCHEMA_NAME(schema_id) as dbschema, name from sys.tables where [name]=@dbname";
                var parameter = providerFactory.CreateParameter();
                parameter.ParameterName = "@dbName";
                parameter.Value = dbName;
                command.Parameters.Add(parameter);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["dbschema"]?.ToString();
                    }
                }
            }

            return String.Empty;
        }

        public SdeLayer LayerFromTableClass(ITableClass tableClass)
        {
            return this.Layers?.Where(l => (l.Owner + "." + l.TableName).Equals(tableClass?.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        async public Task<int> GetInsertRowId(SdeLayer sdeLayer)
        {
            var providerFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;

            string sdeSchemaName = TableSchemaName("sde_layers");
            if (String.IsNullOrWhiteSpace(sdeSchemaName))
            {
                throw new Exception("Can't determine sde db-schema");
            }

            using (DbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                await connection.OpenAsync();

                var command = providerFactory.CreateCommand();
                command.Connection = connection;

                command.CommandText =
$@"declare @newid int
exec {sdeSchemaName}.next_rowid '{sdeLayer.Owner}','{sdeLayer.TableName}',@newid OUTPUT
SELECT @newid ""Next RowID""";

                object newId = await command.ExecuteScalarAsync();
                return Convert.ToInt32(newId);
            }
        }

        async private Task DetermineSqlServerVersion()
        {
            try
            {
                var providerFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;

                using (DbConnection connection = providerFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    await connection.OpenAsync();

                    using (var command = providerFactory.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT SERVERPROPERTY('ProductVersion') AS SqlServerVersion";

                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            string versionString = reader["SqlServerVersion"].ToString();
                            _sqlServerVersion = new Version(versionString);
                        }
                        reader.Close();
                    }
                }
            }
            catch
            {
                _sqlServerVersion = new Version(0, 0, 0, 0);
            }
        }

        async private Task DetermineSdeVersion()
        {
            try
            {
                var providerFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;

                using (DbConnection connection = providerFactory.CreateConnection())
                {
                    connection.ConnectionString = _connectionString;
                    await connection.OpenAsync();

                    using (var command = providerFactory.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT CAST([MAJOR] AS VARCHAR) + '.' + CAST([MINOR] AS VARCHAR) + '.' + CAST([BUGFIX] AS VARCHAR) AS SDE_Version FROM sde.sde_version;";

                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            string versionString = reader["SDE_Version"].ToString();
                            _sdeVersion = new Version(versionString);
                        }
                        reader.Close();
                    }
                }
            }
            catch
            {
                _sdeVersion = new Version(0, 0, 0, 0);
            }
        }

        #region IRepoProvider 

        async public Task<IEnvelope> FeatureClassEnveolpe(IFeatureClass fc)
        {
            await this.Refresh();

            var fcName = fc.Name.ToLower();
            var sdeLayer = SdeLayers.Where(l => $"{l.Owner}.{l.TableName}".ToLower() == fcName).FirstOrDefault();

            if (sdeLayer == null)
            {
                return new Envelope(-1000, -1000, 1000, 1000);
            }

            return new Envelope(sdeLayer.MinX, sdeLayer.MinY, sdeLayer.MaxX, sdeLayer.MaxY);
        }

        async public Task<int> FeatureClassSpatialReference(IFeatureClass fc)
        {
            await this.Refresh();

            var fcName = fc.Name.ToLower();
            var sdeLayer = SdeLayers.Where(l => $"{l.Owner}.{l.TableName}".ToLower() == fcName).FirstOrDefault();

            if (sdeLayer == null)
            {
                return 0;
            }

            var spatialRef = SdeSpatialReferences.Where(s => s.Srid == sdeLayer.Srid).FirstOrDefault();
            if (spatialRef?.AuthSrid == null)
            {
                return 0;
            }

            return spatialRef.AuthSrid.Value;
        }

        async public Task<FieldCollection> FeatureClassFields(IFeatureClass fc)
        {
            await this.Refresh();

            var fields = new FieldCollection();

            var fcName = fc.Name.ToLower();
            foreach (var sdeField in SdeColumns.Where(c => $"{c.Owner}.{c.TableName}".ToLower() == fcName && !c.ColumnName.StartsWith("GDB_")))
            {
                var field = new Field(sdeField.ColumnName, SdeTypes.FieldType(sdeField));
                fields.Add(field);
            }

            return fields;
        }

        async public Task<GeometryType> FeatureClassGeometryType(IFeatureClass fc)
        {
            await this.Refresh();

            var fields = new FieldCollection();

            var fcName = fc.Name.ToLower();
            var geomColumn = SdeGeometryColumns.Where(c => $"{c.Owner}.{c.TableName}".ToLower() == fcName).FirstOrDefault();

            if (geomColumn != null)
            {
                switch (geomColumn.GeometryType.HasValue ? (SdeTypes.SdeGeometryTppe)geomColumn.GeometryType.Value : SdeTypes.SdeGeometryTppe.unknown)
                {
                    case SdeTypes.SdeGeometryTppe.point:
                        return GeometryType.Point;
                    case SdeTypes.SdeGeometryTppe.multipoint:
                        return GeometryType.Multipoint;
                    case SdeTypes.SdeGeometryTppe.linestring:
                    case SdeTypes.SdeGeometryTppe.multilinestring:
                        return GeometryType.Polyline;
                    case SdeTypes.SdeGeometryTppe.polygon:
                    case SdeTypes.SdeGeometryTppe.multipolygon:
                        return GeometryType.Polygon;
                }
            }

            return GeometryType.Unknown;
        }

        #endregion
    }
}
