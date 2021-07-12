using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using gView.MxlUtil.Lib.Abstraction;
using gView.MxlUtil.Lib.Exceptions;
using gView.MxUtil.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.MxlUtil.Lib.Utilities
{
    public class MxlToFdb : IMxlUtility
    {
        #region IMxlUtility

        public string Name => "MxlToFdb";

        async public Task Run(string[] args)
        {
            string inFile = String.Empty;
            string outFile = String.Empty;
            string targetConnectionString = String.Empty;
            IEnumerable<string> dontCopyFeatues = null;
            Guid targetGuid = new Guid();

            for (int i = 1; i < args.Length - 1; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-mxl":
                        inFile = args[++i];
                        break;
                    case "-target-connectionstring":
                        targetConnectionString = args[++i];
                        break;

                    case "-target-guid":
                        var guid = args[++i];
                        switch (guid.ToLower())
                        {
                            case "sqlserver":
                                targetGuid = new Guid("3B870AB5-8BE0-4a00-911D-ECC6C83DD6B4");
                                break;

                            case "postgres":
                                targetGuid = new Guid("33254063-133D-4b17-AAE2-46AF7A7DA733");
                                break;

                            case "sqlite":
                                targetGuid = new Guid("36DEB6AC-EA0C-4B37-91F1-B2E397351555");
                                break;

                            default:
                                targetGuid = new Guid(guid);
                                break;
                        }
                        break;

                    case "-out-mxl":
                        outFile = args[++i];
                        break;

                    case "-dont-copy-features-from":
                        dontCopyFeatues = args[++i].Split(',').Select(n => n.Trim().ToLower());
                        break;
                }
            }

            if (String.IsNullOrEmpty(inFile) ||
               String.IsNullOrEmpty(targetConnectionString) ||
               targetGuid.Equals(new Guid()))
            {
                throw new IncompleteArgumentsException();
            }

            if (String.IsNullOrEmpty(outFile))
            {
                outFile = String.IsNullOrEmpty(inFile) ? String.Empty : inFile.Substring(0, inFile.LastIndexOf(".")) + "_fdb.mxl";
            }

            XmlStream stream = new XmlStream("");
            stream.ReadStream(inFile);

            MxlDocument doc = new MxlDocument();
            await stream.LoadAsync("MapDocument", doc);

            var pluginManager = new PlugInManager();

            #region Destination Dataset

            IFeatureDataset targetFeatureDataset = pluginManager.CreateInstance(targetGuid) as IFeatureDataset;
            if (targetFeatureDataset == null)
            {
                throw new Exception("Plugin with GUID '" + targetGuid.ToString() + "' is not a feature dataset...");
            }
            await targetFeatureDataset.SetConnectionString(targetConnectionString);
            await targetFeatureDataset.Open();

            var targetDatabase = (IFDBDatabase)targetFeatureDataset.Database;

            #endregion Destination Dataset

            var map = doc.Maps.FirstOrDefault() as Map;

            var featureLayers = map.TOC.Layers.Where(l => l is IFeatureLayer)
                                              .Select(l => (IFeatureLayer)l);

            if (map.Datasets != null)
            {
                int datasetId = 0;
                foreach (var dataset in map.Datasets.ToArray())
                {
                    Console.WriteLine();
                    Console.WriteLine($"Dataset: { dataset.DatasetName }");
                    Console.WriteLine($"         { dataset.GetType() }");
                    Console.WriteLine("-------------------------------------------------------");

                    foreach (var dsElement in map.MapElements.Where(e => e.DatasetID == datasetId))
                    {
                        if (dsElement?.Class == null)
                        {
                            continue;
                        }

                        var featureLayer = featureLayers.Where(l => l.DatasetID == datasetId && l.Class == dsElement.Class)
                                                        .FirstOrDefault();

                        if (featureLayer == null)
                        {
                            continue;
                        }

                        Console.WriteLine();
                        Console.WriteLine($"FeatureLayer: { featureLayer.Title }");
                        Console.WriteLine($"       Class: { dsElement.Class.Name }");
                        Console.WriteLine($"              { dsElement.Class.GetType() }");
                        Console.WriteLine();

                        var sourceFc = dsElement.Class as IFeatureClass;
                        if (sourceFc == null)
                        {
                            Console.WriteLine("Class is not a FeatureClass");
                            continue;
                        }

                        #region Create Target Featureclass (if not exists)

                        string targetFcName = dsElement.Class.Name;
                        if (targetFcName.Contains("."))
                        {
                            targetFcName = targetFcName.Substring(targetFcName.LastIndexOf(".") + 1);
                        }

                        var targetFc = (await targetFeatureDataset.Element(targetFcName))?.Class as IFeatureClass;

                        if (targetFc != null)
                        {
                            var count = await targetFc.CountFeatures();
                            if (count > 0)
                            {
                                Console.Write($"Already exists in target fdb ({ count } features)");
                            }
                            else
                            {
                                if (!await targetDatabase.DeleteFeatureClass(targetFcName))
                                {
                                    throw new Exception($"Can't delete existing (empty) featureclass { targetFcName }");
                                }
                            }
                        }
                        else
                        {
                            var fcId = await targetDatabase.CreateFeatureClass(
                                targetFeatureDataset.DatasetName,
                                targetFcName,
                                new GeometryDef()
                                {
                                    GeometryType = sourceFc.GeometryType,
                                    HasM = sourceFc.HasM,
                                    HasZ = sourceFc.HasZ,
                                    SpatialReference = sourceFc.SpatialReference
                                },
                                new Fields(sourceFc.Fields.ToEnumerable().Select(f =>
                                {
                                    if (f != null && f.type == FieldType.ID && f.name.ToUpper().Equals("FDB_OID") == false)  // also include original ID Column
                                    {
                                        return new Field(f.name, FieldType.integer);
                                    }
                                    return f;
                                })));
                            if (fcId <= 0)
                            {
                                throw new Exception($"Can't create featureclass { targetFcName }: { targetDatabase.LastErrorMessage }");
                            }

                            targetFc = (await targetFeatureDataset.Element(targetFcName)).Class as IFeatureClass;
                            if (targetFc == null)
                            {
                                throw new Exception($"Can't load target FeatureClass { targetFcName }");
                            }

                            var copyFeatures = dontCopyFeatues == null ||
                                (!dontCopyFeatues.Contains(sourceFc.Name.ToLower()) && !dontCopyFeatues.Contains(targetFc.Name.ToLower()));

                            if (copyFeatures)
                            {
                                var sIndexDef = new gViewSpatialIndexDef(null, 62);

                                var tree2 = await SpatialIndex2(
                                    targetDatabase,
                                    sourceFc,
                                    sIndexDef);
                                tree2.Trim();

                                List<long> nids = new List<long>();
                                foreach (BinaryTree2BuilderNode node in tree2.Nodes)
                                {
                                    nids.Add(node.Number);
                                }
                                await ((AccessFDB)targetDatabase).ShrinkSpatialIndex(targetFcName, nids);
                                await ((AccessFDB)targetDatabase).SetSpatialIndexBounds(targetFcName, "BinaryTree2", tree2.Bounds, tree2.SplitRatio, tree2.MaxPerNode, tree2.maxLevels);
                                await ((AccessFDB)targetDatabase).SetFeatureclassExtent(targetFcName, tree2.Bounds);

                                #endregion Create Target Featureclass (if not exists)

                                var featureBag = new List<IFeature>();
                                IFeature feature = null;
                                int counter = 0;

                                Console.WriteLine("Copy features:");

                                if (sourceFc is IFeatureClassPerformanceInfo && ((IFeatureClassPerformanceInfo)sourceFc).SupportsHighperformanceOidQueries == false)
                                {
                                    using (var memoryFeatureBag = new FeatureBag())
                                    {
                                        #region Read all Features to FeatureBag (Memory)

                                        //
                                        //  eg. SDE Multiversion Views are very slow, queiried win OID Filter!!
                                        //
                                        Console.WriteLine("Source feature class do not support high performance oid quries!");

                                        QueryFilter filter = new QueryFilter() { WhereClause = "1=1" };
                                        filter.AddField("*");

                                        Console.WriteLine("Read all features to memory feature bag...");

                                        using (var featureCursor = await sourceFc.GetFeatures(filter))
                                        {
                                            if (featureCursor == null)
                                            {
                                                throw new Exception($"Can't query features from soure featureclass: { (sourceFc is IDebugging ? ((IDebugging)sourceFc).LastException?.Message : "") }");
                                            }

                                            while ((feature = await featureCursor.NextFeature()) != null)
                                            {
                                                memoryFeatureBag.AddFeature(feature);
                                                counter++;

                                                if (counter % 10000 == 0)
                                                {
                                                    Console.Write($"...{ counter }");
                                                }
                                            }
                                        }

                                        #endregion Read all Features to FeatureBag (Memory)

                                        #region Write to target featureclass

                                        Console.WriteLine($"...{ counter }");
                                        Console.WriteLine("copy feature to target feature class");
                                        counter = 0;

                                        foreach (BinaryTree2BuilderNode node in tree2.Nodes)
                                        {
                                            foreach (var memoryFeature in memoryFeatureBag.GetFeatures(node.OIDs))
                                            {
                                                memoryFeature.Fields.Add(new FieldValue("$FDB_NID", node.Number));
                                                featureBag.Add(memoryFeature);
                                                counter++;

                                                if (counter % 10000 == 0)
                                                {
                                                    await Store(targetDatabase, targetFc, featureBag, counter);
                                                }
                                            }
                                        }

                                        #endregion Write to target featureclass
                                    }

                                    GC.Collect();
                                }
                                else
                                {
                                    #region Query all per Oid and Node

                                    foreach (BinaryTree2BuilderNode node in tree2.Nodes)
                                    {
                                        RowIDFilter filter = new RowIDFilter(sourceFc.IDFieldName);
                                        filter.IDs = node.OIDs;
                                        filter.SubFields = "*";

                                        using (var featureCursor = await sourceFc.GetFeatures(filter))
                                        {
                                            if (featureCursor == null)
                                            {
                                                throw new Exception($"Can't query features from soure featureclass: { (sourceFc is IDebugging ? ((IDebugging)sourceFc).LastException?.Message : "") }");
                                            }

                                            while ((feature = await featureCursor.NextFeature()) != null)
                                            {
                                                feature.Fields.Add(new FieldValue("$FDB_NID", node.Number));
                                                featureBag.Add(feature);
                                                counter++;

                                                if (counter % 10000 == 0)
                                                {
                                                    await Store(targetDatabase, targetFc, featureBag, counter);
                                                }
                                            }
                                        }
                                    }

                                    #endregion Query all per Oid and Node
                                }
                                await Store(targetDatabase, targetFc, featureBag, counter);

                                await ((AccessFDB)targetDatabase).CalculateExtent(targetFcName);
                            }
                        }

                        dsElement.Title = targetFc.Name;
                        ((DatasetElement)dsElement).Class = targetFc;
                    }

                    ((MapPersist)map).SetDataset(datasetId, targetFeatureDataset);
                    datasetId++;
                }
            }

            map.Compress();

            stream = new XmlStream("");
            stream.Save("MapDocument", doc);

            Console.WriteLine($"Write: { outFile }");
            stream.WriteStream(outFile);
            Console.WriteLine("succeeded...");
        }

        public string Description()
        {
            return
@"
MxlToFdb
--------

Copies all vector data in an MXL file to an FeatureDatabase (fdb) [SqlServer, PostGres or Sqlite].
The result is a new MXL file with the same symbology in changed connections to the new FeatureDatabase.

Example: Use this utitiity to make an existing database driven MXL to an 'offline' file driven (Sqlite)
MXL.

";
        }

        public string HelpText()
        {
            return Description() +
@"
Required arguments:
-mxl <mxl-file>
-target-connectionstring <target fdb connection string>
-target-guid <guid or sqlserver|postgres|sqlite>

Optional arguments:
-out-xml <name/path of the out xml>
-dont-copy-features-from <a comma seperated list of layernames, where only an empty Db-Table-Schema is created>
";
        }

        #endregion IMxlUtility

        #region Helper

        private static async Task<bool> Store(IFeatureUpdater featureUpdater, IFeatureClass fc, List<IFeature> featureBag, int counter)
        {
            Console.Write($"...{ counter }");

            if (!await featureUpdater.Insert(fc, featureBag))
            {
                throw new Exception($"Unable to insert features: { featureUpdater.LastErrorMessage }");
            }

            featureBag.Clear();

            return true;
        }

        async static private Task<BinaryTree2Builder> SpatialIndex2(IFeatureDatabase fdb, IFeatureClass fc, ISpatialIndexDef def, List<IQueryFilter> filters = null)
        {
            if (fc == null)
            {
                return null;
            }

            IEnvelope bounds = null;
            if (fc.Envelope != null)
            {
                bounds = fc.Envelope;
            }
            else if (fc.Dataset is IFeatureDataset && await ((IFeatureDataset)fc.Dataset).Envelope() != null)
            {
                bounds = await ((IFeatureDataset)fc.Dataset).Envelope();
            }

            if (bounds == null)
            {
                return null;
            }
            //if (_transformer != null)
            //{
            //    IGeometry transBounds = _transformer.Transform2D(bounds) as IGeometry;
            //    if (transBounds != null)
            //        bounds = transBounds.Envelope;
            //}
            int maxAllowedLevel = ((fdb is SqlFDB) ? 62 : 30);
            BinaryTree2Builder treeBuilder =
                ((Envelope.IsNull(def.SpatialIndexBounds)) ?
                new BinaryTree2Builder(bounds,
                                ((def.Levels != 0) ? def.Levels : maxAllowedLevel),
                                ((def.MaxPerNode != 0) ? def.MaxPerNode : 500),
                                ((def.SplitRatio != 0.0) ? def.SplitRatio : 0.55),
                                maxVerticesPerNode: 500 * 50) :
                new BinaryTree2Builder2(def.SpatialIndexBounds, def.Levels, def.MaxPerNode, def.SplitRatio));

            if (filters == null)
            {
                QueryFilter filter = new QueryFilter() { WhereClause = "1=1" };
                filter.AddField(fc.ShapeFieldName);
                filters = new List<IQueryFilter>();
                filters.Add(filter);
            }
            foreach (IQueryFilter filter in filters)
            {
                using (IFeatureCursor fCursor = await fc.GetFeatures(filter))
                {
                    if (fCursor == null)
                    {
                        throw new Exception("Fatal error: sourcedb query failed...");
                    }

                    Console.WriteLine("Calculate spatial index:");

                    IEnvelope fcEnvelope = bounds;
                    //if (_transformer != null)
                    //{
                    //    IGeometry geom = _transformer.Transform2D(fcEnvelope) as IGeometry;
                    //    if (geom == null)
                    //    {
                    //        _errMsg = "SpatialIndex: Can't project featureclass extent!";
                    //        return null;
                    //    }
                    //    fcEnvelope = geom.Envelope;
                    //}

                    int counter = 0;
                    IFeature feat;
                    while ((feat = await fCursor.NextFeature()) != null)
                    {
                        IGeometry shape = feat.Shape;
                        //if (_transformer != null)
                        //{
                        //    shape = _transformer.Transform2D(shape) as IGeometry;
                        //}

                        feat.Shape = shape;
                        treeBuilder.AddFeature(feat);

                        counter++;
                        if (counter % 10000 == 0)
                        {
                            Console.Write($"...{ counter }");
                        }
                    }
                    Console.Write($"...{ counter }");
                }
            }

            treeBuilder.Trim();
            Console.WriteLine();

            return treeBuilder;
        }

        #endregion Helper
    }
}