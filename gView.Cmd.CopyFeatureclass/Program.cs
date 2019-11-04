using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.Offline;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.CopyFeatureclass
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("not implemented");
            return 1;
        }
    }
    /*
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            string source_connstr = "", source_fc = "";
            string dest_connstr = "", dest_fc = "";
            string[] sourceFields = null, destFields = null;
            Guid source_guid = Guid.Empty, dest_guid = Guid.Empty;
            bool checkout = false;
            string checkoutDescription = String.Empty;
            string child_rights = "iud";
            string parent_rights = "iud";
            string conflict_handling = "normal";
            ISpatialIndexDef treeDef = null;
            geometryType? sourceGeometryType = null;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-source_connstr")
                    source_connstr = args[++i];

                else if (args[i] == "-source_guid")
                    source_guid = new Guid(args[++i]);

                else if (args[i] == "-source_fc")
                    source_fc = args[++i];

                else if (args[i] == "-dest_connstr")
                    dest_connstr = args[++i];

                else if (args[i] == "-dest_guid")
                    dest_guid = new Guid(args[++i]);

                else if (args[i] == "-dest_fc")
                    dest_fc = args[++i];

                else if (args[i] == "-sourcefields")
                    sourceFields = args[++i].Split(';');

                else if (args[i] == "-destfields")
                    destFields = args[++i].Split(';');

                else if (args[i] == "-checkout")
                {
                    checkout = true;
                    checkoutDescription = args[++i];
                }
                else if (args[i] == "-source_geometrytype")
                {
                    geometryType geomType;
                    if (Enum.TryParse<geometryType>(args[++i], out geomType))
                    {
                        sourceGeometryType = geomType;
                        Console.WriteLine("Source geometry type: " + sourceGeometryType);
                    }
                }

                else if (args[i] == "-pr")
                    parent_rights = args[++i];
                else if (args[i] == "-cr")
                    child_rights = args[++i];
                else if (args[i] == "-ch")
                    conflict_handling = args[++i];

                //else if (args[i] == "-si")
                //{
                //    treeDef = BinaryTreeDef.FromString(args[++i]);
                //    if (treeDef == null)
                //    {
                //        Console.WriteLine("Invalid Spatial Index Def. " + args[i]);
                //    }
                //}
            }

            if (source_connstr == "" || source_fc == "" || source_guid == Guid.Empty ||
                dest_connstr == "" || dest_fc == "" || dest_guid == Guid.Empty)
            {
                Console.WriteLine("USAGE:");
                Console.WriteLine("gView.Cmd.CopyFeatureclass -source_connstr <Source Dataset Connection String>");
                Console.WriteLine("                           -source_guid <GUID of Dataset Extension>");
                Console.WriteLine("                           -source_fc <Featureclass name>");
                Console.WriteLine("                           -dest_connstr <Destination Dataset Connection String>");
                Console.WriteLine("                           -dest_guid <GUID of Dataset Extension>");
                Console.WriteLine("                           -dest_fc <Featureclass name>");
                Console.WriteLine("   when check out featureclass:");
                Console.WriteLine("                -checkout <Description> ... Write checkout information");
                Console.WriteLine("                -pr ... parent rights. <iud|iu|ud|...> (i..INSERT, u..UPDATE, d..DELETE)");
                Console.WriteLine("                -cr ... child rights.  <iud|iu|ud|...> (i..INSERT, u..UPDATE, d..DELETE)");
                Console.WriteLine("                -ch <none|normal|parent_wins|child_wins|newer_wins> ... conflict handling");
                Console.WriteLine("   optional:");
                Console.WriteLine("                -source_geometrytype <Point,Polyline,Polygon> ... if source geometrytype is not explizit specified (SQL Geometry)");
                return 1;
            }

            IFeatureDataset sourceDS, destDS;
            IFeatureClass sourceFC;

            PlugInManager compMan = new PlugInManager();
            object comp = compMan.CreateInstance(source_guid);

            if (!(comp is IFeatureDataset))
            {
                Console.WriteLine("Component with GUID '" + source_guid.ToString() + "' is not a feature dataset...");
                return 1;
            }
            sourceDS = (IFeatureDataset)comp;
            await sourceDS.SetConnectionString(source_connstr);
            await sourceDS.Open();
            sourceFC = await GetFeatureclass(sourceDS, source_fc);
            if (sourceFC == null)
            {
                Console.WriteLine("Can't find featureclass '" + source_fc + "' in source dataset...");
                sourceDS.Dispose();
                return 1;
            }


            if (String.IsNullOrWhiteSpace(sourceFC.IDFieldName))
            {
                Console.WriteLine("WARNING: Souorce FeatureClass has no IDField -> Bad performance!!");
            }
            Console.WriteLine("Source FeatureClass: " + sourceFC.Name);
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Shape Field: " + sourceFC.ShapeFieldName);
            if (String.IsNullOrWhiteSpace(sourceFC.IDFieldName))
            {
                Console.WriteLine("WARNING: Souorce FeatureClass has no IDField -> Bad performance!!");
            }
            else
            {
                Console.WriteLine("Id Field   : " + sourceFC.IDFieldName);
            }

            Console.WriteLine();
            Console.WriteLine("Import: " + source_fc);
            Console.WriteLine("-----------------------------------------------------");

            FieldTranslation fieldTranslation = new FieldTranslation();
            if (sourceFields != null && destFields != null)
            {
                if (sourceFields.Length != destFields.Length)
                {
                    Console.WriteLine("Error in field definition...");
                    sourceDS.Dispose();
                    return 1;
                }
                for (int i = 0; i < sourceFields.Length; i++)
                {
                    IField field = sourceFC.FindField(sourceFields[i]);
                    if (field == null)
                    {
                        Console.WriteLine("Error: Can't find field '" + sourceFields[i] + "'...");
                        sourceDS.Dispose();
                        return 1;
                    }
                    fieldTranslation.Add(field, destFields[i]);
                }
            }
            else
            {
                foreach (IField field in sourceFC.Fields.ToEnumerable())
                {
                    if (field.type == FieldType.ID ||
                        field.type == FieldType.Shape) continue;

                    fieldTranslation.Add(field, FieldTranslation.CheckName(field.name));
                }
            }


            comp = compMan.CreateInstance(dest_guid);

            if (comp is IFileFeatureDatabase)
            {
                IFileFeatureDatabase fileDB = (IFileFeatureDatabase)comp;
                if (!await fileDB.Open(dest_connstr))
                {
                    Console.WriteLine("Error opening destination database:" + fileDB.LastErrorMessage);
                    return 1;
                }
                destDS = await fileDB.GetDataset(dest_connstr);
            }
            else if (comp is IFeatureDataset)
            {
                destDS = (IFeatureDataset)comp;
                await destDS.SetConnectionString(dest_connstr);
                if (!await destDS.Open())
                {
                    Console.WriteLine("Error opening destination dataset:" + destDS.LastErrorMessage);
                    return 1;
                }
            }
            else
            {
                Console.WriteLine("Component with GUID '" + dest_guid.ToString() + "' is not a feature dataset...");
                return 1;
            }

            string replIDField = String.Empty;
            if (checkout)
            {
                if (!(destDS.Database is IFeatureDatabaseReplication) ||
                    !(sourceDS.Database is IFeatureDatabaseReplication))
                {
                    Console.WriteLine("Can't checkout FROM/TO databasetype...");
                    return 1;
                }
                replIDField = await Replication.FeatureClassReplicationIDFieldname(sourceFC);
                if (String.IsNullOrEmpty(replIDField))
                {
                    Console.WriteLine("Can't checkout from source featureclass. No replication ID!");
                    return 1;
                }
                IDatasetElement element = await destDS.Element(dest_fc);
                if (element != null)
                {
                    List<Guid> checkout_guids = await Replication.FeatureClassSessions(element.Class as IFeatureClass);
                    if (checkout_guids != null && checkout_guids.Count != 0)
                    {
                        string errMsg = "Can't check out to this featureclass\n";
                        errMsg += "Check in the following Sessions first:\n";
                        foreach (Guid g in checkout_guids)
                            errMsg += "   CHECKOUT_GUID: " + g.ToString();
                        Console.WriteLine("ERROR:\n" + errMsg);
                        return 1;
                    }
                }
            }

            if (destDS.Database is IFeatureDatabase)
            {
                if (destDS.Database is AccessFDB)
                {
                    //Console.WriteLine();
                    //Console.WriteLine("Import: " + source_fc);
                    //Console.WriteLine("-----------------------------------------------------");


                    FDBImport import = new FDBImport(((IFeatureUpdater)destDS.Database).SuggestedInsertFeatureCountPerTransaction);
                    import.ReportAction += new FDBImport.ReportActionEvent(import_ReportAction);
                    import.ReportProgress += new FDBImport.ReportProgressEvent(import_ReportProgress);

                    if (checkout)
                    {
                        if (sourceDS.Database is AccessFDB)
                        {
                            treeDef = await ((AccessFDB)sourceDS.Database).FcSpatialIndexDef(source_fc);
                            if (destDS.Database is AccessFDB)
                            {
                                ISpatialIndexDef dsTreeDef = await ((AccessFDB)destDS.Database).SpatialIndexDef(destDS.DatasetName);
                                if (treeDef.GeometryType != dsTreeDef.GeometryType)
                                    treeDef = dsTreeDef;
                            }
                        }
                    }

                    if (!import.ImportToNewFeatureclass((IFeatureDatabase)destDS.Database, destDS.DatasetName, dest_fc, sourceFC, fieldTranslation, true, null, treeDef,
                        sourceGeometryType: sourceGeometryType))
                    {
                        Console.WriteLine("ERROR: " + import.lastErrorMsg);
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Create: " + source_fc);
                    Console.WriteLine("-----------------------------------------------------");

                    FeatureImport import = new FeatureImport();

                    import.ReportAction += new FeatureImport.ReportActionEvent(import_ReportAction2);
                    import.ReportProgress += new FeatureImport.ReportProgressEvent(import_ReportProgress2);

                    if (!import.ImportToNewFeatureclass(destDS, dest_fc, sourceFC, fieldTranslation, true,
                        sourceGeometryType: sourceGeometryType))
                    {
                        Console.WriteLine("ERROR: " + import.lastErrorMsg);

                        return 1;
                    }
                }

                if (checkout)
                {
                    IDatasetElement element = await destDS.Element(dest_fc);
                    if (element == null)
                    {
                        Console.WriteLine("ERROR: Can't write checkout information...");
                        return 1;
                    }
                    IFeatureClass destFC = element.Class as IFeatureClass;

                    string errMsg;
                    if (!await Replication.InsertReplicationIDFieldname(destFC, replIDField))
                    {
                        Console.WriteLine("ERROR: at InsertReplicationIDFieldname");
                        return 1;
                    }

                    Replication.VersionRights cr = Replication.VersionRights.NONE;
                    Replication.VersionRights pr = Replication.VersionRights.NONE;
                    Replication.ConflictHandling ch = Replication.ConflictHandling.NORMAL;

                    if (child_rights.ToLower().Contains("i")) cr |= Replication.VersionRights.INSERT;
                    if (child_rights.ToLower().Contains("u")) cr |= Replication.VersionRights.UPDATE;
                    if (child_rights.ToLower().Contains("d")) cr |= Replication.VersionRights.DELETE;

                    if (parent_rights.ToLower().Contains("i")) pr |= Replication.VersionRights.INSERT;
                    if (parent_rights.ToLower().Contains("u")) pr |= Replication.VersionRights.UPDATE;
                    if (parent_rights.ToLower().Contains("d")) pr |= Replication.VersionRights.DELETE;

                    switch (conflict_handling.ToLower())
                    {
                        case "none":
                            ch = Replication.ConflictHandling.NONE;
                            break;
                        case "normal":
                            ch = Replication.ConflictHandling.NORMAL;
                            break;
                        case "parent_wins":
                            ch = Replication.ConflictHandling.PARENT_WINS;
                            break;
                        case "child_wins":
                            ch = Replication.ConflictHandling.CHILD_WINS;
                            break;
                        case "newer_wins":
                            ch = Replication.ConflictHandling.NEWER_WINS;
                            break;
                    }

                    if (!await Replication.InsertNewCheckoutSession(sourceFC,
                        pr,
                        destFC,
                        cr,
                        ch,
                        SystemInformation.Replace(checkoutDescription)))
                    {
                        Console.WriteLine("ERROR: at InsertNewCheckoutSession");
                        return 1;
                    }

                    if (!await Replication.InsertCheckoutLocks(sourceFC, destFC))
                    {
                        Console.WriteLine("ERROR: at  InsertCheckoutLocks");
                        return 1;
                    }
                }
            }
            else
            {
                Console.WriteLine("Destination dataset has no feature database...");
                Console.WriteLine("Can't create featureclasses for this kind of dataset...");

                return 1;
            }
            sourceDS.Dispose();
            destDS.Dispose();

            return 0;
        }

        static bool newLine = false;
        static void import_ReportProgress(FDBImport sender, int progress)
        {
            Console.Write("..." + progress);
            newLine = true;
        }

        static void import_ReportAction(FDBImport sender, string action)
        {
            if (newLine)
            {
                Console.WriteLine();
                newLine = false;
            }
            Console.WriteLine(action);
        }

        static void import_ReportProgress2(FeatureImport sender, int progress)
        {
            Console.Write("..." + progress);
            newLine = true;
        }

        static void import_ReportAction2(FeatureImport sender, string action)
        {
            if (newLine)
            {
                Console.WriteLine();
                newLine = false;
            }
            Console.WriteLine(action);
        }

        async private static Task<IFeatureClass> GetFeatureclass(IFeatureDataset ds, string name)
        {
            IDatasetElement element = await ds.Element(name);
            if (element != null && element.Class is IFeatureClass)
                return element.Class as IFeatureClass;

            foreach (IDatasetElement element2 in await ds.Elements())
            {
                if (element2.Class is IFeatureClass)
                {
                    if (element2.Class.Name == name) return element2.Class as IFeatureClass;
                }
            }

            return null;
        }
    }

    class SystemInformation
    {
        public static string Replace(string str)
        {
            return str.Replace("[MACHINENAME]", MachineName).Replace("[USER]", UserName);
        }
        public static string MachineName
        {
            get { return System.Environment.MachineName; }
        }
        public static string UserName
        {
            get { return System.Environment.UserName; }
        }
    }
    */
}
