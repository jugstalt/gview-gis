using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Cmd.LuceneServer.Lib.Models;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Geometry;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.Geometry.GeoProcessing;
using LuceneServerNET.Client;
using System.Net;
using System.Text;
using System.Text.Json;

namespace gView.Cmd.LuceneServer.Lib;

public class FillCommand : ICommand
{
    public string Name => "LuceneServer.Fill";

    public string Description => "Fills a lucene server index using defintion from an json file";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
        [
            new RequiredCommandParameter<string>("json")
            {
                Description = "Json File with import definition"
            },
            new CommandParameter<int>("package-size")
            {
                Description = "optional: number of items sended to lucene server per request (default 50000)"
            },
            new CommandParameter<string>("basic-auth-user")
            {
                Description = "optional: User for basic authentication"
            },
            new CommandParameter<string>("basic-auth-pwd")
            {
                Description = "optional: User password for basic authentication"
            },
            new CommandParameter<string>("proxy-url")
            {
                Description = "optional: a proxy server"
            },
            new CommandParameter<string>("proxy-user")
            {
                Description = "optional: a proxy server user"
            },
            new CommandParameter<string>("proxy-pwd")
            {
                Description = "optional: a proxy server user password"
            }
        ];

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        string jsonFile = parameters.GetRequiredValue<string>("json");

        int packageSize = parameters.GetValueOrDefault("package-size", 50000);

        string? basicAuthUser = parameters.GetValueOrDefault("basic-auth-user", "");
        string? basicAuthPassword = parameters.GetValueOrDefault("basic-auth-pwd", "");

        string? proxyUrl = parameters.GetValueOrDefault("proxy-url", "");
        string? proxyUser = parameters.GetValueOrDefault("proxy-user", "");
        string? proxyPassword = parameters.GetValueOrDefault("proxy-pwd", "");

        try
        {
            #region Fill Index (with Json File)

            var importConfig = JsonSerializer.Deserialize<ImportConfig>(File.ReadAllText(jsonFile));

            if (importConfig?.Connection == null)
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
                var byteArray = Encoding.ASCII.GetBytes($"{basicAuthUser}:{basicAuthPassword}");
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
                    throw new Exception($"Can't create elasticsearch index {importConfig.Connection.DefaultIndex}");
                }
                if (!await luceneServerClient.MapAsync(new SearchIndexMapping(importConfig.Connection.PhoneticAlgorithm,
                                                                              importConfig.Connection.EncodeCharacters)))
                {
                    throw new Exception($"Can't map item in elasticsearch index {importConfig.Connection.DefaultIndex}");
                }

                ISpatialReference sRefTarget = SpatialReference.FromID("epsg:4326");

                logger?.LogLine("Target Spatial Reference: " + sRefTarget.Name + " " + String.Join(" ", sRefTarget.Parameters));

                if (importConfig.Datasets is null || importConfig.Datasets.Count() == 0)
                {
                    throw new Exception("No datasets to import");
                }

