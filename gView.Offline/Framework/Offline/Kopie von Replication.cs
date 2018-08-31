using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.FDB;
using System.Data.Common;
using System.Data;
using gView.Framework.system;
using System.Reflection;
using gView.Framework.Geometry;

namespace gView.Framework.Offline____
{
    /*
    public class Replication
    {
        public enum SqlStatement { INSERT = 1, UPDATE = 2, DELETE = 3 }
        public enum VersionRights { NONE = 0, INSERT = 1, UPDATE = 2, DELETE = 4 }
        public enum ConflictHandling { NONE = 0, NORMAL = 1, PARENT_WINS = 2, CHILD_WINS = 3, NEWER_WINS = 4 }

        public delegate void ReplicationGuidsAppendedEventHandler(Replication sender, int count_appended);
        public event ReplicationGuidsAppendedEventHandler ReplicationGuidsAppended;

        static public bool CreateRelicationModel(IFeatureDatabaseReplication db, out string errMsg)
        {
            errMsg = String.Empty;

            if (db == null)
            {
                errMsg = "Argument Exception: db==null !";
                return false;
            }

            Fields fields = new Fields();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("FC_ID", FieldType.integer));
            fields.Add(new Field("OBJECT_GUID_FIELDNAME", FieldType.String, 255));
            fields.Add(new Field("PARENT_SESSION_GUID", FieldType.guid));
            if (!db.CreateIfNotExists("GV_CHECKOUT_OBJECT_GUID", fields))
            {
                errMsg = db.lastErrorMsg;
                return false;
            }

            fields = new Fields();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("FC_ID", FieldType.integer));
            fields.Add(new Field("CHECKOUT_GUID", FieldType.guid));
            fields.Add(new Field("CHECKOUT_NAME", FieldType.String, 255));
            fields.Add(new Field("CHILD_DATASET_CONNECTIONSTRING", FieldType.String, 4000));
            fields.Add(new Field("CHILD_DATASET_GUID", FieldType.guid));
            fields.Add(new Field("CHILD_FEATURECLASS", FieldType.String, 255));
            fields.Add(new Field("CHILD_RIGHTS", FieldType.integer));
            fields.Add(new Field("PARENT_DATASET_CONNECTIONSTRING", FieldType.String, 4000));
            fields.Add(new Field("PARENT_DATASET_GUID", FieldType.guid));
            fields.Add(new Field("PARENT_FEATURECLASS", FieldType.String, 255));
            fields.Add(new Field("PARENT_RIGHTS", FieldType.integer));
            fields.Add(new Field("CONFLICT_HANDLING", FieldType.integer));
            fields.Add(new Field("GENERATION", FieldType.integer));
            fields.Add(new Field("REPLICATION_LOCKSTATE", FieldType.integer));
            fields.Add(new Field("REPLICATION_STATE", FieldType.integer));
            if (!db.CreateIfNotExists("GV_CHECKOUT_SESSIONS", fields))
            {
                errMsg = db.lastErrorMsg;
                return false;
            }

            fields = new Fields();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("CHECKOUT_GUID", FieldType.guid));
            fields.Add(new Field("DIFF_DATUM", FieldType.Date));
            fields.Add(new Field("CHECKOUT_SESSION_USER", FieldType.String, 255));
            fields.Add(new Field("OBJECT_GUID", FieldType.guid));
            fields.Add(new Field("SQL_STATEMENT", FieldType.integer));
            fields.Add(new Field("REPLICATION_STATE", FieldType.integer));
            fields.Add(new Field("TRANSACTION_ID", FieldType.guid));
            if (!db.CreateIfNotExists("GV_CHECKOUT_DIFFERENCE", fields))
            {
                errMsg = db.lastErrorMsg;
                return false;
            }

            fields = new Fields();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("FC_ID", FieldType.integer));
            fields.Add(new Field("PARENT_OBJECT_GUID", FieldType.guid));
            fields.Add(new Field("PARENT_DATUM", FieldType.Date));
            fields.Add(new Field("PARENT_USER", FieldType.String, 255));
            fields.Add(new Field("PARENT_SQL_STATEMENT", FieldType.integer));
            fields.Add(new Field("CONFLICT_OBJECT_GUID", FieldType.guid));
            fields.Add(new Field("CONFLICT_DATUM", FieldType.Date));
            fields.Add(new Field("CONFLICT_USER", FieldType.String, 255));
            fields.Add(new Field("CONFLICT_CHECKOUT_NAME", FieldType.String, 255));
            fields.Add(new Field("CONFLICT_SQL_STATEMENT", FieldType.integer));
            if (!db.CreateIfNotExists("GV_CHECKOUT_CONFLICTS", fields))
            {
                errMsg = db.lastErrorMsg;
                return false;
            }

            fields = new Fields();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("FC_ID", FieldType.integer));
            fields.Add(new Field("OBJECT_GUID", FieldType.guid));
            fields.Add(new Field("REPLICATION_LOCK", FieldType.integer));
            fields.Add(new Field("CHECKOUT_GUID", FieldType.guid));
            fields.Add(new Field("REPLICATION_STATE", FieldType.integer));
            if (!db.CreateIfNotExists("GV_CHECKOUT_LOCKS", fields))
            {
                errMsg = db.lastErrorMsg;
                return false;
            }
            return true;
        }

        public bool AppendReplicationIDField(IFeatureDatabaseReplication db, IFeatureClass fc, string fieldName, out string errMsg)
        {
            errMsg = String.Empty;

            try
            {
                int fc_id = db.GetFeatureClassID(fc.Name);
                if (!CreateRelicationModel(db, out errMsg))
                    return false;

                if (!db.CreateObjectGuidColumn(fc.Name, fieldName))
                {
                    errMsg = db.lastErrorMsg;
                    return false;
                }

                try
                {
                    QueryFilter filter = new QueryFilter();
                    filter.AddField(fieldName);

                    int counter = 0;
                    List<IFeature> features = new List<IFeature>();
                    using (IFeatureCursor cursor = fc.Search(filter) as IFeatureCursor)
                    {
                        IFeature feature;
                        while ((feature = cursor.NextFeature) != null)
                        {
                            object guid = feature[fieldName];
                            if (!(guid is System.Guid))
                            {
                                feature[fieldName] = System.Guid.NewGuid();

                                features.Add(feature);
                                counter++;
                            }
                            if (features.Count >= 100)
                            {
                                if (!db.Update(fc, features))
                                {
                                    errMsg = db.lastErrorMsg;
                                    return false;
                                }
                                features.Clear();

                                if (ReplicationGuidsAppended != null)
                                    ReplicationGuidsAppended(this, counter);
                            }
                        }

                        if (features.Count > 0)
                        {
                            if (!db.Update(fc, features))
                            {
                                errMsg = db.lastErrorMsg;
                                return false;
                            }

                            if (ReplicationGuidsAppended != null)
                                ReplicationGuidsAppended(this, counter);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
                Row row = new Row();
                row.Fields.Add(new FieldValue("FC_ID", fc_id));
                row.Fields.Add(new FieldValue("OBJECT_GUID_FIELDNAME", fieldName));
                if (!db.InsertRow("GV_CHECKOUT_OBJECT_GUID", row, null))
                {
                    errMsg = db.lastErrorMsg;
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
            return true;
        }

        public bool RemoveReplicationIDField(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            if (FeatureClassHasReplications(fc))
            {
                // Feld kann nicht gelöscht werden, wenns Replikationen gibt...
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            return db.DeleteRows("GV_CHECKOUT_OBJECT_GUID", db.DbColName("FC_ID") + "=" + fc_id.ToString(), null);
        }

        static public bool InsertNewCheckoutSession(IFeatureClass sourceFC, IFeatureClass destFC, string description, out string errMsg)
        {
            return InsertNewCheckoutSession(
                sourceFC,
                VersionRights.INSERT | VersionRights.UPDATE | VersionRights.DELETE,
                destFC,
                VersionRights.INSERT | VersionRights.UPDATE | VersionRights.DELETE,
                ConflictHandling.NORMAL,
                description,
                out errMsg);
        }
        static public bool InsertNewCheckoutSession(IFeatureClass sourceFC, VersionRights parentRights, IFeatureClass destFC, VersionRights childRights, ConflictHandling confHandling, string description, out string errMsg)
        {
            errMsg = String.Empty;

            if (sourceFC == null ||
                sourceFC.Dataset == null ||
                !(sourceFC.Dataset.Database is IFeatureDatabaseReplication))
                return false;

            if (destFC == null ||
                destFC.Dataset == null ||
                !(destFC.Dataset.Database is IFeatureDatabaseReplication))
                return false;

            IFeatureDatabaseReplication sourceDB = sourceFC.Dataset.Database as IFeatureDatabaseReplication;
            IFeatureDatabaseReplication destDB = destFC.Dataset.Database as IFeatureDatabaseReplication;

            int generation = FeatureClassGeneration(sourceFC);
            if (generation < 0)
            {
                errMsg = "Can't determine source featureclass generation...";
                return false;
            }

            if (!CreateRelicationModel(sourceDB, out errMsg))
                return false;
            if (!CreateRelicationModel(destDB, out errMsg))
                return false;

            int sourceFc_id = sourceDB.GetFeatureClassID(sourceFC.Name);
            int destFc_id = destDB.GetFeatureClassID(destFC.Name);

            if (sourceFc_id < 0)
            {
                return false;
            }
            if (destFc_id < 0)
            {
                return false;
            }

            System.Guid guid = System.Guid.NewGuid();

            // PARENT_SESSION_GUID in Tabelle GV_CHECKOUT_OBJECT_GUID der child version setzen
            int rowID = FeatureClassReplicationID_RowID(destFC);
            if (rowID == -1)
            {
                errMsg = "Can't determine destination replicationfield id...";
                return false;
            }
            Row row = new Row();
            row.OID = rowID;
            row.Fields.Add(new FieldValue("PARENT_SESSION_GUID", guid));
            if (!destDB.UpdateRow("GV_CHECKOUT_OBJECT_GUID", row, "ID", null))
            {
                errMsg = destDB.lastErrorMsg;
                return false;
            }

            row = new Row();
            row.Fields.Add(new FieldValue("FC_ID", sourceFc_id));
            row.Fields.Add(new FieldValue("CHECKOUT_GUID", guid));
            row.Fields.Add(new FieldValue("CHECKOUT_NAME", description));
            row.Fields.Add(new FieldValue("CHILD_DATASET_CONNECTIONSTRING", destFC.Dataset.ConnectionString));
            row.Fields.Add(new FieldValue("CHILD_DATASET_GUID", PlugInManager.PlugInID(destFC.Dataset)));
            row.Fields.Add(new FieldValue("CHILD_FEATURECLASS", destFC.Name));
            row.Fields.Add(new FieldValue("GENERATION", generation));
            row.Fields.Add(new FieldValue("REPLICATION_LOCKSTATE", (int)LockState.Unlock));
            row.Fields.Add(new FieldValue("REPLICATION_STATE", 0));
            row.Fields.Add(new FieldValue("CHILD_RIGHTS", (int)childRights));
            row.Fields.Add(new FieldValue("PARENT_RIGHTS", (int)parentRights));
            row.Fields.Add(new FieldValue("CONFLICT_HANDLING", (int)confHandling));

            if (!sourceDB.InsertRow("GV_CHECKOUT_SESSIONS", row, null))
            {
                errMsg = sourceDB.lastErrorMsg;
                return false;
            }

            row = new Row();
            row.Fields.Add(new FieldValue("FC_ID", destFc_id));
            row.Fields.Add(new FieldValue("CHECKOUT_GUID", guid));
            row.Fields.Add(new FieldValue("CHECKOUT_NAME", description));
            row.Fields.Add(new FieldValue("PARENT_DATASET_CONNECTIONSTRING", sourceFC.Dataset.ConnectionString));
            row.Fields.Add(new FieldValue("PARENT_DATASET_GUID", PlugInManager.PlugInID(sourceFC.Dataset)));
            row.Fields.Add(new FieldValue("PARENT_FEATURECLASS", sourceFC.Name));
            row.Fields.Add(new FieldValue("GENERATION", generation + 1));
            row.Fields.Add(new FieldValue("REPLICATION_LOCKSTATE", 0));
            row.Fields.Add(new FieldValue("REPLICATION_STATE", (int)LockState.Unlock));
            row.Fields.Add(new FieldValue("CHILD_RIGHTS", (int)childRights));
            row.Fields.Add(new FieldValue("PARENT_RIGHTS", (int)parentRights));
            row.Fields.Add(new FieldValue("CONFLICT_HANDLING", (int)confHandling));

            if (!destDB.InsertRow("GV_CHECKOUT_SESSIONS", row, null))
            {
                errMsg = destDB.lastErrorMsg;
                return false;
            }
            return true;
        }
        static public bool InsertReplicationIDFieldname(IFeatureClass fc, string replicationIDFieldName, out string errMsg)
        {
            errMsg = String.Empty;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            if (!CreateRelicationModel(db, out errMsg))
            {
                return false;
            }

            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            Row row = new Row();
            row.Fields.Add(new FieldValue("FC_ID", fc_id));
            row.Fields.Add(new FieldValue("OBJECT_GUID_FIELDNAME", replicationIDFieldName));
            row.Fields.Add(new FieldValue("PARENT_SESSION_GUID", DBNull.Value));
            if (!db.InsertRow("GV_CHECKOUT_OBJECT_GUID", row, null))
            {
                errMsg = db.lastErrorMsg;
                return false;
            }

            return true;
        }
        static public bool InsertCheckoutLocks(IFeatureClass sourceFC, IFeatureClass destFC, out string errMsg)
        {
            errMsg = String.Empty;

            try
            {
                if (sourceFC == null ||
                    sourceFC.Dataset == null ||
                    !(sourceFC.Dataset.Database is IFeatureDatabaseReplication))
                    return false;

                if (destFC == null ||
                    destFC.Dataset == null ||
                    !(destFC.Dataset.Database is IFeatureDatabaseReplication))
                    return false;

                IFeatureDatabaseReplication sourceDB = sourceFC.Dataset.Database as IFeatureDatabaseReplication;
                IFeatureDatabaseReplication destDB = destFC.Dataset.Database as IFeatureDatabaseReplication;

                int generation = FeatureClassGeneration(sourceFC);
                if (generation < 0)
                {
                    errMsg = "Can't determine source featureclass generation...";
                    return false;
                }

                if (!CreateRelicationModel(sourceDB, out errMsg))
                    return false;
                if (!CreateRelicationModel(destDB, out errMsg))
                    return false;

                int sourceFc_id = sourceDB.GetFeatureClassID(sourceFC.Name);
                int destFc_id = destDB.GetFeatureClassID(destFC.Name);

                if (sourceFc_id < 0)
                {
                    return false;
                }
                if (destFc_id < 0)
                {
                    return false;
                }

                DataTable conf_table = new DataTable("CONFLICTS");
                using (DbConnection connection = sourceDB.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = sourceDB.DatabaseConnectionString;
                    DbCommand command = sourceDB.ProviderFactory.CreateCommand();
                    command.CommandText = "SELECT * FROM " + sourceDB.TableName("GV_CHECKOUT_CONFLICTS") + " WHERE " + sourceDB.DbColName("FC_ID") + "=" + sourceFc_id;
                    command.Connection = connection;
                    DbDataAdapter adapter = sourceDB.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(conf_table);
                }
                

                List<Guid> locks = new List<Guid>();
                foreach (DataRow conf_row in conf_table.Rows)
                {
                    if (!locks.Contains((Guid)conf_row["PARENT_OBJECT_GUID"]))
                        locks.Add((Guid)conf_row["PARENT_OBJECT_GUID"]);
                    if (!locks.Contains((Guid)conf_row["CONFLICT_OBJECT_GUID"]))
                        locks.Add((Guid)conf_row["CONFLICT_OBJECT_GUID"]);
                }

                // Bestehende Locks entfernen (IMMER nur aktuelle Locks abspeichern...) 
                using (DbConnection connection = destDB.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = destDB.DatabaseConnectionString;
                    DbCommand command = destDB.ProviderFactory.CreateCommand();
                    command.CommandText = "DELETE FROM " + destDB.TableName("GV_CHECKOUT_LOCKS") + " WHERE " + destDB.DbColName("FC_ID") + "=" + destFc_id;
                    command.Connection = connection;

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                foreach (Guid lockedGuid in locks)
                {
                    Row row = new Row();
                    row.Fields.Add(new FieldValue("FC_ID", destFc_id));
                    row.Fields.Add(new FieldValue("OBJECT_GUID", lockedGuid));
                    row.Fields.Add(new FieldValue("REPLICATION_LOCK", -1));
                    row.Fields.Add(new FieldValue("REPLICATION_STATE", -1));
                    if (!destDB.InsertRow("GV_CHECKOUT_LOCKS", row, null))
                    {
                        errMsg = "Can't insert feature locks!";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = "Exception: " + ex.Message + "\n" + ex.StackTrace;
                return false;
            }
        }

        public static bool FeatureClassHasRelicationID(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("FC_ID") + " FROM " + db.TableName("GV_CHECKOUT_OBJECT_GUID") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    return obj != null;
                }
            }
            catch
            {
                return false;
            }
        }
        public static string FeatureClassReplicationIDFieldname(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return null;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return null;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("OBJECT_GUID_FIELDNAME") + " FROM " + db.TableName("GV_CHECKOUT_OBJECT_GUID") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj != null)
                        return obj.ToString();

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public static bool AllowFeatureClassEditing(IFeatureClass fc, out string replFieldName)
        {
            replFieldName = null;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return true;

            if (!FeatureClassHasRelicationID(fc)) return true;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return false;
            }

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("OBJECT_GUID_FIELDNAME") + "," + db.DbColName("PARENT_SESSION_GUID") + " FROM " + db.TableName("GV_CHECKOUT_OBJECT_GUID") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    DbDataAdapter adapter = db.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;
                    DataTable tab = new DataTable("TAB");
                    adapter.Fill(tab);

                    if (tab.Rows.Count == 0) return true;
                    replFieldName = (string)tab.Rows[0]["OBJECT_GUID_FIELDNAME"];

                    if (tab.Rows[0]["PARENT_SESSION_GUID"] == DBNull.Value ||
                        (Guid)tab.Rows[0]["PARENT_SESSION_GUID"] == new Guid())
                    {
                        return true;
                    }

                    command.CommandText = "SELECT " + db.DbColName("ID") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql((Guid)tab.Rows[0]["PARENT_SESSION_GUID"]);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    object obj = command.ExecuteScalar();
                    return (obj != null && obj.GetType() == typeof(int));
                }
            }
            catch
            {
                return false;
            }
        }

        public static int FeatureClassReplicationID_RowID(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return -1;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return -1;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("ID") + " FROM " + db.TableName("GV_CHECKOUT_OBJECT_GUID") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj != null && obj.GetType() == typeof(int))
                        return (int)obj;

                    return -1;
                }
            }
            catch
            {
                return -1;
            }
        }
        public static bool FeatureClassHasReplications(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT COUNT(" + db.DbColName("FC_ID") + ") AS count_fc_id FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("GENERATION") + "=0 AND " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    int obj = Convert.ToInt32(command.ExecuteScalar());

                    return obj > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool FeatureClassCanReplicate(IFeatureClass fc)
        {
            if (!FeatureClassHasRelicationID(fc)) return false;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT COUNT(" + db.DbColName("FC_ID") + ") AS count_fc_id FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("GENERATION") + "=1 AND " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    int obj = Convert.ToInt32(command.ExecuteScalar());

                    return obj == 0;
                }
            }
            catch
            {
                return false;
            }
        }
        public static int FeatureClassGeneration(IFeatureClass fc)
        {
            if (!FeatureClassHasRelicationID(fc)) return -1;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return -1;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return -1;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT MIN(" + db.DbColName("GENERATION") + ") AS min_generation FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                        return 0;

                    return Convert.ToInt32(obj);
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return -1;
            }
        }
        // gibt NULL zurück, wenn keine Sessions vorhanden...
        public static List<Guid> FeatureClassSessions(IFeatureClass fc)
        {
            if (!FeatureClassHasRelicationID(fc)) return null;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return null;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return null;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("CHECKOUT_GUID") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    DbDataAdapter adapter = db.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    DataTable tab = new DataTable("GUIDS");
                    adapter.Fill(tab);

                    if (tab.Rows.Count == 0)
                    {
                        return null;
                    }

                    List<Guid> checkout_guids = new List<Guid>();
                    foreach (DataRow row in tab.Rows)
                    {
                        checkout_guids.Add((Guid)row["CHECKOUT_GUID"]);
                    }

                    return (checkout_guids.Count > 0) ? checkout_guids : null;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return null;
            }
        }
        public static string SessionName(IFeatureDatabaseReplication db, Guid guid)
        {
            if (db == null) return String.Empty;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("CHECKOUT_NAME") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(guid);

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                        return String.Empty;

                    return obj.ToString();
                }
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }
        private static VersionRights SessionParentRights(IFeatureDatabaseReplication db, Guid sessionGuid)
        {
            if (db == null) return VersionRights.NONE;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("PARENT_RIGHTS") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(sessionGuid);

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                        return VersionRights.NONE;

                    return (VersionRights)Convert.ToInt32(obj);
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return VersionRights.NONE;
            }
        }
        private static VersionRights SessionChildRights(IFeatureDatabaseReplication db, Guid sessionGuid)
        {
            if (db == null) return VersionRights.NONE;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("CHILD_RIGHTS") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(sessionGuid);

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                        return VersionRights.NONE;

                    return (VersionRights)Convert.ToInt32(obj);
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return VersionRights.NONE;
            }
        }
        private static ConflictHandling SessionConflictHandling(IFeatureDatabaseReplication db, Guid sessionGuid)
        {
            if (db == null) return ConflictHandling.NORMAL;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("CONFLICT_HANDLING") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(sessionGuid);

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                        return ConflictHandling.NORMAL;

                    return (ConflictHandling)Convert.ToInt32(obj);
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return ConflictHandling.NORMAL;
            }
        }

        public static bool FeatureClassHasConflicts(IFeatureClass fc)
        {
            if (!FeatureClassHasRelicationID(fc)) return false;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT COUNT(" + db.DbColName("FC_ID") + ") AS fc_id_count FROM " + db.TableName("GV_CHECKOUT_CONFLICTS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                        return false;

                    return true;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return false;
            }
        }
        public static List<Guid> FeatureClassConflictsParentGuids(IFeatureClass fc)
        {
            if (!FeatureClassHasRelicationID(fc)) return null;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return null;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return null;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT DISTINCT(" + db.DbColName("PARENT_OBJECT_GUID") + ") AS guids FROM " + db.TableName("GV_CHECKOUT_CONFLICTS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;
                    DbDataAdapter adapter = db.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    DataTable tab = new DataTable("GUIDS");
                    adapter.Fill(tab);

                    List<Guid> guids = new List<Guid>();
                    foreach (DataRow row in tab.Rows)
                    {
                        guids.Add((Guid)row["GUIDS"]);
                    }
                    return guids;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return null;
            }
        }
        public static Conflict FeatureClassConflict(IFeatureClass fc, Guid objectGuid)
        {
            if (!FeatureClassHasRelicationID(fc)) return null;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return null;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return null;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT * FROM " + db.TableName("GV_CHECKOUT_CONFLICTS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("PARENT_OBJECT_GUID") + "=" + db.GuidToSql(objectGuid);
                    DbDataAdapter adapter = db.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    DataTable tab = new DataTable("GUIDS");
                    if (adapter.Fill(tab) == 0) return null;

                    DataRow row1 = tab.Rows[0];
                    Conflict conflict = new Conflict(
                        fc,
                        FeatureByObjectGuid(fc, objectGuid),
                        objectGuid,
                        (SqlStatement)row1["PARENT_SQL_STATEMENT"],
                        (String)row1["PARENT_USER"],
                        (DateTime)row1["PARENT_DATUM"]);
                    conflict.rowIDs.Add((int)row1["ID"]);

                    foreach (DataRow row in tab.Rows)
                    {
                        SqlStatement cStatement = (SqlStatement)row["CONFLICT_SQL_STATEMENT"];
                        IFeature feature = (cStatement != SqlStatement.DELETE) ?
                            FeatureByObjectGuid(fc, (Guid)row["CONFLICT_OBJECT_GUID"]) :
                            null;

                        ConflictFeature cFeature = new ConflictFeature(
                            feature,
                            (Guid)row["CONFLICT_OBJECT_GUID"],
                            (SqlStatement)row["CONFLICT_SQL_STATEMENT"],
                            (DateTime)row["CONFLICT_DATUM"],
                            (string)row["CONFLICT_CHECKOUT_NAME"],
                            (string)row["CONFLICT_USER"]);

                        conflict.ConflictFeatures.Add(cFeature);
                        conflict.rowIDs.Add((int)row["ID"]);
                    }

                    conflict.Init();
                    return conflict;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return null;
            }
        }
        public static IFeature FeatureByObjectGuid(IFeatureClass fc, Guid objectGuid)
        {
            try
            {
                string repl_field_name = FeatureClassReplicationIDFieldname(fc);
                if (String.IsNullOrEmpty(repl_field_name)) return null;

                IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
                if (db == null) return null;

                QueryFilter filter = new QueryFilter();
                filter.AddField("*");
                filter.WhereClause = db.DbColName(repl_field_name) + "=" + db.GuidToSql(objectGuid);

                using (IFeatureCursor cursor = (IFeatureCursor)fc.GetFeatures(filter))
                {
                    return cursor.NextFeature;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string FeatureClassCheckoutName(IFeatureClass fc)
        {
            if (!FeatureClassHasRelicationID(fc)) return string.Empty;
            int generation = FeatureClassGeneration(fc);
            if (generation < 0) return String.Empty;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return String.Empty;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return String.Empty;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("CHECKOUT_NAME") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("GENERATION") + "=" + generation;

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                        return string.Empty;

                    return obj.ToString();
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return String.Empty;
            }
        }
        public static IFeatureDatabaseReplication FeatureClassDb(IFeatureClass fc)
        {
            if (!FeatureClassHasRelicationID(fc)) return null;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return null;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            return db;
        }
        public static IFeatureClass FeatureClassParentFc(IFeatureClass fc, out string errMsg)
        {
            errMsg = String.Empty;
            try
            {
                IFeatureDatabaseReplication db = FeatureClassDb(fc);
                if (db == null)
                {
                    errMsg = "Featureclass has replication functionallity";
                    return null;
                }
                int generation = FeatureClassGeneration(fc);
                if (generation <= 0)
                {
                    errMsg = "Can't determine generation";
                    return null;
                }
                int fc_id = db.GetFeatureClassID(fc.Name);
                if (fc_id < 0)
                {
                    errMsg = "Can't determine featureclass id";
                    return null;
                }

                DataTable tab = new DataTable("GV_CHECKOUT_SESSIONS");
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT * FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("GENERATION") + "=" + generation;

                    DbDataAdapter adapter = db.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    if (adapter.Fill(tab) != 1)
                    {
                        errMsg = "No session with generation " + generation + " defined";
                        return null;
                    }
                }

                System.Guid checkoutGuid = (System.Guid)tab.Rows[0]["CHECKOUT_GUID"];
                System.Guid dsGuid = (System.Guid)tab.Rows[0]["PARENT_DATASET_GUID"];
                string connectionString = tab.Rows[0]["PARENT_DATASET_CONNECTIONSTRING"].ToString();

                IDataset parentDs = PlugInManager.Create(dsGuid) as IDataset;
                if (parentDs == null)
                {
                    errMsg = "Can't create dataset from guid";
                    return null;
                }
                parentDs.ConnectionString = connectionString;
                if (!parentDs.Open())
                {
                    errMsg = parentDs.lastErrorMsg;
                    return null;
                }
                if (!(parentDs.Database is IFeatureDatabaseReplication))
                {
                    errMsg = "Can't checkin to parent database type";
                    return null;
                }

                IFeatureDatabaseReplication parentDb = (IFeatureDatabaseReplication)parentDs.Database;
                DataTable tab2 = new DataTable("GV_CHECKOUT_SESSIONS");
                using (DbConnection connection = parentDb.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = parentDb.DatabaseConnectionString;
                    DbCommand command = parentDb.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT * FROM " + parentDb.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + parentDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + parentDb.DbColName("GENERATION") + "=" + (generation - 1).ToString();
                    DbParameter parameter = parentDb.ProviderFactory.CreateParameter();
                    parameter.ParameterName = "@CHECKOUT_GUID";
                    parameter.Value = checkoutGuid;
                    command.Parameters.Add(parameter);

                    DbDataAdapter adapter = parentDb.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    if (adapter.Fill(tab2) != 1)
                    {
                        errMsg = "No parent session with generation " + (generation - 1).ToString() + " defined";
                        return null;
                    }
                }
                string parendFcName = parentDb.GetFeatureClassName((int)tab2.Rows[0]["FC_ID"]);
                if (String.IsNullOrEmpty(parendFcName))
                {
                    errMsg = "Can't determine parent featureclass name";
                    return null;
                }

                IDatasetElement element = parentDs[parendFcName];
                if (element == null || !(element.Class is IFeatureClass))
                {
                    errMsg = "Can't determine parent featureclass '" + parendFcName + "'";
                    return null;
                }

                return element.Class as IFeatureClass;
            }
            catch (Exception ex)
            {
                errMsg = "Exception-" + ex.Message + "\n" + ex.StackTrace;
                return null;
            }
        }
        public static void AllocateNewObjectGuid(IFeature feature, string replicationFieldName)
        {
            bool foundReplField = false;
            foreach (FieldValue fv in feature.Fields)
            {
                if (fv.Name == replicationFieldName)
                {
                    foundReplField = true;
                }
            }

            if (!foundReplField)
                feature.Fields.Add(new FieldValue(replicationFieldName));

            // neue GUID nur vergeben, wenn noch keine vorhanden ist...
            object objectGuid = feature[replicationFieldName];
            if (objectGuid == null || objectGuid == System.DBNull.Value || objectGuid.Equals(new Guid()))
            {
                feature[replicationFieldName] = System.Guid.NewGuid();
            }
        }
        public static Guid FeatureObjectGuid(IFeatureClass fc, IFeature feature, string replicationFieldName)
        {
            IDatabaseNames dn = (fc!=null && fc.Dataset!=null) ? fc.Dataset.Database as IDatabaseNames : null;

            QueryFilter filter = new QueryFilter();
            filter.AddField(replicationFieldName);
            filter.WhereClause = (dn != null ? dn.DbColName(fc.IDFieldName) : fc.IDFieldName) + "=" + feature.OID;

            using (IFeatureCursor cursor = fc.GetFeatures(filter) as IFeatureCursor)
            {
                IFeature feat = cursor.NextFeature;
                return (System.Guid)feat[replicationFieldName];
            }
        }
        public static bool WriteDifferencesToTable(IFeatureClass fc, System.Guid objectGuid, SqlStatement statement, ReplicationTransaction replTrans, out string errMsg)
        {
            return WriteDifferencesToTable(fc,
                objectGuid,
                statement,
                replTrans,
                ((fc is FeatureClassDifferenceDateTime) ?
                    ((FeatureClassDifferenceDateTime)fc).DiffTime :
                    DateTime.Now),
                out errMsg);
        }
        private static bool WriteDifferencesToTable(IFeatureClass fc, System.Guid objectGuid, SqlStatement statement, ReplicationTransaction replTrans, DateTime td, out string errMsg)
        {
            errMsg = String.Empty;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            try
            {
                string username = System.Environment.MachineName + "@" + System.Environment.UserName;
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + db.DbColName("CHECKOUT_GUID") + "," + db.DbColName("REPLICATION_STATE") + "," + db.DbColName("REPLICATION_LOCKSTATE") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;
                    connection.Open();

                    DbDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Guid checkout_guid = (Guid)reader.GetValue(0);
                        int repl_state = (int)reader.GetValue(1);
                        LockState lock_state = (LockState)((int)reader.GetValue(2));
                        Guid runningReplicationGuid = new Guid();

                        if (fc is FeatureClassDiffLocks)
                        {
                            if (checkout_guid.Equals(((FeatureClassDiffLocks)fc).SessionGUID))
                                continue;
                        }
                        if (fc is FeatureClassDifferenceDateTime)
                        {
                            if (checkout_guid.Equals(((FeatureClassDifferenceDateTime)fc).SessionGUID) &&
                                ((FeatureClassDifferenceDateTime)fc).ReplicationState >= 0)
                                repl_state = ((FeatureClassDifferenceDateTime)fc).ReplicationState;

                            runningReplicationGuid = ((FeatureClassDifferenceDateTime)fc).SessionGUID;
                        }

                        Guid transaction_guid = Guid.NewGuid();
                        Row row = new Row();
                        row.Fields.Add(new FieldValue("CHECKOUT_GUID", checkout_guid));
                        row.Fields.Add(new FieldValue("DIFF_DATUM", td));
                        row.Fields.Add(new FieldValue("CHECKOUT_SESSION_USER", username));
                        row.Fields.Add(new FieldValue("OBJECT_GUID", objectGuid));
                        row.Fields.Add(new FieldValue("SQL_STATEMENT", (int)statement));
                        row.Fields.Add(new FieldValue("REPLICATION_STATE", repl_state));
                        row.Fields.Add(new FieldValue("TRANSACTION_ID", transaction_guid));

                        switch (statement)
                        {
                            case SqlStatement.INSERT:
                                break;
                            case SqlStatement.UPDATE:
                                if (lock_state == LockState.Hardlock)
                                {
                                    errMsg = "Session is hardlocked, no updates possible. Try in again in few seconds...";
                                    return false;
                                }
                                // Konflikte testen
                                switch (HasLocksOrConflicts(db, fc_id, objectGuid, runningReplicationGuid))
                                {
                                    case LockFeatureType.ConflictFeatureLock:
                                        errMsg = "Can't update feature. Solve conflict first...";
                                        return false;
                                    case LockFeatureType.CheckinReconileLock:
                                        errMsg = "Can't update feature. Feature is locked for checkin/reconcile. Try later again.";
                                        return false;
                                    case LockFeatureType.PrivateCheckinReconcileLock:
                                        // kein echte lock -> ok, weitermachen
                                        break;
                                }
                                break;
                            case SqlStatement.DELETE:
                                if (lock_state == LockState.Hardlock)
                                {
                                    errMsg = "Session is hardlocked, no delete possible. Try in again in few seconds...";
                                    return false;
                                }
                                // Konflikte testen
                                switch (HasLocksOrConflicts(db, fc_id, objectGuid, runningReplicationGuid))
                                {
                                    case LockFeatureType.ConflictFeatureLock:
                                        errMsg = "Can't delete feature. Solve conflict first...";
                                        return false;
                                    case LockFeatureType.CheckinReconileLock:
                                        errMsg = "Can't delete feature. Feature is locked for checkin/reconcile. Try later again.";
                                        return false;
                                    case LockFeatureType.PrivateCheckinReconcileLock:
                                        // kein echte lock -> ok, weitermachen
                                        break;
                                }
                                break;
                        }

                        if (!db.InsertRow("GV_CHECKOUT_DIFFERENCE", row, replTrans))
                        {
                            errMsg = db.lastErrorMsg;
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        #region old_functions
        
        #endregion
        private enum LockFeatureType { None = 0, ConflictFeatureLock = 1, CheckinReconileLock = 2, PrivateCheckinReconcileLock = 3 }
        private static LockFeatureType HasLocksOrConflicts(IFeatureDatabaseReplication db, int fc_id, Guid object_guid, Guid runningReplicationSessionGuid)
        {
            if (db == null) return LockFeatureType.None;

            using (DbConnection connection = db.ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = db.DatabaseConnectionString;
                DbCommand command = db.ProviderFactory.CreateCommand();
                command.Connection = connection;
                connection.Open();

                // Conflict lock!
                command.CommandText = "SELECT MAX(" + db.DbColName("FC_ID") + ") AS max_id FROM " + db.TableName("GV_CHECKOUT_LOCKS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("OBJECT_GUID") + "=" + db.GuidToSql(object_guid) + " AND " + db.DbColName("REPLICATION_LOCK") + "=-1";
                object obj = command.ExecuteScalar();
                if (obj != null && obj != System.DBNull.Value && Convert.ToInt32(obj) == fc_id)
                {
                    return LockFeatureType.ConflictFeatureLock;
                }

                // CheckIn/Reconcile lock!
                // alle durchsuchen und schauen obs einen Lock aus einer anderen Session gibt...
                // sonst ist Locktype PrivateCheckinReconileLock
                command.CommandText = "SELECT " + db.DbColName("REPLICATION_LOCK") + "," + db.DbColName("CHECKOUT_GUID") + " FROM " + db.TableName("GV_CHECKOUT_LOCKS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("OBJECT_GUID") + "=" + db.GuidToSql(object_guid) + " AND " + db.DbColName("REPLICATION_LOCK") + ">=0";
                DbDataReader reader = command.ExecuteReader();
                LockFeatureType lType = LockFeatureType.None;
                while (reader.Read())
                {
                    lType = LockFeatureType.PrivateCheckinReconcileLock;

                    int repl_lock = (int)reader.GetValue(0);
                    if (reader.GetValue(1) == null || reader.GetValue(1) == DBNull.Value)
                    {
                        // sollte nie vorkommen
                        return LockFeatureType.ConflictFeatureLock;
                    }
                    Guid repl_checkout_guid = (Guid)reader.GetValue(1);

                    if (!repl_checkout_guid.Equals(runningReplicationSessionGuid))
                    {
                        return LockFeatureType.CheckinReconileLock;
                    }
                }
                reader.Close();
                if (lType != LockFeatureType.None) return lType;

                // Conflict!
                command.CommandText = "SELECT MAX(" + db.DbColName("FC_ID") + ") AS max_id FROM " + db.TableName("GV_CHECKOUT_CONFLICTS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND (" + db.DbColName("PARENT_OBJECT_GUID") + "=" + db.GuidToSql(object_guid) + " OR " + db.DbColName("CONFLICT_OBJECT_GUID") + "=" + db.GuidToSql(object_guid) + ")";
                obj = command.ExecuteScalar();
                if (obj != null && obj != System.DBNull.Value && Convert.ToInt32(obj) == fc_id)
                {
                    return LockFeatureType.ConflictFeatureLock;
                }
            }
            return LockFeatureType.None;
        }

        #region Helper
        private static void AppendInsertInto(IFeatureDatabaseReplication db, DbCommand command, string table, IRow row)
        {
            StringBuilder fields = new StringBuilder(), parameters = new StringBuilder();
            command.Parameters.Clear();

            foreach (IFieldValue fv in row.Fields)
            {
                string name = fv.Name;

                if (fields.Length != 0) fields.Append(",");
                if (parameters.Length != 0) parameters.Append(",");

                DbParameter parameter = db.ProviderFactory.CreateParameter();
                parameter.ParameterName = "@param_" + Guid.NewGuid().ToString("N"); ;
                parameter.Value = fv.Value;

                fields.Append(db.DbColName(name));
                parameters.Append(parameter.ParameterName);
                command.Parameters.Add(parameter);
            }

            command.CommandText += "INSERT INTO " + db.TableName(table) + " (" + fields.ToString() + ") VALUES (" + parameters + ")";

        }
        #endregion

        #region CheckIn Procs
        public delegate void CheckIn_FeatureInsertedEventHandler(Replication sender, int count_inserted);
        public delegate void CheckIn_FeatureUpdatedEventHandler(Replication sender, int count_updated);
        public delegate void CheckIn_FeatureDeletedEventHandler(Replication sender, int count_deleted);
        public delegate void CheckIn_ConflictDetectedEventHandler(Replication sender, int count_confilicts);
        public delegate void CheckIn_IgnoredSqlStatementEventHandler(Replication sender, int count_ignored, SqlStatement statement);
        public delegate void CheckIn_BeginPostEventHandler(Replication sender);
        public delegate void CheckIn_BeginCheckInEventHandler(Replication sender);
        public delegate void CheckIn_ChangeSessionLockStateEventHandler(Replication sender, string className, Guid session_guid, LockState lockState);
        public delegate void CheckIn_MessageEventHandler(Replication sender, string message);
        public event CheckIn_FeatureInsertedEventHandler CheckIn_FeatureInserted;
        public event CheckIn_FeatureUpdatedEventHandler CheckIn_FeatureUpdated;
        public event CheckIn_FeatureDeletedEventHandler CheckIn_FeatureDeleted;
        public event CheckIn_ConflictDetectedEventHandler CheckIn_ConflictDetected;
        public event CheckIn_IgnoredSqlStatementEventHandler CheckIn_IgnoredSqlStatement;
        public event CheckIn_BeginPostEventHandler CheckIn_BeginPost;
        public event CheckIn_BeginCheckInEventHandler CheckIn_BeginCheckIn;
        public event CheckIn_ChangeSessionLockStateEventHandler CheckIn_ChangeSessionLockState;
        public event CheckIn_MessageEventHandler CheckIn_Message;

        public enum ProcessType { CheckinAndRelease = 0, Reconcile = 1 }
        public bool Process(IFeatureClass parentFc, IFeatureClass childFc, ProcessType type, out string errMsg)
        {
            CheckinConst c = new CheckinConst();
            if (!c.Init(this, parentFc, childFc, true, out errMsg))
                return false;

            try
            {
                if (!CheckForLockedFeatureclassSessions(parentFc, c.checkout_guid, 60, out errMsg))
                    return false;
                if (!CheckForLockedFeatureclassSessions(childFc, c.checkout_guid, 60, out errMsg))
                    return false;

                LockReplicationSession(parentFc, c.checkout_guid, LockState.Hardlock);
                LockReplicationSession(childFc, c.checkout_guid, LockState.Hardlock);

                if (!CheckForLockedFeatureclassSessions(parentFc, c.checkout_guid, out errMsg))
                    return false;
                if (!CheckForLockedFeatureclassSessions(childFc, c.checkout_guid, out errMsg))
                    return false;

                IncReplicationState(parentFc, c.checkout_guid);
                IncReplicationState(childFc, c.checkout_guid);
                // 0,5 sec warten, damit letzte Änderungen, die mit alter 
                // StateID geschrieben werden in der Datenbank landen...
                System.Threading.Thread.Sleep(500);

                #region Thin Difference Tables
                if (!ThinDifferencesTable(c.childDb, c.checkout_guid, c.childReplicationState, out errMsg))
                    return false;
                if (!ThinDifferencesTable(c.parentDb, c.checkout_guid, c.parentReplactionState, out errMsg))
                    return false;
                #endregion

                #region WriteChildLocks
                using (DbConnection connection = c.childDb.ProviderFactory.CreateConnection())
                {
                    #region Eigene Locks
                    connection.ConnectionString = c.childDb.DatabaseConnectionString;

                    DbCommand command = c.childDb.ProviderFactory.CreateCommand();
                    command.CommandText =

@"INSERT INTO " + c.childDb.TableName("GV_CHECKOUT_LOCKS") + " (" + c.childDb.DbColName("FC_ID") + "," + c.childDb.DbColName("OBJECT_GUID") + "," + c.childDb.DbColName("REPLICATION_LOCK") + "," + c.childDb.DbColName("CHECKOUT_GUID") + "," + c.childDb.DbColName("REPLICATION_STATE") + @")
SELECT " + c.childFc_id + "," + c.childDb.DbColName("OBJECT_GUID") + ",0," + c.childDb.DbColName("CHECKOUT_GUID") + "," + c.childDb.DbColName("REPLICATION_STATE") +
" FROM " + c.childDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + c.childDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + c.childDb.DbColName("REPLICATION_STATE") + "<=" + c.childReplicationState;

                    DbParameter parameter = c.childDb.ProviderFactory.CreateParameter();
                    parameter.ParameterName = "@CHECKOUT_GUID";
                    parameter.Value = c.checkout_guid;
                    command.Parameters.Add(parameter);
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
                    #endregion

                    #region Locks aus Parent
                    using (DbConnection parentConn = c.parentDb.ProviderFactory.CreateConnection())
                    {
                        parentConn.ConnectionString = c.parentDb.DatabaseConnectionString;
                        DbCommand command2 = c.parentDb.ProviderFactory.CreateCommand();
                        command2.CommandText = "SELECT " + c.parentDb.DbColName("OBJECT_GUID") + " FROM " + c.parentDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + c.parentDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + c.parentDb.DbColName("REPLICATION_STATE") + "<=" + c.parentReplactionState;
                        DbParameter parameter2 = c.parentDb.ProviderFactory.CreateParameter();
                        parameter2.ParameterName = "@CHECKOUT_GUID";
                        parameter2.Value = c.checkout_guid;
                        command2.Parameters.Add(parameter2);
                        command2.Connection = parentConn;

                        DbDataAdapter adapter = c.parentDb.ProviderFactory.CreateDataAdapter();
                        adapter.SelectCommand = command2;

                        DataTable diff_table = new DataTable();
                        adapter.Fill(diff_table);
                        parentConn.Close();

                        Row row = new Row();
                        row.Fields.Add(new FieldValue("OBJECT_GUID", null));
                        row.Fields.Add(new FieldValue("FC_ID", c.childFc_id));
                        row.Fields.Add(new FieldValue("REPLICATION_LOCK", (int)1));
                        row.Fields.Add(new FieldValue("CHECKOUT_GUID", c.checkout_guid));
                        row.Fields.Add(new FieldValue("REPLICATION_STATE", c.childReplicationState));

                        foreach (DataRow diffRow in diff_table.Rows)
                        {
                            row.Fields[0].Value = diffRow["OBJECT_GUID"];
                            c.childDb.InsertRow("GV_CHECKOUT_LOCKS", row, null);
                        }
                    }
                    #endregion
                }
                #endregion

                #region WriteParentLocks
                using (DbConnection connection = c.parentDb.ProviderFactory.CreateConnection())
                {
                    #region Eigene Locks, nur wenn Reconcile
                    if (type == ProcessType.Reconcile)
                    {
                        connection.ConnectionString = c.parentDb.DatabaseConnectionString;
                        DbCommand command = c.parentDb.ProviderFactory.CreateCommand();
                        command.CommandText =
    @"INSERT INTO " + c.parentDb.TableName("GV_CHECKOUT_LOCKS") + " (" + c.parentDb.DbColName("FC_ID") + "," + c.parentDb.DbColName("OBJECT_GUID") + "," + c.parentDb.DbColName("REPLICATION_LOCK") + "," + c.parentDb.DbColName("CHECKOUT_GUID") + "," + c.parentDb.DbColName("REPLICATION_STATE") + @")
SELECT " + c.parentFc_id + @"," + c.parentDb.DbColName("OBJECT_GUID") + ",0," + c.parentDb.DbColName("CHECKOUT_GUID") + "," + c.parentDb.DbColName("REPLICATION_STATE") +
" FROM " + c.parentDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + c.parentDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + c.parentDb.DbColName("REPLICATION_STATE") + "<=" + c.parentReplactionState;

                        DbParameter parameter = c.parentDb.ProviderFactory.CreateParameter();
                        parameter.ParameterName = "@CHECKOUT_GUID";
                        parameter.Value = c.checkout_guid;
                        command.Parameters.Add(parameter);
                        command.Connection = connection;
                        connection.Open();

                        command.ExecuteNonQuery();
                    }
                    #endregion

                    #region Locks aus Child
                    using (DbConnection childConn = c.childDb.ProviderFactory.CreateConnection())
                    {
                        childConn.ConnectionString = c.childDb.DatabaseConnectionString;
                        DbCommand command2 = c.childDb.ProviderFactory.CreateCommand();
                        command2.CommandText = "SELECT " + c.childDb.DbColName("OBJECT_GUID") + " FROM " + c.childDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + c.childDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + c.childDb.DbColName("REPLICATION_STATE") + "<=" + c.parentReplactionState;
                        DbParameter parameter2 = c.childDb.ProviderFactory.CreateParameter();
                        parameter2.ParameterName = "@CHECKOUT_GUID";
                        parameter2.Value = c.checkout_guid;
                        command2.Parameters.Add(parameter2);
                        command2.Connection = childConn;

                        DbDataAdapter adapter = c.childDb.ProviderFactory.CreateDataAdapter();
                        adapter.SelectCommand = command2;

                        DataTable diff_table = new DataTable();
                        adapter.Fill(diff_table);
                        childConn.Close();

                        Row row = new Row();
                        row.Fields.Add(new FieldValue("OBJECT_GUID", null));
                        row.Fields.Add(new FieldValue("FC_ID", c.parentFc_id));
                        row.Fields.Add(new FieldValue("REPLICATION_LOCK", (int)1));
                        row.Fields.Add(new FieldValue("CHECKOUT_GUID", c.checkout_guid));
                        row.Fields.Add(new FieldValue("REPLICATION_STATE", c.childReplicationState));

                        foreach (DataRow diffRow in diff_table.Rows)
                        {
                            row.Fields[0].Value = diffRow["OBJECT_GUID"];
                            c.parentDb.InsertRow("GV_CHECKOUT_LOCKS", row, null);
                        }
                    }
                    #endregion
                }
                #endregion

                LockReplicationSession(parentFc, c.checkout_guid, LockState.Softlock);
                LockReplicationSession(childFc, c.checkout_guid, LockState.Softlock);

                if (!CheckIn(parentFc, childFc, c, out errMsg))
                {
                    errMsg = "ERROR ON CHECKIN:\n" + errMsg;
                    return false;
                }

                switch (type)
                {
                    case ProcessType.CheckinAndRelease:
                        if (!ReleaseVersion(parentFc, childFc, out errMsg))
                        {
                            errMsg = "ERROR ON RELEASE VERSION:\n" + errMsg;
                            return false;
                        }
                        break;
                    case ProcessType.Reconcile:
                        c.SwapParentChild();
                        if (!Post(parentFc, childFc, c, out errMsg))
                        {
                            errMsg = "ERROR ON POST:\n" + errMsg;
                            return false;
                        }
                        if (!Replication.InsertCheckoutLocks(parentFc, childFc, out errMsg))
                        {
                            errMsg = "ERROR ON INSERT LOCKS: " + errMsg;
                            return false;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                errMsg = "Exception: " + ex.Message;
                return false;
            }
            finally
            {
                RemoveReplicationLocks(childFc, c.checkout_guid, c.childReplicationState);
                RemoveReplicationLocks(parentFc, c.checkout_guid, c.parentReplactionState);

                LockReplicationSession(childFc, c.checkout_guid, LockState.Unlock);
                LockReplicationSession(parentFc, c.checkout_guid, LockState.Unlock);
            }
            return true;
        }

        private class CheckinConst
        {
            public IFeatureDatabaseReplication parentDb = null;
            public IFeatureDatabaseReplication childDb = null;
            public int parentFc_id = -1;
            public int childFc_id = -1;
            public int child_generation = -1;
            public int parent_generation = -1;
            public string child_repl_id_fieldname = String.Empty;
            public string parent_repl_id_fieldname = String.Empty;
            public string checkout_name = String.Empty;
            public Guid checkout_guid = new System.Guid();
            public VersionRights versionRights = VersionRights.NONE;
            public ConflictHandling conflictHandling = ConflictHandling.NONE;
            public int parentReplactionState = -1;
            public int childReplicationState = -1;
            LockState parentLocked = LockState.Error;
            LockState childLocked = LockState.Error;

            public bool Init(Replication repl, IFeatureClass parentFc, IFeatureClass childFc, bool checkin, out string errMsg)
            {
                errMsg = String.Empty;

                #region Initalisierung
                parentDb = FeatureClassDb(parentFc);
                if (parentDb == null)
                {
                    errMsg = "Can't checkin to parent database...";
                    return false;
                }
                if (!CreateRelicationModel(parentDb, out errMsg))
                {
                    return false;
                }
                childDb = FeatureClassDb(childFc);
                if (childDb == null)
                {
                    errMsg = "Can't checkout from child database...";
                    return false;
                }

                parentFc_id = parentDb.GetFeatureClassID(parentFc.Name);
                if (parentFc_id < 0)
                {
                    errMsg = "Can't determine parent featureclass id...";
                    return false;
                }
                childFc_id = childDb.GetFeatureClassID(childFc.Name);
                if (childFc_id < 0)
                {
                    errMsg = "Can't determine child featureclass id...";
                    return false;
                }
                child_generation = FeatureClassGeneration(childFc);
                if (checkin)
                {
                    if (child_generation <= 0)
                    {
                        errMsg = "Can't determine child featureclass generation";
                        return false;
                    }
                }
                else
                {
                    if (child_generation < 0)
                    {
                        errMsg = "Can't determine child featureclass generation";
                        return false;
                    }
                }
                parent_generation = FeatureClassGeneration(parentFc);
                if (checkin)
                {
                    if (parent_generation != child_generation - 1)
                    {
                        errMsg = "Can't determine parent featureclass generation";
                        return false;
                    }
                }
                else
                {
                    if (parent_generation != child_generation + 1)
                    {
                        errMsg = "Can't determine parent featureclass generation";
                        return false;
                    }
                }
                child_repl_id_fieldname = FeatureClassReplicationIDFieldname(childFc);
                if (String.IsNullOrEmpty(child_repl_id_fieldname))
                {
                    errMsg = "Can't determine child featureclass replication ID fieldname";
                    return false;
                }
                parent_repl_id_fieldname = FeatureClassReplicationIDFieldname(parentFc);
                if (String.IsNullOrEmpty(parent_repl_id_fieldname))
                {
                    errMsg = "Can't determine parent featureclass replication ID fieldname";
                    return false;
                }
                checkout_name = FeatureClassCheckoutName(childFc);
                if (checkin)
                {
                    using (DbConnection connection = childDb.ProviderFactory.CreateConnection())
                    {
                        connection.ConnectionString = childDb.DatabaseConnectionString;
                        DbCommand command = childDb.ProviderFactory.CreateCommand();
                        command.Connection = connection;
                        command.CommandText = "SELECT " + childDb.DbColName("CHECKOUT_GUID") + " FROM " + childDb.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + childDb.DbColName("FC_ID") + "=" + childFc_id + " AND " + childDb.DbColName("GENERATION") + "=" + child_generation;
                        connection.Open();

                        checkout_guid = (System.Guid)command.ExecuteScalar();
                    }
                }
                else
                {
                    using (DbConnection connection = parentDb.ProviderFactory.CreateConnection())
                    {
                        connection.ConnectionString = parentDb.DatabaseConnectionString;
                        DbCommand command = parentDb.ProviderFactory.CreateCommand();
                        command.Connection = connection;
                        command.CommandText = "SELECT " + parentDb.DbColName("CHECKOUT_GUID") + " FROM " + parentDb.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + parentDb.DbColName("FC_ID") + "=" + parentFc_id + " AND " + parentDb.DbColName("GENERATION") + "=" + parent_generation;
                        connection.Open();

                        checkout_guid = (System.Guid)command.ExecuteScalar();
                    }
                }

                versionRights = ((checkin) ?
                    SessionChildRights(parentDb, checkout_guid) :
                    SessionParentRights(childDb, checkout_guid));
                conflictHandling = ((checkin) ?
                    SessionConflictHandling(parentDb, checkout_guid) :
                    SessionConflictHandling(childDb, checkout_guid));


                parentReplactionState = repl.GetReplicationState(parentFc, checkout_guid);
                if (parentReplactionState == -1)
                {
                    errMsg = "Can't determine parent replication state";
                    return false;
                }
                childReplicationState = repl.GetReplicationState(childFc, checkout_guid);
                if (childReplicationState == -1)
                {
                    errMsg = "Can't determine child replication state";
                    return false;
                }

                parentLocked = repl.IsReplicationSessionLocked(parentFc, checkout_guid);
                childLocked = repl.IsReplicationSessionLocked(childFc, checkout_guid);

                switch (parentLocked)
                {
                    case LockState.Error:
                        errMsg = "Can't determine parent lock state";
                        return false;
                    case LockState.Hardlock:
                    case LockState.Softlock:
                        errMsg = "Parent Session is locked...";
                        return false;
                }

                switch (childLocked)
                {
                    case LockState.Error:
                        errMsg = "Can't determine child lock state";
                        return false;
                    case LockState.Hardlock:
                    case LockState.Softlock:
                        errMsg = "Child Session is locked...";
                        return false;
                }

                #endregion

                return true;
            }
            public void SwapParentChild()
            {
                H<IFeatureDatabaseReplication>.Swap(ref parentDb, ref childDb);
                H<int>.Swap(ref parentFc_id, ref childFc_id);
                H<int>.Swap(ref parent_generation, ref child_generation);
                H<string>.Swap(ref parent_repl_id_fieldname, ref child_repl_id_fieldname);
                H<int>.Swap(ref parentReplactionState, ref childReplicationState);
                H<LockState>.Swap(ref parentLocked, ref childLocked);
            }

            private class H<T>
            {
                public static void Swap(ref T obj1, ref T obj2)
                {
                    T obj = obj1;
                    obj1 = obj2;
                    obj2 = obj;
                }
            }
        }

        private bool CheckIn(IFeatureClass parentFc, IFeatureClass childFc, CheckinConst c, out string errMsg)
        {
            if (CheckIn_BeginCheckIn != null)
                CheckIn_BeginCheckIn(this);
            return CheckIn(parentFc, childFc, out errMsg, true, c);
        }
        private bool Post(IFeatureClass parentFc, IFeatureClass childFc, CheckinConst c, out string errMsg)
        {
            if (CheckIn_BeginPost != null)
                CheckIn_BeginPost(this);
            return CheckIn(childFc, parentFc, out errMsg, false, c);
        }

        public bool ReleaseVersion(IFeatureClass parentFc, IFeatureClass childFc, out string errMsg)
        {
            errMsg = String.Empty;
            try
            {
                #region Initalisierung
                IFeatureDatabaseReplication parentDb = FeatureClassDb(parentFc);
                if (parentDb == null)
                {
                    errMsg = "Can't checkin to parent database...";
                    return false;
                }
                IFeatureDatabaseReplication childDb = FeatureClassDb(childFc);
                if (childDb == null)
                {
                    errMsg = "Can't checkout from child database...";
                    return false;
                }
                int childFc_id = childDb.GetFeatureClassID(childFc.Name);
                if (childFc_id < 0)
                {
                    errMsg = "Can't determine child featureclass id...";
                    return false;
                }
                int child_generation = FeatureClassGeneration(childFc);
                if (child_generation <= 0)
                {
                    errMsg = "Can't determine child featureclass generation";
                    return false;
                }
                int parent_generation = FeatureClassGeneration(parentFc);
                if (parent_generation != child_generation - 1)
                {
                    errMsg = "Can't determine parent featureclass generation";
                    return false;
                }
                Guid checkout_guid;
                using (DbConnection connection = childDb.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = childDb.DatabaseConnectionString;
                    DbCommand command = childDb.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT " + childDb.DbColName("CHECKOUT_GUID") + " FROM " + childDb.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + childDb.DbColName("FC_ID") + "=" + childFc_id + " AND " + childDb.DbColName("GENERATION") + "=" + child_generation;
                    connection.Open();

                    checkout_guid = (System.Guid)command.ExecuteScalar();
                }
                #endregion

                #region Release Version
                using (DbConnection connection = childDb.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = childDb.DatabaseConnectionString;
                    DbCommand command = childDb.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "DELETE FROM " + childDb.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + childDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID";
                    DbParameter parameter = childDb.ProviderFactory.CreateParameter();
                    parameter.ParameterName = "@CHECKOUT_GUID";
                    parameter.Value = checkout_guid;
                    command.Parameters.Add(parameter);
                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM " + childDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + childDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID";
                    command.ExecuteNonQuery();

                    command = childDb.ProviderFactory.CreateCommand();
                    command.CommandText = "DELETE FROM " + childDb.TableName("GV_CHECKOUT_LOCKS") + " WHERE " + childDb.DbColName("FC_ID") + "=" + childFc_id;
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }
                using (DbConnection connection = parentDb.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = parentDb.DatabaseConnectionString;
                    DbCommand command = parentDb.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "DELETE FROM " + parentDb.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + parentDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID";
                    DbParameter parameter = parentDb.ProviderFactory.CreateParameter();
                    parameter.ParameterName = "@CHECKOUT_GUID";
                    parameter.Value = checkout_guid;
                    command.Parameters.Add(parameter);
                    connection.Open();
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM " + parentDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + parentDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID";
                    command.ExecuteNonQuery();
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                errMsg = "Exception: " + ex.Message;
                return false;
            }
        }
        public bool RemoveReplicationLocks(IFeatureClass fc, Guid checkout_guid, int replState)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication)) return false;

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = "DELETE FROM " + db.TableName("GV_CHECKOUT_LOCKS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(checkout_guid) + " AND " + db.DbColName("REPLICATION_STATE") + "<=" + replState + " AND " + db.DbColName("REPLICATION_STATE") + ">=0 AND " + db.DbColName("REPLICATION_LOCK") + ">=0";

                    connection.Open();
                    command.ExecuteNonQuery();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private bool CheckIn(IFeatureClass parentFc, IFeatureClass childFc, out string errMsg, bool checkin, CheckinConst c)
        {
            errMsg = String.Empty;
            if (parentFc == null || childFc == null || c == null) return false;

            if (!CheckForLockedFeatureclassSessions(parentFc, c.checkout_guid, out errMsg))
            {
                return false;
            }

            ISpatialReference parentSRef = parentFc.SpatialReference;
            try
            {
                #region Get Diff-Tables
                DataTable child_diff_table = new DataTable("DIFF_TABLE");
                using (DbConnection connection = c.childDb.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = c.childDb.DatabaseConnectionString;
                    DbCommand command = c.childDb.ProviderFactory.CreateCommand();
                    command.CommandText = "SELECT * FROM " + c.childDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + c.childDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + c.childDb.DbColName("REPLICATION_STATE") + "<=" + c.childReplicationState.ToString() + " ORDER BY " + c.childDb.DbColName("ID");
                    DbParameter parameter = c.childDb.ProviderFactory.CreateParameter();
                    parameter.ParameterName = "@CHECKOUT_GUID";
                    parameter.Value = c.checkout_guid;
                    command.Parameters.Add(parameter);
                    command.Connection = connection;

                    DbDataAdapter adapter = c.childDb.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    adapter.Fill(child_diff_table);
                }
                DataTable parent_diff_table = new DataTable("DIFF_TABLE");
                using (DbConnection connection = c.parentDb.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = c.parentDb.DatabaseConnectionString;
                    DbCommand command = c.parentDb.ProviderFactory.CreateCommand();
                    command.CommandText = "SELECT * FROM " + c.parentDb.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + c.parentDb.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + c.parentDb.DbColName("REPLICATION_STATE") + "<=" + c.parentReplactionState.ToString() + " ORDER BY " + c.parentDb.DbColName("ID");
                    DbParameter parameter = c.parentDb.ProviderFactory.CreateParameter();
                    parameter.ParameterName = "@CHECKOUT_GUID";
                    parameter.Value = c.checkout_guid;
                    command.Parameters.Add(parameter);
                    command.Connection = connection;

                    DbDataAdapter adapter = c.parentDb.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    adapter.Fill(parent_diff_table);
                }
                #endregion

                #region CheckIn Process
                int count_inserted = 0, count_updated = 0, count_deleted = 0, count_conflicts = 0, count_ignored = 0;
                IFeature child_feature, parent_feature;

                FeatureClassDifferenceDateTime parentFc_diff = new FeatureClassDifferenceDateTime(parentFc, c.checkout_guid, c.parentReplactionState);
                FeatureClassDiffLocks parentFc_lock = new FeatureClassDiffLocks(parentFc, c.checkout_guid);
                foreach (DataRow childDiffRow in child_diff_table.Rows)
                {
                    Guid object_guid = (Guid)childDiffRow["OBJECT_GUID"];
                    parentFc_diff.DiffTime = (DateTime)childDiffRow["DIFF_DATUM"];
                    parentFc_lock.DiffTime = (DateTime)childDiffRow["DIFF_DATUM"];

                    switch ((Replication.SqlStatement)childDiffRow["SQL_STATEMENT"])
                    {
                        case SqlStatement.INSERT:
                            if (!Bit.Has(c.versionRights, VersionRights.INSERT))
                            {
                                if (CheckIn_IgnoredSqlStatement != null) CheckIn_IgnoredSqlStatement(this, ++count_ignored, SqlStatement.INSERT);
                                break;
                            }

                            #region Insert
                            child_feature = GetFeatureByObjectGuid(c.childDb, childFc, c.child_repl_id_fieldname, object_guid, parentSRef);
                            if (child_feature == null)
                            {
                                // dürte eigentlich nicht vorkommen
                                continue;
                            }
                            if (!c.parentDb.Insert(parentFc_lock, child_feature))
                            {
                                errMsg = "Error on INSERT Feature:\n" + c.parentDb.lastErrorMsg;
                                return false;
                            }
                            if (CheckIn_FeatureInserted != null) CheckIn_FeatureInserted(this, ++count_inserted);
                            break;
                            #endregion
                        case SqlStatement.UPDATE:
                            if (!Bit.Has(c.versionRights, VersionRights.UPDATE))
                            {
                                if (CheckIn_IgnoredSqlStatement != null) CheckIn_IgnoredSqlStatement(this, ++count_ignored, SqlStatement.UPDATE);
                                break;
                            }

                            #region Update
                            child_feature = GetFeatureByObjectGuid(c.childDb, childFc, c.child_repl_id_fieldname, object_guid, parentSRef);
                            if (child_feature == null)
                            {
                                // dürte eigentlich nicht vorkommen
                                continue;
                            }

                            #region Conflict Handling
                            DataRow diff = DifferenceTableRow(parent_diff_table, c.checkout_guid, object_guid);
                            if (diff != null)
                            {
                                if (c.conflictHandling == ConflictHandling.NONE)
                                {
                                    child_feature[c.child_repl_id_fieldname] = null;
                                    if (!c.parentDb.Insert(parentFc_diff, child_feature))
                                    {
                                        errMsg = "Error on INSERT Feature:\n" + c.parentDb.lastErrorMsg;
                                        return false;
                                    }
                                    if (CheckIn_FeatureInserted != null) CheckIn_FeatureInserted(this, ++count_inserted);
                                    break;
                                }
                                else if (c.conflictHandling == ConflictHandling.NORMAL)
                                {
                                    child_feature[c.child_repl_id_fieldname] = null;
                                    if (!c.parentDb.Insert(parentFc_diff, child_feature))
                                    {
                                        errMsg = "Error on INSERT Feature:\n" + c.parentDb.lastErrorMsg;
                                        return false;
                                    }
                                    if (CheckIn_FeatureInserted != null) CheckIn_FeatureInserted(this, ++count_inserted);

                                    WriteConflict(c.parentDb, c.parentFc_id, c.checkout_name, diff, childDiffRow, (Guid)child_feature[c.child_repl_id_fieldname]);
                                    if (CheckIn_ConflictDetected != null) CheckIn_ConflictDetected(this, ++count_conflicts);
                                    break;
                                }
                                else if (c.conflictHandling == ConflictHandling.PARENT_WINS)
                                {
                                    break;
                                }
                                else if (c.conflictHandling == ConflictHandling.CHILD_WINS)
                                {
                                    // Parent Updaten... (standard, so tun als ob kein Konflikt
                                }
                                else if (c.conflictHandling == ConflictHandling.NEWER_WINS)
                                {
                                    DateTime td_parent = (DateTime)diff["DIFF_DATUM"];
                                    DateTime td_child = (DateTime)childDiffRow["DIFF_DATUM"];

                                    if (td_parent >= td_child)
                                    {
                                        // wie PARENT_WINS
                                        break;
                                    }
                                    else
                                    {
                                        // wie CHILD_WINS
                                    }
                                }
                            }
                            #endregion

                            parent_feature = GetFeatureByObjectGuid(c.parentDb, parentFc, c.parent_repl_id_fieldname, object_guid, parentSRef);
                            if (parent_feature == null)
                            {
                                // dürfe nicht sein, außer Feature wurde gelöscht
                                // dann sollte es allerdings im diff table sichtbar sein!!!
                                //if (checkin)
                                //{
                                //    throw new Exception("inkonsistent data!");
                                //}
                                //else
                                {
                                    // kann allerdings auch beim Post vorkommen, wenn Feature in Parent verändert
                                    // und in der Child Verions gelöscht wurde
                                    // Dann: INSERT
                                    if (!c.parentDb.Insert(parentFc_lock, child_feature))
                                    {
                                        errMsg = "Error on INSERT Feature:\n" + c.parentDb.lastErrorMsg;
                                        return false;
                                    }
                                    if (CheckIn_FeatureInserted != null) CheckIn_FeatureInserted(this, ++count_inserted);
                                    break;
                                }
                            }
                            Feature.CopyFrom(parent_feature, child_feature);
                            if (!c.parentDb.Update(parentFc_lock, parent_feature))
                            {
                                errMsg = "Error on UPDATE Feature:\n" + c.parentDb.lastErrorMsg;
                                return false;
                            }
                            if (CheckIn_FeatureUpdated != null) CheckIn_FeatureUpdated(this, ++count_updated);
                            break;
                            #endregion
                        case SqlStatement.DELETE:
                            if (!Bit.Has(c.versionRights, VersionRights.DELETE))
                            {
                                if (CheckIn_IgnoredSqlStatement != null) CheckIn_IgnoredSqlStatement(this, ++count_ignored, SqlStatement.DELETE);
                                break;
                            }

                            #region Delete

                            #region Conflict Handling
                            DataRow diff2 = DifferenceTableRow(parent_diff_table, c.checkout_guid, object_guid);
                            if (diff2 != null)
                            {
                                if (c.conflictHandling == ConflictHandling.NONE)
                                {
                                    // nix tun
                                    break;
                                }
                                else if (c.conflictHandling == ConflictHandling.NORMAL)
                                {
                                    // wenn beide gelöscht wurden -> kein konflikt!
                                    if ((SqlStatement)diff2["SQL_STATEMENT"] != SqlStatement.DELETE)
                                    {
                                        WriteConflict(c.parentDb, c.parentFc_id, c.checkout_name, diff2, childDiffRow, object_guid);
                                        if (CheckIn_ConflictDetected != null) CheckIn_ConflictDetected(this, ++count_conflicts);
                                    }
                                    break;
                                }
                                else if (c.conflictHandling == ConflictHandling.PARENT_WINS)
                                {
                                    // nix tun
                                    break;
                                }
                                else if (c.conflictHandling == ConflictHandling.CHILD_WINS)
                                {
                                    // Parent löschen. Standard -> so tun als ob kein Konflikt
                                }
                                else if (c.conflictHandling == ConflictHandling.NEWER_WINS)
                                {
                                    DateTime td_parent = (DateTime)diff2["DIFF_DATUM"];
                                    DateTime td_child = (DateTime)childDiffRow["DIFF_DATUM"];

                                    if (td_parent >= td_child)
                                    {
                                        // wie PARENT_WINS
                                        break;
                                    }
                                    else
                                    {
                                        // wie CHILD_WINS
                                    }
                                }
                            }
                            #endregion

                            parent_feature = GetFeatureByObjectGuid(c.parentDb, parentFc, c.parent_repl_id_fieldname, object_guid, parentSRef);
                            if (parent_feature == null)
                            {
                                // dürfe nicht sein, außer Feature wurde gelöscht
                                // dann sollte es allerdings im diff table sichtbar sein!!!
                                //throw new Exception("inkonsistent data!");
                                break;
                            }
                            if (!c.parentDb.Delete(parentFc_lock, parent_feature.OID))
                            {
                                errMsg = "Error on DELETE Feature:\n" + c.parentDb.lastErrorMsg;
                                return false;
                            }
                            if (CheckIn_FeatureDeleted != null) CheckIn_FeatureDeleted(this, ++count_deleted);
                            break;
                            #endregion
                    }

                    c.childDb.DeleteRows("GV_CHECKOUT_DIFFERENCE", c.childDb.DbColName("TRANSACTION_ID") + "=" + c.childDb.GuidToSql((Guid)childDiffRow["TRANSACTION_ID"]), null);
                    c.childDb.DeleteRows("GV_CHECKOUT_LOCKS", c.childDb.DbColName("OBJECT_GUID") + "=" + c.childDb.GuidToSql(object_guid) + " AND " + c.childDb.DbColName("REPLICATION_LOCK") + "=0 AND " + c.childDb.DbColName("CHECKOUT_GUID") + "=" + c.childDb.GuidToSql(c.checkout_guid) + " AND " + c.childDb.DbColName("REPLICATION_STATE") + "=" + (int)childDiffRow["REPLICATION_STATE"], null);
                    c.parentDb.DeleteRows("GV_CHECKOUT_LOCKS", c.childDb.DbColName("OBJECT_GUID") + "=" + c.parentDb.GuidToSql(object_guid) + " AND " + c.childDb.DbColName("REPLICATION_LOCK") + "=1 AND " + c.childDb.DbColName("CHECKOUT_GUID") + "=" + c.parentDb.GuidToSql(c.checkout_guid) + " AND " + c.childDb.DbColName("REPLICATION_STATE") + "=" + c.parentReplactionState, null);
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                return false;
            }
            finally
            {

            }
        }
        private IFeature GetFeatureByObjectGuid(IFeatureDatabaseReplication db, IFeatureClass fc, string replicGuidFieldName, Guid guid, ISpatialReference destSRef)
        {
            QueryFilter filter = new QueryFilter();
            filter.AddField("*");
            filter.WhereClause = (db != null ? db.DbColName(replicGuidFieldName) : replicGuidFieldName) + "=" + db.GuidToSql(guid);
            filter.FeatureSpatialReference = destSRef;

            using (IFeatureCursor cursor = (IFeatureCursor)fc.GetFeatures(filter))
            {
                if (cursor == null) return null;
                return cursor.NextFeature;
            }
        }
        private DataRow DifferenceTableRow(DataTable tab, Guid checkout_guid, Guid object_guid)
        {
            DataRow[] diffs = tab.Select("CHECKOUT_GUID='" + checkout_guid + "' AND OBJECT_GUID='" + object_guid.ToString() + "'");
            switch (diffs.Length)
            {
                case 0:
                    return null;
                case 1:
                    return diffs[0];
                default:
                    return diffs[diffs.Length - 1];
                //throw new Exception("Difference table is inkonsistent!");
            }
        }
        private bool WriteConflict(IFeatureDatabaseReplication db, int fc_id, string checkout_name, DataRow parentDiff, DataRow childDiff, Guid newObjectGuid)
        {
            Row row = new Row();
            row.Fields.Add(new FieldValue("FC_ID", fc_id));
            row.Fields.Add(new FieldValue("PARENT_OBJECT_GUID", parentDiff["OBJECT_GUID"]));
            row.Fields.Add(new FieldValue("PARENT_DATUM", parentDiff["DIFF_DATUM"]));
            row.Fields.Add(new FieldValue("PARENT_USER", parentDiff["CHECKOUT_SESSION_USER"]));
            row.Fields.Add(new FieldValue("PARENT_SQL_STATEMENT", parentDiff["SQL_STATEMENT"]));
            row.Fields.Add(new FieldValue("CONFLICT_OBJECT_GUID", newObjectGuid));
            row.Fields.Add(new FieldValue("CONFLICT_DATUM", childDiff["DIFF_DATUM"]));
            row.Fields.Add(new FieldValue("CONFLICT_USER", childDiff["CHECKOUT_SESSION_USER"]));
            row.Fields.Add(new FieldValue("CONFLICT_CHECKOUT_NAME", checkout_name));
            row.Fields.Add(new FieldValue("CONFLICT_SQL_STATEMENT", childDiff["SQL_STATEMENT"]));
            return db.InsertRow("GV_CHECKOUT_CONFLICTS", row, null);
        }

        public static bool ThinDifferencesTable(IFeatureDatabaseReplication db, Guid checkout_guid, int replState, out string errMsg)
        {
            errMsg = String.Empty;
            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.CommandText = "SELECT " + db.DbColName("OBJECT_GUID") + ",COUNT(" + db.DbColName("OBJECT_GUID") + ") as count_object_guids FROM " + db.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + db.DbColName("REPLICATION_STATE") + "<=" + replState.ToString() + " GROUP BY " + db.DbColName("OBJECT_GUID");
                    DbParameter parameter = db.ProviderFactory.CreateParameter();
                    parameter.ParameterName = "@CHECKOUT_GUID";
                    parameter.Value = checkout_guid;
                    command.Parameters.Add(parameter);
                    command.Connection = connection;

                    DbDataAdapter adapter = db.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    DataTable diff_table = new DataTable();
                    adapter.Fill(diff_table);

                    foreach (DataRow diffRow in diff_table.Select("count_object_guids>1"))
                    {
                        DbCommand command2 = db.ProviderFactory.CreateCommand();
                        command2.CommandText = "SELECT " + db.DbColName("ID") + "," + db.DbColName("SQL_STATEMENT") + "," + db.DbColName("DIFF_DATUM") + " FROM " + db.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + db.DbColName("OBJECT_GUID") + "=@OBJECT_GUID ORDER BY " + db.DbColName("DIFF_DATUM") + "," + db.DbColName("ID");
                        DbParameter p1 = db.ProviderFactory.CreateParameter();
                        p1.ParameterName = "@CHECKOUT_GUID";
                        p1.Value = checkout_guid;
                        DbParameter p2 = db.ProviderFactory.CreateParameter();
                        p2.ParameterName = "@OBJECT_GUID";
                        p2.Value = diffRow["OBJECT_GUID"];
                        command2.Parameters.Add(p1);
                        command2.Parameters.Add(p2);
                        command2.Connection = connection;

                        DataTable diffs = new DataTable();
                        adapter.SelectCommand = command2;
                        adapter.Fill(diffs);

                        SqlStatement first = (SqlStatement)diffs.Rows[0]["SQL_STATEMENT"];
                        int firstID = (int)diffs.Rows[0]["ID"];
                        SqlStatement last = (SqlStatement)diffs.Rows[diffs.Rows.Count - 1]["SQL_STATEMENT"];
                        int lastID = (int)diffs.Rows[diffs.Rows.Count - 1]["ID"];

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        if (first == SqlStatement.INSERT && last == SqlStatement.UPDATE)
                        {
                            // erste behalten
                            command2.CommandText = "DELETE FROM " + db.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + db.DbColName("OBJECT_GUID") + "=@OBJECT_GUID AND " + db.DbColName("ID") + "<>" + firstID.ToString() + " AND " + db.DbColName("REPLICATION_STATE") + "<=" + replState.ToString();
                            command2.ExecuteNonQuery();
                        }
                        else if (first == SqlStatement.INSERT && last == SqlStatement.DELETE)
                        {
                            // alle löschen
                            command2.CommandText = "DELETE FROM " + db.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + db.DbColName("OBJECT_GUID") + "=@OBJECT_GUID AND " + db.DbColName("REPLICATION_STATE") + "<=" + replState.ToString();
                            command2.ExecuteNonQuery();
                        }
                        else
                        {
                            // letzten beibehalten
                            command2.CommandText = "DELETE FROM " + db.TableName("GV_CHECKOUT_DIFFERENCE") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=@CHECKOUT_GUID AND " + db.DbColName("OBJECT_GUID") + "=@OBJECT_GUID AND " + db.DbColName("ID") + "<>" + lastID.ToString() + " AND " + db.DbColName("REPLICATION_STATE") + "<=" + replState.ToString();
                            command2.ExecuteNonQuery();
                        }

                        command2.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                errMsg = "ThinDifferencesTable: " + ex.Message;
                return false;
            }
            return true;
        }

        public enum LockState { Error = -1, Unlock = 0, Softlock = 1, Hardlock = 2 }
        private bool LockReplicationSession(IFeatureClass fc, Guid session_guid, LockState lockState)
        {
            IFeatureDatabaseReplication db = FeatureClassDb(fc);
            if (db == null)
                return false;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return false;
            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.CommandText = "UPDATE " + db.TableName("GV_CHECKOUT_SESSIONS") + " SET " + db.DbColName("REPLICATION_LOCKSTATE") + "=" + (int)lockState + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(session_guid);
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
                }
                if (CheckIn_ChangeSessionLockState != null)
                    CheckIn_ChangeSessionLockState(this, fc.Name, session_guid, lockState);
                return true;
            }
            catch { return false; }
        }
        private LockState IsReplicationSessionLocked(IFeatureClass fc, Guid session_guid)
        {
            IFeatureDatabaseReplication db = FeatureClassDb(fc);
            if (db == null)
                return LockState.Error;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return LockState.Error;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.CommandText = "SELECT " + db.DbColName("REPLICATION_LOCKSTATE") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(session_guid);
                    command.Connection = connection;
                    connection.Open();

                    command.Connection = connection;

                    object obj = command.ExecuteScalar();
                    if (obj != null && obj.GetType() == typeof(int))
                        return (LockState)((int)obj);

                    return LockState.Error;
                }
            }
            catch (Exception ex)
            {
                return LockState.Error;
            }
        }

        public int GetReplicationState(IFeatureClass fc, Guid session_guid)
        {
            IFeatureDatabaseReplication db = FeatureClassDb(fc);
            if (db == null)
                return -1;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.CommandText = "SELECT " + db.DbColName("REPLICATION_STATE") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(session_guid);
                    command.Connection = connection;
                    connection.Open();

                    object obj = command.ExecuteScalar();
                    if (obj != null && obj.GetType() == typeof(int))
                        return (int)obj;

                    return -1;
                }
            }
            catch { return -1; }
        }
        public bool IncReplicationState(IFeatureClass fc, Guid session_guid)
        {
            int replState = GetReplicationState(fc, session_guid);
            if (replState == -1) return false;

            IFeatureDatabaseReplication db = FeatureClassDb(fc);
            if (db == null)
                return false;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.CommandText = "UPDATE " + db.TableName("GV_CHECKOUT_SESSIONS") + " SET " + db.DbColName("REPLICATION_STATE") + "=" + (replState + 1).ToString() + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(session_guid);
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch { return false; }

        }

        private DataTable LockedSessions(IFeatureClass fc)
        {
            IFeatureDatabaseReplication db = FeatureClassDb(fc);
            if (db == null)
                return null;
            int fc_id = db.GetFeatureClassID(fc.Name);
            if (fc_id < 0) return null;

            try
            {
                using (DbConnection connection = db.ProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = db.DatabaseConnectionString;
                    DbCommand command = db.ProviderFactory.CreateCommand();
                    command.CommandText = "SELECT " + db.DbColName("FC_ID") + "," + db.DbColName("CHECKOUT_GUID") + "," + db.DbColName("CHECKOUT_NAME") + "," + db.DbColName("REPLICATION_LOCKSTATE") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("REPLICATION_LOCKSTATE") + ">0";
                    command.Connection = connection;

                    DbDataAdapter adapter = db.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    DataTable tab = new DataTable("LOCKED_GV_CHECKOUT_SESSIONS");
                    adapter.Fill(tab);

                    return tab;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private bool CheckForLockedFeatureclassSessions(IFeatureClass fc, Guid checkout_guid, out string errMsg)
        {
            errMsg = String.Empty;

            DataTable tab = LockedSessions(fc);
            if (tab == null)
            {
                errMsg = "Can't derminate featureclass session lockstate";
                return false;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("checkin featureclass is locked by to following sessions:\n");
            bool found = false;
            foreach (DataRow row in tab.Rows)
            {
                if (row["CHECKOUT_GUID"] == null || row["CHECKOUT_GUID"] == DBNull.Value)
                    continue;

                if (!checkout_guid.Equals((Guid)row["CHECKOUT_GUID"]))
                {
                    found = true;
                    sb.Append(row["CHECKOUT_GUID"].ToString() + " -> " + row["CHECKOUT_NAME"] + "\n");
                }
            }

            if (found)
            {
                sb.Append("Try checkin later again...");
                errMsg = sb.ToString();
                return false;
            }

            return true;
        }
        private bool CheckForLockedFeatureclassSessions(IFeatureClass fc, Guid checkout_guid, int waitSeconds, out string errMsg)
        {
            DateTime td = DateTime.Now;

            while (true)
            {
                if (CheckForLockedFeatureclassSessions(fc, checkout_guid, out errMsg))
                    return true;

                TimeSpan ts = DateTime.Now - td;
                if (ts.TotalSeconds > waitSeconds) break;

                if (CheckIn_Message != null)
                    CheckIn_Message(this, "Featureclass is locked. Try again in 3 seconds...");

                System.Threading.Thread.Sleep(3000);
            }

            return false;
        }
        #endregion

        #region Conflict Classes
        public class Conflict
        {
            public Guid ParentObjectGuid;
            public string User;
            public SqlStatement SqlStatement;
            public IFeature Feature;
            public IFeatureClass FeatureClass;
            public List<ConflictFeature> ConflictFeatures = new List<ConflictFeature>();
            public List<FieldConflict> FieldConflicts = new List<FieldConflict>();
            public DateTime Date;
            internal List<int> rowIDs = new List<int>();

            public Conflict(IFeatureClass fc, IFeature feature, Guid objectGuid, SqlStatement sqlStatement, string user, DateTime date)
            {
                FeatureClass = fc;
                Feature = feature;
                ParentObjectGuid = objectGuid;
                SqlStatement = sqlStatement;
                User = user;
                Date = date;
            }

            internal void Init()
            {
                if (FeatureClass == null || FeatureClass.Fields == null) return;

                string repl_field_name = Replication.FeatureClassReplicationIDFieldname(FeatureClass);

                FieldConflicts.Add(new FieldConflict(this, FeatureClass.ShapeFieldName));
                foreach (IField field in FeatureClass.Fields)
                {
                    if (field == null ||
                        field.type == FieldType.ID ||
                        field.type == FieldType.Shape ||
                        field.name == repl_field_name) continue;

                    FieldConflicts.Add(new FieldConflict(this, field.name));
                }
            }

            public List<FieldConflict> ConflictFields
            {
                get
                {
                    List<FieldConflict> fields = new List<FieldConflict>();
                    foreach (FieldConflict field in FieldConflicts)
                    {
                        if (field.ConflictTable != null)
                            fields.Add(field);
                    }
                    return fields;
                }
            }
            public List<FieldConflict> NeutralFields
            {
                get
                {
                    List<FieldConflict> fields = new List<FieldConflict>();
                    foreach (FieldConflict field in FieldConflicts)
                    {
                        if (field.ConflictTable == null)
                            fields.Add(field);
                    }
                    return fields;
                }
            }

            public IFeature SolvedFeature
            {
                get
                {
                    if (FeatureClass == null) return null;

                    Feature feature = new Feature();
                    feature.OID = (Feature != null) ? Feature.OID : -1;

                    foreach (FieldConflict cField in (Feature != null) ? ConflictFields : FieldConflicts)
                    {
                        IFeature f = GetFeatureByIndex(cField.ValueIndex);

                        if (cField.FieldName == FeatureClass.ShapeFieldName)
                        {
                            if (f == null) return null;
                            feature.Shape = f.Shape;
                        }
                        else if (f != null)
                        {
                            feature.Fields.Add(new FieldValue(cField.FieldName, f[cField.FieldName]));
                        }
                        else if (f == null)
                        {
                            feature.Fields.Add(new FieldValue(cField.FieldName, cField.Value));
                        }
                    }

                    return feature;
                }
            }
            private IFeature GetFeatureByIndex(int index)
            {
                if (index == 0)
                    return Feature;
                else if (index > 0 && index <= ConflictFeatures.Count)
                    return ConflictFeatures[index - 1].Feature;

                return null;
            }

            public class FieldConflict
            {
                private string _fieldName;
                private DataTable _tab = null;
                private object _fieldValue;
                private int _valueIndex = 0;

                public FieldConflict(Conflict conflict, string fieldName)
                {
                    _fieldName = fieldName;

                    bool hasConflict = false;

                    #region Shape
                    if (conflict.FeatureClass != null && fieldName == conflict.FeatureClass.ShapeFieldName)
                    {
                        IGeometry shape = (conflict.Feature != null) ? conflict.Feature.Shape : null;

                        if (shape != null)
                        {
                            foreach (ConflictFeature cFeature in conflict.ConflictFeatures)
                            {
                                IGeometry featShape = (cFeature.Feature != null) ? cFeature.Feature.Shape : null;
                                if (!shape.Equals(featShape, 1e-18))
                                {
                                    hasConflict = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            hasConflict = true;
                        }
                    }
                    #endregion

                    #region Field
                    if (conflict.FeatureClass != null && fieldName != conflict.FeatureClass.ShapeFieldName)
                    {
                        _fieldValue = (conflict.Feature != null) ? conflict.Feature[fieldName] : null;
                        if (_fieldValue != null)
                        {
                            foreach (ConflictFeature cFeature in conflict.ConflictFeatures)
                            {
                                object fieldValue = (cFeature.Feature != null) ? cFeature.Feature[fieldName] : null;

                                if (!_fieldValue.Equals(fieldValue))
                                {
                                    hasConflict = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            hasConflict = true;
                        }
                    }
                    #endregion

                    if (hasConflict)
                    {
                        _tab = new DataTable("CONFLICT");
                        _tab.Columns.Add("Value", typeof(String));
                        _tab.Columns.Add("CheckoutName", typeof(String));
                        _tab.Columns.Add("Statement", typeof(String));
                        _tab.Columns.Add("User", typeof(String));
                        _tab.Columns.Add("Date", typeof(String));

                        DataRow row = _tab.NewRow();
                        if (conflict.FeatureClass != null && conflict.FeatureClass.ShapeFieldName == fieldName)
                        {
                            row["Value"] = (conflict.Feature != null) ? "Shape1" : "DELETE FEATURE";
                            row["CheckoutName"] = "Parent";
                            row["Statement"] = conflict.SqlStatement.ToString();
                            row["User"] = conflict.User;
                            row["Date"] = conflict.Date.ToShortDateString();
                            _tab.Rows.Add(row);
                        }
                        else if (conflict.Feature != null)
                        {
                            {
                                if (conflict.Feature[FieldName] != null)
                                    row["Value"] = conflict.Feature[FieldName].ToString();
                                else
                                    row["Value"] = "NULL";
                            }

                            row["CheckoutName"] = "Parent";
                            row["Statement"] = conflict.SqlStatement.ToString();
                            row["User"] = conflict.User;
                            row["Date"] = conflict.Date.ToShortDateString();
                            _tab.Rows.Add(row);
                        }

                        int sCount = 2;
                        foreach (ConflictFeature cFeature in conflict.ConflictFeatures)
                        {
                            row = _tab.NewRow();
                            if (conflict.FeatureClass != null && conflict.FeatureClass.ShapeFieldName == fieldName)
                            {
                                row["Value"] = (cFeature.Feature != null) ? "Shape" + (sCount++).ToString() : "DELETE FEATURE";
                                row["CheckoutName"] = cFeature.CheckoutName;
                                row["Statement"] = cFeature.SqlStatement.ToString();
                                row["User"] = cFeature.User;
                                row["Date"] = cFeature.Date.ToShortDateString();
                                _tab.Rows.Add(row);
                            }
                            else if (cFeature.Feature != null)
                            {
                                if (cFeature.Feature[FieldName] != null)
                                    row["Value"] = cFeature.Feature[FieldName].ToString();
                                else
                                    row["Value"] = "NULL";

                                row["CheckoutName"] = cFeature.CheckoutName;
                                row["Statement"] = cFeature.SqlStatement.ToString();
                                row["User"] = cFeature.User;
                                row["Date"] = cFeature.Date.ToShortDateString();
                                _tab.Rows.Add(row);
                            }
                        }

                        if (_tab.Rows.Count == 1)
                        {
                            _fieldValue = _tab.Rows[0]["Value"];
                            _tab = null;
                        }
                    }
                }

                public string FieldName
                {
                    get { return _fieldName; }
                }
                public DataTable ConflictTable
                {
                    get
                    {
                        return _tab;
                    }
                }
                public object Value
                {
                    get
                    {
                        return _fieldValue;
                    }
                }

                public int ValueIndex
                {
                    get { return _valueIndex; }
                    set
                    {
                        if (this.ConflictTable == null) return;

                        if (this.ConflictTable.Rows.Count <= value)
                            return;

                        _valueIndex = value;
                    }
                }
            }

            public bool RemoveConflict(out string errMsg)
            {
                errMsg = String.Empty;
                IFeatureDatabaseReplication db = Replication.FeatureClassDb(FeatureClass);
                if (db == null) return false;

                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (int id in rowIDs)
                    {
                        if (sb.Length > 0) sb.Append(",");
                        sb.Append(id.ToString());
                    }
                    using (DbConnection connection = db.ProviderFactory.CreateConnection())
                    {
                        connection.ConnectionString = db.DatabaseConnectionString;
                        DbCommand command = db.ProviderFactory.CreateCommand();
                        command.Connection = connection;
                        command.CommandText = "DELETE FROM " + db.TableName("GV_CHECKOUT_CONFLICTS") + " WHERE " + db.DbColName("ID") + " in (" + sb.ToString() + ")";
                        connection.Open();

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    errMsg = "RemoveConflict -> " + ex.Message;
                    return false;
                }
            }
            public bool SolveConflict(out string errMsg)
            {
                errMsg = String.Empty;
                if (!RemoveConflict(out errMsg))
                    return false;

                IFeatureDatabaseReplication db = Replication.FeatureClassDb(FeatureClass);
                if (db == null) return false;

                IFeature sFeature = this.SolvedFeature;
                if (sFeature == null)
                {
                    // Feature ist zu löschen
                    if (Feature != null && db.Delete(FeatureClass, Feature.OID) == false)
                    {
                        errMsg = "SolveConflict -> " + db.lastErrorMsg;
                        return false;
                    }
                }
                else if (sFeature.OID == -1)
                {
                    // Ursprüngliches Features wurde gelöscht und soll jetzt
                    // "upgedatet" werden. -> neuer Insert mit OBJECT_GUID
                    string repl_field_name = Replication.FeatureClassReplicationIDFieldname(FeatureClass);
                    if (String.IsNullOrEmpty(repl_field_name))
                    {
                        errMsg = "SolveConflict -> can't get replication field name";
                        return false;
                    }
                    Replication.AllocateNewObjectGuid(sFeature, repl_field_name);
                    sFeature[repl_field_name] = this.ParentObjectGuid;
                    if (!db.Insert(FeatureClass, sFeature))
                    {
                        errMsg = "SolveConflict -> " + db.lastErrorMsg;
                        return false;
                    }
                }
                else
                {
                    if (!db.Update(FeatureClass, sFeature))
                    {
                        errMsg = "SolveConflict -> " + db.lastErrorMsg;
                        return false;
                    }
                }
                foreach (ConflictFeature cFeature in ConflictFeatures)
                {
                    if (cFeature == null || cFeature.Feature == null) continue;
                    if (!db.Delete(FeatureClass, cFeature.Feature.OID))
                    {
                        errMsg = "SolveConflict -> " + db.lastErrorMsg;
                        return false;
                    }
                }
                return true;
            }
        }
        public class ConflictFeature
        {
            public Guid ObjectGuid;
            public SqlStatement SqlStatement;
            public DateTime Date;
            public string CheckoutName, User;
            public IFeature Feature;

            public ConflictFeature(IFeature feature, Guid objectGuid, SqlStatement sqlStatement, DateTime date, string checkoutName, string user)
            {
                Feature = feature;
                ObjectGuid = objectGuid;
                SqlStatement = sqlStatement;
                Date = date;
                CheckoutName = checkoutName;
                User = user;
            }
        }
        #endregion

        #region Featureclass Wrapper
        private class FeatureClassDifferenceDateTime : IFeatureClass
        {
            protected IFeatureClass _fc;
            private DateTime _td;
            private int _replication_state = -1;
            protected Guid _sessionGUID;

            public FeatureClassDifferenceDateTime(IFeatureClass fc, Guid sessionGUID, int repl_state)
            {
                _fc = fc;
                _td = DateTime.Now;
                _sessionGUID = sessionGUID;
                _replication_state = repl_state;
            }

            public DateTime DiffTime
            {
                get
                {
                    return _td;
                }
                set
                {
                    _td = value;
                }
            }
            public int ReplicationState
            {
                get { return _replication_state; }
            }

            public Guid SessionGUID
            {
                get { return _sessionGUID; }
            }

            #region IFeatureClass Member

            public string ShapeFieldName
            {
                get { return _fc.ShapeFieldName; }
            }

            public gView.Framework.Geometry.IEnvelope Envelope
            {
                get { return _fc.Envelope; }
            }

            public int CountFeatures
            {
                get { return _fc.CountFeatures; }
            }

            public IFeatureCursor GetFeatures(IQueryFilter filter)
            {
                return _fc.GetFeatures(filter);
            }

            #endregion

            #region ITableClass Member

            public ICursor Search(IQueryFilter filter)
            {
                return _fc.Search(filter);
            }

            public ISelectionSet Select(IQueryFilter filter)
            {
                return _fc.Select(filter);
            }

            public IFields Fields
            {
                get { return _fc.Fields; }
            }

            public IField FindField(string name)
            {
                return _fc.FindField(name);
            }

            public string IDFieldName
            {
                get { return _fc.IDFieldName; }
            }

            #endregion

            #region IClass Member

            public string Name
            {
                get { return _fc.Name; }
            }

            public string Aliasname
            {
                get { return _fc.Aliasname; }
            }

            public IDataset Dataset
            {
                get { return _fc.Dataset; }
            }

            #endregion

            #region IGeometryDef Member

            public bool HasZ
            {
                get { return _fc.HasZ; }
            }

            public bool HasM
            {
                get { return _fc.HasM; }
            }

            public gView.Framework.Geometry.ISpatialReference SpatialReference
            {
                get { return _fc.SpatialReference; }
            }

            public gView.Framework.Geometry.geometryType GeometryType
            {
                get { return _fc.GeometryType; }
            }

            //public gView.Framework.Data.GeometryFieldType GeometryFieldType
            //{
            //    get
            //    {
            //        return _fc.GeometryFieldType;
            //    }
            //}

            #endregion
        }

        private class FeatureClassDiffLocks : FeatureClassDifferenceDateTime
        {
            public FeatureClassDiffLocks(IFeatureClass fc, Guid sessionGUID)
                : base(fc, sessionGUID, -1)
            {
                if (fc == null)
                    throw new ArgumentException();
            }

        }
        #endregion
    }

    public class ReplicationUI
    {
        public static void ShowAddReplicationIDDialog(IFeatureClass fc)
        {
            string appPath = SystemVariables.ApplicationDirectory;
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Offline.UI.dll");

            IFeatureClassDialog p = uiAssembly.CreateInstance("gView.Offline.UI.FormAddReplicationID") as IFeatureClassDialog;
            if (p != null)
            {
                p.ShowDialog(fc);
            }
        }
        public static void ShowCheckoutDialog(IFeatureClass fc)
        {
            string appPath = SystemVariables.ApplicationDirectory;
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Offline.UI.dll");

            IFeatureClassDialog p = uiAssembly.CreateInstance("gView.Offline.UI.FormCheckout") as IFeatureClassDialog;
            if (p != null)
            {
                p.ShowDialog(fc);
            }
        }
        public static void ShowCheckinDialog(IFeatureClass fc)
        {
            string appPath = SystemVariables.ApplicationDirectory;
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"\gView.Offline.UI.dll");

            IFeatureClassDialog p = uiAssembly.CreateInstance("gView.Offline.UI.FormCheckin") as IFeatureClassDialog;
            if (p != null)
            {
                p.ShowDialog(fc);
            }
        }
    }

    public class ReplicationTransaction : IReplicationTransaction
    {
        private DbConnection _connection = null;
        private DbTransaction _transasction = null;

        public ReplicationTransaction(DbConnection conn, DbTransaction trans)
        {
            _connection = conn;
            _transasction = trans;
        }
        public bool IsValid
        {
            get { return (_connection != null && _transasction != null); }
        }
        public int ExecuteNonQuery(DbCommand command)
        {
            if (command == null)
                throw new ArgumentException("Command is null!");
            if (!IsValid)
                throw new ArgumentException("ReplicationTransaction is not valid for ExecuteNonQuery");

            command.Connection = _connection;
            command.Transaction = _transasction;

            return command.ExecuteNonQuery();
        }

        public object ExecuteScalar(DbCommand command)
        {
            if (command == null)
                throw new ArgumentException("Command is null!");
            if (!IsValid)
                throw new ArgumentException("ReplicationTransaction is not valid for ExecuteNonQuery");

            command.Connection = _connection;
            command.Transaction = _transasction;

            return command.ExecuteScalar();
        }

    }
     * */
}
