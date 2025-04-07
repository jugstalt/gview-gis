using System;
using System.Data.Entity.Infrastructure;

namespace gView.Framework.Db.Extensions;

static public class SqlServerConnectionStringExtensions
{
    static public string AppendTrustServerCertificate(this string connectionString)
    {
        // append the TrustServerCertificate=true; to the connection string if not existes
        if (string.IsNullOrEmpty(connectionString))
        {
            return "TrustServerCertificate=true;";
        }
        if (connectionString.Replace(" ","").Contains("TrustServerCertificate=", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }
        if (connectionString.EndsWith(";"))
        {
            return connectionString + "TrustServerCertificate=true;";
        }
        else
        {
            return connectionString + ";TrustServerCertificate=true;";
        }
    }
}
