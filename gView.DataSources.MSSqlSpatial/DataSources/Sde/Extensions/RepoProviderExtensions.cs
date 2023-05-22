using gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Extensions
{
    internal static class RepoProviderExtensions
    {
        public static bool PreferTopThanOffset(this RepoProvider repo)
        {
            // SQL Server <= 2016 bad performance with
            // "offset 0 rows fetch next 100 rows only" filters
            // use top(1000) for old SQL Servers when query

            if (repo == null ||
                repo.SqlServerVersion.Major == 0 ||
                repo.SqlServerVersion.Major >= 14)    // 14.x  SQL 2017
            {
                return false;
            }

            return true;
        }
    }
}
