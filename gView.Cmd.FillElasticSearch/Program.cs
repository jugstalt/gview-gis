using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.SpatialAlgorithms;
using gView.Framework.system;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Cmd.FillElasticSearch
{
    class Program
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        async static Task<int> Main(string[] args)
        {
            string cmd = "fill", jsonFile = (args.Length == 1 && args[0] != "fill" ? args[0] : String.Empty), indexUrl = String.Empty, indexName = String.Empty, category = String.Empty;
            bool replace = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "fill" && i < args.Length - 1)
                {
                    cmd = "fill";
                    jsonFile = args[i + 1];
                    i++;
                }
                if (args[i] == "remove-category")
                {
                    cmd = "remove-category";
                }
                if (args[i] == "-s" && i < args.Length - 1)
                {
                    indexUrl = args[i + 1];
                }

                if (args[i] == "-i" && i < args.Length - 1)
                {
                    indexName = args[i + 1];
                }

                if (args[i] == "-c" && i < args.Length - 1)
                {
                    category = args[i + 1];
                }

                if (args[i] == "-r")
                {
                    replace = true;
                }
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: gView.Cmd.ElasticSearch fill|remove-catetory [Options]");
                return 1;
            }

            if (cmd == "fill" && String.IsNullOrEmpty(jsonFile))
            {
                Console.WriteLine("Usage: gView.Cmd.ElasticSearch fill {json-file}");
                return 1;
            }
            else if (cmd == "remove-category" && (String.IsNullOrWhiteSpace(indexUrl) || String.IsNullOrWhiteSpace(category)))
            {
                Console.WriteLine("Usage: gView.cmd.ElasticSearch remove-category -s {index-url} -i {index-name} -c {category} [-r]");
                Console.WriteLine("  -r ... raplace german Umlaute:");
                Console.WriteLine("            _ae_, _oe_, _ue_   =>  ä, ö, ü");
                Console.WriteLine("            _Ae_, _Oe_, _Ue_   =>  Ä, Ö, Ü");
                Console.WriteLine("            _sz_               =>  ß");
                return 1;
            }

            try
            {
                //gView.Framework.system.SystemVariables.CustomApplicationDirectory =
                //    System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                Console.WriteLine("Environment");

                Console.WriteLine("Working Directory: " + gView.Framework.system.SystemVariables.StartupDirectory);
                Console.WriteLine("64Bit=" + gView.Framework.system.Wow.Is64BitProcess);

                if (cmd == "fill")
                {
                    #region Fill Index (with Json File)

                    var importConfig = JsonConvert.DeserializeObject<ImportConfig>(File.ReadAllText(jsonFile));

                    var searchContext = new ElasticSearchContext(importConfig.Connection.Url, importConfig.Connection.DefaultIndex);

                    if (importConfig.Connection.DeleteIndex)
                    {
                        searchContext.DeleteIndex();
                    }

                    searchContext.CreateIndex();
                    searchContext.Map<Item>();
                    searchContext.Map<Meta>();

                    ISpatialReference sRefTarget = SpatialReference.FromID("epsg:4326");

                    Console.WriteLine("Target Spatial Reference: " + sRefTarget.Name + " " + String.Join(" ", sRefTarget.Parameters));

                    foreach (var datasetConfig in importConfig.Datasets)
                    {
                        if (datasetConfig.FeatureClasses == null)
                        {
                            continue;
                        }

                        IDataset dataset = new PlugInManager().CreateInstance(datasetConfig.DatasetGuid) as IDataset;
                        if (dataset == null)
                        {
                            throw new ArgumentException("Can't load dataset with guid " + datasetConfig.DatasetGuid.ToString());
                        }

                        await dataset.SetConnectionString(datasetConfig.ConnectionString);
                        await dataset.Open();

                        foreach (var featureClassConfig in datasetConfig.FeatureClasses)
                        {
                            var itemProto = featureClassConfig.IndexItemProto;
                            if (itemProto == null)
                            {
                                continue;
                            }

                            string metaId = Guid.NewGuid().ToString("N").ToLower();
                            category = featureClassConfig.Category;
                            if (!String.IsNullOrWhiteSpace(category))
                            {
                                var meta = new Meta()
                                {
                                    Id = metaId,
                                    Category = category,
                                    Descrption = featureClassConfig.Meta?.Descrption,
                                    Sample = featureClassConfig?.Meta.Sample,
                                    Service = featureClassConfig?.Meta.Service,
                                    Query = featureClassConfig?.Meta.Query
                                };
                                searchContext.Index<Meta>(meta);
                            }

                            bool useGeometry = featureClassConfig.UserGeometry;

                            IDatasetElement dsElement = await dataset.Element(featureClassConfig.Name);
                            if (dsElement == null)
                            {
                                throw new ArgumentException("Unknown dataset element " + featureClassConfig.Name);
                            }

                            IFeatureClass fc = dsElement.Class as IFeatureClass;
                            if (fc == null)
                            {
                                throw new ArgumentException("Dataobject is not a featureclass " + featureClassConfig.Name);
                            }

                            Console.WriteLine("Index " + fc.Name);
                            Console.WriteLine("=====================================================================");

                            QueryFilter filter = new QueryFilter();
                            filter.SubFields = "*";
                            if (!String.IsNullOrWhiteSpace(featureClassConfig.Filter))
                            {
                                filter.WhereClause = featureClassConfig.Filter;
                                Console.WriteLine("Filter: " + featureClassConfig.Filter);
                            }

                            List<Item> items = new List<Item>();
                            int count = 0;

                            ISpatialReference sRef = fc.SpatialReference ?? SpatialReference.FromID("epsg:" + featureClassConfig.SRefId);
                            Console.WriteLine("Source Spatial Reference: " + sRef.Name + " " + String.Join(" ", sRef.Parameters));
                            Console.WriteLine("IDField: " + fc.IDFieldName);

                            using (var transformer = GeometricTransformerFactory.Create())
                            {
                                if (useGeometry)
                                {
                                    transformer.SetSpatialReferences(sRef, sRefTarget);
                                }

                                IFeatureCursor cursor = await fc.GetFeatures(filter);
                                IFeature feature;
                                while ((feature = await cursor.NextFeature()) != null)
                                {
                                    var indexItem = ParseFeature(metaId, category, feature, itemProto, useGeometry, transformer, featureClassConfig);
                                    items.Add(indexItem);
                                    count++;

                                    if (items.Count >= 500)
                                    {
                                        searchContext.IndexMany<Item>(items.ToArray());
                                        items.Clear();

                                        Console.Write(count + "...");
                                    }
                                }

                                if (items.Count > 0)
                                {
                                    searchContext.IndexMany<Item>(items.ToArray());
                                    Console.WriteLine(count + "...finish");
                                }
                            }
                        }
                    }

                    #endregion
                }
                else if (cmd == "remove-category")
                {
                    #region Remove Category

                    RemoveCategory(indexUrl, indexName, replace ? Replace(category) : category);

                    #endregion
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return 1;
            }
        }

        static private bool RemoveCategory(string indexUrl, string indexName, string category)
        {
            var searchContext = new ElasticSearchContext(indexUrl, indexName);

            var metas = searchContext.Query<Meta>(new ElasticSearchContext.SearchFilter()
            {
                Field = "category",
                Value = "\"" + category + "\""
            }, indexName: indexName).Where(m => m.Category.ToLower() == category.ToLower());  // Exact suchen!!

            if (metas.Count() == 0)
            {
                Console.WriteLine("ERROR: No metadata definition found for " + category);
                return false;
            }

            foreach (var meta in metas)
            {
                Console.WriteLine("Delete items (" + meta.Id + ")...");
                int count = 0;

                while (true)
                {
                    var items = searchContext.Query<Item>(new ElasticSearchContext.SearchFilter()
                    {
                        Field = "category",
                        Value = "\"" + category + "\""
                    }, indexName: indexName).Where(i => i.Category.ToLower() == category.ToLower() && i.Id.StartsWith(meta.Id + ".")); // exakt suchen ... nach ID kann man nur exact suchen... darum über Category suchen und dann einschränken

                    count += items.Count();
                    if (items.Count() == 0 || count > 1e7)
                    {
                        break;
                    }

                    if (searchContext.RemoveMany<Item>(items, indexName))
                    {
                        Console.WriteLine("Successfully deleted " + items.Count() + " items");
                    }

                    System.Threading.Thread.Sleep(1000);
                }

                Console.WriteLine("================================================================");
                Console.WriteLine(count + " items deleted");
                Console.WriteLine();

                if (searchContext.Remove<Meta>(meta.Id, indexName))
                {
                    Console.WriteLine("Successfully deleted meta info " + meta.Category + " id=" + meta.Id);
                }
            }

            return true;
        }

        static private Item ParseFeature(string metaId, string category, IFeature feature, Item proto, bool useGeometry, IGeometricTransformer transformer, ImportConfig.FeatureClassDefinition featureClassDef)
        {
            var replace = featureClassDef.Replacements;

            var result = new Item();

            string oid = feature.OID.ToString();
            if (feature.OID <= 0 && !String.IsNullOrWhiteSpace(featureClassDef.ObjectOidField))
            {
                var idFieldValue = feature.FindField(featureClassDef.ObjectOidField);
                if (idFieldValue != null)
                {
                    oid = idFieldValue.Value?.ToString();
                }
            }

            result.Id = metaId + "." + oid;
            result.SuggestedText = ParseFeatureField(feature, proto.SuggestedText);
            result.SubText = ParseFeatureField(feature, proto.SubText);
            result.ThumbnailUrl = ParseFeatureField(feature, proto.ThumbnailUrl);
            result.Category = category;

            if (replace != null)
            {
                foreach (var r in replace)
                {
                    result.SuggestedText = result.SuggestedText?.Replace(r.From, r.To);
                    result.SubText = result.SubText?.Replace(r.From, r.To);
                }
            }

            if (useGeometry == true && feature.Shape != null)
            {
                IGeometry shape = feature.Shape;

                if (shape is IPoint)
                {
                    IPoint point = (IPoint)transformer.Transform2D(feature.Shape);
                    result.Geo = new Nest.GeoLocation(point.Y, point.X);
                }
                else if (shape is IPolyline)
                {
                    IEnvelope env = shape.Envelope;
                    if (env != null)
                    {
                        IPoint point = Algorithm.PolylinePoint((IPolyline)shape, ((IPolyline)shape).Length / 2.0);

                        if (point != null)
                        {
                            point = (IPoint)transformer.Transform2D(point);
                            result.Geo = new Nest.GeoLocation(point.Y, point.X);
                        }

                        result.BBox = GetBBox(env, transformer);
                    }
                }
                else if (shape is IPolygon)
                {
                    IEnvelope env = shape.Envelope;
                    if (env != null)
                    {
                        var points = Algorithm.OrderPoints(Algorithm.PolygonLabelPoints((IPolygon)shape), env.Center);
                        if (points != null && points.PointCount > 0)
                        {
                            IPoint point = (IPoint)transformer.Transform2D(points[0]);
                            result.Geo = new Nest.GeoLocation(point.Y, point.X);
                        }

                        result.BBox = GetBBox(env, transformer);
                    }
                }
            }

            return result;
        }

        static private string ParseFeatureField(IFeature feature, string pattern)
        {
            if (pattern == null || !pattern.Contains("{"))
            {
                return pattern;
            }

            string[] parameters = GetKeyParameters(pattern);
            foreach (string parameter in parameters)
            {
                var fieldValue = feature.FindField(parameter);
                if (fieldValue == null)
                {
                    continue;
                }

                string val = fieldValue.Value != null ? fieldValue.Value.ToString() : String.Empty;
                pattern = pattern.Replace("{" + parameter + "}", val);
            }

            return pattern;
        }

        static private string[] GetKeyParameters(string pattern)
        {
            int pos1 = 0, pos2;
            pos1 = pattern.IndexOf("{");
            string parameters = "";

            while (pos1 != -1)
            {
                pos2 = pattern.IndexOf("}", pos1);
                if (pos2 == -1)
                {
                    break;
                }

                if (parameters != "")
                {
                    parameters += ";";
                }

                parameters += pattern.Substring(pos1 + 1, pos2 - pos1 - 1);
                pos1 = pattern.IndexOf("{", pos2);
            }
            if (parameters != "")
            {
                return parameters.Split(';');
            }
            else
            {
                return new string[0];
            }
        }

        static private string GetBBox(IEnvelope env, IGeometricTransformer transformer)
        {
            try
            {
                env = ((IGeometry)transformer.Transform2D(env)).Envelope;

                return Math.Round(env.minx, 7).ToString(_nhi) + "," +
                       Math.Round(env.miny, 7).ToString(_nhi) + "," +
                       Math.Round(env.maxx, 7).ToString(_nhi) + "," +
                       Math.Round(env.maxy, 7).ToString(_nhi);
            }
            catch { return String.Empty; }
        }

        static private string Replace(string str)
        {
            return str
                .Replace("_ae_", "ä")
                .Replace("_oe_", "ö")
                .Replace("_üe_", "ü")
                .Replace("_Ae_", "Ä")
                .Replace("_Oe_", "Ö")
                .Replace("_Üe_", "Ü")
                .Replace("_sz_", "ß");
        }
    }
}
