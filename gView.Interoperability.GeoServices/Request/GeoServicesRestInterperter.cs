using gView.Framework.Carto;
using gView.Framework.Carto.Rendering;
using gView.Framework.Data;
using gView.Framework.Editor.Core;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Interoperability.GeoServices.Rest.Json;
using gView.Interoperability.GeoServices.Rest.Json.Features;
using gView.Interoperability.GeoServices.Rest.Json.FeatureServer;
using gView.Interoperability.GeoServices.Rest.Json.Renderers.SimpleRenderers;
using gView.Interoperability.GeoServices.Rest.Json.Request;
using gView.Interoperability.GeoServices.Rest.Json.Response;
using gView.MapServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Request
{
    [gView.Framework.system.RegisterPlugIn("6702B376-8848-435B-8611-6516A9726D3F")]
    public class GeoServicesRestInterperter : IServiceRequestInterpreter
    {
        private IMapServer _mapServer;
        private JsonExportMap _exportMap = null;
        private bool _useTOC = false;

        #region IServiceRequestInterpreter

        public string IdentityName => "geoservices";
        public string IdentityLongName => "OGC/ESRI GeoServices (REST)";

        public InterpreterCapabilities Capabilities =>
            new InterpreterCapabilities(new InterpreterCapabilities.Capability[]{
                    new InterpreterCapabilities.SimpleCapability("Catalog (REST)",InterpreterCapabilities.Method.Post,"{server}/geoservices/rest/services/{folder}","1.0"),
                    new InterpreterCapabilities.SimpleCapability("Service (REST)",InterpreterCapabilities.Method.Post,"{server}/geoservices/rest/services/{folder/service}/MapServer","1.0")
            });

        public void OnCreate(IMapServer mapServer)
        {
            _mapServer = mapServer;
        }

        async public Task Request(IServiceRequestContext context)
        {
            switch (context.ServiceRequest.Method.ToLower())
            {
                case "export":
                    await ExportMapRequest(context);
                    break;
                case "query":
                    await Query(context);
                    break;
                case "legend":
                    await Legend(context);
                    break;
                case "featureserver_query":
                    await Query(context, true);
                    break;
                case "featureserver_addfeatures":
                    await AddFeatures(context);
                    break;
                case "featureserver_updatefeatures":
                    await UpdateFeatures(context);
                    break;
                case "featureserver_deletefeatures":
                    await DeleteFeatures(context);
                    break;
                default:
                    throw new NotImplementedException(context.ServiceRequest.Method + " is not support for geoservices rest");
            }
        }

        public AccessTypes RequiredAccessTypes(IServiceRequestContext context)
        {
            var accessTypes = AccessTypes.None;

            switch (context.ServiceRequest.Method.ToLower())
            {
                case "export":
                    accessTypes |= AccessTypes.Map;
                    break;
                case "query":
                    accessTypes |= AccessTypes.Query;
                    break;
                case "legend":
                    accessTypes |= AccessTypes.Map;
                    break;
                case "featureserver_query":
                    accessTypes |= AccessTypes.Query;
                    break;
                case "featureserver_addfeatures":
                case "featureserver_updatefeatures":
                case "featureserver_deletefeatures":
                    accessTypes |= AccessTypes.Edit;
                    break;
            }

            return accessTypes;
        }

        #region Export

        async private Task ExportMapRequest(IServiceRequestContext context)
        {
            try
            {
                _exportMap = JsonConvert.DeserializeObject<JsonExportMap>(context.ServiceRequest.Request);
                using (var serviceMap = await context.CreateServiceMapInstance())
                {
                    #region Display

                    serviceMap.Display.dpi = _exportMap.Dpi;

                    var size = _exportMap.Size.ToSize();
                    serviceMap.Display.iWidth = size[0];
                    serviceMap.Display.iHeight = size[1];

                    var bbox = _exportMap.BBox.ToBBox();
                    serviceMap.Display.ZoomTo(new Envelope(bbox[0], bbox[1], bbox[2], bbox[3]));

                    #endregion

                    #region ImageFormat / Transparency

                    var imageFormat = (ImageFormat)Enum.Parse(typeof(ImageFormat), _exportMap.ImageFormat);
                    var iFormat = System.Drawing.Imaging.ImageFormat.Png;
                    if (imageFormat == ImageFormat.jpg)
                        iFormat = System.Drawing.Imaging.ImageFormat.Jpeg;

                    if (_exportMap.Transparent)
                    {
                        serviceMap.Display.MakeTransparent = true;
                        serviceMap.Display.TransparentColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        serviceMap.Display.MakeTransparent = false;
                    }

                    if (serviceMap.Display.MakeTransparent && iFormat == System.Drawing.Imaging.ImageFormat.Png)
                    {
                        // Beim Png sollt dann beim zeichnen keine Hintergrund Rectangle gemacht werden
                        // Darum Farbe mit A=0
                        // Sonst schaut das Bild beim PNG32 und Antialiasing immer zerrupft aus...
                        serviceMap.Display.BackgroundColor = System.Drawing.Color.Transparent;
                    }

                    #endregion

                    serviceMap.BeforeRenderLayers += ServiceMap_BeforeRenderLayers;
                    await serviceMap.Render();

                    if (serviceMap.MapImage != null)
                    {
                        string fileName = serviceMap.Name
                            .Replace("/", "_")
                            .Replace(",", "_") + "_" + System.Guid.NewGuid().ToString("N") + "." + iFormat.ToString().ToLower();

                        string path = (_mapServer.OutputPath + @"/" + fileName).ToPlattformPath();
                        serviceMap.SaveImage(path, iFormat);

                        context.ServiceRequest.Succeeded = true;
                        context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonExportResponse()
                        {
                            Href = _mapServer.OutputUrl + "/" + fileName,
                            Width = serviceMap.Display.iWidth,
                            Height = serviceMap.Display.iHeight,
                            ContentType = "image/" + iFormat.ToString().ToLower(),
                            Scale = serviceMap.Display.mapScale,
                            Extent = new JsonExtent()
                            {
                                Xmin = serviceMap.Display.Envelope.minx,
                                Ymin = serviceMap.Display.Envelope.miny,
                                Xmax = serviceMap.Display.Envelope.maxx,
                                Ymax = serviceMap.Display.Envelope.maxy
                                // ToDo: SpatialReference
                            }
                        });
                    }
                    else
                    {
                        context.ServiceRequest.Succeeded = false;
                        context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonError()
                        {
                            Error = new JsonError.ErrorDef()
                            {
                                Code = -1,
                                Message = "No image data"
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                context.ServiceRequest.Succeeded = false;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonError()
                {
                    Error = new JsonError.ErrorDef()
                    {
                        Code = -1,
                        Message = ex.Message
                    }
                });
            }
        }

        private void ServiceMap_BeforeRenderLayers(Framework.Carto.IServiceMap sender, List<Framework.Data.ILayer> layers)
        {
            if (String.IsNullOrWhiteSpace(_exportMap?.Layers) || !_exportMap.Layers.Contains(":"))
                return;

            string option = _exportMap.Layers.Substring(0, _exportMap.Layers.IndexOf(":")).ToLower();
            int[] layerIds = _exportMap.Layers.Substring(_exportMap.Layers.IndexOf(":") + 1)
                                    .Split(',').Select(l => int.Parse(l)).ToArray();

            foreach (var layer in layers)
            {
                switch (option)
                {
                    case "show":
                        layer.Visible = layerIds.Contains(layer.ID);
                        break;
                    case "hide":
                        layer.Visible = !layerIds.Contains(layer.ID);
                        break;
                    case "include":
                        if (layerIds.Contains(layer.ID))
                            layer.Visible = true;
                        break;
                    case "exclude":
                        if (layerIds.Contains(layer.ID))
                            layer.Visible = false;
                        break;
                }
            }

            if (!String.IsNullOrWhiteSpace(_exportMap.DynamicLayers))
            {
                var jsonDynamicLayers = JsonConvert.DeserializeObject<JsonDynamicLayer[]>(_exportMap.DynamicLayers);
                foreach (var jsonDynamicLayer in jsonDynamicLayers)
                {
                    if (jsonDynamicLayer.Source != null)
                    {
                        var featureLayers = MapServerHelper.FindMapLayers(sender, _useTOC, jsonDynamicLayer.Source.MapLayerId.ToString());

                        foreach (var featureLayer in featureLayers)
                        {
                            if (!(featureLayer.Class is IFeatureClass))
                                continue;

                            IFeatureClass fc = (IFeatureClass)featureLayer.Class;
                            var dynLayer = LayerFactory.Create(featureLayer.Class, featureLayer) as IFeatureLayer;
                            if (dynLayer != null)
                            {
                                if (jsonDynamicLayer.DrawingInfo?.Renderer != null)
                                {
                                    dynLayer.FeatureRenderer = JsonRenderer.FromJsonRenderer(jsonDynamicLayer.DrawingInfo.Renderer);
                                }
                                else
                                {
                                    dynLayer.FeatureRenderer = null;
                                }

                                if (jsonDynamicLayer.DrawingInfo?.LabelingInfo != null && jsonDynamicLayer.DrawingInfo.LabelingInfo.Length==1)
                                {
                                    var labelRenderer = jsonDynamicLayer.DrawingInfo.LabelingInfo[0].ToLabelRenderer();
                                    dynLayer.LabelRenderer = labelRenderer;
                                }
                                else
                                {
                                    dynLayer.LabelRenderer = null;
                                }
                                dynLayer.FilterQuery = new QueryFilter()
                                {
                                    SubFields = "*",
                                    WhereClause = jsonDynamicLayer.DefinitionExpression
                                };
                                dynLayer.Visible = true;
                                layers.Add(dynLayer);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Query

        async private Task Query(IServiceRequestContext context, bool isFeatureServer = false)
        {
            try
            {
                var query = JsonConvert.DeserializeObject<JsonQueryLayer>(context.ServiceRequest.Request);

                int featureCount = 0;
                List<JsonFeature> jsonFeatures = new List<JsonFeature>();
                List<JsonFeatureResponse.Field> jsonFields = new List<JsonFeatureResponse.Field>();
                string objectIdFieldName = String.Empty;
                EsriGeometryType esriGeometryType = EsriGeometryType.esriGeometryAny;
                JsonSpatialReference featureSref = null;

                using (var serviceMap = await context.CreateServiceMapInstance())
                {
                    string filterQuery;

                    var tableClasses = FindTableClass(serviceMap, query.LayerId.ToString(), out filterQuery);
                    if (isFeatureServer == true && tableClasses.Count > 1)
                    {
                        throw new Exception("FeatureService can't be used with aggregated feature classes");
                    }

                    foreach (var tableClass in tableClasses)
                    {
                        objectIdFieldName = tableClass.IDFieldName;
                        if (tableClass is IFeatureClass)
                        {
                            esriGeometryType = JsonLayer.ToGeometryType(((IFeatureClass)tableClass).GeometryType);
                        }

                        QueryFilter filter;
                    
                        if (!String.IsNullOrWhiteSpace(query.Geometry))
                        {
                            filter = new SpatialFilter();
                            var jsonGeometry = JsonConvert.DeserializeObject<Rest.Json.Features.Geometry.JsonGeometry>(query.Geometry);
                            ((SpatialFilter)filter).Geometry = jsonGeometry.ToGeometry();
                            ((SpatialFilter)filter).FilterSpatialReference = SRef(query.InSRef);
                        }
                        else
                        {
                            filter = new QueryFilter();
                        }
                        filter.WhereClause = query.Where;

                        if (query.ReturnCountOnly == true)
                        {
                            if (tableClass is ITableClass2 && !String.IsNullOrWhiteSpace(tableClass.IDFieldName))
                            {
                                featureCount += await ((ITableClass2)tableClass).ExecuteCount(filter);
                                continue;
                            }
                            else
                            {
                                filter.SubFields = !String.IsNullOrWhiteSpace(tableClass.IDFieldName) ? tableClass.IDFieldName : "*";
                            }
                        }
                        else
                        {
                            filter.SubFields = query.OutFields;
                        }

                        if (!String.IsNullOrWhiteSpace(query.OutSRef))
                        {
                            filter.FeatureSpatialReference = SRef(query.OutSRef);
                        }
                        else if (tableClass is IFeatureClass)
                        {
                            filter.FeatureSpatialReference = ((IFeatureClass)tableClass).SpatialReference;
                        }
                        if (filter.FeatureSpatialReference != null)
                        {
                            try
                            {
                                featureSref = new JsonSpatialReference()
                                {
                                    Wkid = int.Parse(filter.FeatureSpatialReference.Name.Split(':')[1])
                                };
                            }
                            catch { }
                        }

                        if (filterQuery != String.Empty)
                            filter.WhereClause = (filter.WhereClause != String.Empty) ?
                                "(" + filter.WhereClause + ") AND " + filterQuery :
                                filterQuery;

                        #region Feature Spatial Reference

                        if (query.OutSRef != null)
                        {
                            filter.FeatureSpatialReference = SRef(query.OutSRef);
                        }

                        filter.Limit = query.ResultRecordCount > 0 ?
                            Math.Min(query.ResultRecordCount, _mapServer.FeatureQueryLimit) :
                            _mapServer.FeatureQueryLimit;
                        filter.BeginRecord = query.ResultOffset + 1;  // Start is 1 by IQueryFilter definition

                        filter.OrderBy = query.OrderByFields;

                        #endregion

                        using (var cursor = await tableClass.Search(filter))
                        {

                            bool firstFeature = true;
                            if (cursor is IFeatureCursor)
                            {
                                IFeature feature;
                                IFeatureCursor featureCursor = (IFeatureCursor)cursor;
                                while ((feature = await featureCursor.NextFeature()) != null)
                                {
                                    featureCount++;
                                    var jsonFeature = new JsonFeature();
                                    var attributesDict = (IDictionary<string, object>)jsonFeature.Attributes;

                                    if (feature.Fields != null)
                                    {
                                        foreach (var field in feature.Fields)
                                        {
                                            object val = field.Value;
                                            if (val is DateTime)
                                            {
                                                val = Convert.ToInt64(((DateTime)val - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
                                            }
                                            attributesDict[field.Name] = val;

                                            if (firstFeature)
                                            {
                                                var tableField = tableClass.FindField(field.Name);
                                                if (tableField != null)
                                                {
                                                    jsonFields.Add(new JsonFeatureResponse.Field()
                                                    {
                                                        Name = tableField.name,
                                                        Alias = tableField.aliasname,
                                                        Length = tableField.size,
                                                        Type = JsonField.ToType(tableField.type).ToString()
                                                    });
                                                }
                                            }
                                        }
                                    }

                                    jsonFeature.Geometry = feature.Shape?.ToJsonGeometry();

                                    jsonFeatures.Add(jsonFeature);
                                    firstFeature = false;
                                }
                            }
                        }
                    }
                }

                context.ServiceRequest.Succeeded = true;

                if (isFeatureServer)
                {
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureServiceQueryResponse()
                    {
                        ObjectIdFieldName = objectIdFieldName,
                        GeometryType = esriGeometryType.ToString(),
                        SpatialReference = featureSref,
                        Fields = jsonFields.ToArray(),
                        Features = jsonFeatures.ToArray()
                    });
                }
                else if (query.ReturnCountOnly == true)
                {
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureCountResponse()
                    {
                        Count = featureCount  //jsonFeatures.Count()
                    });
                }
                else
                {
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureResponse()
                    {
                        GeometryType = esriGeometryType.ToString(),
                        SpatialReference = featureSref,
                        Fields = jsonFields.ToArray(),
                        Features = jsonFeatures.ToArray()
                    });
                }
            }
            catch (Exception ex)
            {
                context.ServiceRequest.Succeeded = false;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonError()
                {
                    Error = new JsonError.ErrorDef()
                    {
                        Code = -1,
                        Message = ex.Message
                    }
                });
            }
        }

        #endregion

        #region Legend

        async private Task Legend(IServiceRequestContext context)
        {
            try
            {
                var legendLayers = new List<Rest.Json.Legend.Layer>();

                using (var serviceMap = await context.CreateServiceMapInstance())
                {
                    foreach (var layer in serviceMap.MapElements)
                    {
                        if (!(layer is IFeatureLayer) || ((IFeatureLayer)layer).FeatureRenderer == null)
                            continue;

                        var featureLayer = (IFeatureLayer)layer;

                        var tocElement = serviceMap.TOC.GetTOCElement(featureLayer);
                        if (tocElement == null)
                            continue;

                        using (var tocLegendItems = serviceMap.TOC.LegendSymbol(tocElement))
                        {
                            if (tocLegendItems.Items == null || tocLegendItems.Items.Count() == 0)
                                continue;

                            var legendLayer = new Rest.Json.Legend.Layer()
                            {
                                LayerId = featureLayer.ID,
                                LayerName = tocElement.Name,
                                LayerType = "Feature-Layer",
                                MinScale = Convert.ToInt32(featureLayer.MaximumScale > 1 ? featureLayer.MaximumScale : 0),
                                MaxScale = Convert.ToInt32(featureLayer.MinimumScale > 1 ? featureLayer.MinimumScale : 0)
                            };

                            var legends = new List<Rest.Json.Legend.Legend>();
                            foreach (var tocLegendItem in tocLegendItems.Items)
                            {
                                if (tocLegendItem.Image == null)
                                    continue;

                                MemoryStream ms = new MemoryStream();
                                tocLegendItem.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                                legends.Add(new Rest.Json.Legend.Legend()
                                {
                                    Label = tocLegendItem?.Label,
                                    Url = Guid.NewGuid().ToString("N").ToString(),
                                    ImageData = Convert.ToBase64String(ms.ToArray()),
                                    ContentType = "image/png",
                                    Width = tocLegendItem.Image.Width,
                                    Height = tocLegendItem.Image.Height
                                });
                            }
                            legendLayer.Legend = legends.ToArray();
                            legendLayers.Add(legendLayer);
                        }
                    }
                }

                context.ServiceRequest.Succeeded = true;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new Rest.Json.Legend.LegendResponse()
                {
                    Layers = legendLayers.ToArray()
                });
            }
            catch (Exception ex)
            {
                context.ServiceRequest.Succeeded = false;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonError()
                {
                    Error = new JsonError.ErrorDef()
                    {
                        Code = -1,
                        Message = ex.Message
                    }
                });
            }
        }

        #endregion

        #region FeatureService

        async private Task AddFeatures(IServiceRequestContext context)
        {
            try
            {
                var editRequest = JsonConvert.DeserializeObject<JsonFeatureServerUpdateRequest>(context.ServiceRequest.Request);

                using (var serviceMap = await context.CreateServiceMapInstance())
                {
                    CheckEditableStatement(serviceMap, editRequest, EditStatements.INSERT);

                    var featureClass = GetFeatureClass(serviceMap, editRequest);
                    var dataset = featureClass.Dataset;
                    var database = dataset?.Database as IFeatureUpdater;
                    if (database == null)
                    {
                        throw new Exception("Featureclass is not editable");
                    }

                    List<IFeature> features = GetFeatures(featureClass, editRequest);
                    if (features.Count == 0)
                        throw new Exception("No features to add");

                    if (features.Where(f => f.OID > 0).Count() > 0)
                        throw new Exception("Can't insert features with existing ObjectId");

                    if (!await database.Insert(featureClass, features))
                        throw new Exception(database.LastErrorMessage);

                    context.ServiceRequest.Succeeded = true;
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(
                        new JsonFeatureServerResponse()
                        {
                            AddResults = new JsonFeatureServerResponse.JsonResponse[]
                            {
                            new JsonFeatureServerResponse.JsonResponse()
                            {
                                Success=true
                            }
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                context.ServiceRequest.Succeeded = false;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureServerResponse()
                {
                    AddResults = new JsonFeatureServerResponse.JsonResponse[]
                    {
                        new JsonFeatureServerResponse.JsonResponse()
                        {
                            Success=false,
                            Error=new JsonFeatureServerResponse.JsonError()
                            {
                                Code=999,
                                Description=ex.Message.Split('\n')[0]
                            }
                        }
                    }
                });
            }
        }

        async private Task UpdateFeatures(IServiceRequestContext context)
        {
            try
            {
                var editRequest = JsonConvert.DeserializeObject<JsonFeatureServerUpdateRequest>(context.ServiceRequest.Request);

                using (var serviceMap = await context.CreateServiceMapInstance())
                {
                    CheckEditableStatement(serviceMap, editRequest, EditStatements.UPDATE);

                    var featureClass = GetFeatureClass(serviceMap, editRequest);
                    var dataset = featureClass.Dataset;
                    var database = dataset?.Database as IFeatureUpdater;
                    if (database == null)
                    {
                        throw new Exception("Featureclass is not editable");
                    }

                    List<IFeature> features = GetFeatures(featureClass, editRequest);
                    if (features.Count == 0)
                        throw new Exception("No features to add");

                    if (features.Where(f => f.OID <= 0).Count() > 0)
                        throw new Exception("Can't update features without existing ObjectId");

                    if (!await database.Update(featureClass, features))
                        throw new Exception(database.LastErrorMessage);

                    context.ServiceRequest.Succeeded = true;
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(
                        new JsonFeatureServerResponse()
                        {
                            UpdateResults = new JsonFeatureServerResponse.JsonResponse[]
                            {
                            new JsonFeatureServerResponse.JsonResponse()
                            {
                                Success=true
                            }
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                context.ServiceRequest.Succeeded = false;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureServerResponse()
                {
                    UpdateResults = new JsonFeatureServerResponse.JsonResponse[]
                    {
                        new JsonFeatureServerResponse.JsonResponse()
                        {
                            Success=false,
                            Error=new JsonFeatureServerResponse.JsonError()
                            {
                                Code=999,
                                Description=ex.Message.Split('\n')[0]
                            }
                        }
                    }
                });
            }
        }

        async private Task DeleteFeatures(IServiceRequestContext context)
        {
            try
            {
                var editRequest = JsonConvert.DeserializeObject<JsonFeatureServerDeleteRequest>(context.ServiceRequest.Request);

                using (var serviceMap = await context.CreateServiceMapInstance())
                {
                    CheckEditableStatement(serviceMap, editRequest, EditStatements.DELETE);

                    var featureClass = GetFeatureClass(serviceMap, editRequest);
                    var dataset = featureClass.Dataset;
                    var database = dataset?.Database as IFeatureUpdater;
                    if (database == null)
                    {
                        throw new Exception("Featureclass is not editable");
                    }

                    foreach (int objectId in editRequest.ObjectIds.Split(',').Select(s => int.Parse(s)))
                    {
                        if (!await database.Delete(featureClass, objectId))
                            throw new Exception(database.LastErrorMessage);
                    }

                    context.ServiceRequest.Succeeded = true;
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(
                        new JsonFeatureServerResponse()
                        {
                            DeleteResults = new JsonFeatureServerResponse.JsonResponse[]
                            {
                            new JsonFeatureServerResponse.JsonResponse()
                            {
                                Success=true
                            }
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                context.ServiceRequest.Succeeded = false;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureServerResponse()
                {
                    DeleteResults = new JsonFeatureServerResponse.JsonResponse[]
                    {
                        new JsonFeatureServerResponse.JsonResponse()
                        {
                            Success=false,
                            Error=new JsonFeatureServerResponse.JsonError()
                            {
                                Code=999,
                                Description=ex.Message.Split('\n')[0]
                            }
                        }
                    }
                });
            }
        }

        #region Helper

        private IFeatureLayer GetFeatureLayer(IServiceMap serviceMap, JsonFeatureServerEditRequest editRequest)
        {
            return serviceMap.MapElements.Where(e => e.ID == editRequest.LayerId).FirstOrDefault() as IFeatureLayer;
        }

        private IFeatureClass GetFeatureClass(IServiceMap serviceMap, JsonFeatureServerEditRequest editRequest)
        {
            string filterQuery;

            var tableClasses = FindTableClass(serviceMap, editRequest.LayerId.ToString(), out filterQuery);
            if (tableClasses.Count > 1)
            {
                throw new Exception("FeatureService can't be used with aggregated feature classes");
            }
            if (tableClasses.Count == 0 || !(tableClasses[0] is IFeatureClass))
            {
                throw new Exception("FeatureService can only used with feature classes");
            }

            var featureClass = (IFeatureClass)tableClasses[0];

            return featureClass;
        }

        private List<IFeature> GetFeatures(IFeatureClass featureClass, JsonFeatureServerUpdateRequest editRequest)
        {
            List<IFeature> features = new List<IFeature>();
            foreach (var jsonFeature in editRequest.Features)
            {
                features.Add(ToFeature(featureClass, jsonFeature));
            }

            return features;
        }

        private void CheckEditableStatement(IServiceMap serviceMap, JsonFeatureServerEditRequest editRequest, EditStatements statement)
        {
            var editModule = serviceMap.GetModule<gView.Plugins.Modules.EditorModule>();
            if (editModule == null)
                throw new Exception("No editor module available for service");

            var editLayer = editModule.GetEditLayer(editRequest.LayerId);
            if (editLayer == null)
                throw new Exception("No editable layer found with id=" + editRequest.LayerId);

            if (!editLayer.Statements.HasFlag(statement))
                throw new Exception("Editoperation " + statement + " not allowed for layer with id=" + editRequest.LayerId);
        }

        #endregion

        #endregion

        #endregion

        #region Helper

        private Feature ToFeature(IFeatureClass fc, JsonFeature jsonFeature)
        {
            var feature = new Feature();

            feature.Shape = jsonFeature.Geometry.ToGeometry();
            var attributes = (IDictionary<string, object>)jsonFeature.Attributes;
            if (attributes == null)
                throw new ArgumentException("No features attributes!");

            for (int f = 0, fieldCount = fc.Fields.Count; f < fieldCount; f++)
            {
                var field = fc.Fields[f];

                if(attributes.ContainsKey(field.name))
                {
                    switch(field.type)
                    {
                        case FieldType.ID:
                            feature.Fields.Add(new FieldValue(field.name, attributes[field.name]));
                            feature.OID = Convert.ToInt32(attributes[field.name]);
                            break;
                        case FieldType.Date:
                            object val = attributes[field.name];
                            if (val is string)
                            {
                                if (val.ToString().Contains(" "))
                                {
                                    val = DateTime.ParseExact(val.ToString(),
                                        new string[]{
                                            "dd.MM.yyyy HH:mm:ss",
                                            "dd.MM.yyyy HH:mm",
                                            "yyyy.MM.dd HH:mm:ss",
                                            "yyyy.MM.dd HH:mm",
                                            "yyyy-MM-dd HH:mm:ss",
                                            "yyyy-MM-dd HH:mm"
                                        },
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None);
                                }
                                else
                                {
                                    val = DateTime.ParseExact(val.ToString(),
                                        new string[]{
                                            "dd.MM.yyyy",
                                            "yyyy.MM.dd",
                                            "yyyy-MM-dd"
                                            },
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None);
                                }
                            }
                            else if (val is long || val is int)
                            {
                                long esriDate = Convert.ToInt64(val);
                                val = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(esriDate);
                            }
                            feature.Fields.Add(new FieldValue(field.name, val));
                            break;
                        default:
                            feature.Fields.Add(new FieldValue(field.name, attributes[field.name]));
                            break;
                    }
                }
            }

            return feature;
        }

        private List<ITableClass> FindTableClass(IServiceMap map, string id, out string filterQuery)
        {
            filterQuery = String.Empty;
            if (map == null) return null;

            List<ITableClass> classes = new List<ITableClass>();
            foreach (ILayer element in MapServerHelper.FindMapLayers(map, _useTOC, id))
            {
                if (element.Class is ITableClass)
                    classes.Add(element.Class as ITableClass);

                if (element is IFeatureLayer)
                {
                    if (((IFeatureLayer)element).FilterQuery != null)
                    {
                        string fquery = ((IFeatureLayer)element).FilterQuery.WhereClause;
                        if (filterQuery == String.Empty)
                        {
                            filterQuery = fquery;
                        }
                        else if (filterQuery != fquery && fquery.Trim() != String.Empty)
                        {
                            filterQuery += " AND " + fquery;
                        }
                    }
                }
            }
            return classes;
        }

        private ISpatialReference SRef(string sref)
        {
            if (String.IsNullOrWhiteSpace(sref))
                return null;

            sref = sref.Trim();
            if (sref.StartsWith("{") && sref.EndsWith("}"))
            {
                var spatialReference = JsonConvert.DeserializeObject<Rest.Json.JsonMapService.SpatialReference>(sref);

                return SpatialReference.FromID("epsg:" + spatialReference.Wkid) ??
                       SpatialReference.FromID("epsg:" + spatialReference.LatestWkid);
            }

            return SpatialReference.FromID("epsg:" + sref);
        }

        private System.Drawing.Color ToColor(int[] col)
        {
            if (col == null)
                return System.Drawing.Color.Transparent;
            if (col.Length == 3)
                return System.Drawing.Color.FromArgb(col[0], col[1], col[2]);
            if(col.Length==4)
                return System.Drawing.Color.FromArgb(col[3], col[0], col[1], col[2]);

            throw new Exception("Invalid symbol color: [" + String.Join(",", col) + "]");
        }

        #endregion
    }
}
