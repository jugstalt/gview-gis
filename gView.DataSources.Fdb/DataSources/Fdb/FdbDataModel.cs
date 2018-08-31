using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using gView.Framework.Db;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace gView.DataSources.Fdb
{
    public class FdbDataModel
    {
        static public bool UpdateLinkedFcDatatables(AccessFDB fdb)
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

                    var fields = new Fields();
                    fields.Add(new Field("ID", FieldType.ID));
                    fields.Add(new Field("Plugin", FieldType.guid));
                    fields.Add(new Field("Connection", FieldType.String, 4000));

                    if (!fdb.CreateIfNotExists("FDB_LinkedConnections", fields))
                        throw new Exception("Can't create 'FDB_LinkedConnections':" + fdb.lastErrorMsg);
                    #endregion
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }
}
