using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace gView.Framework.Offline
{
    public class DbAccess
    {
        static public void GetReplConflicts(IFeatureDatabaseReplication sourceDB, int sourceFc_id, DataTable conf_table)
        {
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
        }

        static public void DeleteReplLocks(IFeatureDatabaseReplication destDB, int destFc_id)
        {
            using (DbConnection connection = destDB.ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = destDB.DatabaseConnectionString;
                DbCommand command = destDB.ProviderFactory.CreateCommand();
                command.CommandText = "DELETE FROM " + destDB.TableName("GV_CHECKOUT_LOCKS") + " WHERE " + destDB.DbColName("FC_ID") + "=" + destFc_id;
                command.Connection = connection;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        static public bool FeatureClassHasReplicationID(IFeatureDatabaseReplication db, int fc_id)
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
    }
}
