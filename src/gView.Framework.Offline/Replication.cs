using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gView.Framework.Offline
{
    public class Replication
    {
        public enum SqlStatement { INSERT = 1, UPDATE = 2, DELETE = 3 }
        public enum VersionRights { NONE = 0, INSERT = 1, UPDATE = 2, DELETE = 4 }
        public enum ConflictHandling { NONE = 0, NORMAL = 1, PARENT_WINS = 2, CHILD_WINS = 3, NEWER_WINS = 4 }

        public delegate void ReplicationGuidsAppendedEventHandler(Replication sender, int count_appended);
        public event ReplicationGuidsAppendedEventHandler ReplicationGuidsAppended;

        async static public Task<bool> CreateRelicationModel(IFeatureDatabaseReplication db)
        {
            if (db == null)
            {
                throw new Exception("Argument Exception: db==null !");
            }

            FieldCollection fields = new FieldCollection();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("FC_ID", FieldType.integer));
            fields.Add(new Field("OBJECT_GUID_FIELDNAME", FieldType.String, 255));
            fields.Add(new Field("PARENT_SESSION_GUID", FieldType.guid));
            if (!await db.CreateIfNotExists("GV_CHECKOUT_OBJECT_GUID", fields))
            {
                throw new Exception(db.LastErrorMessage);
            }

            fields = new FieldCollection();
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
            if (!await db.CreateIfNotExists("GV_CHECKOUT_SESSIONS", fields))
            {
                throw new Exception(db.LastErrorMessage);
            }

            fields = new FieldCollection();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("CHECKOUT_GUID", FieldType.guid));
            fields.Add(new Field("DIFF_DATUM", FieldType.Date));
            fields.Add(new Field("CHECKOUT_SESSION_USER", FieldType.String, 255));
            fields.Add(new Field("OBJECT_GUID", FieldType.guid));
            fields.Add(new Field("SQL_STATEMENT", FieldType.integer));
            fields.Add(new Field("REPLICATION_STATE", FieldType.integer));
            fields.Add(new Field("TRANSACTION_ID", FieldType.guid));
            if (!await db.CreateIfNotExists("GV_CHECKOUT_DIFFERENCE", fields))
            {
                throw new Exception(db.LastErrorMessage);
            }

            fields = new FieldCollection();
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
            if (!await db.CreateIfNotExists("GV_CHECKOUT_CONFLICTS", fields))
            {
                throw new Exception(db.LastErrorMessage);
            }

            fields = new FieldCollection();
            fields.Add(new Field("ID", FieldType.ID));
            fields.Add(new Field("FC_ID", FieldType.integer));
            fields.Add(new Field("OBJECT_GUID", FieldType.guid));
            fields.Add(new Field("REPLICATION_LOCK", FieldType.integer));
            fields.Add(new Field("CHECKOUT_GUID", FieldType.guid));
            fields.Add(new Field("REPLICATION_STATE", FieldType.integer));
            if (!await db.CreateIfNotExists("GV_CHECKOUT_LOCKS", fields))
            {
                throw new Exception(db.LastErrorMessage);
            }
            return true;
        }

        async public Task<(bool success, string errMsg)> AppendReplicationIDField(IFeatureDatabaseReplication db, IFeatureClass fc, string fieldName)
        {
            string errMsg = String.Empty;

            try
            {
                int fc_id = await db.GetFeatureClassID(fc.Name);
                if (!await CreateRelicationModel(db))
                {
                    return (false, errMsg);
                }

                if (!await db.CreateObjectGuidColumn(fc.Name, fieldName))
                {
                    errMsg = db.LastErrorMessage;
                    return (false, errMsg);
                }

                try
                {
                    QueryFilter filter = new QueryFilter();
                    filter.AddField(fieldName);

                    int counter = 0;
                    List<IFeature> features = new List<IFeature>();

                    if (db.IsFilebaseDatabase)
                    {
                        // First read all features
                        using (IFeatureCursor cursor = await fc.Search(filter) as IFeatureCursor)
                        {
                            IFeature feature;
                            while ((feature = await cursor.NextFeature()) != null)
                            {
                                object guid = feature[fieldName];
                                if (!(guid is System.Guid))
                                {
                                    feature[fieldName] = System.Guid.NewGuid();

                                    features.Add(feature);
                                }

                            }
                        }

                        // now update features -> no locks!!
                        List<IFeature> features2 = new List<IFeature>();
                        foreach (IFeature feature in features)
                        {
                            counter++;
                            features2.Add(feature);
                            if (features2.Count >= 100)
                            {
                                if (!await db.Update(fc, features2))
                                {
                                    errMsg = db.LastErrorMessage;
                                    return (false, errMsg);
                                }
                                features2.Clear();

                                if (ReplicationGuidsAppended != null)
                                {
                                    ReplicationGuidsAppended(this, counter);
                                }
                            }
                        }
                        if (features2.Count > 0)
                        {
                            if (!await db.Update(fc, features2))
                            {
                                errMsg = db.LastErrorMessage;
                                return (false, errMsg);
                            }

                            if (ReplicationGuidsAppended != null)
                            {
                                ReplicationGuidsAppended(this, counter);
                            }
                        }
                    }
                    else
                    {
                        using (IFeatureCursor cursor = await fc.Search(filter) as IFeatureCursor)
                        {
                            IFeature feature;
                            while ((feature = await cursor.NextFeature()) != null)
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
                                    if (!await db.Update(fc, features))
                                    {
                                        errMsg = db.LastErrorMessage;
                                        return (false, errMsg);
                                    }
                                    features.Clear();

                                    if (ReplicationGuidsAppended != null)
                                    {
                                        ReplicationGuidsAppended(this, counter);
                                    }
                                }
                            }

                            if (features.Count > 0)
                            {
                                if (!await db.Update(fc, features))
                                {
                                    errMsg = db.LastErrorMessage;
                                    return (false, errMsg);
                                }

                                if (ReplicationGuidsAppended != null)
                                {
                                    ReplicationGuidsAppended(this, counter);
                                }
                            }
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
                    errMsg = db.LastErrorMessage;
                    return (false, errMsg);
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return (false, errMsg);
            }
            return (true, errMsg);
        }

        async public Task<bool> RemoveReplicationIDField(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            if (await FeatureClassHasReplications(fc))
            {
                // Feld kann nicht gelöscht werden, wenns Replikationen gibt...
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return false;
            }

            return db.DeleteRows("GV_CHECKOUT_OBJECT_GUID", db.DbColName("FC_ID") + "=" + fc_id.ToString(), null);
        }

        static public Task<bool> InsertNewCheckoutSession(IFeatureClass sourceFC, IFeatureClass destFC, string description)
        {
            return InsertNewCheckoutSession(
                sourceFC,
                VersionRights.INSERT | VersionRights.UPDATE | VersionRights.DELETE,
                destFC,
                VersionRights.INSERT | VersionRights.UPDATE | VersionRights.DELETE,
                ConflictHandling.NORMAL,
                description);
        }
        async static public Task<bool> InsertNewCheckoutSession(IFeatureClass sourceFC, VersionRights parentRights, IFeatureClass destFC, VersionRights childRights, ConflictHandling confHandling, string description)
        {

            if (sourceFC == null ||
                sourceFC.Dataset == null ||
                !(sourceFC.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            if (destFC == null ||
                destFC.Dataset == null ||
                !(destFC.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication sourceDB = sourceFC.Dataset.Database as IFeatureDatabaseReplication;
            IFeatureDatabaseReplication destDB = destFC.Dataset.Database as IFeatureDatabaseReplication;

            int generation = await FeatureClassGeneration(sourceFC);
            if (generation < 0)
            {
                throw new Exception("Can't determine source featureclass generation...");
            }

            if (!await CreateRelicationModel(sourceDB))
            {
                return false;
            }

            if (!await CreateRelicationModel(destDB))
            {
                return false;
            }

            int sourceFc_id = await sourceDB.GetFeatureClassID(sourceFC.Name);
            int destFc_id = await destDB.GetFeatureClassID(destFC.Name);

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
            int rowID = await FeatureClassReplicationID_RowID(destFC);
            if (rowID == -1)
            {
                throw new Exception("Can't determine destination replicationfield id...");
            }
            Row row = new Row();
            row.OID = rowID;
            row.Fields.Add(new FieldValue("PARENT_SESSION_GUID", guid));
            if (!destDB.UpdateRow("GV_CHECKOUT_OBJECT_GUID", row, "ID", null))
            {
                throw new Exception(destDB.LastErrorMessage);
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
                throw new Exception(sourceDB.LastErrorMessage);
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
                throw new Exception(destDB.LastErrorMessage);
            }
            return true;
        }
        async static public Task<bool> InsertReplicationIDFieldname(IFeatureClass fc, string replicationIDFieldName)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            if (!await CreateRelicationModel(db))
            {
                return false;
            }

            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return false;
            }

            Row row = new Row();
            row.Fields.Add(new FieldValue("FC_ID", fc_id));
            row.Fields.Add(new FieldValue("OBJECT_GUID_FIELDNAME", replicationIDFieldName));
            row.Fields.Add(new FieldValue("PARENT_SESSION_GUID", DBNull.Value));
            if (!db.InsertRow("GV_CHECKOUT_OBJECT_GUID", row, null))
            {
                throw new Exception(db.LastErrorMessage);
            }

            return true;
        }
        async static public Task<bool> InsertCheckoutLocks(IFeatureClass sourceFC, IFeatureClass destFC)
        {
            try
            {
                if (sourceFC == null ||
                    sourceFC.Dataset == null ||
                    !(sourceFC.Dataset.Database is IFeatureDatabaseReplication))
                {
                    return false;
                }

                if (destFC == null ||
                    destFC.Dataset == null ||
                    !(destFC.Dataset.Database is IFeatureDatabaseReplication))
                {
                    return false;
                }

                IFeatureDatabaseReplication sourceDB = sourceFC.Dataset.Database as IFeatureDatabaseReplication;
                IFeatureDatabaseReplication destDB = destFC.Dataset.Database as IFeatureDatabaseReplication;

                int generation = await FeatureClassGeneration(sourceFC);
                if (generation < 0)
                {
                    throw new Exception("Can't determine source featureclass generation...");
                }

                if (!await CreateRelicationModel(sourceDB))
                {
                    return false;
                }

                if (!await CreateRelicationModel(destDB))
                {
                    return false;
                }

                int sourceFc_id = await sourceDB.GetFeatureClassID(sourceFC.Name);
                int destFc_id = await destDB.GetFeatureClassID(destFC.Name);

                if (sourceFc_id < 0)
                {
                    return false;
                }
                if (destFc_id < 0)
                {
                    return false;
                }

                DataTable conf_table = new DataTable("CONFLICTS");
                DbAccess.GetReplConflicts(sourceDB, sourceFc_id, conf_table);

                List<Guid> locks = new List<Guid>();
                foreach (DataRow conf_row in conf_table.Rows)
                {
                    Guid prarentObjectGuid = conf_row["PARENT_OBJECT_GUID"] is Guid ? (Guid)conf_row["PARENT_OBJECT_GUID"] : new Guid(conf_row["PARENT_OBJECT_GUID"].ToString());
                    Guid conflictObjectGuid = conf_row["CONFLICT_OBJECT_GUID"] is Guid ? (Guid)conf_row["CONFLICT_OBJECT_GUID"] : new Guid(conf_row["CONFLICT_OBJECT_GUID"].ToString());

                    if (!locks.Contains(prarentObjectGuid))
                    {
                        locks.Add(prarentObjectGuid);
                    }

                    if (!locks.Contains(conflictObjectGuid))
                    {
                        locks.Add(conflictObjectGuid);
                    }
                }

                // Bestehende Locks entfernen (IMMER nur aktuelle Locks abspeichern...) 
                DbAccess.DeleteReplLocks(destDB, destFc_id);

                foreach (Guid lockedGuid in locks)
                {
                    Row row = new Row();
                    row.Fields.Add(new FieldValue("FC_ID", destFc_id));
                    row.Fields.Add(new FieldValue("OBJECT_GUID", lockedGuid));
                    row.Fields.Add(new FieldValue("REPLICATION_LOCK", -1));
                    row.Fields.Add(new FieldValue("REPLICATION_STATE", -1));
                    if (!destDB.InsertRow("GV_CHECKOUT_LOCKS", row, null))
                    {
                        throw new Exception("Can't insert feature locks!");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        async public static Task<bool> FeatureClassHasRelicationID(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return false;
            }

            try
            {
                return DbAccess.FeatureClassHasReplicationID(db, fc_id);
            }
            catch
            {
                return false;
            }
        }
        async public static Task<string> FeatureClassReplicationIDFieldname(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return null;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return null;
            }

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
                    {
                        return obj.ToString();
                    }

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        async public static Task<(bool allow, string replFieldName)> AllowFeatureClassEditing(IFeatureClass fc)
        {
            string replFieldName = null;

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return (true, replFieldName);
            }

            if (!await FeatureClassHasRelicationID(fc))
            {
                return (true, replFieldName);
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return (false, replFieldName);
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

                    if (tab.Rows.Count == 0)
                    {
                        return (true, replFieldName);
                    }

                    replFieldName = (string)tab.Rows[0]["OBJECT_GUID_FIELDNAME"];

                    if (tab.Rows[0]["PARENT_SESSION_GUID"] == DBNull.Value ||
                        tab.Rows[0]["PARENT_SESSION_GUID"].Equals(new Guid()))
                    {
                        return (true, replFieldName);
                    }

                    Guid parentSessionGuid = Convert2Guid(tab.Rows[0]["PARENT_SESSION_GUID"]);
                    command.CommandText = "SELECT " + db.DbColName("ID") + " FROM " + db.TableName("GV_CHECKOUT_SESSIONS") + " WHERE " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(parentSessionGuid);
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    object obj = command.ExecuteScalar();
                    return ((obj != null && obj.GetType() == typeof(int)), replFieldName);
                }
            }
            catch
            {
                return (false, replFieldName);
            }
        }

        async public static Task<int> FeatureClassReplicationID_RowID(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return -1;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return -1;
            }

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

                    if (obj != null && (obj.GetType() == typeof(int) || obj.GetType() == typeof(long)))
                    {
                        return Convert.ToInt32(obj);
                    }

                    return -1;
                }
            }
            catch
            {
                return -1;
            }
        }
        async public static Task<bool> FeatureClassHasReplications(IFeatureClass fc)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
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
        async public static Task<bool> FeatureClassCanReplicate(IFeatureClass fc)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return false;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
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
        async public static Task<int> FeatureClassGeneration(IFeatureClass fc)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return -1;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return -1;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return -1;
            }

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
                    {
                        return 0;
                    }

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
        async public static Task<List<Guid>> FeatureClassSessions(IFeatureClass fc)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return null;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return null;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return null;
            }

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
                        checkout_guids.Add(Convert2Guid(row["CHECKOUT_GUID"]));
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
            if (db == null)
            {
                return String.Empty;
            }

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
                    {
                        return String.Empty;
                    }

                    return obj.ToString();
                }
            }
            catch (Exception /*ex*/)
            {
                return String.Empty;
            }
        }
        private static VersionRights SessionParentRights(IFeatureDatabaseReplication db, Guid sessionGuid)
        {
            if (db == null)
            {
                return VersionRights.NONE;
            }

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
                    {
                        return VersionRights.NONE;
                    }

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
            if (db == null)
            {
                return VersionRights.NONE;
            }

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
                    {
                        return VersionRights.NONE;
                    }

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
            if (db == null)
            {
                return ConflictHandling.NORMAL;
            }

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
                    {
                        return ConflictHandling.NORMAL;
                    }

                    return (ConflictHandling)Convert.ToInt32(obj);
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return ConflictHandling.NORMAL;
            }
        }

        async public static Task<bool> FeatureClassHasConflicts(IFeatureClass fc)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return false;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
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
                    command.CommandText = "SELECT COUNT(" + db.DbColName("FC_ID") + ") AS fc_id_count FROM " + db.TableName("GV_CHECKOUT_CONFLICTS") + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id;

                    connection.Open();
                    object obj = command.ExecuteScalar();

                    if (obj == null || obj == DBNull.Value)
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return false;
            }
        }
        async public static Task<List<Guid>> FeatureClassConflictsParentGuids(IFeatureClass fc)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return null;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return null;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return null;
            }

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
                        guids.Add(Convert2Guid(row["GUIDS"]));
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
        async public static Task<Conflict> FeatureClassConflict(IFeatureClass fc, Guid objectGuid)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return null;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return null;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return null;
            }

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
                    if (adapter.Fill(tab) == 0)
                    {
                        return null;
                    }

                    DataRow row1 = tab.Rows[0];
                    Conflict conflict = new Conflict(
                        fc,
                        await FeatureByObjectGuid(fc, objectGuid),
                        objectGuid,
                        (SqlStatement)row1["PARENT_SQL_STATEMENT"],
                        (String)row1["PARENT_USER"],
                        (DateTime)row1["PARENT_DATUM"]);
                    conflict.rowIDs.Add(Convert.ToInt32(row1["ID"]));

                    foreach (DataRow row in tab.Rows)
                    {
                        SqlStatement cStatement = (SqlStatement)Convert.ToInt32(row["CONFLICT_SQL_STATEMENT"]);

                        IFeature feature = (cStatement != SqlStatement.DELETE) ?
                            await FeatureByObjectGuid(fc, Convert2Guid(row["CONFLICT_OBJECT_GUID"])) :
                            null;

                        ConflictFeature cFeature = new ConflictFeature(
                            feature,
                            Convert2Guid(row["CONFLICT_OBJECT_GUID"]),
                            (SqlStatement)row["CONFLICT_SQL_STATEMENT"],
                            (DateTime)row["CONFLICT_DATUM"],
                            (string)row["CONFLICT_CHECKOUT_NAME"],
                            (string)row["CONFLICT_USER"]);

                        conflict.ConflictFeatures.Add(cFeature);
                        conflict.rowIDs.Add(Convert.ToInt32(row["ID"]));
                    }

                    await conflict.Init();
                    return conflict;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return null;
            }
        }
        async public static Task<IFeature> FeatureByObjectGuid(IFeatureClass fc, Guid objectGuid)
        {
            try
            {
                string repl_field_name = await FeatureClassReplicationIDFieldname(fc);
                if (String.IsNullOrEmpty(repl_field_name))
                {
                    return null;
                }

                IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
                if (db == null)
                {
                    return null;
                }

                QueryFilter filter = new QueryFilter();
                filter.AddField("*");
                filter.WhereClause = db.DbColName(repl_field_name) + "=" + db.GuidToSql(objectGuid);

                using (IFeatureCursor cursor = await fc.GetFeatures(filter))
                {
                    return await cursor.NextFeature();
                }
            }
            catch
            {
                return null;
            }
        }

        async public static Task<string> FeatureClassCheckoutName(IFeatureClass fc)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return string.Empty;
            }

            int generation = await FeatureClassGeneration(fc);
            if (generation < 0)
            {
                return String.Empty;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return String.Empty;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return String.Empty;
            }

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
                    {
                        return string.Empty;
                    }

                    return obj.ToString();
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return String.Empty;
            }
        }
        async public static Task<IFeatureDatabaseReplication> FeatureClassDb(IFeatureClass fc)
        {
            if (!await FeatureClassHasRelicationID(fc))
            {
                return null;
            }

            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return null;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            return db;
        }
        async public static Task<IFeatureClass> FeatureClassParentFc(IFeatureClass fc)
        {
            try
            {
                IFeatureDatabaseReplication db = await FeatureClassDb(fc);
                if (db == null)
                {
                    throw new Exception("Featureclass has no replication functionallity");
                }
                int generation = await FeatureClassGeneration(fc);
                if (generation <= 0)
                {
                    throw new Exception("Can't determine generation");
                }
                int fc_id = await db.GetFeatureClassID(fc.Name);
                if (fc_id < 0)
                {
                    throw new Exception("Can't determine featureclass id");
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
                        throw new Exception("No session with generation " + generation + " defined");
                    }
                }

                System.Guid checkoutGuid = (System.Guid)tab.Rows[0]["CHECKOUT_GUID"];
                System.Guid dsGuid = (System.Guid)tab.Rows[0]["PARENT_DATASET_GUID"];
                string connectionString = tab.Rows[0]["PARENT_DATASET_CONNECTIONSTRING"].ToString();

                IDataset parentDs = PlugInManager.Create(dsGuid) as IDataset;
                if (parentDs == null)
                {
                    throw new Exception("Can't create dataset from guid");
                }
                await parentDs.SetConnectionString(connectionString);
                if (!await parentDs.Open())
                {
                    throw new Exception(parentDs.LastErrorMessage);
                }
                if (!(parentDs.Database is IFeatureDatabaseReplication))
                {
                    throw new Exception("Can't checkin to parent database type");
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
                    parameter.DbType = DbType.Guid;
                    parentDb.ModifyDbParameter(parameter);

                    command.Parameters.Add(parameter);

                    DbDataAdapter adapter = parentDb.ProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    if (adapter.Fill(tab2) != 1)
                    {
                        throw new Exception("No parent session with generation " + (generation - 1).ToString() + " defined");
                    }
                }
                string parendFcName = await parentDb.GetFeatureClassName(Convert.ToInt32(tab2.Rows[0]["FC_ID"]));
                if (String.IsNullOrEmpty(parendFcName))
                {
                    throw new Exception("Can't determine parent featureclass name");
                }

                IDatasetElement element = await parentDs.Element(parendFcName);
                if (element == null || !(element.Class is IFeatureClass))
                {
                    throw new Exception("Can't determine parent featureclass '" + parendFcName + "'");
                }

                return element.Class as IFeatureClass;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception-" + ex.Message + "\n" + ex.StackTrace);
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
            {
                feature.Fields.Add(new FieldValue(replicationFieldName));
            }

            // neue GUID nur vergeben, wenn noch keine vorhanden ist...
            object objectGuid = feature[replicationFieldName];
            if (objectGuid == null || objectGuid == System.DBNull.Value || objectGuid.Equals(new Guid()))
            {
                feature[replicationFieldName] = System.Guid.NewGuid();
            }

            if (!(feature[replicationFieldName] is Guid))
            {
                feature[replicationFieldName] = new Guid(feature[replicationFieldName].ToString());
            }
        }
        async public static Task<Guid> FeatureObjectGuid(IFeatureClass fc, IFeature feature, string replicationFieldName)
        {
            IDatabaseNames dn = (fc != null && fc.Dataset != null) ? fc.Dataset.Database as IDatabaseNames : null;

            QueryFilter filter = new QueryFilter();
            filter.AddField(replicationFieldName);
            filter.WhereClause = (dn != null ? dn.DbColName(fc.IDFieldName) : fc.IDFieldName) + "=" + feature.OID;

            using (IFeatureCursor cursor = await fc.GetFeatures(filter) as IFeatureCursor)
            {
                IFeature feat = await cursor.NextFeature();
                return Convert2Guid(feat[replicationFieldName]);
            }
        }
        public static Task<bool> WriteDifferencesToTable(IFeatureClass fc, System.Guid objectGuid, SqlStatement statement, ReplicationTransaction replTrans)
        {
            return WriteDifferencesToTable(
                fc,
                objectGuid,
                statement,
                replTrans,
                ((fc is FeatureClassDifferenceDateTime) ?
                    ((FeatureClassDifferenceDateTime)fc).DiffTime :
                    DateTime.Now));
        }
        async private static Task<bool> WriteDifferencesToTable(IFeatureClass fc, System.Guid objectGuid, SqlStatement statement, ReplicationTransaction replTrans, DateTime td)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return false;
            }

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
                        Guid checkout_guid = Convert2Guid(reader.GetValue(0));
                        int repl_state = Convert.ToInt32(reader.GetValue(1));
                        LockState lock_state = (LockState)(Convert.ToInt32(reader.GetValue(2)));
                        Guid runningReplicationGuid = new Guid();

                        if (fc is FeatureClassDiffLocks)
                        {
                            if (checkout_guid.Equals(((FeatureClassDiffLocks)fc).SessionGUID))
                            {
                                continue;
                            }
                        }
                        if (fc is FeatureClassDifferenceDateTime)
                        {
                            if (checkout_guid.Equals(((FeatureClassDifferenceDateTime)fc).SessionGUID) &&
                                ((FeatureClassDifferenceDateTime)fc).ReplicationState >= 0)
                            {
                                repl_state = ((FeatureClassDifferenceDateTime)fc).ReplicationState;
                            }

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
                                    throw new Exception("Session is hardlocked, no updates possible. Try in again in few seconds...");
                                }
                                // Konflikte testen
                                switch (HasLocksOrConflicts(db, fc_id, objectGuid, runningReplicationGuid))
                                {
                                    case LockFeatureType.ConflictFeatureLock:
                                        throw new Exception("Can't update feature. Solve conflict first...");
                                    case LockFeatureType.CheckinReconileLock:
                                        throw new Exception("Can't update feature. Feature is locked for checkin/reconcile. Try later again.");
                                    case LockFeatureType.PrivateCheckinReconcileLock:
                                        // kein echte lock -> ok, weitermachen
                                        break;
                                }
                                break;
                            case SqlStatement.DELETE:
                                if (lock_state == LockState.Hardlock)
                                {
                                    throw new Exception("Session is hardlocked, no delete possible. Try in again in few seconds...");
                                }
                                // Konflikte testen
                                switch (HasLocksOrConflicts(db, fc_id, objectGuid, runningReplicationGuid))
                                {
                                    case LockFeatureType.ConflictFeatureLock:
                                        throw new Exception("Can't delete feature. Solve conflict first...");
                                    case LockFeatureType.CheckinReconileLock:
                                        throw new Exception("Can't delete feature. Feature is locked for checkin/reconcile. Try later again.");
                                    case LockFeatureType.PrivateCheckinReconcileLock:
                                        // kein echte lock -> ok, weitermachen
                                        break;
                                }
                                break;
                        }

                        if (!db.InsertRow("GV_CHECKOUT_DIFFERENCE", row, replTrans))
                        {
                            throw new Exception(db.LastErrorMessage);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region old_functions
        /*
        private static bool WriteDifferencesToTable_old(IFeatureClass fc, System.Guid objectGuid, SqlStatement statement, ReplicationTransaction replTrans, DateTime td, out string errMsg)
        {
            errMsg=String.Empty;

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
                    command.CommandText = "SELECT CHECKOUT_GUID FROM GV_CHECKOUT_SESSIONS WHERE FC_ID=" + fc_id;
                    connection.Open();

                    DbDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Guid checkout_guid = (Guid)reader.GetValue(0);
                        if (fc is FeatureClassDiffLocks)
                        {
                            if(checkout_guid.Equals(((FeatureClassDiffLocks)fc).SessionGUID))
                                continue;
                        }

                        Guid transaction_guid = Guid.NewGuid();
                        Row row = new Row();
                        switch (statement)
                        {
                            case SqlStatement.INSERT:
                                row.Fields.Add(new FieldValue("CHECKOUT_GUID", checkout_guid));
                                row.Fields.Add(new FieldValue("DIFF_DATUM", td));
                                row.Fields.Add(new FieldValue("CHECKOUT_SESSION_USER", username));
                                row.Fields.Add(new FieldValue("OBJECT_GUID", objectGuid));
                                row.Fields.Add(new FieldValue("SQL_STATEMENT", statement));
                                row.Fields.Add(new FieldValue("TRANSACTION_ID",transaction_guid));
                                if (!db.InsertRow("GV_CHECKOUT_DIFFERENCE", row, replTrans))
                                {
                                    errMsg = db.lastErrorMsg;
                                    return false;
                                }
                                break;
                            case SqlStatement.UPDATE:
                                // Konflikte testen
                                if (HasLocksOrConflicts(db, fc_id, objectGuid))
                                {
                                    errMsg = "Can't update feature. Solve conflict first...";
                                    return false;
                                }

                                if (!WriteDifferencesToTable_UPDATE(replTrans, db, checkout_guid, objectGuid, transaction_guid, username, td, out errMsg))
                                    return false;
                                break;
                            case SqlStatement.DELETE:
                                // Konflikte testen
                                if (HasLocksOrConflicts(db, fc_id, objectGuid))
                                {
                                    errMsg = "Can't delete feature. Solve conflict first...";
                                    return false;
                                }
                                // erst überprüfen ob objekt schon in der Difftable steht!
                                // wenn ja, einträge Löschen und nix schreiben.
                                using (DbConnection connection3 = db.ProviderFactory.CreateConnection())
                                {
                                    connection3.ConnectionString = db.DatabaseConnectionString;
                                    DbCommand command3 = db.ProviderFactory.CreateCommand();
                                    command3.CommandText = "SELECT MIN(SQL_STATEMENT) AS STAT FROM GV_CHECKOUT_DIFFERENCE WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID";
                                    DbParameter p3 = db.ProviderFactory.CreateParameter();
                                    p3.ParameterName = "@CHECKOUT_GUID";
                                    p3.Value = checkout_guid;
                                    DbParameter p4 = db.ProviderFactory.CreateParameter();
                                    p4.ParameterName = "@OBJECT_GUID";
                                    p4.Value = objectGuid;
                                    command3.Parameters.Add(p3);
                                    command3.Parameters.Add(p4);

                                    object obj3 = null;
                                    if (replTrans != null && replTrans.IsValid)
                                    {
                                        obj3 = replTrans.ExecuteScalar(command3);
                                    }
                                    else
                                    {
                                        connection3.Open();
                                        command3.Connection = connection3;
                                        obj3 = command3.ExecuteScalar();
                                    }
                                    if (obj3 != null && obj3 != System.DBNull.Value)
                                    {
                                        command3.CommandText = "DELETE FROM GV_CHECKOUT_DIFFERENCE WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID";
                                        if (replTrans != null && replTrans.IsValid)
                                        {
                                            replTrans.ExecuteNonQuery(command3);
                                        }
                                        else
                                        {
                                            command3.ExecuteNonQuery();
                                        }
                                        // wenn insert dieses Objektes schon im diff table steht
                                        // braucht delete auch nicht mehr geschrieben werden...
                                        if (Convert.ToInt32(obj3) == (int)SqlStatement.INSERT)
                                            break;
                                    }
                                }
                                row.Fields.Add(new FieldValue("CHECKOUT_GUID", checkout_guid));
                                row.Fields.Add(new FieldValue("DIFF_DATUM", td));
                                row.Fields.Add(new FieldValue("CHECKOUT_SESSION_USER", username));
                                row.Fields.Add(new FieldValue("OBJECT_GUID", objectGuid));
                                row.Fields.Add(new FieldValue("SQL_STATEMENT", statement));
                                row.Fields.Add(new FieldValue("TRANSACTION_ID", transaction_guid));
                                if (!db.InsertRow("GV_CHECKOUT_DIFFERENCE", row,replTrans))
                                {
                                    errMsg = db.lastErrorMsg;
                                    return false;
                                }
                                break;
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
        private static bool WriteDifferencesToTable_UPDATE(IFeatureDatabaseReplication db, Guid checkout_guid, Guid objectGuid, Guid transaction_guid, string username, DateTime td, out string errMsg)
        {
            errMsg = String.Empty;

            // erst überprüfen ob objekt schon in der Difftable steht!
            // wenn ja, dann braucht nix eingetragen werden.
            using (DbConnection connection = db.ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = db.DatabaseConnectionString;
                DbCommand command2 = db.ProviderFactory.CreateCommand();
                command2.CommandText = "SELECT COUNT(ID) AS ID_COUNT FROM GV_CHECKOUT_DIFFERENCE WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID";
                DbParameter p1 = db.ProviderFactory.CreateParameter();
                p1.ParameterName = "@CHECKOUT_GUID";
                p1.Value = checkout_guid;
                DbParameter p2 = db.ProviderFactory.CreateParameter();
                p2.ParameterName = "@OBJECT_GUID";
                p2.Value = objectGuid;
                command2.Parameters.Add(p1);
                command2.Parameters.Add(p2);

                object obj2 = null;
                connection.Open();
                command2.Connection = connection;
                obj2 = command2.ExecuteScalar();

                if (obj2 != null && Convert.ToInt32(obj2) != 0)
                {
                    // bei einem Udpate einfach das aktuelle Datum für alle Differences setzen...
                    if (connection.GetType().Equals(typeof(System.Data.OleDb.OleDbConnection)))
                    {
                        // weil: Access schreibt nix, wenn Datum als Parameter übergeben wird...
                        command2.CommandText = "UPDATE GV_CHECKOUT_DIFFERENCE SET DIFF_DATUM='" + td.ToShortDateString() + " " + td.ToLongTimeString() + "' WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID";
                    }
                    else
                    {
                        command2.CommandText = "UPDATE GV_CHECKOUT_DIFFERENCE SET DIFF_DATUM=@DIFF_DATUM WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID";
                        DbParameter p11 = db.ProviderFactory.CreateParameter();
                        p11.ParameterName = "@DIFF_DATUM";
                        p11.Value = td;
                        command2.Parameters.Add(p11);
                    }
                    command2.ExecuteNonQuery();
                    return true;
                }
            }
            Row row = new Row();
            row.Fields.Add(new FieldValue("CHECKOUT_GUID", checkout_guid));
            row.Fields.Add(new FieldValue("DIFF_DATUM", td));
            row.Fields.Add(new FieldValue("CHECKOUT_SESSION_USER", username));
            row.Fields.Add(new FieldValue("OBJECT_GUID", objectGuid));
            row.Fields.Add(new FieldValue("SQL_STATEMENT", SqlStatement.UPDATE));
            row.Fields.Add(new FieldValue("TRANSACTION_ID", transaction_guid));
            if (!db.InsertRow("GV_CHECKOUT_DIFFERENCE", row, null))
            {
                errMsg = db.lastErrorMsg;
                return false;
            }
            return true;
        }
        private static bool WriteDifferencesToTable_UPDATE(ReplicationTransaction replTrans, IFeatureDatabaseReplication db, Guid checkout_guid, Guid objectGuid, Guid transaction_guid, string username, DateTime td, out string errMsg)
        {
            errMsg = String.Empty;

            if (replTrans == null || !replTrans.IsValid)
                return WriteDifferencesToTable_UPDATE(db, checkout_guid, objectGuid, transaction_guid, username, td, out errMsg);

            //if exists (SELECT ID FROM GV_CHECKOUT_DIFFERENCE WHERE CHECKOUT_GUID='c2a906d6-0fec-48ab-9e52-f1bc92a4cb85' AND OBJECT_GUID='c93c3871-bb7f-4517-80f6-14c6f5ec2e76')
            //   UPDATE DATE
            //else
            //   INSERT NEW TRANSACTION

            Row row = new Row();
            row.Fields.Add(new FieldValue("CHECKOUT_GUID", checkout_guid));
            row.Fields.Add(new FieldValue("DIFF_DATUM", td));
            row.Fields.Add(new FieldValue("CHECKOUT_SESSION_USER", username));
            row.Fields.Add(new FieldValue("OBJECT_GUID", objectGuid));
            row.Fields.Add(new FieldValue("SQL_STATEMENT", SqlStatement.UPDATE));
            row.Fields.Add(new FieldValue("TRANSACTION_ID", transaction_guid));

            DbCommand command = db.ProviderFactory.CreateCommand();
            //command.CommandText  ="if exists (SELECT ID FROM GV_CHECKOUT_DIFFERENCE WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID) ";
            //command.CommandText += "UPDATE GV_CHECKOUT_DIFFERENCE SET DIFF_DATUM=@DIFF_DATUM WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID ";
            //command.CommandText += "ELSE ";
            command.CommandText = "DELETE FROM GV_CHECKOUT_DIFFERENCE WHERE CHECKOUT_GUID=@CHECKOUT_GUID AND OBJECT_GUID=@OBJECT_GUID AND SQL_STATEMENT=" + (int)SqlStatement.UPDATE;
            
            //AppendInsertInto(db, command, "GV_CHECKOUT_DIFFERENCE", row);

            DbParameter p1 = db.ProviderFactory.CreateParameter();
            p1.ParameterName = "@CHECKOUT_GUID";
            p1.Value = checkout_guid;
            DbParameter p2 = db.ProviderFactory.CreateParameter();
            p2.ParameterName = "@OBJECT_GUID";
            p2.Value = objectGuid;
            command.Parameters.Add(p1);
            command.Parameters.Add(p2);
            //DbParameter p3 = db.ProviderFactory.CreateParameter();
            //p3.ParameterName = "@DIFF_DATUM";
            //p3.Value = td;
            //command.Parameters.Add(p3);

            //replTrans.ExecuteNonQuery(command);
            db.InsertRow("GV_CHECKOUT_DIFFERENCE", row, replTrans);

            return true;
        }
        */
        #endregion
        private enum LockFeatureType { None = 0, ConflictFeatureLock = 1, CheckinReconileLock = 2, PrivateCheckinReconcileLock = 3 }
        private static LockFeatureType HasLocksOrConflicts(IFeatureDatabaseReplication db, int fc_id, Guid object_guid, Guid runningReplicationSessionGuid)
        {
            if (db == null)
            {
                return LockFeatureType.None;
            }

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
                    Guid repl_checkout_guid = Convert2Guid(reader.GetValue(1));

                    if (!repl_checkout_guid.Equals(runningReplicationSessionGuid))
                    {
                        return LockFeatureType.CheckinReconileLock;
                    }
                }
                reader.Close();
                if (lType != LockFeatureType.None)
                {
                    return lType;
                }

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

                if (fields.Length != 0)
                {
                    fields.Append(",");
                }

                if (parameters.Length != 0)
                {
                    parameters.Append(",");
                }

                DbParameter parameter = db.ProviderFactory.CreateParameter();
                parameter.ParameterName = "@param_" + Guid.NewGuid().ToString("N"); ;
                parameter.Value = fv.Value;
                db.ModifyDbParameter(parameter);

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
        async public Task<(bool sucess, string errMsg)> Process(IFeatureClass parentFc, IFeatureClass childFc, ProcessType type, IDatumTransformations datumTransformations)
        {
            string errMsg = String.Empty;
            CheckinConst c = new CheckinConst();

            try
            {
                if (!await c.Init(this, parentFc, childFc, true))
                {
                    return (false, "Unknown Error");
                }

                if (!await CheckForLockedFeatureclassSessions(parentFc, c.checkout_guid, 60))
                {
                    return (false, "Unknown Error");
                }

                if (!await CheckForLockedFeatureclassSessions(childFc, c.checkout_guid, 60))
                {
                    return (false, "Unknown Error");
                }

                await LockReplicationSession(parentFc, c.checkout_guid, LockState.Hardlock);
                await LockReplicationSession(childFc, c.checkout_guid, LockState.Hardlock);

                if (!await CheckForLockedFeatureclassSessions(parentFc, c.checkout_guid))
                {
                    return (false, "Unknown Error");
                }

                if (!await CheckForLockedFeatureclassSessions(childFc, c.checkout_guid))
                {
                    return (false, "Unknown Error");
                }

                await IncReplicationState(parentFc, c.checkout_guid);
                await IncReplicationState(childFc, c.checkout_guid);
                // 0,5 sec warten, damit letzte Änderungen, die mit alter 
                // StateID geschrieben werden in der Datenbank landen...
                System.Threading.Thread.Sleep(500);

                #region Thin Difference Tables
                if (!ThinDifferencesTable(c.childDb, c.checkout_guid, c.childReplicationState, out errMsg))
                {
                    return (false, "Unknown Error");
                }

                if (!ThinDifferencesTable(c.parentDb, c.checkout_guid, c.parentReplactionState, out errMsg))
                {
                    return (false, "Unknown Error");
                }
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
                    parameter.DbType = DbType.Guid;
                    c.childDb.ModifyDbParameter(parameter);

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
                        parameter2.DbType = DbType.Guid;
                        c.parentDb.ModifyDbParameter(parameter2);

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
                        parameter.DbType = DbType.Guid;
                        c.parentDb.ModifyDbParameter(parameter);

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
                        parameter2.DbType = DbType.Guid;
                        c.childDb.ModifyDbParameter(parameter2);

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

                await LockReplicationSession(parentFc, c.checkout_guid, LockState.Softlock);
                await LockReplicationSession(childFc, c.checkout_guid, LockState.Softlock);

                var checkInResult = await CheckIn(parentFc, childFc, c, datumTransformations);

                if (!checkInResult.success)
                {
                    throw new Exception("ERROR ON CHECKIN:\n" + checkInResult.errMsg);
                }

                switch (type)
                {
                    case ProcessType.CheckinAndRelease:
                        if (!await ReleaseVersion(parentFc, childFc))
                        {
                            throw new Exception("ERROR ON RELEASE VERSION:\n");
                        }
                        break;
                    case ProcessType.Reconcile:
                        c.SwapParentChild();

                        var postResult = await Post(parentFc, childFc, c, datumTransformations);

                        if (!postResult.success)
                        {
                            throw new Exception("ERROR ON POST:\n" + postResult.errMsg);
                        }
                        if (!await Replication.InsertCheckoutLocks(parentFc, childFc))
                        {
                            throw new Exception("ERROR ON INSERT LOCKS: ");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception: " + ex.Message);
            }
            finally
            {
                await RemoveReplicationLocks(childFc, c.checkout_guid, c.childReplicationState);
                await RemoveReplicationLocks(parentFc, c.checkout_guid, c.parentReplactionState);

                await LockReplicationSession(childFc, c.checkout_guid, LockState.Unlock);
                await LockReplicationSession(parentFc, c.checkout_guid, LockState.Unlock);
            }
            return (true, errMsg);
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

            async public Task<bool> Init(Replication repl, IFeatureClass parentFc, IFeatureClass childFc, bool checkin)
            {
                #region Initalisierung

                parentDb = await FeatureClassDb(parentFc);
                if (parentDb == null)
                {
                    throw new Exception("Can't checkin to parent database...");
                }
                if (!await CreateRelicationModel(parentDb))
                {
                    return false;
                }
                childDb = await FeatureClassDb(childFc);
                if (childDb == null)
                {
                    throw new Exception("Can't checkout from child database...");
                }

                parentFc_id = await parentDb.GetFeatureClassID(parentFc.Name);
                if (parentFc_id < 0)
                {
                    throw new Exception("Can't determine parent featureclass id...");
                }
                childFc_id = await childDb.GetFeatureClassID(childFc.Name);
                if (childFc_id < 0)
                {
                    throw new Exception("Can't determine child featureclass id...");
                }
                child_generation = await FeatureClassGeneration(childFc);
                if (checkin)
                {
                    if (child_generation <= 0)
                    {
                        throw new Exception("Can't determine child featureclass generation");
                    }
                }
                else
                {
                    if (child_generation < 0)
                    {
                        throw new Exception("Can't determine child featureclass generation");
                    }
                }
                parent_generation = await FeatureClassGeneration(parentFc);
                if (checkin)
                {
                    if (parent_generation != child_generation - 1)
                    {
                        throw new Exception("Can't determine parent featureclass generation");
                    }
                }
                else
                {
                    if (parent_generation != child_generation + 1)
                    {
                        throw new Exception("Can't determine parent featureclass generation");
                    }
                }
                child_repl_id_fieldname = await FeatureClassReplicationIDFieldname(childFc);
                if (String.IsNullOrEmpty(child_repl_id_fieldname))
                {
                    throw new Exception("Can't determine child featureclass replication ID fieldname");
                }
                parent_repl_id_fieldname = await FeatureClassReplicationIDFieldname(parentFc);
                if (String.IsNullOrEmpty(parent_repl_id_fieldname))
                {
                    throw new Exception("Can't determine parent featureclass replication ID fieldname");
                }
                checkout_name = await FeatureClassCheckoutName(childFc);
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


                parentReplactionState = await repl.GetReplicationState(parentFc, checkout_guid);
                if (parentReplactionState == -1)
                {
                    throw new Exception("Can't determine parent replication state");
                }
                childReplicationState = await repl.GetReplicationState(childFc, checkout_guid);
                if (childReplicationState == -1)
                {
                    throw new Exception("Can't determine child replication state");
                }

                parentLocked = await repl.IsReplicationSessionLocked(parentFc, checkout_guid);
                childLocked = await repl.IsReplicationSessionLocked(childFc, checkout_guid);

                switch (parentLocked)
                {
                    case LockState.Error:
                        throw new Exception("Can't determine parent lock state");
                    case LockState.Hardlock:
                    case LockState.Softlock:
                        throw new Exception("Parent Session is locked...");
                }

                switch (childLocked)
                {
                    case LockState.Error:
                        throw new Exception("Can't determine child lock state");
                    case LockState.Hardlock:
                    case LockState.Softlock:
                        throw new Exception("Child Session is locked...");
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

        async private Task<(bool success, string errMsg)> CheckIn(IFeatureClass parentFc, IFeatureClass childFc, CheckinConst c, IDatumTransformations datumTransformations)
        {
            if (CheckIn_BeginCheckIn != null)
            {
                CheckIn_BeginCheckIn(this);
            }

            return await CheckIn(parentFc, childFc, true, c, datumTransformations);
        }
        async private Task<(bool success, string errMsg)> Post(IFeatureClass parentFc, IFeatureClass childFc, CheckinConst c, IDatumTransformations datumTransformations)
        {
            if (CheckIn_BeginPost != null)
            {
                CheckIn_BeginPost(this);
            }

            return await CheckIn(childFc, parentFc, false, c, datumTransformations);
        }

        async public Task<bool> ReleaseVersion(IFeatureClass parentFc, IFeatureClass childFc)
        {
            try
            {
                #region Initalisierung
                IFeatureDatabaseReplication parentDb = await FeatureClassDb(parentFc);
                if (parentDb == null)
                {
                    throw new Exception("Can't checkin to parent database...");
                }
                IFeatureDatabaseReplication childDb = await FeatureClassDb(childFc);
                if (childDb == null)
                {
                    throw new Exception("Can't checkout from child database...");
                }
                int childFc_id = await childDb.GetFeatureClassID(childFc.Name);
                if (childFc_id < 0)
                {
                    throw new Exception("Can't determine child featureclass id...");
                }
                int child_generation = await FeatureClassGeneration(childFc);
                if (child_generation <= 0)
                {
                    throw new Exception("Can't determine child featureclass generation");
                }
                int parent_generation = await FeatureClassGeneration(parentFc);
                if (parent_generation != child_generation - 1)
                {
                    throw new Exception("Can't determine parent featureclass generation");
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
                    parameter.DbType = DbType.Guid;
                    childDb.ModifyDbParameter(parameter);

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
                    parameter.DbType = DbType.Guid;
                    parentDb.ModifyDbParameter(parameter);

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
                throw new Exception("Exception: " + ex.Message);
            }
        }
        async public Task<bool> RemoveReplicationLocks(IFeatureClass fc, Guid checkout_guid, int replState)
        {
            if (fc == null ||
                fc.Dataset == null ||
                !(fc.Dataset.Database is IFeatureDatabaseReplication))
            {
                return false;
            }

            IFeatureDatabaseReplication db = fc.Dataset.Database as IFeatureDatabaseReplication;
            int fc_id = await db.GetFeatureClassID(fc.Name);
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
        async private Task<(bool success, string errMsg)> CheckIn(IFeatureClass parentFc, IFeatureClass childFc, bool checkin, CheckinConst c, IDatumTransformations datumTransformations)
        {
            string errMsg = String.Empty;
            if (parentFc == null || childFc == null || c == null)
            {
                return (false, errMsg);
            }

            if (!await CheckForLockedFeatureclassSessions(parentFc, c.checkout_guid))
            {
                return (false, errMsg);
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
                    parameter.DbType = DbType.Guid;
                    c.childDb.ModifyDbParameter(parameter);

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
                    parameter.DbType = DbType.Guid;
                    c.parentDb.ModifyDbParameter(parameter);

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
                    Guid object_guid = Convert2Guid(childDiffRow["OBJECT_GUID"]);
                    parentFc_diff.DiffTime = (DateTime)childDiffRow["DIFF_DATUM"];
                    parentFc_lock.DiffTime = (DateTime)childDiffRow["DIFF_DATUM"];

                    switch ((Replication.SqlStatement)Convert.ToInt32(childDiffRow["SQL_STATEMENT"]))
                    {
                        case SqlStatement.INSERT:
                            if (!Bit.Has(c.versionRights, VersionRights.INSERT))
                            {
                                if (CheckIn_IgnoredSqlStatement != null)
                                {
                                    CheckIn_IgnoredSqlStatement(this, ++count_ignored, SqlStatement.INSERT);
                                }

                                break;
                            }

                            #region Insert
                            child_feature = await GetFeatureByObjectGuid(c.childDb, childFc, c.child_repl_id_fieldname, object_guid, parentSRef, datumTransformations);
                            if (child_feature == null)
                            {
                                // dürte eigentlich nicht vorkommen
                                continue;
                            }
                            if (!await c.parentDb.Insert(parentFc_lock, child_feature))
                            {
                                errMsg = "Error on INSERT Feature:\n" + c.parentDb.LastErrorMessage;
                                return (false, errMsg);
                            }
                            if (CheckIn_FeatureInserted != null)
                            {
                                CheckIn_FeatureInserted(this, ++count_inserted);
                            }

                            break;
                        #endregion
                        case SqlStatement.UPDATE:
                            if (!Bit.Has(c.versionRights, VersionRights.UPDATE))
                            {
                                if (CheckIn_IgnoredSqlStatement != null)
                                {
                                    CheckIn_IgnoredSqlStatement(this, ++count_ignored, SqlStatement.UPDATE);
                                }

                                break;
                            }

                            #region Update
                            child_feature = await GetFeatureByObjectGuid(c.childDb, childFc, c.child_repl_id_fieldname, object_guid, parentSRef, datumTransformations);
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
                                    if (!await c.parentDb.Insert(parentFc_diff, child_feature))
                                    {
                                        errMsg = "Error on INSERT Feature:\n" + c.parentDb.LastErrorMessage;
                                        return (false, errMsg);
                                    }
                                    if (CheckIn_FeatureInserted != null)
                                    {
                                        CheckIn_FeatureInserted(this, ++count_inserted);
                                    }

                                    break;
                                }
                                else if (c.conflictHandling == ConflictHandling.NORMAL)
                                {
                                    child_feature[c.child_repl_id_fieldname] = null;
                                    if (c.parentDb is IFeatureDatabaseCloudReplication)
                                    {
                                        AllocateNewObjectGuid(child_feature, c.child_repl_id_fieldname);
                                    }

                                    if (!await c.parentDb.Insert(parentFc_diff, child_feature))
                                    {
                                        errMsg = "Error on INSERT Feature:\n" + c.parentDb.LastErrorMessage;
                                        return (false, errMsg);
                                    }
                                    if (CheckIn_FeatureInserted != null)
                                    {
                                        CheckIn_FeatureInserted(this, ++count_inserted);
                                    }

                                    WriteConflict(c.parentDb, c.parentFc_id, c.checkout_name, diff, childDiffRow, Convert2Guid(child_feature[c.child_repl_id_fieldname]));
                                    if (CheckIn_ConflictDetected != null)
                                    {
                                        CheckIn_ConflictDetected(this, ++count_conflicts);
                                    }

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

                            parent_feature = await GetFeatureByObjectGuid(c.parentDb, parentFc, c.parent_repl_id_fieldname, object_guid, parentSRef, datumTransformations);
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
                                    if (!await c.parentDb.Insert(parentFc_lock, child_feature))
                                    {
                                        errMsg = "Error on INSERT Feature:\n" + c.parentDb.LastErrorMessage;
                                        return (false, errMsg);
                                    }
                                    if (CheckIn_FeatureInserted != null)
                                    {
                                        CheckIn_FeatureInserted(this, ++count_inserted);
                                    }

                                    break;
                                }
                            }
                            Feature.CopyFrom(parent_feature, child_feature);
                            if (!await c.parentDb.Update(parentFc_lock, parent_feature))
                            {
                                errMsg = "Error on UPDATE Feature:\n" + c.parentDb.LastErrorMessage;
                                return (false, errMsg);
                            }
                            if (CheckIn_FeatureUpdated != null)
                            {
                                CheckIn_FeatureUpdated(this, ++count_updated);
                            }

                            break;
                        #endregion
                        case SqlStatement.DELETE:
                            if (!Bit.Has(c.versionRights, VersionRights.DELETE))
                            {
                                if (CheckIn_IgnoredSqlStatement != null)
                                {
                                    CheckIn_IgnoredSqlStatement(this, ++count_ignored, SqlStatement.DELETE);
                                }

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
                                        if (CheckIn_ConflictDetected != null)
                                        {
                                            CheckIn_ConflictDetected(this, ++count_conflicts);
                                        }
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

                            parent_feature = await GetFeatureByObjectGuid(c.parentDb, parentFc, c.parent_repl_id_fieldname, object_guid, parentSRef, datumTransformations);
                            if (parent_feature == null)
                            {
                                // dürfe nicht sein, außer Feature wurde gelöscht
                                // dann sollte es allerdings im diff table sichtbar sein!!!
                                //throw new Exception("inkonsistent data!");
                                break;
                            }
                            if (!await c.parentDb.Delete(parentFc_lock, parent_feature.OID))
                            {
                                errMsg = "Error on DELETE Feature:\n" + c.parentDb.LastErrorMessage;
                                return (false, errMsg);
                            }
                            if (CheckIn_FeatureDeleted != null)
                            {
                                CheckIn_FeatureDeleted(this, ++count_deleted);
                            }

                            break;
                            #endregion
                    }

                    c.childDb.DeleteRows("GV_CHECKOUT_DIFFERENCE", c.childDb.DbColName("TRANSACTION_ID") + "=" + c.childDb.GuidToSql(Convert2Guid(childDiffRow["TRANSACTION_ID"])), null);
                    c.childDb.DeleteRows("GV_CHECKOUT_LOCKS", c.childDb.DbColName("OBJECT_GUID") + "=" + c.childDb.GuidToSql(object_guid) + " AND " + c.childDb.DbColName("REPLICATION_LOCK") + "=0 AND " + c.childDb.DbColName("CHECKOUT_GUID") + "=" + c.childDb.GuidToSql(c.checkout_guid) + " AND " + c.childDb.DbColName("REPLICATION_STATE") + "=" + Convert.ToInt32(childDiffRow["REPLICATION_STATE"]), null);
                    c.parentDb.DeleteRows("GV_CHECKOUT_LOCKS", c.childDb.DbColName("OBJECT_GUID") + "=" + c.parentDb.GuidToSql(object_guid) + " AND " + c.childDb.DbColName("REPLICATION_LOCK") + "=1 AND " + c.childDb.DbColName("CHECKOUT_GUID") + "=" + c.parentDb.GuidToSql(c.checkout_guid) + " AND " + c.childDb.DbColName("REPLICATION_STATE") + "=" + c.parentReplactionState, null);
                }
                #endregion

                return (true, errMsg);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
                return (false, errMsg);
            }
            finally
            {

            }
        }
        async private Task<IFeature> GetFeatureByObjectGuid(
                IFeatureDatabaseReplication db, 
                IFeatureClass fc, 
                string replicGuidFieldName, 
                Guid guid, 
                ISpatialReference destSRef,
                IDatumTransformations datumTransformations)
        {
            QueryFilter filter = new QueryFilter();
            filter.AddField("*");
            filter.WhereClause = (db != null ? db.DbColName(replicGuidFieldName) : replicGuidFieldName) + "=" + db.GuidToSql(guid);
            filter.SetFeatureSpatialReference(destSRef, datumTransformations);

            using (IFeatureCursor cursor = await fc.GetFeatures(filter))
            {
                if (cursor == null)
                {
                    return null;
                }

                return await cursor.NextFeature();
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
                    parameter.DbType = DbType.Guid;
                    db.ModifyDbParameter(parameter);

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
                        p1.DbType = DbType.Guid;
                        db.ModifyDbParameter(p1);

                        DbParameter p2 = db.ProviderFactory.CreateParameter();
                        p2.ParameterName = "@OBJECT_GUID";
                        p2.Value = diffRow["OBJECT_GUID"];
                        db.ModifyDbParameter(p2);

                        command2.Parameters.Add(p1);
                        command2.Parameters.Add(p2);
                        command2.Connection = connection;

                        DataTable diffs = new DataTable();
                        adapter.SelectCommand = command2;
                        adapter.Fill(diffs);

                        SqlStatement first = (SqlStatement)diffs.Rows[0]["SQL_STATEMENT"];
                        int firstID = Convert.ToInt32(diffs.Rows[0]["ID"]);
                        SqlStatement last = (SqlStatement)diffs.Rows[diffs.Rows.Count - 1]["SQL_STATEMENT"];
                        int lastID = Convert.ToInt32(diffs.Rows[diffs.Rows.Count - 1]["ID"]);

                        if (connection.State != ConnectionState.Open)
                        {
                            connection.Open();
                        }

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
        async private Task<bool> LockReplicationSession(IFeatureClass fc, Guid session_guid, LockState lockState)
        {
            IFeatureDatabaseReplication db = await FeatureClassDb(fc);
            if (db == null)
            {
                return false;
            }

            int fc_id = await db.GetFeatureClassID(fc.Name);
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
                    command.CommandText = "UPDATE " + db.TableName("GV_CHECKOUT_SESSIONS") + " SET " + db.DbColName("REPLICATION_LOCKSTATE") + "=" + (int)lockState + " WHERE " + db.DbColName("FC_ID") + "=" + fc_id + " AND " + db.DbColName("CHECKOUT_GUID") + "=" + db.GuidToSql(session_guid);
                    command.Connection = connection;
                    connection.Open();

                    command.ExecuteNonQuery();
                }
                if (CheckIn_ChangeSessionLockState != null)
                {
                    CheckIn_ChangeSessionLockState(this, fc.Name, session_guid, lockState);
                }

                return true;
            }
            catch { return false; }
        }
        async private Task<LockState> IsReplicationSessionLocked(IFeatureClass fc, Guid session_guid)
        {
            IFeatureDatabaseReplication db = await FeatureClassDb(fc);
            if (db == null)
            {
                return LockState.Error;
            }

            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return LockState.Error;
            }

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
                    if (obj != null && (obj.GetType() == typeof(int) || obj.GetType() == typeof(long)))
                    {
                        return (LockState)(Convert.ToInt32(obj));
                    }

                    return LockState.Error;
                }
            }
            catch (Exception)
            {
                return LockState.Error;
            }
        }

        async public Task<int> GetReplicationState(IFeatureClass fc, Guid session_guid)
        {
            IFeatureDatabaseReplication db = await FeatureClassDb(fc);
            if (db == null)
            {
                return -1;
            }

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
                    if (obj != null && (obj.GetType() == typeof(int) || obj.GetType() == typeof(long)))
                    {
                        return Convert.ToInt32(obj);
                    }

                    return -1;
                }
            }
            catch { return -1; }
        }
        async public Task<bool> IncReplicationState(IFeatureClass fc, Guid session_guid)
        {
            int replState = await GetReplicationState(fc, session_guid);
            if (replState == -1)
            {
                return false;
            }

            IFeatureDatabaseReplication db = await FeatureClassDb(fc);
            if (db == null)
            {
                return false;
            }

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

        async private Task<DataTable> LockedSessions(IFeatureClass fc)
        {
            IFeatureDatabaseReplication db = await FeatureClassDb(fc);
            if (db == null)
            {
                return null;
            }

            int fc_id = await db.GetFeatureClassID(fc.Name);
            if (fc_id < 0)
            {
                return null;
            }

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
            catch (Exception)
            {
                return null;
            }
        }
        async private Task<bool> CheckForLockedFeatureclassSessions(IFeatureClass fc, Guid checkout_guid)
        {
            DataTable tab = await LockedSessions(fc);
            if (tab == null)
            {
                throw new Exception("Can't derminate featureclass session lockstate");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("checkin featureclass is locked by to following sessions:\n");
            bool found = false;
            foreach (DataRow row in tab.Rows)
            {
                if (row["CHECKOUT_GUID"] == null || row["CHECKOUT_GUID"] == DBNull.Value)
                {
                    continue;
                }

                // SQLite Guids are strings...
                if (!checkout_guid.Equals(Convert2Guid(row["CHECKOUT_GUID"])))
                {
                    found = true;
                    sb.Append(row["CHECKOUT_GUID"].ToString() + " -> " + row["CHECKOUT_NAME"] + "\n");
                }
            }

            if (found)
            {
                sb.Append("Try checkin later again...");
                throw new Exception(sb.ToString());
            }

            return true;
        }
        async private Task<bool> CheckForLockedFeatureclassSessions(IFeatureClass fc, Guid checkout_guid, int waitSeconds)
        {
            DateTime td = DateTime.Now;

            while (true)
            {
                if (await CheckForLockedFeatureclassSessions(fc, checkout_guid))
                {
                    return true;
                }

                TimeSpan ts = DateTime.Now - td;
                if (ts.TotalSeconds > waitSeconds)
                {
                    break;
                }

                if (CheckIn_Message != null)
                {
                    CheckIn_Message(this, "Featureclass is locked. Try again in 3 seconds...");
                }

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

            async internal Task Init()
            {
                if (FeatureClass == null || FeatureClass.Fields == null)
                {
                    return;
                }

                string repl_field_name = await Replication.FeatureClassReplicationIDFieldname(FeatureClass);

                FieldConflicts.Add(new FieldConflict(this, FeatureClass.ShapeFieldName));
                foreach (IField field in FeatureClass.Fields.ToEnumerable())
                {
                    if (field == null ||
                        field.type == FieldType.ID ||
                        field.type == FieldType.Shape ||
                        field.name == repl_field_name)
                    {
                        continue;
                    }

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
                        {
                            fields.Add(field);
                        }
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
                        {
                            fields.Add(field);
                        }
                    }
                    return fields;
                }
            }

            public IFeature SolvedFeature
            {
                get
                {
                    if (FeatureClass == null)
                    {
                        return null;
                    }

                    Feature feature = new Feature();
                    feature.OID = (Feature != null) ? Feature.OID : -1;

                    foreach (FieldConflict cField in (Feature != null) ? ConflictFields : FieldConflicts)
                    {
                        IFeature f = GetFeatureByIndex(cField.ValueIndex);

                        if (cField.FieldName == FeatureClass.ShapeFieldName)
                        {
                            if (f == null)
                            {
                                return null;
                            }

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
                {
                    return Feature;
                }
                else if (index > 0 && index <= ConflictFeatures.Count)
                {
                    return ConflictFeatures[index - 1].Feature;
                }

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
                                {
                                    row["Value"] = conflict.Feature[FieldName].ToString();
                                }
                                else
                                {
                                    row["Value"] = "NULL";
                                }
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
                                {
                                    row["Value"] = cFeature.Feature[FieldName].ToString();
                                }
                                else
                                {
                                    row["Value"] = "NULL";
                                }

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
                        if (this.ConflictTable == null)
                        {
                            return;
                        }

                        if (this.ConflictTable.Rows.Count <= value)
                        {
                            return;
                        }

                        _valueIndex = value;
                    }
                }
            }

            async public Task<bool> RemoveConflict()
            {
                IFeatureDatabaseReplication db = await Replication.FeatureClassDb(FeatureClass);
                if (db == null)
                {
                    return false;
                }

                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (int id in rowIDs)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(",");
                        }

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
                    throw new Exception("RemoveConflict -> " + ex.Message);
                }
            }
            async public Task<(bool success, string errMsg)> SolveConflict()
            {
                string errMsg = String.Empty;
                if (!await RemoveConflict())
                {
                    return (false, errMsg);
                }

                IFeatureDatabaseReplication db = await Replication.FeatureClassDb(FeatureClass);
                if (db == null)
                {
                    return (false, errMsg);
                }

                IFeature sFeature = this.SolvedFeature;
                if (sFeature == null)
                {
                    // Feature ist zu löschen
                    if (Feature != null && await db.Delete(FeatureClass, Feature.OID) == false)
                    {
                        errMsg = "SolveConflict -> " + db.LastErrorMessage;
                        return (false, errMsg);
                    }
                }
                else if (sFeature.OID == -1)
                {
                    // Ursprüngliches Features wurde gelöscht und soll jetzt
                    // "upgedatet" werden. -> neuer Insert mit OBJECT_GUID
                    string repl_field_name = await Replication.FeatureClassReplicationIDFieldname(FeatureClass);
                    if (String.IsNullOrEmpty(repl_field_name))
                    {
                        errMsg = "SolveConflict -> can't get replication field name";
                        return (false, errMsg);
                    }
                    Replication.AllocateNewObjectGuid(sFeature, repl_field_name);
                    sFeature[repl_field_name] = this.ParentObjectGuid;
                    if (!await db.Insert(FeatureClass, sFeature))
                    {
                        errMsg = "SolveConflict -> " + db.LastErrorMessage;
                        return (false, errMsg);
                    }
                }
                else
                {
                    if (!await db.Update(FeatureClass, sFeature))
                    {
                        errMsg = "SolveConflict -> " + db.LastErrorMessage;
                        return (false, errMsg);
                    }
                }
                foreach (ConflictFeature cFeature in ConflictFeatures)
                {
                    if (cFeature == null || cFeature.Feature == null)
                    {
                        continue;
                    }

                    if (!await db.Delete(FeatureClass, cFeature.Feature.OID))
                    {
                        errMsg = "SolveConflict -> " + db.LastErrorMessage;
                        return (false, errMsg);
                    }
                }
                return (true, errMsg);
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
        public class FeatureClassDifferenceDateTime : IFeatureClass, IObjectWrapper
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

            public string ToUrlParameters
            {
                get
                {
                    return "&ReplicationState=" + _replication_state + "&SessionGUID=" + _sessionGUID.ToString();
                }
            }

            #region IFeatureClass Member

            public string ShapeFieldName
            {
                get { return _fc.ShapeFieldName; }
            }

            public IEnvelope Envelope
            {
                get { return _fc.Envelope; }
            }

            public Task<int> CountFeatures()
            {
                return _fc.CountFeatures();
            }

            async public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
            {
                return await _fc.GetFeatures(filter);
            }

            #endregion

            #region ITableClass Member

            async public Task<ICursor> Search(IQueryFilter filter)
            {
                return await _fc.Search(filter);
            }

            async public Task<ISelectionSet> Select(IQueryFilter filter)
            {
                return await _fc.Select(filter);
            }

            public IFieldCollection Fields
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

            public ISpatialReference SpatialReference
            {
                get { return _fc?.SpatialReference; }
            }

            public GeometryType GeometryType
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

            #region IObjectWrapper Member

            public object WrappedObject
            {
                get { return _fc; }
            }

            #endregion
        }

        private class FeatureClassDiffLocks : FeatureClassDifferenceDateTime
        {
            public FeatureClassDiffLocks(IFeatureClass fc, Guid sessionGUID)
                : base(fc, sessionGUID, -1)
            {
                if (fc == null)
                {
                    throw new ArgumentException();
                }
            }

        }
        #endregion

        static private Guid Convert2Guid(object obj)
        {
            if (obj == null)
            {
                return new Guid();
            }

            if (obj is Guid)
            {
                return (Guid)obj;
            }

            return new Guid(obj.ToString());
        }
    }

    public class ReplicationUI
    {
        public static void ShowAddReplicationIDDialog(IFeatureClass fc)
        {
            string appPath = SystemVariables.ApplicationDirectory;
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Offline.UI.dll");

            IFeatureClassDialog p = uiAssembly.CreateInstance("gView.Framework.Offline.UI.FormAddReplicationID") as IFeatureClassDialog;
            if (p != null)
            {
                p.ShowDialog(fc);
            }
        }
        public static void ShowCheckoutDialog(IFeatureClass fc)
        {
            string appPath = SystemVariables.ApplicationDirectory;
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Offline.UI.dll");

            IFeatureClassDialog p = uiAssembly.CreateInstance("gView.Framework.Offline.UI.FormCheckout") as IFeatureClassDialog;
            if (p != null)
            {
                p.ShowDialog(fc);
            }
        }
        public static void ShowCheckinDialog(IFeatureClass fc)
        {
            string appPath = SystemVariables.ApplicationDirectory;
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Offline.UI.dll");

            IFeatureClassDialog p = uiAssembly.CreateInstance("gView.Framework.Offline.UI.FormCheckin") as IFeatureClassDialog;
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
            {
                throw new ArgumentException("Command is null!");
            }

            if (!IsValid)
            {
                throw new ArgumentException("ReplicationTransaction is not valid for ExecuteNonQuery");
            }

            command.Connection = _connection;
            command.Transaction = _transasction;

            return command.ExecuteNonQuery();
        }

        public object ExecuteScalar(DbCommand command)
        {
            if (command == null)
            {
                throw new ArgumentException("Command is null!");
            }

            if (!IsValid)
            {
                throw new ArgumentException("ReplicationTransaction is not valid for ExecuteNonQuery");
            }

            command.Connection = _connection;
            command.Transaction = _transasction;

            return command.ExecuteScalar();
        }


    }
}
