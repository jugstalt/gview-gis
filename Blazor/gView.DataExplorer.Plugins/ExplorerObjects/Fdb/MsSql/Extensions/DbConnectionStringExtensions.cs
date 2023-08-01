using gView.Framework.Db;
using gView.Framework.IO;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.MsSql.Extensions;

static internal class DbConnectionStringExtensions
{
    static public SqlFdbExplorerObject ToSqlFdbExplorerObjectt(this DbConnectionString dbConnectionString,
                                                               SqlFdbExplorerGroupObject parent)
    {
        ConfigConnections connStream = new ConfigConnections("sqlfdb", "546B0513-D71D-4490-9E27-94CD5D72C64A");
        string connectionString = dbConnectionString.ConnectionString;
        string id = ConfigTextStream.ExtractValue(connectionString, "database");
        id += "@" + ConfigTextStream.ExtractValue(connectionString, "server");
        if (id == "@")
        {
            id = "SqlFDB Connection";
        }

        id = connStream.GetName(id);

        connStream.Add(id, connStream.ToString());
        return new SqlFdbExplorerObject(parent, id, dbConnectionString);
    }
}
