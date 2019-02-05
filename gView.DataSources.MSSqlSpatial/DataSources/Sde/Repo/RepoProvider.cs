using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.OGC.Framework.OGC.DB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo
{
    public class RepoProvider : IRepoProvider
    {
        private string _connectionString;

        public void Init(string connectionString)
        {
            _connectionString = connectionString;
            Refresh();
        }

        private void Refresh()
        {
            if (SdeLayers.Count != 0)  // DoTo: Refresh if older than 1 min...
                return;

            SdeLayers.Clear();
            SdeSpatialReferences.Clear();
            SdeColumns.Clear();
            SdeGeometryColumns.Clear();

            var providerFactory = System.Data.SqlClient.SqlClientFactory.Instance;

            using (DbConnection connection = providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();

                var command = providerFactory.CreateCommand();
                command.Connection = connection;

                command.CommandText = "select * from sde.sde_layers";
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        SdeLayers.Add(new SdeLayer(reader));
                    }
                }

                command.CommandText = "select * from sde.sde_spatial_references";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SdeSpatialReferences.Add(new SdeSpatialReference(reader));
                    }
                }

                command.CommandText = "select * from sde.sde_column_registry";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SdeColumns.Add(new SdeColumn(reader));
                    }
                }

                command.CommandText = "select * from sde.sde_geometry_columns";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SdeGeometryColumns.Add(new SdeGeometryColumn(reader));
                    }
                }
            }
        }

        private List<SdeLayer> SdeLayers = new List<SdeLayer>();
        private List<SdeSpatialReference> SdeSpatialReferences = new List<SdeSpatialReference>();
        private List<SdeColumn> SdeColumns = new List<SdeColumn>();
        private List<SdeGeometryColumn> SdeGeometryColumns = new List<SdeGeometryColumn>();

        public IEnumerable<SdeLayer> Layers => this.SdeLayers.ToArray();

        #region IRepoProvider 

        public IEnvelope FeatureClassEnveolpe(IFeatureClass fc)
        {
            this.Refresh();

            var fcName = fc.Name.ToLower();
            var sdeLayer = SdeLayers.Where(l => (l.Owner + "." + l.TableName).ToLower() == fcName).FirstOrDefault();

            if (sdeLayer == null)
                return new Envelope(-1000, -1000, 1000, 1000);

            return new Envelope(sdeLayer.MinX, sdeLayer.MinY, sdeLayer.MaxX, sdeLayer.MaxY);
        }

        public int FeatureClassSpatialReference(IFeatureClass fc)
        {
            this.Refresh();

            var fcName = fc.Name.ToLower();
            var sdeLayer = SdeLayers.Where(l => (l.Owner + "." + l.TableName).ToLower() == fcName).FirstOrDefault();

            if (sdeLayer == null)
                return 0;

            var spatialRef = SdeSpatialReferences.Where(s => s.Srid == sdeLayer.Srid).FirstOrDefault();
            if (spatialRef?.AuthSrid == null)
                return 0;

            return spatialRef.AuthSrid.Value;
        }

        public Fields FeatureClassFields(IFeatureClass fc)
        {
            this.Refresh();

            var fields = new Fields();

            var fcName = fc.Name.ToLower();
            foreach (var sdeField in SdeColumns.Where(c => (c.Owner + "." + c.TableName).ToLower() == fcName && !c.ColumnName.StartsWith("GDB_")))
            {
                var field = new Field(sdeField.ColumnName, SdeTypes.FieldType(sdeField));
                fields.Add(field);
            }

            return fields;
        }

        public geometryType FeatureClassGeometryType(IFeatureClass fc)
        {
            this.Refresh();

            var fields = new Fields();

            var fcName = fc.Name.ToLower();
            var geomColumn = SdeGeometryColumns.Where(c => (c.Owner + "." + c.TableName).ToLower() == fcName).FirstOrDefault();

            if(geomColumn!=null)
            {
                switch(geomColumn.GeometryType.HasValue ? (SdeTypes.SdeGeometryTppe)geomColumn.GeometryType.Value : SdeTypes.SdeGeometryTppe.unknown)
                {
                    case SdeTypes.SdeGeometryTppe.point:
                        return geometryType.Point;
                    case SdeTypes.SdeGeometryTppe.multipoint:
                        return geometryType.Multipoint;
                    case SdeTypes.SdeGeometryTppe.linestring:
                    case SdeTypes.SdeGeometryTppe.multilinestring:
                        return geometryType.Polyline;
                    case SdeTypes.SdeGeometryTppe.polygon:
                    case SdeTypes.SdeGeometryTppe.multipolygon:
                        return geometryType.Polygon;
                }
            }

            return geometryType.Unknown;
        }

        #endregion


    }
}