                foreach (var datasetConfig in importConfig.Datasets)
                {
                    if (datasetConfig.FeatureClasses == null)
                    {
                        continue;
                    }

                    IDataset? dataset = new PlugInManager().CreateInstance(datasetConfig.DatasetGuid) as IDataset;
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
                        string category = featureClassConfig.Category;
                        if (!String.IsNullOrWhiteSpace(category))
                        {
                            var meta = new Meta()
                            {
                                Id = metaId,
                                Category = category,
                                Descrption = featureClassConfig.Meta?.Descrption ?? "",
                                Sample = featureClassConfig.Meta?.Sample ?? "",
                                Service = featureClassConfig.Meta?.Service ?? "",
                                Query = featureClassConfig.Meta?.Query ?? ""
                            };
                            if (!await luceneServerClient.AddCustomMetadataAsync(metaId, JsonSerializer.Serialize(meta)))
                            {
                                throw new Exception($"Can't index meta item in elasticsearch index {importConfig.Connection.MetaIndex}");
                            }
                        }

                        bool useGeometry = featureClassConfig.UseGeometry;

                        IDatasetElement dsElement = await dataset.Element(featureClassConfig.Name);
                        if (dsElement == null)
                        {
                            throw new ArgumentException("Unknown dataset element " + featureClassConfig.Name);
                        }

                        IFeatureClass? fc = dsElement.Class as IFeatureClass;
                        if (fc == null)
                        {
                            throw new ArgumentException("Dataobject is not a featureclass " + featureClassConfig.Name);
                        }

                        logger?.LogLine("Index " + fc.Name);
                        logger?.LogLine("=====================================================================");

                        QueryFilter filter = new QueryFilter();
                        filter.SubFields = "*";
                        if (!String.IsNullOrWhiteSpace(featureClassConfig.Filter))
                        {
                            filter.WhereClause = featureClassConfig.Filter;
                            logger?.LogLine("Filter: " + featureClassConfig.Filter);
                        }

                        List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
                        int count = 0;

                        ISpatialReference sRef = fc.SpatialReference ?? SpatialReference.FromID("epsg:" + featureClassConfig.SRefId);
                        logger?.LogLine("Source Spatial Reference: " + sRef.Name + " " + String.Join(" ", sRef.Parameters));
                        logger?.LogLine("IDField: " + fc.IDFieldName);

                        using (var transformer = GeometricTransformerFactory.Create(null))
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
                                        throw new Exception($"Error on indexing {items.Count} items on elasticsearch index {importConfig.Connection.DefaultIndex}");
                                    }
                                    items.Clear();

                                    logger?.Log(count + "...");
                                }
                            }

                            if (items.Count > 0)
                            {
                                if (!await luceneServerClient.IndexDocumentsAsync(items.ToArray()))
                                {
                                    throw new Exception($"Error on indexing {items.Count} items on elasticsearch index {importConfig.Connection.DefaultIndex}");
                                }
                                logger?.LogLine(count + "...finish");
                            }
                        }
                    }
                }


            }

            #endregion

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogLine("Exception:");
            logger?.LogLine("-----------------------------------------------------");
            logger?.LogLine(ex.Message);

            return false;
        }
    }

    #region Helper

    private static IFormatProvider _nhi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

    private Dictionary<string, object> ParseFeature(string metaId, string category, IFeature feature, Item proto, bool useGeometry, IGeometricTransformer transformer, ImportConfig.FeatureClassDefinition featureClassDef)
    {
        var replace = featureClassDef.Replacements;

        var result = new Dictionary<string, object>();

        string oid = feature.OID.ToString();
        if (feature.OID <= 0 && !String.IsNullOrWhiteSpace(featureClassDef.ObjectOidField))
        {
            var idFieldValue = feature.FindField(featureClassDef.ObjectOidField);
            if (idFieldValue != null)
            {
                oid = idFieldValue.Value?.ToString() ?? "";
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
                result["suggested_text"] = result["suggested_text"]?.ToString()?.Replace(r.From, r.To) ?? "";
                result["subtext"] = result["subtext"]?.ToString()?.Replace(r.From, r.To) ?? "";
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

    private string ParseFeatureField(IFeature feature, string pattern)
    {
        if (!pattern.Contains("{"))
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

            string val = fieldValue.Value != null
                ? fieldValue.Value.ToString() ?? ""
                : "";
            pattern = pattern.Replace("{" + parameter + "}", val);
        }

        return pattern;
    }

    private string[] GetKeyParameters(string pattern)
    {
        int pos1, pos2;
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
            return [];
        }
    }

    private string GetBBox(IEnvelope env, IGeometricTransformer transformer)
    {
        try
        {
            env = ((IGeometry)transformer.Transform2D(env)).Envelope;

            return Math.Round(env.MinX, 7).ToString(_nhi) + "," +
                   Math.Round(env.MinY, 7).ToString(_nhi) + "," +
                   Math.Round(env.MaxX, 7).ToString(_nhi) + "," +
                   Math.Round(env.MaxY, 7).ToString(_nhi);
        }
        catch { return String.Empty; }
    }

    #endregion
}
