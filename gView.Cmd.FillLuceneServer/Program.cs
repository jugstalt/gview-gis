using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.SpatialAlgorithms;
using gView.Framework.system;
using LuceneServerNET.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.FillLuceneServer
{
    class Program
    {
        private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        async static Task<int> Main(string[] args)
        {
            try
            {
                string cmd = "fill", jsonFile = (args.Length == 1 && args[0] != "fill" ? args[0] : String.Empty), indexUrl = String.Empty, indexName = String.Empty, category = String.Empty;

                string proxyUrl = String.Empty, proxyUser = String.Empty, proxyPassword = String.Empty;
                string basicAuthUser = String.Empty, basicAuthPassword = String.Empty;

                int packageSize = 50000;
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

                    if (args[i] == "-packagesize" && i < args.Length - 1)
                    {
                        packageSize = int.Parse(args[i + 1]);
                    }

                    #region Proxy

                    if (args[i] == "-proxy")
                    {
                        proxyUrl = args[i + 1];
                    }
                    if (args[i] == "-proxy-user")
                    {
                        proxyUser = args[i + 1];
                    }
                    if (args[i] == "-proxy-pwd")
                    {
                        proxyPassword = args[i + 1];
                    }

                    #endregion

                    #region Basic Authentication

                    if (args[i] == "-basic-auth-user")
                    {
                        basicAuthUser = args[i + 1];
                    }

                    if (args[i] == "-basic-auth-pwd")
                    {
                        basicAuthPassword = args[i + 1];
                    }

                    #endregion
                }

                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: gView.Cmd.ElasticSearch fill|remove-catetory [Options]");
                    return 1;
                }


                //gView.Framework.system.SystemVariables.CustomApplicationDirectory =
                //    System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                Console.WriteLine("Environment");

                Console.WriteLine("Working Directory: " + gView.Framework.system.SystemVariables.StartupDirectory);
                Console.WriteLine("64Bit=" + gView.Framework.system.Wow.Is64BitProcess);

                if (cmd == "fill")
                {
                    #region Fill Index (with Json File)

                    var importConfig = JsonConvert.DeserializeObject<ImportConfig>(File.ReadAllText(jsonFile));

                    if(importConfig?.Connection==null)
                    {
                        throw new Exception("Invalid config. No connection defined");
                    }

                    var httpClientHandler = new HttpClientHandler();
                    if (!String.IsNullOrEmpty(proxyUrl))
                    {
                        httpClientHandler.Proxy = new WebProxy
                        {
                            Address = new Uri(proxyUrl),
                            BypassProxyOnLocal = false,
                            UseDefaultCredentials = false,

                            Credentials = new NetworkCredential(proxyUser, proxyPassword)
                        };
                    }

                    var httpClient = new HttpClient(handler: httpClientHandler, disposeHandler: true);
                    if (!String.IsNullOrEmpty(basicAuthUser))
                    {
                        var byteArray = Encoding.ASCII.GetBytes($"{ basicAuthUser }:{ basicAuthPassword }");
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }

                    using (var luceneServerClient = new LuceneServerClient(
                        importConfig.Connection.Url,
                        importConfig.Connection.DefaultIndex,
                        httpClient: httpClient))
                    {
                        if (importConfig.Connection.DeleteIndex)
                        {
                            await luceneServerClient.RemoveIndexAsync();
                        }

                        if (!await luceneServerClient.CreateIndexAsync())
                        {
                            throw new Exception($"Can't create elasticsearch index { importConfig.Connection.DefaultIndex }");
                        }
                        if (!await luceneServerClient.MapAsync(new SearchIndexMapping(importConfig.Connection.PhoneticAlgorithm,
                                                                                      importConfig.Connection.EncodeCharacters)))
                        {
                            throw new Exception($"Can't map item in elasticsearch index { importConfig.Connection.DefaultIndex }");
                        }

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
                                    if (!await luceneServerClient.AddCustomMetadataAsync(metaId, JsonConvert.SerializeObject(meta)))
                                    {
                                        throw new Exception($"Can't index meta item in elasticsearch index { importConfig.Connection.MetaIndex }");
                                    }
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

                                List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
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

                                        if (items.Count >= packageSize)
                                        {
                                            if (!await luceneServerClient.IndexDocumentsAsync(items.ToArray()))
                                            {
                                                throw new Exception($"Error on indexing { items.Count } items on elasticsearch index { importConfig.Connection.DefaultIndex }");
                                            }
                                            items.Clear();

                                            Console.Write(count + "...");
                                        }
                                    }

                                    if (items.Count > 0)
                                    {
                                        if (!await luceneServerClient.IndexDocumentsAsync(items.ToArray()))
                                        {
                                            throw new Exception($"Error on indexing { items.Count } items on elasticsearch index { importConfig.Connection.DefaultIndex }");
                                        }
                                        Console.WriteLine(count + "...finish");
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
                else if (cmd == "remove-category")
                {
                    #region Remove Category

                    //RemoveCategory(indexUrl, indexName, replace ? Replace(category) : category,
                    //               proxyUrl, proxyUser, proxyPassword,
                    //               basicAuthUser, basicAuthPassword);

                    #endregion
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                return 1;
            }
        }

        static private Dictionary<string, object> ParseFeature(string metaId, string category, IFeature feature, Item proto, bool useGeometry, IGeometricTransformer transformer, ImportConfig.FeatureClassDefinition featureClassDef)
        {
            var replace = featureClassDef.Replacements;

            var result = new Dictionary<string, object>();

            string oid = feature.OID.ToString();
            if (feature.OID <= 0 && !String.IsNullOrWhiteSpace(featureClassDef.ObjectOidField))
            {
                var idFieldValue = feature.FindField(featureClassDef.ObjectOidField);
                if (idFieldValue != null)
                {
                    oid = idFieldValue.Value?.ToString();
                }
            }

            result["id"] = metaId + "." + oid;
            result["suggested_text"] = ParseFeatureField(feature, proto.SuggestedText);
            result["subtext"] = ParseFeatureField(feature, proto.SubText);
            result["thumbnail_url"] = ParseFeatureField(feature, proto.ThumbnailUrl);
            result["category"] = category;

            if (replace != null)
            {
                foreach (var r in replace)
                {
                    result["suggested_text"] = result["suggested_text"]?.ToString().Replace(r.From, r.To);
                    result["subtext"] = result["subtext"]?.ToString().Replace(r.From, r.To);
                }
            }

            if (useGeometry == true && feature.Shape != null)
            {
                IGeometry shape = feature.Shape;

                if (shape is IPoint)
                {
                    IPoint point = (IPoint)transformer.Transform2D(feature.Shape);
                    //result["longitude"] = point.X;
                    //result["latitude"] = point.Y;
                    result["geo"] = new LuceneServerNET.Core.Models.Spatial.GeoPoint()
                    {
                        Longidute = point.X,
                        Latitude = point.Y
                    };
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
                            //result["longitude"] = point.X;
                            //result["latitude"] = point.Y;
                            result["geo"] = new LuceneServerNET.Core.Models.Spatial.GeoPoint()
                            {
                                Longidute = point.X,
                                Latitude = point.Y
                            };
                        }

                        result["bbox"] = GetBBox(env, transformer);
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
                            //result["longitude"] = point.X;
                            //result["latitude"] = point.Y;
                            result["geo"] = new LuceneServerNET.Core.Models.Spatial.GeoPoint()
                            {
                                Longidute = point.X,
                                Latitude = point.Y
                            };
                        }

                        result["bbox"] = GetBBox(env, transformer);
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
    }
}
