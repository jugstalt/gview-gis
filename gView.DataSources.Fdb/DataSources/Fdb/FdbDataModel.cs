using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using gView.Framework.Db;
using System;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb
{
    public class FdbDataModel
    {
        async static public Task<bool> UpdateLinkedFcDatatables(AccessFDB fdb)
        {
            try
            {
                ICommonDbConnection conn = fdb.DbConnection;

                if (conn != null)
                {
                    #region Add Column "LinkedFcId" to "FDB_FetaureClasses"

                    //System.Data.DataTable schema = conn.GetSchema2("FDB_FeatureClasses");
                    //bool hasColumn = schema.Select("ColumnName='LinkedFcId'").Length > 0;

                    //if (!hasColumn)
                    //{
                    //    fdb.AlterTable("FDB_FeatureClasses", null, new Field()
                    //    {
                    //        name = "LinkedFcId",
                    //        type = FieldType.integer
                    //    });
                    //}

                    // For identify a linked FC: Column SI=Linked, SIVersion=LinkedId

                    #endregion

                    #region Add "FDB_LinkedFeatureClasses"

                    var fields = new FieldCollection();
                    fields.Add(new Field("ID", FieldType.ID));
                    fields.Add(new Field("Plugin", FieldType.guid));
                    fields.Add(new Field("Connection", FieldType.String, 4000));

                    if (!await fdb.CreateIfNotExists("FDB_LinkedConnections", fields))
                    {
                        throw new Exception("Can't create 'FDB_LinkedConnections':" + fdb.LastErrorMessage);
                    }
                    #endregion
                }

            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }
    }
}
