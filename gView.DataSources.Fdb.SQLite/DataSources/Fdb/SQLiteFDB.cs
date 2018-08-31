using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.Offline;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.DataSources.Fdb.SQLite
{
    public class SQLiteFDB : gView.DataSources.Fdb.MSAccess.AccessFDB
    {
        private string _filename;

        internal string FileName { get { return _filename; } }

        #region IFDB

        override public bool Create(string name)
        {
            try
            {
                FileInfo fi = new FileInfo(name);
                if (fi.Exists)
                    return false;

                using (SQLiteConnection connection = new SQLiteConnection())
                using (StreamReader reader = new StreamReader(SystemVariables.StartupDirectory + @"\sql\SQLiteFDB\createdatabase.sql"))
                {
                    connection.ConnectionString = "Data Source=" + name;
                    connection.Open();

                    string line = "";
                    StringBuilder sql = new StringBuilder();
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Trim().ToLower() == "go")
                        {
                            using (SQLiteCommand command = new SQLiteCommand(connection))
                            {
                                command.CommandText = sql.ToString();
                                command.ExecuteNonQuery();
                            }

                            sql = new StringBuilder();
                        }
                        else
                        {
                            sql.Append(line);
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
            return true;
        }
        override public bool Open(string connectionString)
        {
            _filename = parseConnectionString(connectionString);
            if (String.IsNullOrEmpty(_filename))
                _filename = connectionString;

            if (!connectionString.Contains("="))
                connectionString = "Data Source=" + _filename;

            _conn = new SQLiteConn(connectionString);

            SetVersion();
            return true;
        }
        override public void Dispose()
        {
            //if (_conn == null) return;
            //_conn.Dispose();
            //_conn = null;

            if (LinkedDatasetCacheInstance != null)
                LinkedDatasetCacheInstance.Dispose();
        }

        override public List<IDatasetElement> DatasetLayers(IDataset dataset)
        {
            _errMsg = "";
            if (_conn == null) return null;

            int dsID = this.DatasetID(dataset.DatasetName);
            if (dsID == -1) return null;

            DataSet ds = new DataSet();
            if (!_conn.SQLQuery(ref ds, "SELECT * FROM FDB_FeatureClasses WHERE DatasetID=" + dsID, "FC"))
            {
                _errMsg = _conn.errorMessage;
                return null;
            }

            List<IDatasetElement> layers = new List<IDatasetElement>();
            ISpatialReference sRef = SpatialReference(dataset.DatasetName);

            string imageSpace;
            if (IsImageDataset(dataset.DatasetName, out imageSpace))
            {
                if (TableExists("FC_" + dataset.DatasetName + "_IMAGE_POLYGONS"))
                {
                    IFeatureClass fc = new SQLiteFDBFeatureClass(this, dataset, new GeometryDef(geometryType.Polygon, sRef, false));
                    ((SQLiteFDBFeatureClass)fc).Name = dataset.DatasetName + "_IMAGE_POLYGONS";
                    ((SQLiteFDBFeatureClass)fc).Envelope = this.FeatureClassExtent(fc.Name);
                    ((SQLiteFDBFeatureClass)fc).IDFieldName = "FDB_OID";
                    ((SQLiteFDBFeatureClass)fc).ShapeFieldName = "FDB_SHAPE";

                    var fields = this.FeatureClassFields(dataset.DatasetName, fc.Name);
                    if (fields != null)
                    {
                        foreach (IField field in fields)
                        {
                            ((Fields)fc.Fields).Add(field);
                        }
                    }

                    IClass iClass = null;
                    if (imageSpace.ToLower() == "dataset") // gibts eigentlich noch gar nicht...
                    {
                        throw new NotImplementedException("Rasterdatasets are not implemented in this software version!");
                        //iClass = new SqlFDBImageDatasetClass(dataset as IRasterDataset, this, fc, sRef, imageSpace);
                        //((SqlFDBImageDatasetClass)iClass).SpatialReference = sRef;
                    }
                    else  // RasterCatalog
                    {
                        iClass = new SQLiteFDBImageCatalogClass(dataset as IRasterDataset, this, fc, sRef, imageSpace);
                        ((SQLiteFDBImageCatalogClass)iClass).SpatialReference = sRef;
                    }

                    layers.Add(new DatasetElement(iClass));
                }
            }

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                DataRow fcRow = row;

                if (IsLinkedFeatureClass(row))
                {
                    IDataset linkedDs = LinkedDataset(LinkedDatasetCacheInstance, LinkedDatasetId(row));
                    if (linkedDs == null)
                        continue;
                    IDatasetElement linkedElement = linkedDs[(string)row["Name"]];

                    LinkedFeatureClass fc = new LinkedFeatureClass(dataset,
                        linkedElement != null && linkedElement.Class is IFeatureClass ? linkedElement.Class as IFeatureClass : null,
                        (string)row["Name"]);

                    SQLiteFDBDatasetElement linkedLayer = new SQLiteFDBDatasetElement(fc);
                    layers.Add(linkedLayer);

                    continue;
                }

                if (row["Name"].ToString().Contains("@"))  // Geographic View
                {
                    string[] viewNames = row["Name"].ToString().Split('@');
                    if (viewNames.Length != 2)
                        continue;
                    DataRow[] fcRows = ds.Tables[0].Select("Name='" + viewNames[0] + "'");
                    if (fcRows == null || fcRows.Length != 1)
                        continue;
                    fcRow = fcRows[0];
                }

                if (!TableExists("FC_" + fcRow["Name"].ToString()))
                    continue;
                //if (_seVersion != 0)
                //{
                //    _conn.ExecuteNoneQuery("execute LoadIndex '" + row["Name"].ToString() + "'");
                //}
                GeometryDef geomDef;
                if (fcRow.Table.Columns["HasZ"] != null)
                {
                    geomDef = new GeometryDef((geometryType)fcRow["GeometryType"], null, (bool)fcRow["HasZ"]);
                }
                else
                {  // alte Version war immer 3D
                    geomDef = new GeometryDef((geometryType)fcRow["GeometryType"], null, true);
                }

                SQLiteFDBDatasetElement layer = new SQLiteFDBDatasetElement(this, dataset, row["Name"].ToString(), geomDef);
                if (layer.Class is SQLiteFDBFeatureClass)
                {
                    ((SQLiteFDBFeatureClass)layer.Class).Envelope = this.FeatureClassExtent(layer.Class.Name);
                    ((SQLiteFDBFeatureClass)layer.Class).IDFieldName = "FDB_OID";
                    ((SQLiteFDBFeatureClass)layer.Class).ShapeFieldName = "FDB_SHAPE";
                    if (sRef != null)
                    {
                        ((SQLiteFDBFeatureClass)layer.Class).SpatialReference = (ISpatialReference)(new SpatialReference((SpatialReference)sRef));
                    }
                }
                var fields = this.FeatureClassFields(dataset.DatasetName, layer.Class.Name);
                if (fields != null && layer.Class is ITableClass)
                {
                    foreach (IField field in fields)
                    {
                        ((Fields)((ITableClass)layer.Class).Fields).Add(field);
                    }
                }
                layers.Add(layer);
            }

            return layers;
        }

        #endregion

        #region IFeatureUpdater Member

        /*
		virtual public bool Insert(IFeatureClass fClass, List<IFeature> features)
		{
            if (fClass == null || features == null || features.Count == 0) return false;

			try 
			{
				if(_conn==null) return false;

				DataSet ds=new DataSet();
				if(!_conn.SQLQuery(ref ds,"SELECT * FROM FC_"+fClass.Name+" WHERE [FDB_OID]=-1","FC_"+fClass.Name,true)) 
				{
					_errMsg=_conn.errorMessage;
					return false;
				}
				
				foreach(IFeature feature in features) 
				{
					DataRow row=ds.Tables[0].NewRow();
					if(feature.Shape!=null) 
					{
						BinaryWriter writer=new BinaryWriter(new MemoryStream());
						feature.Shape.Serialize(writer,fClass);

						byte [] geometry=new byte[writer.BaseStream.Length];
						writer.BaseStream.Position=0;
						writer.BaseStream.Read(geometry,(int)0,(int)writer.BaseStream.Length);
						writer.Close();

						row["FDB_SHAPE"]=geometry;
						row["FDB_NID"]=0;

						foreach(IFieldValue fv in feature.Fields) 
						{
                            if (fv.Name == "FDB_NID" || fv.Name == "FDB_SHAPE") continue;
                            string name = fv.Name.Replace("$", "");
							if(row.Table.Columns[name]==null) continue;
                            
							try 
							{
								if( row.Table.Columns[name].DataType!=typeof(string) &&
									fv.Value.GetType()==typeof(string) )
									fv.Value=fv.Value.ToString().Replace(".",",");

								row[name]=fv.Value;
							} 
							catch 
							{
								
							}
						}
					}
					ds.Tables[0].Rows.Add(row);
				}
				if(!_conn.UpdateData(ref ds,"FC_"+fClass.Name)) 
				{
					_errMsg=_conn.errorMessage;
					return false;
				}
				
				return true;
			} 
			catch(Exception ex)
			{
				_errMsg=ex.Message;
				return false;
			}	
		}

        virtual public bool Insert(IFeatureClass fClass, IFeature feature)
		{
            if (fClass == null || feature == null) return false;

			try 
			{
				if(_conn==null) return false;

				DataSet ds=new DataSet();
				if(!_conn.SQLQuery(ref ds,"SELECT * FROM FC_"+fClass.Name+" WHERE [FDB_OID]=-1","FC_"+fClass.Name,true)) 
				{
					_errMsg=_conn.errorMessage;
					return false;
				}

				DataRow row=ds.Tables[0].NewRow();
				if(feature.Shape!=null) 
				{
					BinaryWriter writer=new BinaryWriter(new MemoryStream());
					feature.Shape.Serialize(writer,fClass);

					byte [] geometry=new byte[writer.BaseStream.Length];
					writer.BaseStream.Position=0;
					writer.BaseStream.Read(geometry,(int)0,(int)writer.BaseStream.Length);
					writer.Close();

					row["FDB_SHAPE"]=geometry;
					row["FDB_NID"]=0;

					foreach(IFieldValue fv in feature.Fields) 
					{
						if(row.Table.Columns[fv.Name]==null) continue;
						try 
						{
							row[fv.Name]=fv.Value;
						} 
						catch 
						{
						}
					}
				}
		
				ds.Tables[0].Rows.Add(row);
				if(!_conn.UpdateData(ref ds,"FC_"+fClass.Name)) 
				{
					_errMsg=_conn.errorMessage;
					return false;
				}
				return true;
			} 
			catch(Exception ex)
			{
				_errMsg=ex.Message;
				return false;
			}
		}
        */

        public override bool Insert(IFeatureClass fClass, IFeature feature)
        {
            List<IFeature> features = new List<IFeature>();
            features.Add(feature);
            return Insert(fClass, features);
        }
        public override bool Insert(IFeatureClass fClass, List<IFeature> features)
        {
            if (fClass == null || features == null) return false;
            if (features.Count == 0) return true;

            BinarySearchTree2 tree = null;
            CheckSpatialSearchTreeVersion(fClass.Name);
            if (_spatialSearchTrees[fClass.Name] == null)
            {
                _spatialSearchTrees[fClass.Name] = this.SpatialSearchTree(fClass.Name);
            }
            tree = _spatialSearchTrees[fClass.Name] as BinarySearchTree2;

            string replicationField = null;
            if (!Replication.AllowFeatureClassEditing(fClass, out replicationField))
            {
                _errMsg = "Replication Error: can't edit checked out and released featureclass...";
                return false;
            }
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                {
                    connection.Open();

                    using(var command=new SQLiteCommand(connection))
                    using (var transaction = connection.BeginTransaction())
                    {
                        ReplicationTransaction replTrans = new ReplicationTransaction(connection, transaction);

                        foreach (IFeature feature in features)
                        {
                            if (!feature.BeforeInsert(fClass))
                            {
                                _errMsg = "Insert: Error in Feature.BeforeInsert (AutoFields,...)";
                                return false;
                            }
                            if (!String.IsNullOrEmpty(replicationField))
                            {
                                Replication.AllocateNewObjectGuid(feature, replicationField);
                                if (!Replication.WriteDifferencesToTable(fClass,
                                    feature[replicationField] is Guid ? (Guid)feature[replicationField] : new Guid(feature[replicationField].ToString()),
                                    Replication.SqlStatement.INSERT, replTrans, out _errMsg))
                                {
                                    _errMsg = "Replication Error: " + _errMsg;
                                    return false;
                                }
                            }

                            StringBuilder fields = new StringBuilder(), parameters = new StringBuilder();
                            command.Parameters.Clear();
                            if (feature.Shape != null)
                            {
                                GeometryDef.VerifyGeometryType(feature.Shape, fClass);

                                BinaryWriter writer = new BinaryWriter(new MemoryStream());
                                feature.Shape.Serialize(writer, fClass);

                                byte[] geometry = new byte[writer.BaseStream.Length];
                                writer.BaseStream.Position = 0;
                                writer.BaseStream.Read(geometry, (int)0, (int)writer.BaseStream.Length);
                                writer.Close();

                                SQLiteParameter parameter = new SQLiteParameter("@FDB_SHAPE", geometry);
                                fields.Append("[FDB_SHAPE]");
                                parameters.Append("@FDB_SHAPE");
                                command.Parameters.Add(parameter);
                            }

                            bool hasNID = false;
                            if (fClass.Fields != null)
                            {
                                foreach (IFieldValue fv in feature.Fields)
                                {
                                    if (fv.Name == "FDB_NID" || fv.Name == "FDB_SHAPE" || fv.Name == "FDB_OID") continue;
                                    string name = fv.Name.Replace("$", "");

                                    IField field = fClass.FindField(name);
                                    if (name == "FDB_NID")
                                    {
                                        hasNID = true;
                                    }
                                    else if (field == null) continue;

                                    if (fields.Length != 0) fields.Append(",");
                                    if (parameters.Length != 0) parameters.Append(",");

                                    SQLiteParameter parameter;
                                    parameter = new SQLiteParameter("@" + name, fv.Value);
                                    ModifyDbParameter(parameter);

                                    fields.Append("[" + name + "]");
                                    parameters.Append("@" + name);
                                    command.Parameters.Add(parameter);
                                }
                            }

                            if (!hasNID)
                            {
                                long NID = 0;
                                if (tree != null && feature.Shape != null)
                                    NID = tree.InsertSINode(feature.Shape.Envelope);

                                if (fields.Length != 0) fields.Append(",");
                                if (parameters.Length != 0) parameters.Append(",");

                                SQLiteParameter parameter = new SQLiteParameter("@FDB_NID", NID);
                                fields.Append("[FDB_NID]");
                                parameters.Append("@FDB_NID");
                                command.Parameters.Add(parameter);

                                //if (!_nids.Contains(NID)) _nids.Add(NID);
                            }

                            command.CommandText = "INSERT INTO " + FcTableName(fClass) + " (" + fields.ToString() + ") VALUES (" + parameters + ")";
                            command.ExecuteNonQuery();
                        }

                        //return SplitIndexNodes(fClass, connection, _nids);

                        transaction.Commit();
                    }
                }

                AddTreeNodes();
            }
            catch (Exception ex)
            {
                _errMsg = "Insert: " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                return false;
            }

            return true;
        }

        /*
        virtual public bool Update(IFeatureClass fClass, IFeature feature)
        {
            if (fClass==null || feature == null) return false;

            QueryFilter filter = new QueryFilter();
            foreach (FieldValue fv in feature.Fields)
            {
                filter.AddField(fv.Name);
            }
            filter.AddField("FDB_OID");
            if(feature.Shape!=null) filter.AddField("FDB_SHAPE");
            filter.AddField("FDB_NID");
            filter.fieldPrefix = "[";
            filter.fieldPostfix = "]";

            DataTable tab = _conn.Select(filter.SubFields.Replace(" ",","), "FC_" + fClass.Name, "FDB_OID=" + feature.OID, "", true);
            if (tab == null)
            {
                _errMsg = _conn.errorMessage;
                return false;
            }
            if (tab.Rows.Count != 1)
            {
                _errMsg = "Can't find feature with OID=" + feature.OID;
                return false;
            }

            foreach (FieldValue fv in feature.Fields)
            {
                try
                {
                    tab.Rows[0][fv.Name] = fv.Value;
                }
                catch(Exception ex)
                {
                    _errMsg = "Field " + fv.Name + ": " + ex.Message;
                    return false;
                }
            }

            if (feature.Shape != null)
            {
                BinaryWriter writer = new BinaryWriter(new MemoryStream());
                feature.Shape.Serialize(writer,fClass);

                byte[] geometry = new byte[writer.BaseStream.Length];
                writer.BaseStream.Position = 0;
                writer.BaseStream.Read(geometry, (int)0, (int)writer.BaseStream.Length);
                writer.Close();

                tab.Rows[0]["FDB_SHAPE"] = geometry;
                tab.Rows[0]["FDB_NID"] = 0;
            }

            if (!_conn.Update(tab))
            {
                _errMsg = _conn.errorMessage;
                return false;
            }

            return true;
        }

        virtual public bool Update(IFeatureClass fClass, List<IFeature> features)
        {
            foreach (IFeature feature in features)
            {
                if (!Update(fClass, feature))
                {
                    return false;
                }
            }
            return true;
        }
        */

        public override bool Update(IFeatureClass fClass, IFeature feature)
        {
            List<IFeature> features = new List<IFeature>();
            features.Add(feature);
            return Update(fClass, features);
        }
        public override bool Update(IFeatureClass fClass, List<IFeature> features)
        {
            if (fClass == null || features == null) return false;
            if (features.Count == 0) return true;

            //int counter = 0;
            BinarySearchTree2 tree = null;
            CheckSpatialSearchTreeVersion(fClass.Name);
            if (_spatialSearchTrees[fClass.Name] == null)
            {
                _spatialSearchTrees[fClass.Name] = this.SpatialSearchTree(fClass.Name);
            }
            tree = _spatialSearchTrees[fClass.Name] as BinarySearchTree2;

            string replicationField = null;
            if (!Replication.AllowFeatureClassEditing(fClass, out replicationField))
            {
                _errMsg = "Replication Error: can't edit checked out and released featureclass...";
                return false;
            }
            try
            {
                //List<long> _nids = new List<long>();

                using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    using (var transaction = connection.BeginTransaction())
                    {
                        ReplicationTransaction replTrans = new ReplicationTransaction(connection, transaction);

                        foreach (IFeature feature in features)
                        {
                            if (!feature.BeforeUpdate(fClass))
                            {
                                _errMsg = "Insert: Error in Feature.BeforeInsert (AutoFields,...)";
                                return false;
                            }
                            if (!String.IsNullOrEmpty(replicationField))
                            {
                                if (!Replication.WriteDifferencesToTable(fClass,
                                    Replication.FeatureObjectGuid(fClass, feature, replicationField),
                                    Replication.SqlStatement.UPDATE, replTrans, out _errMsg))
                                {
                                    _errMsg = "Replication Error: " + _errMsg;
                                    return false;
                                }
                            }

                            long NID = 0;

                            command.Parameters.Clear();
                            StringBuilder commandText = new StringBuilder();

                            if (feature == null) continue;

                            if (feature.OID < 0)
                            {
                                _errMsg = "Can't update feature with OID=" + feature.OID;
                                return false;
                            }

                            StringBuilder fields = new StringBuilder();
                            command.Parameters.Clear();
                            if (feature.Shape != null)
                            {
                                GeometryDef.VerifyGeometryType(feature.Shape, fClass);

                                BinaryWriter writer = new BinaryWriter(new MemoryStream());
                                feature.Shape.Serialize(writer, fClass);

                                byte[] geometry = new byte[writer.BaseStream.Length];
                                writer.BaseStream.Position = 0;
                                writer.BaseStream.Read(geometry, (int)0, (int)writer.BaseStream.Length);
                                writer.Close();

                                SQLiteParameter parameter = new SQLiteParameter("@FDB_SHAPE", geometry);
                                fields.Append("[FDB_SHAPE]=@FDB_SHAPE");
                                command.Parameters.Add(parameter);
                            }

                            if (fClass.Fields != null)
                            {
                                foreach (IFieldValue fv in feature.Fields)
                                {
                                    if (fv.Name == "FDB_NID" || fv.Name == "FDB_OID" || fv.Name == "FDB_SHAPE") continue;
                                    if (fv.Name == "$FDB_NID")
                                    {
                                        long.TryParse(fv.Value.ToString(), out NID);
                                        continue;
                                    }
                                    string name = fv.Name;
                                    if (fClass.FindField(name) == null) continue;

                                    if (fields.Length != 0) fields.Append(",");

                                    SQLiteParameter parameter = new SQLiteParameter("@" + name, fv.Value);
                                    ModifyDbParameter(parameter);

                                    fields.Append("[" + name + "]=@" + name);
                                    command.Parameters.Add(parameter);
                                }
                            }

                            // Wenn Shape upgedatet wird, auch neuen TreeNode berechnen
                            if (feature.Shape != null)
                            {
                                if (tree != null)
                                {
                                    if (feature.Shape != null)
                                    {
                                        NID = (NID != 0) ? tree.UpdadeSINode(feature.Shape.Envelope, NID) : tree.InsertSINode(feature.Shape.Envelope);
                                    }
                                    else
                                    {
                                        NID = 0;
                                    }

                                    if (fields.Length != 0) fields.Append(",");

                                    SQLiteParameter parameterNID = new SQLiteParameter("@FDB_NID", NID);
                                    fields.Append("[FDB_NID]=@FDB_NID");
                                    command.Parameters.Add(parameterNID);

                                    //if (!_nids.Contains(NID)) _nids.Add(NID);
                                }
                            }

                            commandText.Append("UPDATE " + FcTableName(fClass) + " SET " + fields.ToString() + " WHERE FDB_OID=" + feature.OID);
                            command.CommandText = commandText.ToString();
                            command.ExecuteNonQuery();
                        }

                        //return SplitIndexNodes(fClass, connection, _nids);
                        transaction.Commit();
                    }
                }

                AddTreeNodes();
            }

            catch (Exception ex)
            {
                _errMsg = "Update: " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                return false;
            }

            return true;
        }

        public override bool Delete(IFeatureClass fClass, int oid)
        {
            return Delete(fClass, "FDB_OID=" + oid.ToString());
        }
        public override bool Delete(IFeatureClass fClass, string where)
        {
            if (fClass == null) return false;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                {
                    connection.Open();

                    string sql = "DELETE FROM " + FcTableName(fClass) + ((where != String.Empty) ? " WHERE " + where : "");
                    using(SQLiteCommand command = new SQLiteCommand(sql, connection))
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        ReplicationTransaction replTrans = new ReplicationTransaction(connection, transaction);
                        command.Transaction = transaction;

                        string replicationField = null;
                        if (!Replication.AllowFeatureClassEditing(fClass, out replicationField))
                        {
                            _errMsg = "Replication Error: can't edit checked out and released featureclass...";
                            return false;
                        }
                        if (!String.IsNullOrEmpty(replicationField))
                        {
                            DataTable tab = _conn.Select(replicationField, FcTableName(fClass), ((where != String.Empty) ? where : ""));
                            if (tab == null)
                            {
                                _errMsg = "Replication Error: " + _conn.errorMessage;
                                return false;
                            }

                            foreach (DataRow row in tab.Rows)
                            {
                                if (!Replication.WriteDifferencesToTable(fClass,
                                    row[replicationField] is Guid ? (Guid)row[replicationField] : new Guid(row[replicationField].ToString()),
                                    Replication.SqlStatement.DELETE,
                                    replTrans,
                                    out _errMsg))
                                {
                                    _errMsg = "Replication Error: " + _errMsg;
                                    return false;
                                }
                            }
                        }

                        command.ExecuteNonQuery();
                        transaction.Commit();

                        connection.Close();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public override int SuggestedInsertFeatureCountPerTransaction
        {
            get
            {
                return 1000;
            }
        }

        private Dictionary<string, List<long>> _addTreeNodes = new Dictionary<string, List<long>>();
        private object thisLock = new object();
        protected override void AddTreeNode(string fcName, long nid)
        {
            lock (thisLock)
            {
                if (!_addTreeNodes.ContainsKey(fcName))
                {
                    _addTreeNodes.Add(fcName, new List<long>());
                }
                if (!_addTreeNodes[fcName].Contains(nid))
                    _addTreeNodes[fcName].Add(nid);
            }
        }
        private void AddTreeNodes()
        {
            lock (thisLock)
            {
                foreach (string fcName in _addTreeNodes.Keys)
                {
                    foreach (long nid in _addTreeNodes[fcName])
                    {
                        base.AddTreeNode(fcName, nid);
                    }
                }
                _addTreeNodes.Clear();
            }
        }

        #endregion

        #region Internals

        internal IDatasetElement DatasetElement(SQLiteFDBDataset dataset, string elementName)
        {
            ISpatialReference sRef = this.SpatialReference(dataset.DatasetName);

            if (dataset.DatasetName == elementName)
            {
                string imageSpace;
                if (IsImageDataset(dataset.DatasetName, out imageSpace))
                {
                    IDatasetElement fLayer = DatasetElement(dataset, elementName + "_IMAGE_POLYGONS") as IDatasetElement;
                    if (fLayer != null && fLayer.Class is IFeatureClass)
                    {
                        SQLiteFDBImageCatalogClass iClass = new SQLiteFDBImageCatalogClass(dataset, this, fLayer.Class as IFeatureClass, sRef, imageSpace);
                        iClass.SpatialReference = sRef;
                        return new DatasetElement(iClass);
                    }
                }
            }

            DataTable tab = _conn.Select("*", TableName("FDB_FeatureClasses"), DbColName("DatasetID") + "=" + dataset._dsID + " AND " + DbColName("Name") + "='" + elementName + "'");
            if (tab == null || tab.Rows == null)
            {
                _errMsg = _conn.errorMessage;
                return null;
            }
            else if (tab.Rows.Count == 0)
            {
                _errMsg = "Can't find dataset element '" + elementName + "'...";
                return null;
            }

            //if (_seVersion != 0)
            //{
            //    _conn.ExecuteNoneQuery("execute LoadIndex '" + elementName + "'");
            //}
            DataRow row = tab.Rows[0];
            DataRow fcRow = row;

            if (IsLinkedFeatureClass(row))
            {
                IDataset linkedDs = LinkedDataset(LinkedDatasetCacheInstance, LinkedDatasetId(row));
                if (linkedDs == null)
                    return null;
                IDatasetElement linkedElement = linkedDs[(string)row["Name"]];

                LinkedFeatureClass fc = new LinkedFeatureClass(dataset,
                    linkedElement != null && linkedElement.Class is IFeatureClass ? linkedElement.Class as IFeatureClass : null,
                    (string)row["Name"]);

                SQLiteFDBDatasetElement linkedLayer = new SQLiteFDBDatasetElement(fc);
                return linkedLayer;
            }

            if (row["Name"].ToString().Contains("@"))  // Geographic View
            {
                string[] viewNames = row["Name"].ToString().Split('@');
                if (viewNames.Length != 2)
                    return null;
                DataTable tab2 = _conn.Select("*", TableName("FDB_FeatureClasses"), DbColName("DatasetID") + "=" + dataset._dsID + " AND " + DbColName("Name") + "='" + viewNames[0] + "'");
                if (tab2 == null || tab2.Rows.Count != 1)
                    return null;
                fcRow = tab2.Rows[0];
            }

            GeometryDef geomDef;
            if (fcRow.Table.Columns["hasz"] != null)
            {
                geomDef = new GeometryDef((geometryType)fcRow["geometrytype"], null, (bool)fcRow["hasz"]);
            }
            else
            {  // alte Version war immer 3D
                geomDef = new GeometryDef((geometryType)fcRow["geometrytype"], null, true);
            }

            DatasetElement layer = new SQLiteFDBDatasetElement(this, dataset, row["name"].ToString(), geomDef);
            if (layer.Class is SQLiteFDBFeatureClass) // kann auch SqlFDBNetworkClass sein
            {
                ((SQLiteFDBFeatureClass)layer.Class).Envelope = this.FeatureClassExtent(layer.Class.Name);
                ((SQLiteFDBFeatureClass)layer.Class).IDFieldName = "FDB_OID";
                ((SQLiteFDBFeatureClass)layer.Class).ShapeFieldName = "FDB_SHAPE";
                //((SqlFDBFeatureClass)layer.FeatureClass).SetSpatialTreeInfo(this.SpatialTreeInfo(row["Name"].ToString()));
                ((SQLiteFDBFeatureClass)layer.Class).SpatialReference = sRef;
            }
            var fields = this.FeatureClassFields(dataset._dsID, layer.Class.Name);
            if (fields != null && layer.Class is ITableClass)
            {
                foreach (IField field in fields)
                {
                    ((Fields)((ITableClass)layer.Class).Fields).Add(field);
                }
            }

            return layer;
        }

        internal DataTable Select(string fields, string from, string where)
        {
            if (_conn == null) return null;
            return _conn.Select(fields, from, where);
        }

        #endregion

        public override IFeatureCursor Query(IFeatureClass fc, IQueryFilter filter)
        {
            if (_conn == null) return null;

            string subfields = "";
            if (filter != null)
            {
                filter.fieldPrefix = "[";
                filter.fieldPostfix = "]";
                if (filter is ISpatialFilter)
                {
                    if (((ISpatialFilter)filter).SpatialRelation != spatialRelation.SpatialRelationEnvelopeIntersects) filter.AddField("FDB_SHAPE");
                }
                //subfields = filter.SubFields.Replace(" ", ",");
                subfields = filter.SubFieldsAndAlias;
            }
            if (subfields == "") subfields = "*";

            List<long> NIDs = null;
            //IGeometry queryGeometry = null;
            ISpatialFilter sFilter = null;

            if (filter is ISpatialFilter)
            {
                sFilter = new SpatialFilter();
                sFilter.SpatialRelation = ((ISpatialFilter)filter).SpatialRelation;

                ISpatialReference filterSRef = ((ISpatialFilter)filter).FilterSpatialReference;
                if (filterSRef != null && !filterSRef.Equals(fc.SpatialReference))
                {
                    sFilter.Geometry = GeometricTransformer.Transform2D(((ISpatialFilter)filter).Geometry, filterSRef, fc.SpatialReference);
                    if (sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
                    {
                        sFilter.Geometry = sFilter.Geometry.Envelope;
                    }
                }
                else
                {
                    sFilter.Geometry = ((ISpatialFilter)filter).Geometry;
                }
                CheckSpatialSearchTreeVersion(fc.Name);
                if (_spatialSearchTrees[OriginFcName(fc.Name)] == null)
                {
                    _spatialSearchTrees[OriginFcName(fc.Name)] = this.SpatialSearchTree(fc.Name);
                }
                ISearchTree tree = (ISearchTree)_spatialSearchTrees[OriginFcName(fc.Name)];
                if (tree != null && ((ISpatialFilter)filter).Geometry != null)
                {
                   if (sFilter.SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects &&
                                sFilter.Geometry is IEnvelope)
                        NIDs = tree.CollectNIDsPlus((IEnvelope)sFilter.Geometry);
                    else
                        NIDs = tree.CollectNIDs(sFilter.Geometry);
                }

                if (((ISpatialFilter)filter).SpatialRelation == spatialRelation.SpatialRelationMapEnvelopeIntersects)
                {
                    sFilter = null;
                }
            }

            string sql = "SELECT " + subfields + " FROM " + FcTableName(fc);
            string where = "";
            if (filter != null)
            {
                if (filter.WhereClause != "")
                {
                    where = filter.WhereClause;
                }
            }
            return new SQLiteFDBFeatureCursor(_conn.ConnectionString, sql, where, filter.OrderBy, NIDs, sFilter, fc,
                ((filter != null) ? filter.FeatureSpatialReference : null));
        }

        public override IFeatureCursor QueryIDs(IFeatureClass fc, string subFields, List<int> IDs, ISpatialReference toSRef)
        {
            string sql = "SELECT " + subFields + " FROM " + FcTableName(fc);
            return new SQLiteFDBFeatureCursorIDs(_conn.ConnectionString, sql, IDs, fc, toSRef);
        }

        private string parseConnectionString(string connString)
        {
            string filename = connString;
            foreach (string p in connString.Split(';'))
            {
                if (p.IndexOf("dsname=") == 0)
                {
                    _dsname = gView.Framework.IO.ConfigTextStream.ExtractValue(connString, "dsname");
                    continue;
                }
                if (p.IndexOf("layers=") == 0)
                {
                    continue;
                }
                if (p.IndexOf("Data Source=") == 0)
                {
                    filename = gView.Framework.IO.ConfigTextStream.ExtractValue(connString, "Data Source");
                }
            }
            return filename;
        }

        public override IFeatureClass GetFeatureclass(string fcName)
        {
            if (_conn == null) return null;

            int fcID = this.GetFeatureClassID(fcName);

            DataTable featureclasses = _conn.Select(DbColName("DatasetID"), TableName("FDB_FeatureClasses"), DbColName("ID") + "=" + fcID);
            if (featureclasses == null || featureclasses.Rows.Count != 1)
                return null;

            DataTable datasets = _conn.Select(DbColName("Name"), TableName("FDB_Datasets"), DbColName("ID") + "=" + featureclasses.Rows[0]["DatasetID"].ToString());
            if (datasets == null || datasets.Rows.Count != 1)
                return null;

            return GetFeatureclass(datasets.Rows[0]["Name"].ToString(), fcName);
        }

        public override IFeatureClass GetFeatureclass(string dsName, string fcName)
        {
            SQLiteFDBDataset dataset = new SQLiteFDBDataset();

            string connString = "Data Source=" + _filename;
            if (!connString.Contains(";dsname=" + dsName))
                connString += ";dsname=" + dsName;

            dataset.ConnectionString = connString;
            dataset.Open();

            IDatasetElement element = dataset[fcName];
            if (element != null && element.Class is IFeatureClass)
            {
                return element.Class as IFeatureClass;
            }
            else
            {
                dataset.Dispose();
                return null;
            }
        }

        protected override bool TableExists(string tableName)
        {
            if (_conn == null) return false;

            using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
            {
                connection.Open();
                DataTable tables = connection.GetSchema("Tables");
                return tables.Select("TABLE_NAME='" + tableName + "'").Length == 1;
            }
            //DataTable tab = _conn.Select("MSysObjects.Name, MSysObjects.Type", "MSysObjects", "(MSysObjects.Name=\"" + tableName + "\")  AND (MSysObjects.Type=1)");
            //if (tab == null || tab.Rows.Count == 0) return false;

            //return true;
        }

        public override IFeatureDataset this[string dsname]
        {
            get
            {
                SQLiteFDBDataset dataset = new SQLiteFDBDataset(this, dsname);
                if (dataset._dsID == -1)
                {
                    _errMsg = "Dataset '" + dsname + "' does not exist!";
                    return null;
                }
                return dataset;
            }
        }

        protected override string FieldDataType(IField field)
        {
            if (field == null) return "";
            switch (field.type)
            {
                case FieldType.biginteger:
                case FieldType.integer:
                case FieldType.smallinteger:
                    return "INTEGER";
                case FieldType.boolean:
                    return "BOOLEAN";
                case FieldType.Float:
                    return "REAL";
                case FieldType.Double:
                    return "DOUBLE";
                case FieldType.Date:
                    return "DATE";
                case FieldType.ID:
                    return "INTEGER";
                case FieldType.binary:
                    return "BLOB";
                case FieldType.character:
                    return "NVARCHAR(1)";
                case FieldType.String:
                    if (field.size > 0)
                        return "NVARCHAR(" + field.size + ")";
                    else if (field.size <= 0)
                        return "TEXT(255)";
                    break;
                case FieldType.guid:
                    return "NVARCHAR(36)";
                case FieldType.replicationID:
                    return "NVARCHAR(36)";
                default:
                    return "NVARCHAR(255)";
            }
            return "";
        }

        protected override bool CreateTable(string name, IFields Fields, bool msSpatial)
        {
            try
            {
                StringBuilder fields = new StringBuilder();
                StringBuilder types = new StringBuilder();

                bool first = true;
                bool hasID = false;

                string idField = "";

                foreach (IField field in Fields.ToEnumerable())
                {
                    //if( field.type==FieldType.ID ||
                    if (field.type == FieldType.Shape) continue;

                    if (!first)
                    {
                        fields.Append(";");
                        types.Append(";");
                    }
                    first = false;

                    string fname = field.name.Replace("#", "_");

                    fields.Append(fname);

                    switch (field.type)
                    {
                        case FieldType.biginteger:
                        case FieldType.integer:
                        case FieldType.smallinteger:
                            types.Append("INTEGER");
                            break;
                        case FieldType.boolean:
                            types.Append("BOOLEAN");
                            break;
                        case FieldType.Float:
                            types.Append("REAL");
                            break;
                        case FieldType.Double:
                            types.Append("DOUBLE");
                            break;
                        case FieldType.Date:
                            types.Append("DATE");
                            break;
                        case FieldType.ID:
                            if (!hasID)
                            {
                                types.Append("INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
                                hasID = true;
                                idField = field.name;
                            }
                            else
                            {
                                types.Append("INTEGER");
                            }
                            break;
                        case FieldType.GEOMETRY:  // gibts nicht im Access
                        case FieldType.GEOGRAPHY:
                        case FieldType.Shape:
                        case FieldType.binary:
                            types.Append("BLOB");
                            break;
                        case FieldType.character:
                            types.Append("TEXT");
                            break;
                        case FieldType.String:
                            if (field.size > 0)
                                types.Append("nvarchar(" + field.size + ")");
                            else if (field.size <= 0)
                                types.Append("nvarchar(255)");
                            break;
                        case FieldType.guid:
                            types.Append("nvarchar(40)");
                            break;
                        case FieldType.replicationID:
                            types.Append("nvarchar(40) NOT NULL");
                            break;
                        default:
                            types.Append("nvarchar(255)");
                            break;

                    }
                }

                if (!_conn.createTable(name, fields.ToString().Split(';'), types.ToString().Split(';')))
                {
                    _errMsg = _conn.errorMessage;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message + "\n" + ex.StackTrace; ;
                return false;
            }
        }

        public override string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(base.ConnectionString))
                    return "Data Source=" + _filename;

                return base.ConnectionString;
            }
        }

        #region IAltertable
        public override bool AlterTable(string table, IField oldField, IField newField)
        {
            if (oldField == null && newField == null) return true;

            if (oldField != null && newField != null &&
                oldField.name != newField.name &&
                this.GetType().Equals(typeof(gView.DataSources.Fdb.MSAccess.AccessFDB)))
            {
                // Umbenennen erst einmal verhindern. ADOX nicht verwenden...
                // Neues Feld anlegen, daten kopieren, ... erst prüfen!
                _errMsg = "Changing the field name is not possible for MS Access FDB";
                return false;
            }

            if (_conn == null) return false;
            int dsID = DatasetIDFromFeatureClassName(table);
            string dsname = DatasetNameFromFeatureClassName(table);
            if (dsname == "")
            {
                return false;
            }
            int fcID = FeatureClassID(dsID, table);
            if (fcID == -1)
            {
                return false;
            }

            IFeatureClass fc = GetFeatureclass(dsname, table);

            if (fc == null || fc.Fields == null) return false;

            try
            {
                if (oldField != null)
                {
                    if (fc.FindField(oldField.name) == null)
                    {
                        _errMsg = "Featureclass " + table + " do not contain field '" + oldField.name + "'";
                        return false;
                    }
                    if (oldField.Equals(newField)) return true;

                    if (newField != null)   // ALTER COLUMN
                    {
                        string sql = "ALTER TABLE " + FcTableName(table) +
                            " ALTER COLUMN " + DbColName(oldField.name) + " " + FieldDataType(newField);

                        if (!_conn.ExecuteNoneQuery(sql))
                        {
                            _errMsg = _conn.errorMessage;
                            return false;
                        }
                        if (oldField.name != newField.name)
                        {
                            if (!RenameField(table, oldField, newField))
                            {
                                return false;
                            }
                        }
                    }
                    else    // DROP COLUMN
                    {
                        string replIDFieldname = Replication.FeatureClassReplicationIDFieldname(fc);
                        if (!String.IsNullOrEmpty(replIDFieldname) &&
                            replIDFieldname == oldField.name)
                        {
                            Replication repl = new Replication();
                            if (!repl.RemoveReplicationIDField(fc))
                            {
                                _errMsg = "Can't remove replication id field...";
                                return false;
                            }
                        }

                        string sql = "ALTER TABLE " + FcTableName(table) +
                            " DROP COLUMN " + DbColName(oldField.name);

                        if (!_conn.ExecuteNoneQuery(sql))
                        {
                            _errMsg = _conn.errorMessage;
                            return false;
                        }
                    }
                }
                else  // ADD COLUMN
                {
                    string sql = "ALTER TABLE " + FcTableName(table) +
                        " ADD " + DbColName(newField.name) + " " + FieldDataType(newField);

                    if (!_conn.ExecuteNoneQuery(sql))
                    {
                        _errMsg = _conn.errorMessage;
                        return false;
                    }
                }

                return AlterFeatureclassField(fcID, oldField, newField);
            }
            finally
            {
                base.FireTableAltered(table);
            }
        }
        #endregion

        public override bool InsertRow(string table, IRow row, IReplicationTransaction replTrans)
        {
            try
            {
                bool useTrans = replTrans != null && replTrans.IsValid;
                using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                {
                    using (SQLiteCommand command =  new SQLiteCommand())
                    {
                        if (!useTrans)
                        {
                            command.Connection = connection;
                            connection.Open();
                        }
                        StringBuilder fields = new StringBuilder(), parameters = new StringBuilder();
                        command.Parameters.Clear();

                        foreach (IFieldValue fv in row.Fields)
                        {
                            string name = fv.Name;

                            if (fields.Length != 0) fields.Append(",");
                            if (parameters.Length != 0) parameters.Append(",");

                            SQLiteParameter parameter;
                            parameter = new SQLiteParameter("@" + name, fv.Value);
                            ModifyDbParameter(parameter);

                            fields.Append(this.DbColName(name));
                            parameters.Append("@" + name);
                            command.Parameters.Add(parameter);
                        }

                        command.CommandText = "INSERT INTO " + this.TableName(table) + " (" + fields.ToString() + ") VALUES (" + parameters + ")";

                        if (useTrans)
                        {
                            replTrans.ExecuteNonQuery(command);
                        }
                        else
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _errMsg = "Insert: " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                return false;
            }
        }
        public override bool InsertRows(string table, List<IRow> rows, IReplicationTransaction replTrans)
        {
            if (rows == null || rows.Count == 0)
                return true;

            try
            {
                bool useTrans = replTrans != null && replTrans.IsValid;
                using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                {
                    if (useTrans)
                    {
                        using (SQLiteCommand command = new SQLiteCommand())
                        {
                            if (!useTrans)
                            {
                                command.Connection = connection;
                                connection.Open();
                            }
                            foreach (IRow row in rows)
                            {
                                StringBuilder fields = new StringBuilder(), parameters = new StringBuilder();
                                command.Parameters.Clear();

                                foreach (IFieldValue fv in row.Fields)
                                {
                                    string name = fv.Name;

                                    if (fields.Length != 0) fields.Append(",");
                                    if (parameters.Length != 0) parameters.Append(",");

                                    SQLiteParameter parameter = new SQLiteParameter("@" + name, fv.Value);

                                    fields.Append("[" + name + "]");
                                    parameters.Append("@" + name);
                                    command.Parameters.Add(parameter);
                                }

                                string tabname = table;
                                if (!tabname.StartsWith("[") && !tabname.EndsWith("]"))
                                    tabname = "[" + tabname + "]";

                                command.CommandText = "INSERT INTO " + tabname + " (" + fields.ToString() + ") VALUES (" + parameters + ")";

                                if (useTrans)
                                {
                                    replTrans.ExecuteNonQuery(command);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (SQLiteCommand command = new SQLiteCommand())
                        using (var transaction = connection.BeginTransaction())
                        {
                            command.Connection = connection;
                            connection.Open();
                            
                            foreach (IRow row in rows)
                            {
                                StringBuilder fields = new StringBuilder(), parameters = new StringBuilder();
                                command.Parameters.Clear();

                                foreach (IFieldValue fv in row.Fields)
                                {
                                    string name = fv.Name;

                                    if (fields.Length != 0) fields.Append(",");
                                    if (parameters.Length != 0) parameters.Append(",");

                                    SQLiteParameter parameter = new SQLiteParameter("@" + name, fv.Value);

                                    fields.Append("[" + name + "]");
                                    parameters.Append("@" + name);
                                    command.Parameters.Add(parameter);
                                }

                                string tabname = table;
                                if (!tabname.StartsWith("[") && !tabname.EndsWith("]"))
                                    tabname = "[" + tabname + "]";

                                command.CommandText = "INSERT INTO " + tabname + " (" + fields.ToString() + ") VALUES (" + parameters + ")";
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _errMsg = "Insert: " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                return false;
            }
        }
        public override bool UpdateRow(string table, IRow row, string IDField, IReplicationTransaction replTrans)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand();
                command.Parameters.Clear();
                StringBuilder commandText = new StringBuilder();

                if (row.OID < 0)
                {
                    _errMsg = "Can't update feature with OID=" + row.OID;
                    return false;
                }

                StringBuilder fields = new StringBuilder();
                foreach (IFieldValue fv in row.Fields)
                {
                    string name = fv.Name;
                    if (fields.Length != 0) fields.Append(",");

                    SQLiteParameter parameter = new SQLiteParameter("@" + name, fv.Value);
                    fields.Append("[" + name + "]=@" + name);
                    command.Parameters.Add(parameter);
                }

                string tabname = table;
                if (!tabname.StartsWith("[") && !tabname.EndsWith("]"))
                    tabname = "[" + tabname + "]";

                commandText.Append("UPDATE " + tabname + " SET " + fields.ToString() + " WHERE " + IDField + "=" + row.OID);
                command.CommandText = commandText.ToString();

                if (replTrans != null && replTrans.IsValid)
                {
                    replTrans.ExecuteNonQuery(command);
                }
                else
                {
                    using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                    {
                        connection.Open();
                        command.Connection = connection;
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _errMsg = "Update: " + ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                return false;
            }
        }
        public override bool DeleteRows(string table, string where, IReplicationTransaction replTrans)
        {
            try
            {
                string sql = "DELETE FROM " + table + ((where != String.Empty) ? " WHERE " + where : "");
                SQLiteCommand command = new SQLiteCommand();
                command.CommandText = sql;

                if (replTrans != null && replTrans.IsValid)
                {
                    replTrans.ExecuteNonQuery(command);
                }
                else
                {
                    using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                    {
                        connection.Open();
                        command.Connection = connection;

                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message;
                return false;
            }
        }

        public override string GuidToSql(Guid guid)
        {
            return "'" + guid.ToString() + "'";
        }
        public override void ModifyDbParameter(DbParameter parameter)
        {
            if (parameter.Value is Guid)
            {
                parameter.Value = parameter.Value.ToString();
                parameter.DbType = DbType.String;
            }
        }

        public override bool IsFilebaseDatabase
        {
            get
            {
                return true;
            }
        }

        DbProviderFactory _factory = null;
        public override DbProviderFactory ProviderFactory
        {
            get
            {
                if (_factory == null)
                    _factory = new System.Data.SQLite.SQLiteFactory();

                return _factory;
            }
        }

        #region Spatial Index

        #region Rebuild Index
        protected override bool UpdateFeatureSpatialNodeID(IFeatureClass fc, int oid, long nid)
        {
            if (fc == null || _conn == null) return false;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                {
                    connection.Open();

                    using(SQLiteCommand command = new SQLiteCommand("UPDATE " + FcTableName(fc) + " SET " + DbColName("FDB_NID") + "=" + nid.ToString() + " WHERE " + DbColName(fc.IDFieldName) + "=" + oid.ToString(), connection))
                        command.ExecuteNonQuery();

                    connection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                _errMsg = "Fatal Exception:" + ex.Message + "\n" + ex.StackTrace; ;
                return false;
            }
        }
        #endregion

        public override bool ShrinkSpatialIndex(string fcName, List<long> NIDs)
        {
            if (NIDs == null || _conn == null) return false;

            NIDs.Sort();

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_conn.ConnectionString))
                {
                    connection.Open();

                    using(SQLiteCommand command = new SQLiteCommand(connection))
                    using(var transaction=connection.BeginTransaction())
                    {
                        command.CommandText="delete from "+FcsiTableName(fcName);
                        command.ExecuteNonQuery();
                        command.CommandText = "delete from sqlite_sequence where name='FCSI_" + fcName + "'";
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (long nid in NIDs)
                        {
                            command.Parameters.Clear();

                            SQLiteParameter parameter = new SQLiteParameter("@NID", nid);
                            command.CommandText = "INSERT INTO " + FcsiTableName(fcName) + " (NID) VALUES (@NID);";
                            command.Parameters.Add(parameter);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                _errMsg = ex.Message + "\n" + ex.StackTrace;
                return false;
            }

            IncSpatialIndexVersion(fcName);
            return true;
        }

        private void IncSpatialIndexVersion(string fcName)
        {
            if (_conn == null) return;

            fcName = OriginFcName(fcName);
            try
            {
                int siVersion = 1;
                object siVersionObject = _conn.QuerySingleField("SELECT " + DbColName("SIVersion") + " FROM " + TableName("FDB_FeatureClasses") + " WHERE " + DbColName("Name") + "='" + fcName + "'", ColumnName("SIVersion"));
                if (siVersionObject != System.DBNull.Value && siVersionObject != null)
                {
                    siVersion = Convert.ToInt32(siVersionObject) + 1;
                }
                if (!_conn.ExecuteNoneQuery("UPDATE " + TableName("FDB_FeatureClasses") + " SET " + DbColName("SIVersion") + "=" + siVersion + " WHERE " + DbColName("Name") + "='" + fcName + "'"))
                {

                }
            }
            catch { }
        }

        #endregion

        #region Helper Classes

        internal class SQLiteFDBFeatureCursorIDs : FeatureCursor
        {
            SQLiteConnection _connection;
            SQLiteDataReader _reader;
            //DataTable _schemaTable;
            IGeometryDef _geomDef;
            List<int> _IDs;
            int _id_pos = 0;
            string _sql;

            public SQLiteFDBFeatureCursorIDs(string connString, string sql, List<int> IDs, IGeometryDef geomDef, ISpatialReference toSRef) :
                base((geomDef != null) ? geomDef.SpatialReference : null, toSRef)
            {
                try
                {
                    _connection = new SQLiteConnection(connString);
                    SQLiteCommand command = new SQLiteCommand(_sql = sql, _connection);
                    _connection.Open();
                    _geomDef = geomDef;

                    // Schema auslesen...
                    //OleDbDataReader schema=command.ExecuteReader(CommandBehavior.SchemaOnly);
                    //_schemaTable=schema.GetSchemaTable();
                    //schema.Close();

                    _IDs = IDs;

                    ExecuteReader();
                    //_reader=command.ExecuteReader(CommandBehavior.SequentialAccess);
                }
                catch (Exception ex)
                {
                    Dispose();
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                if (_reader != null)
                {
                    _reader.Close();
                    _reader = null;
                }
                if (_connection != null)
                {
                    if (_connection.State == ConnectionState.Open)
                        try { _connection.Close(); }
                        catch { }
                    _connection.Dispose();
                    _connection = null;
                }
            }

            private bool ExecuteReader()
            {
                if (_reader != null)
                {
                    _reader.Close();
                    _reader = null;
                }
                if (_IDs == null) return false;
                if (_id_pos >= _IDs.Count) return false;

                int counter = 0;
                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(_IDs[_id_pos].ToString());
                    counter++;
                    _id_pos++;
                    if (_id_pos >= _IDs.Count || counter > 1000) break;
                }
                if (sb.Length == 0) return false;

                string where = " WHERE [FDB_OID] IN (" + sb.ToString() + ")";

                SQLiteCommand command = new SQLiteCommand(_sql + where, _connection);
                _reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                return true;
            }

            #region IFeatureCursor Member

            int _pos;
            public void Reset()
            {
                _pos = 0;
            }
            public void Release()
            {
                this.Dispose();
            }

            public override IFeature NextFeature
            {
                get
                {
                    try
                    {
                        if (_reader == null) return null;
                        if (!_reader.Read())
                        {
                            ExecuteReader();
                            return NextFeature;
                        }

                        Feature feature = new Feature();
                        for (int i = 0; i < _reader.FieldCount; i++)
                        {
                            string name = _reader.GetName(i);
                            object obj = _reader.GetValue(i);

                            if (/*_schemaTable.Rows[i][0].ToString()*/name == "FDB_SHAPE")
                            {
                                BinaryReader r = new BinaryReader(new MemoryStream());
                                r.BaseStream.Write((byte[])obj, 0, ((byte[])obj).Length);
                                r.BaseStream.Position = 0;

                                IGeometry p = null;
                                switch (_geomDef.GeometryType)
                                {
                                    case geometryType.Point:
                                        p = new gView.Framework.Geometry.Point();
                                        break;
                                    case geometryType.Polyline:
                                        p = new gView.Framework.Geometry.Polyline();
                                        break;
                                    case geometryType.Polygon:
                                        p = new gView.Framework.Geometry.Polygon();
                                        break;
                                }
                                if (p != null)
                                {
                                    p.Deserialize(r, _geomDef);
                                    r.Close();

                                    feature.Shape = p;
                                }
                            }
                            else
                            {
                                FieldValue fv = new FieldValue(/*_schemaTable.Rows[i][0].ToString()*/name, obj);
                                feature.Fields.Add(fv);
                                if (fv.Name == "FDB_OID")
                                    feature.OID = Convert.ToInt32(obj);
                            }
                        }
                        return feature;
                    }
                    catch
                    {
                        Dispose();
                        return null;
                    }
                }
            }

            #endregion
        }

        internal class SQLiteFDBFeatureCursor : FeatureCursor
        {
            SQLiteConnection _connection = null;
            SQLiteDataReader _reader = null;
            //DataTable _schemaTable=null;
            string _sql = "", _where = "", _orderby = "";
            int _nid_pos = 0;
            List<long> _nids;
            //IGeometry _queryGeometry;
            //Envelope _queryEnvelope;
            ISpatialFilter _spatialFilter = null;
            IGeometryDef _geomDef;

            public SQLiteFDBFeatureCursor(string connString, string sql, string where, string orderby, List<long> nids, ISpatialFilter filter, IGeometryDef geomDef, ISpatialReference toSRef) :
                base((geomDef != null) ? geomDef.SpatialReference : null,
                    /*(filter!=null) ? filter.FeatureSpatialReference : null*/
                           toSRef)
            {
                //try 
                {
                    _connection = new SQLiteConnection(connString);
                    SQLiteCommand command = new SQLiteCommand(_sql = sql, _connection);
                    _connection.Open();
                    _geomDef = geomDef;


                    _where = where;
                    _orderby = orderby;

                    if (nids != null)
                    {
                        /*if (nids.Count > 0)*/ _nids = nids;
                    }

                    _spatialFilter = filter;

                    ExecuteReader();
                }
                //catch(Exception ex) 
                {
                    //Dispose();
                    //throw(ex);
                }
            }

            public override void Dispose()
            {
                base.Dispose();
                if (_reader != null)
                {
                    _reader.Close();
                    _reader = null;
                }
                if (_connection != null)
                {
                    if (_connection.State == ConnectionState.Open) _connection.Close();
                    _connection = null;
                }
            }

            private bool ExecuteReader()
            {
                if (_reader != null)
                {
                    _reader.Close();
                    _reader = null;
                }

                string where = _where;

                StringBuilder sb = new StringBuilder();
                SQLiteParameter parameter = null, parameter2 = null;
                long pVal = 0, p2Val = 0;
                if (_nids != null)
                {
                    //if (_nids != null)
                    //{
                    //    if (_nid_pos >= _nids.Count) return false;
                    //    if (where != "") where += " AND ";
                    //    where += "(FDB_NID=" + _nids[_nid_pos].ToString() + ")";
                    //}

                    if (_nid_pos >= _nids.Count) 
                        return false;
                    if (_nids[_nid_pos] < 0)
                    {
                        where = "(FDB_NID BETWEEN @FDB_NID_FROM AND @FDB_NID_TO)" + (!String.IsNullOrEmpty(where) ? " AND (" + where + ")" : String.Empty);
                        //where = "(FDB_NID>=@FDB_NID_FROM AND FDB_NID<=@FDB_NID_TO)" + (!String.IsNullOrEmpty(where) ? " AND (" + where + ")" : String.Empty);
                        parameter = new SQLiteParameter("@FDB_NID_FROM");
                        parameter2 = new SQLiteParameter("@FDB_NID_TO");

                        pVal = -_nids[_nid_pos];
                        p2Val = _nids[_nid_pos + 1];
                        _nid_pos++;
                    }
                    else
                    {
                        where = "(FDB_NID=@FDB_NID)" + (!String.IsNullOrEmpty(where) ? " AND (" + where + ")" : String.Empty);
                        parameter = new SQLiteParameter("@FDB_NID");

                        pVal = _nids[_nid_pos];
                    }
                }
                else
                {
                    if (_nid_pos > 0) return false;
                }
                if (where != "") where = " WHERE " + where;

                _nid_pos++;
                using (SQLiteCommand command = new SQLiteCommand(_sql +
                                                        where +
                                                        ((_orderby != String.Empty) ? " ORDER BY " + _orderby : ""),
                                                        _connection))
                {
                    if (parameter != null) command.Parameters.Add(parameter);
                    if (parameter2 != null) command.Parameters.Add(parameter2);

                    command.Prepare();

                    if (parameter != null) parameter.Value = pVal;
                    if (parameter2 != null) parameter2.Value = p2Val;

                    _reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                }
                return true;
            }

            #region IFeatureCursor Member

            int _pos;
            public void Reset()
            {
                _pos = 0;
            }
            public void Release()
            {
                this.Dispose();
            }

            public override IFeature NextFeature
            {
                get
                {
                    try
                    {
                        while (true)
                        {
                            if (_reader == null) return null;
                            if (!_reader.Read())
                            {
                                ExecuteReader();
                                //return NextFeature;
                                continue;
                            }

                            bool nextFeature = false;
                            Feature feature = new Feature();
                            for (int i = 0; i < _reader.FieldCount; i++)
                            {
                                string name = _reader.GetName(i);
                                object obj = null;
                                try
                                {
                                    obj = _reader.GetValue(i);
                                }
                                catch { }

                                if (/*_schemaTable.Rows[i][0].ToString()*/name == "FDB_SHAPE" && obj != DBNull.Value)
                                {
                                    BinaryReader r = new BinaryReader(new MemoryStream());
                                    r.BaseStream.Write((byte[])obj, 0, ((byte[])obj).Length);
                                    r.BaseStream.Position = 0;

                                    IGeometry p = null;
                                    switch (_geomDef.GeometryType)
                                    {
                                        case geometryType.Point:
                                            p = new gView.Framework.Geometry.Point();
                                            break;
                                        case geometryType.Polyline:
                                            p = new gView.Framework.Geometry.Polyline();
                                            break;
                                        case geometryType.Polygon:
                                            p = new gView.Framework.Geometry.Polygon();
                                            break;
                                    }
                                    if (p != null)
                                    {
                                        p.Deserialize(r, _geomDef);
                                        r.Close();

                                        /*
                                        if(_queryEnvelope!=null) 
                                        {
                                            if(!_queryEnvelope.Intersects(p.Envelope)) 
                                                return NextFeature;
                                            if (_queryGeometry is IEnvelope)
                                            {
                                                if (!Algorithm.InBox(p, (IEnvelope)_queryGeometry))
                                                    return NextFeature;
                                            }
                                        }
                                        */
                                        if (_spatialFilter != null)
                                        {
                                            if (!gView.Framework.Geometry.SpatialRelation.Check(_spatialFilter, p))
                                            {
                                                //return NextFeature;
                                                nextFeature = true;
                                                break;
                                            }
                                        }
                                        feature.Shape = p;
                                    }
                                }
                                else
                                {
                                    FieldValue fv = new FieldValue(/*_schemaTable.Rows[i][0].ToString()*/name, obj);
                                    feature.Fields.Add(fv);
                                    if (fv.Name == "FDB_OID")
                                        feature.OID = Convert.ToInt32(obj);
                                }
                            }

                            if (nextFeature == true)
                                continue;

                            Transform(feature);
                            return feature;
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispose();
                        throw (ex);
                        //return null;
                    }
                }
            }

            #endregion
        }

        internal class SQLiteFDBDatasetElement : gView.Framework.Data.DatasetElement, IFeatureSelection
        {
            private ISelectionSet m_selectionset;

            public SQLiteFDBDatasetElement(SQLiteFDB fdb, IDataset dataset, string name, GeometryDef geomDef)
            {
                if (geomDef.GeometryType == geometryType.Network)
                {
                    _class = new SQLiteFDBNetworkFeatureClass(fdb, dataset, name, geomDef);
                }
                else
                {
                    _class = new SQLiteFDBFeatureClass(fdb, dataset, geomDef);
                    ((SQLiteFDBFeatureClass)_class).Name =
                    ((SQLiteFDBFeatureClass)_class).Aliasname = name;
                }
                this.Title = name;
            }

            public SQLiteFDBDatasetElement(LinkedFeatureClass fc)
            {
                _class = fc;
                this.Title = fc.Name;
            }

            #region IFeatureSelection Member

            public ISelectionSet SelectionSet
            {
                get
                {
                    return (ISelectionSet)m_selectionset;
                }
                set
                {
                    if (m_selectionset != null && m_selectionset != value) m_selectionset.Clear();

                    m_selectionset = value;
                }
            }

            public bool Select(IQueryFilter filter, gView.Framework.Data.CombinationMethod methode)
            {
                if (!(this.Class is ITableClass)) return false;
                ISelectionSet selSet = ((ITableClass)this.Class).Select(filter);

                SelectionSet = selSet;
                FireSelectionChangedEvent();

                return true;
            }

            public event gView.Framework.Data.FeatureSelectionChangedEvent FeatureSelectionChanged;
            public event BeforeClearSelectionEvent BeforeClearSelection;

            public void ClearSelection()
            {
                if (m_selectionset != null)
                {
                    m_selectionset.Clear();
                    m_selectionset = null;
                    FireSelectionChangedEvent();
                }
            }

            public void FireSelectionChangedEvent()
            {
                if (FeatureSelectionChanged != null)
                    FeatureSelectionChanged(this);
            }

            #endregion
        }

        #endregion
    }
}
