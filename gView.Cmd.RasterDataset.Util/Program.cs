using gView.DataSources.Fdb.ImageDataset;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gView.Cmd.RasterDataset.Util
{
    class Program
    {
        enum Jobs { add, truncate, removeUnexisting, unknown };
        static Jobs job = Jobs.unknown;
        static string connectinString = String.Empty;
        static string dbType = "sql", provider = "none";
        static string fileName = String.Empty, rootPath = String.Empty, Filters = String.Empty;
        static bool continueOnError = true;

        async static Task<int> Main(string[] args)
        {
            PlugInManager.InitSilent = true;

            #region Parse Parameters

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-add":
                        if (job != Jobs.unknown)
                        {
                            Console.WriteLine("Can't do more than one job. Run programm twice...");
                            return 1;
                        }
                        job = Jobs.add;
                        break;
                    case "-clean":
                        if (job != Jobs.unknown)
                        {
                            Console.WriteLine("Can't do more than one job. Run programm twice...");
                            return 1;
                        }
                        job = Jobs.removeUnexisting;
                        break;
                    case "-truncate":
                        if (job != Jobs.unknown)
                        {
                            Usage();
                            Console.WriteLine("Can't do more than one job. Run programm twice...");
                            return 1;
                        }
                        job = Jobs.truncate;
                        break;
                    case "-s":
                        connectinString = args[++i];
                        break;
                    case "-db":
                        dbType = args[++i].ToLower();
                        break;
                    case "-provider":
                        provider = args[++i].ToLower();
                        break;
                    case "-fn":
                        if (rootPath != String.Empty)
                        {
                            Usage();
                            Console.WriteLine("Filename OR Rootdirectory...");
                            return 1;
                        }
                        fileName = args[++i];
                        break;
                    case "-rd":
                        if (fileName != String.Empty)
                        {
                            Usage();
                            Console.WriteLine("Filename OR Rootdirectory...");
                            return 1;
                        }
                        rootPath = args[++i];
                        break;
                    case "-f":
                        Filters = args[++i];
                        break;
                    case "-debug":
                        PlugInManager.InitSilent = false;
                        break;
                }
            }
            #endregion

            #region Check Parameters
            if (connectinString == String.Empty)
            {
                Usage();
                Console.WriteLine("No connection string...");
                return 1;
            }
            switch (job)
            {
                case Jobs.removeUnexisting:
                case Jobs.truncate:
                    break;
                case Jobs.add:
                    if (fileName == String.Empty &&
                        (rootPath == String.Empty || Filters == String.Empty))
                    {
                        Usage();
                        Console.WriteLine("No file or rootdirectory and filter defined...");
                        return 1;
                    }
                    break;
                case Jobs.unknown:
                    Usage();
                    Console.WriteLine("No job defined...");
                    return 1;
            }
            #endregion

            DateTime dt = DateTime.Now;

            string mdb = ConfigTextStream.ExtractValue(connectinString, "mdb");
            string dsname = ConfigTextStream.ExtractValue(connectinString, "dsname");
            string connStr = ConfigTextStream.RemoveValue(connectinString, "dsname");

            IFeatureDataset ds = null;
            //if (mdb != String.Empty)
            //{
            //    AccessFDB fdb = new AccessFDB();
            //    await fdb.Open(connStr);
            //    IFeatureDataset dataset = fdb[dsname];
            //    if (dataset == null)
            //    {
            //        Console.WriteLine("Error opening dataset: " + fdb.LastErrorMessage);
            //        return;
            //    }
            //    //dataset.ConnectionString = connectinString;
            //    if (!await dataset.Open())
            //    {
            //        Console.WriteLine("Error opening dataset: " + dataset.LastErrorMessage);
            //        return;
            //    }
            //    ds = dataset;
            //}
            //else 
            if (dbType == "sql")
            {
                SqlFDB fdb = new SqlFDB();
                await fdb.Open(connStr);
                IFeatureDataset dataset = await fdb.GetDataset(dsname);
                if (dataset == null)
                {
                    Console.WriteLine("Error opening dataset: " + fdb.LastErrorMessage);
                    return 1;
                }
                //dataset.ConnectionString = connectinString;
                if (!await dataset.Open())
                {
                    Console.WriteLine("Error opening dataset: " + dataset.LastErrorMessage);
                    return 1;
                }
                ds = dataset;
            }
            else if (dbType == "postgres")
            {
                pgFDB fdb = new pgFDB();
                await fdb.Open(connStr);
                IFeatureDataset dataset = await fdb.GetDataset(dsname);
                if (dataset == null)
                {
                    Console.WriteLine("Error opening dataset: " + fdb.LastErrorMessage);
                    return 1;
                }
                //dataset.ConnectionString = connectinString;
                if (!await dataset.Open())
                {
                    Console.WriteLine("Error opening dataset: " + dataset.LastErrorMessage);
                    return 1;
                }
                ds = dataset;
            }
            else if (dbType == "sqlite")
            {
                SQLiteFDB fdb = new SQLiteFDB();
                await fdb.Open(connStr);
                IFeatureDataset dataset = await fdb.GetDataset(dsname);
                if (dataset == null)
                {
                    Console.WriteLine("Error opening dataset: " + fdb.LastErrorMessage);
                    return 1;
                }
                //dataset.ConnectionString = connectinString;
                if (!await dataset.Open())
                {
                    Console.WriteLine("Error opening dataset: " + dataset.LastErrorMessage);
                    return 1;
                }
                ds = dataset;
            }
            else
            {
                Console.WriteLine("Unknown fdb type");
                return 1;
            }

            IRasterFileDataset rds = null;
            if (provider == "gdal")
            {
                rds = PlugInManager.Create(new Guid("43DFABF1-3D19-438c-84DA-F8BA0B266592")) as IRasterFileDataset;
            }
            else if (provider == "raster")
            {
                rds = PlugInManager.Create(new Guid("D4812641-3F53-48eb-A66C-FC0203980C79")) as IRasterFileDataset;
            }

            Dictionary<string, Guid> providers = new Dictionary<string, Guid>();
            if (rds != null)
            {
                foreach (string format in rds.SupportedFileFilter.Split('|'))
                {
                    string extension = format;

                    int pos = format.LastIndexOf(".");
                    if (pos > 0)
                    {
                        extension = format.Substring(pos, format.Length - pos);
                    }

                    providers.Add(extension, PlugInManager.PlugInID(rds));
                    Console.WriteLine("Provider " + extension + ": " + rds.ToString() + " {" + PlugInManager.PlugInID(rds).ToString() + "}");
                }
            }
            if (providers.Count == 0)
            {
                providers = null;
            }

            switch (job)
            {
                case Jobs.truncate:
                    await Truncate(ds, dsname + "_IMAGE_POLYGONS");
                    break;
                case Jobs.removeUnexisting:
                    await RemoveUnexisting(ds);
                    await CalculateExtent(ds);
                    break;
                case Jobs.add:
                    if (fileName != String.Empty)
                    {
                        if (!await ImportFiles(ds, fileName.Split(';'), providers))
                        {
                            if (!continueOnError)
                            {
                                return 1;
                            }
                        }
                    }
                    else if (rootPath != String.Empty && Filters != String.Empty)
                    {
                        if (!await ImportDirectory(ds, new DirectoryInfo(rootPath), Filters.Split(';'), providers))
                        {
                            if (!continueOnError)
                            {
                                return 1;
                            }
                        }
                    }
                    await CalculateExtent(ds);
                    break;
            }
            Console.WriteLine("\n" + ((TimeSpan)(DateTime.Now - dt)).TotalSeconds + "s");
            Console.WriteLine("done...");

            return 0;
        }

        async static Task Truncate(IFeatureDataset ds, string fcname)
        {
            if (!((AccessFDB)ds.Database).TruncateTable(fcname))
            {
                Console.WriteLine("Error: " + ds.LastErrorMessage);
            }

            AccessFDB fdb = ds.Database as AccessFDB;
            if (fdb != null)
            {
                IFeatureClass fc = await fdb.GetFeatureclass(ds.DatasetName, ds.DatasetName + "_IMAGE_POLYGONS");
                await fdb.CalculateExtent(fc);
                //fdb.RebuildSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
            }
        }

        async static Task<bool> RemoveUnexisting(IFeatureDataset ds)
        {
            FDBImageDataset import = new FDBImageDataset(ds.Database as IImageDB, ds.DatasetName);
            return await import.RemoveUnexisting();
        }

        static int ProgressCounter = 0;
        async static Task<bool> ImportFiles(IFeatureDataset ds, string[] filenames, Dictionary<string, Guid> providers)
        {
            FDBImageDataset import = new FDBImageDataset(ds.Database as IImageDB, ds.DatasetName);
            import.handleNonGeorefAsError = !(rootPath != String.Empty);

            import.ReportProgress += (sender, progress) =>
            {
                ProgressCounter++;

                if (ProgressCounter % 100 == 0)
                {
                    Console.WriteLine($"... {ProgressCounter}");
                }
            };

            foreach (string filename in filenames)
            {
                //Console.WriteLine("Import: " + filename);
                if (!await import.Import(filename, providers))
                {
                    Console.WriteLine("Error: " + import.lastErrorMessage);
                    if (!continueOnError)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        async static Task<bool> ImportDirectory(IFeatureDataset ds, DirectoryInfo di, string[] filters, Dictionary<string, Guid> providers)
        {
            foreach (string filter in filters)
            {
                WildcardEx wildcard = new WildcardEx(filter, RegexOptions.IgnoreCase);

                FileInfo[] fis = di.GetFiles(filter);

                List<string> filenames = new List<string>();
                foreach (FileInfo fi in fis)
                {
                    if (wildcard.IsMatch(fi.Name))
                    {
                        filenames.Add(fi.FullName);
                    }
                }

                if (filenames.Count != 0)
                {
                    if (!await ImportFiles(ds, filenames.ToArray(), providers))
                    {
                        if (!continueOnError)
                        {
                            return false;
                        }
                    }
                }
            }

            foreach (DirectoryInfo sub in di.GetDirectories())
            {
                if (!await ImportDirectory(ds, sub, filters, providers))
                {
                    if (!continueOnError)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        async static Task<bool> CalculateExtent(IFeatureDataset ds)
        {
            Console.WriteLine("\nCalculete new Extent");
            AccessFDB fdb = ds.Database as AccessFDB;
            if (fdb != null)
            {
                IFeatureClass fc = await fdb.GetFeatureclass(ds.DatasetName, ds.DatasetName + "_IMAGE_POLYGONS");
                await fdb.CalculateExtent(fc);
                //fdb.ShrinkSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
                //fdb.RebuildSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
            }
            return true;
        }

        public static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern).
            Replace("\\*", ".*").
            Replace("\\?", ".") + "$";
        }

        static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("gView.Cmd.Rds.Util -db <sql|access|postgres|sqlite> -s <Source Dataset Connection String>");
            Console.WriteLine("            -add ... append files");
            Console.WriteLine("                    -fn <filename> or");
            Console.WriteLine("                    -rd <rootdirectory> -f <filter>");
            Console.WriteLine("                    -provider <first|gdal|raster>");
            Console.WriteLine("            -clean ... Remove Unexisting");
            Console.WriteLine("            -truncate ... remove all files");
        }
    }
}
