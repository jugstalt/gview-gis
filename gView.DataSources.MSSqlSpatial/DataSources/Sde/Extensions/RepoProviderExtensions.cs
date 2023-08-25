using gView.DataSources.MSSqlSpatial.DataSources.Sde.Repo;
using System;

namespace gView.DataSources.MSSqlSpatial.DataSources.Sde.Extensions
{
    internal static class RepoProviderExtensions
    {
        static private readonly Version MinSqlServerVersion = new Version(14, 0);
        static private readonly Version MinSdeVersion = new Version(10, 7);

        public static bool PreferTopThanOffset(this RepoProvider repo)
        {
            // no version dedected => use offset anyway

            if (repo == null || repo.SqlServerVersion.Major == 0 || repo.SdeVersion.Major == 0)
            {
                return false;
            }

            // SQL Server <= 2016 && SDE <= 10.6 bad performance with
            // "offset 0 rows fetch next 100 rows only" filters
            // use top(1000) for old SQL Servers when query

            if (repo.SqlServerVersion < MinSqlServerVersion &&
               repo.SdeVersion < MinSdeVersion)
            {
                return true;
            }

            // use offset 0.... 

            return false;
        }
    }
}
