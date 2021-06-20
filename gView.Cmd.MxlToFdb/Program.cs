using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.Framework.Data;
using gView.Framework.Document;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Cmd.MxlToFdb
{
    class Program
    {
        async static Task<int> Main(string[] args)
        {
            PlugInManager.InitSilent = true;

            try
            {
                string inFile = args.FirstOrDefault();
                string targetConnectionString = String.Empty;
                Guid targetGuid = new Guid();

                for (int i = 1; i < args.Length - 1; i++)
                {
                    switch (args[i].ToLower())
                    {
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
                    }
                }

                if (String.IsNullOrWhiteSpace(inFile))
                {
                    Console.WriteLine("Usage: gView.Cmd.MxlToFdb mxl-file [Options]");
                }

                XmlStream stream = new XmlStream("");
                stream.ReadStream(inFile);

                MapDocumentPersist doc = new MapDocumentPersist();
                await stream.LoadAsync("MapDocument", doc);

                var pluginManager = new PlugInManager();

                #region Destination Dataset

                IFeatureDataset targetFeatureDataset = pluginManager.CreateInstance(targetGuid) as IFeatureDataset;
                if (targetFeatureDataset == null)
                {
                    Console.WriteLine("Plugin with GUID '" + targetGuid.ToString() + "' is not a feature dataset...");
                    return 1;
                }
                await targetFeatureDataset.SetConnectionString(targetConnectionString);
                await targetFeatureDataset.Open();

                var targetDatabase = (IFDBDatabase)targetFeatureDataset.Database;

                #endregion

                var map = doc.Maps.FirstOrDefault();

                var featureLayers = map.TOC.Layers.Where(l => l is IFeatureLayer)
                                                  .Select(l => (IFeatureLayer)l);

                if (map.Datasets != null)
                {
                    int datasetID = 0;
                    foreach (var dataset in map.Datasets)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Dataset: { dataset.DatasetName }");
                        Console.WriteLine($"         { dataset.GetType() }");
                        Console.WriteLine("-------------------------------------------------------");


                        foreach (var dsElement in map.MapElements.Where(e => e.DatasetID == datasetID))
                        {
                            if (dsElement?.Class == null)
                                continue;

                            var featureLayer = featureLayers.Where(l => l.DatasetID == datasetID && l.Class == dsElement.Class)
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

                            var sIndexDef = new gViewSpatialIndexDef(null, 5);

                            var tree2 = await SpatialIndex2(
                                targetDatabase,
                                sourceFc,
                                sIndexDef);
                            tree2.Trim();

                            #region Create Target Featureclass

                            string targetFcName = dsElement.Class.Name;
                            if (targetFcName.Contains("."))
                                targetFcName = targetFcName.Substring(targetFcName.LastIndexOf(".") + 1);

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
                                sourceFc.Fields);
                            if (fcId <= 0)
                            {
                                throw new Exception($"Can't create featureclass { targetFcName }");
                            }

                            var targetFc = (await targetFeatureDataset.Element(targetFcName)).Class as IFeatureClass;
                            if (targetFc == null)
                            {
                                throw new Exception($"Can't load target FeatureClass { targetFcName }");
                            }

                            List<long> nids = new List<long>();
                            foreach (BinaryTree2BuilderNode node in tree2.Nodes)
                            {
                                nids.Add(node.Number);
                            }
                            await ((AccessFDB)targetDatabase).ShrinkSpatialIndex(targetFcName, nids);
                            await ((AccessFDB)targetDatabase).SetSpatialIndexBounds(targetFcName, "BinaryTree2", tree2.Bounds, tree2.SplitRatio, tree2.MaxPerNode, tree2.maxLevels);
                            await ((AccessFDB)targetDatabase).SetFeatureclassExtent(targetFcName, tree2.Bounds);

                            #endregion

                            

                            var featureBag = new List<IFeature>();
                            IFeature feature = null;
                            int counter = 0;

                            Console.WriteLine("Copy features:");

                            foreach (BinaryTree2BuilderNode node in tree2.Nodes)
                            {
                                RowIDFilter filter = new RowIDFilter(sourceFc.IDFieldName);
                                filter.IDs = node.OIDs;
                                filter.SubFields = "*";

                                using (var featureCursor = await sourceFc.GetFeatures(filter))
                                {
                                    while ((feature = await featureCursor.NextFeature()) != null)
                                    {
                                        featureBag.Add(feature);
                                        counter++;

                                        if (counter % 1000 == 0)
                                        {
                                            await Store(targetDatabase, targetFc, featureBag, counter);
                                        }
                                    }
                                }
                            }
                            await Store(targetDatabase, targetFc, featureBag, counter);
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.Message);

                return 1;
            }
        }

        async static Task<bool> Store(IFeatureUpdater featureUpdater, IFeatureClass fc, List<IFeature> featureBag, int counter)
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
            if (fc == null) return null;

            IEnvelope bounds = null;
            if (fc.Envelope != null)
                bounds = fc.Envelope;
            else if (fc.Dataset is IFeatureDataset && await ((IFeatureDataset)fc.Dataset).Envelope() != null)
                bounds = await ((IFeatureDataset)fc.Dataset).Envelope();
            if (bounds == null) return null;
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
                                ((def.SplitRatio != 0.0) ? def.SplitRatio : 0.55)) :
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
                        if (counter % 1000 == 0)
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
    }
}
