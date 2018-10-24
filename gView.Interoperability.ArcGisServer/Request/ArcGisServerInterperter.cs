using gView.Framework.Carto;
using gView.Framework.Carto.Rendering;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Interoperability.ArcGisServer.Rest.Json;
using gView.Interoperability.ArcGisServer.Rest.Json.Features;
using gView.Interoperability.ArcGisServer.Rest.Json.Renderers.SimpleRenderers;
using gView.Interoperability.ArcGisServer.Rest.Json.Request;
using gView.Interoperability.ArcGisServer.Rest.Json.Response;
using gView.MapServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.Interoperability.ArcGisServer.Request
{
    [gView.Framework.system.RegisterPlugIn("6702B376-8848-435B-8611-6516A9726D3F")]
    public class ArcGisServerInterperter : IServiceRequestInterpreter
    {
        private IMapServer _mapServer;
        private JsonExportMap _exportMap = null;
        private bool _useTOC = false;

        #region IServiceRequestInterpreter

        public string IntentityName => "ags";

        public InterpreterCapabilities Capabilities =>
            new InterpreterCapabilities(new InterpreterCapabilities.Capability[]{
                    new InterpreterCapabilities.SimpleCapability("Emulating ArcGIS Server ",InterpreterCapabilities.Method.Post,"{server}/arcgis/rest/services/default/{service}/MapServer","1.0")
            });


        public void OnCreate(IMapServer mapServer)
        {
            _mapServer = mapServer;
        }

        public void Request(IServiceRequestContext context)
        {
            switch(context.ServiceRequest.Method.ToLower())
            {
                case "export":
                    ExportMapRequest(context);
                    break;
                case "query":
                    Query(context);
                    break;
                case "legend":
                    Legend(context);
                    break;
                case "featureservice_query":
                    break;
                default:
                    throw new NotImplementedException(context.ServiceRequest.Method + " is not support for arcgis server emulator");
            }
        }

        #region Export

        private void ExportMapRequest(IServiceRequestContext context)
        {
            _exportMap = JsonConvert.DeserializeObject<JsonExportMap>(context.ServiceRequest.Request);
            using (var serviceMap = context.CreateServiceMapInstance())
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
                serviceMap.Render();

                if (serviceMap.MapImage != null)
                {
                    

                    string fileName = serviceMap.Name.Replace(",", "_") + "_" + System.Guid.NewGuid().ToString("N") + "." + iFormat.ToString().ToLower();
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
                        error = new JsonError.Error()
                        {
                            code = -1,
                            message = "No image data"
                        }
                    });
                }
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
                switch(option)
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

            if(!String.IsNullOrWhiteSpace(_exportMap.DynamicLayers))
            {
                var jsonDynamicLayers = JsonConvert.DeserializeObject<JsonDynamicLayer[]>(_exportMap.DynamicLayers);
                foreach(var jsonDynamicLayer in jsonDynamicLayers)
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
                                if (jsonDynamicLayer.DrawingInfo.Renderer != null)
                                {
                                    if (jsonDynamicLayer.DrawingInfo.Renderer.Type.ToLower() == "simple")
                                    {
                                        var renderer = new SimpleRenderer();
                                        if (fc.GeometryType == geometryType.Point)
                                        {
                                            var jsonRenderer = JsonConvert.DeserializeObject<SimpleMarkerSymbol>(jsonDynamicLayer.DrawingInfo.Renderer.Symbol.ToString());

                                            if (jsonRenderer.Style == "esriSMSCircle")
                                            {
                                                var symbol = new gView.Framework.Symbology.SimplePointSymbol();
                                                symbol.SymbolSmothingMode = Framework.Symbology.SymbolSmoothing.AntiAlias;
                                                symbol.FillColor = ToColor(jsonRenderer.Color);
                                                symbol.Size = jsonRenderer.Size;
                                                if (jsonRenderer.Outline != null)
                                                {
                                                    symbol.OutlineColor = ToColor(jsonRenderer.Outline.Color);
                                                    symbol.OutlineWidth = jsonRenderer.Outline.Width;
                                                }
                                                renderer.Symbol = symbol;
                                            }
                                            else
                                            {
                                                throw new Exception("Unsupported MarkerSymbolStyle: " + jsonRenderer.Style);
                                            }
                                        }
                                        else if (fc.GeometryType == geometryType.Polyline)
                                        {
                                            var jsonRenderer = JsonConvert.DeserializeObject<SimpleLineSymbol>(jsonDynamicLayer.DrawingInfo.Renderer.Symbol.ToString());

                                            var symbol = new gView.Framework.Symbology.SimpleLineSymbol();
                                            symbol.SymbolSmothingMode = Framework.Symbology.SymbolSmoothing.AntiAlias;
                                            symbol.Color = ToColor(jsonRenderer.Color);
                                            symbol.Width = jsonRenderer.Width;
                                            renderer.Symbol = symbol;
                                        }
                                        else if (fc.GeometryType == geometryType.Polygon)
                                        {
                                            var jsonRenderer = JsonConvert.DeserializeObject<SimpleFillSymbol>(jsonDynamicLayer.DrawingInfo.Renderer.Symbol.ToString());

                                            var symbol = new gView.Framework.Symbology.SimpleFillSymbol();
                                            symbol.SymbolSmothingMode = Framework.Symbology.SymbolSmoothing.AntiAlias;
                                            symbol.FillColor = ToColor(jsonRenderer.Color);

                                            if(jsonRenderer.Outline!=null)
                                            {
                                                symbol.OutlineColor = ToColor(jsonRenderer.Outline.Color);
                                                symbol.OutlineWidth = jsonRenderer.Outline.Width;
                                            }
                                            renderer.Symbol = symbol;
                                        }
                                        else
                                        {
                                            throw new ArgumentException("Unsupported dynamic layer geometry: " + fc.GeometryType.ToString());
                                        }

                                        dynLayer.FeatureRenderer = renderer;
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Unknwon renderer type: " + jsonDynamicLayer.DrawingInfo.Renderer.Type);
                                    }
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

        private void Query(IServiceRequestContext context)
        {
            try
            {
                var query = JsonConvert.DeserializeObject<JsonQueryLayer>(context.ServiceRequest.Request);

                List<JsonFeature> jsonFeatures = new List<JsonFeature>();
                List<JsonFeatureResponse.Field> jsonFields = new List<JsonFeatureResponse.Field>();

                using (var serviceMap = context.CreateServiceMapInstance())
                {
                    string filterQuery;
                    foreach (var tableClass in FindTableClass(serviceMap, query.LayerId.ToString(), out filterQuery))
                    {
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
                        if (query.ReturnCountOnly == true)
                        {
                            filter.SubFields = !String.IsNullOrWhiteSpace(tableClass.IDFieldName) ? tableClass.IDFieldName : "*";
                        }
                        else
                        {
                            filter.SubFields = query.OutFields;
                        }
                        filter.WhereClause = query.Where;

                        if (filterQuery != String.Empty)
                            filter.WhereClause = (filter.WhereClause != String.Empty) ?
                                "(" + filter.WhereClause + ") AND " + filterQuery :
                                filterQuery;

                        #region Feature Spatial Reference

                        if (query.OutSRef != null)
                        {
                            filter.FeatureSpatialReference = SRef(query.OutSRef);
                        }

                        #endregion

                        var cursor = tableClass.Search(filter);

                        bool firstFeature = true;
                        if (cursor is IFeatureCursor)
                        {
                            IFeature feature;
                            IFeatureCursor featureCursor = (IFeatureCursor)cursor;
                            while ((feature = featureCursor.NextFeature) != null)
                            {
                                var jsonFeature = new JsonFeature();
                                var attributesDict = (IDictionary<string, object>)jsonFeature.Attributes;

                                if (feature.Fields != null)
                                {
                                    foreach (var field in feature.Fields)
                                    {
                                        attributesDict[field.Name] = field.Value;

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

                context.ServiceRequest.Succeeded = true;

                if (query.ReturnCountOnly == true)
                {
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureCountResponse()
                    {
                        Count = jsonFeatures.Count()
                    });
                }
                else
                {
                    context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonFeatureResponse()
                    {
                        Fields = jsonFields.ToArray(),
                        Features = jsonFeatures.ToArray()
                    });
                }
            }
            catch (Exception ex)
            {
                context.ServiceRequest.Succeeded = true;
                context.ServiceRequest.Response = JsonConvert.SerializeObject(new JsonError()
                {
                    error = new JsonError.Error()
                    {
                        code = -1,
                        message = ex.Message
                    }
                });
            }
        } 

        #endregion

        #region Legend

        private void Legend(IServiceRequestContext context)
        {
            var legendLayers = new List<Rest.Json.Legend.Layer>();

            using (var serviceMap = context.CreateServiceMapInstance())
            {
                foreach (var layer in serviceMap.MapElements)
                {
                    if (!(layer is IFeatureLayer) || ((IFeatureLayer)layer).FeatureRenderer == null)
                        continue;

                    var featureLayer = (IFeatureLayer)layer;

                    var tocElement = serviceMap.TOC.GetTOCElement(featureLayer);
                    using (var tocLegendItems = serviceMap.TOC.LegendSymbol(tocElement))
                    {
                        if (tocLegendItems.Items == null || tocLegendItems.Items.Count() == 0)
                            continue;

                        var legendLayer = new Rest.Json.Legend.Layer()
                        {
                            LayerId = featureLayer.ID,
                            LayerName = featureLayer.Title,
                            LayerType = "Feature-Layer",
                            MinScale = Convert.ToInt32(featureLayer.MaximumScale > 1 ? featureLayer.MaximumScale : 0),
                            MaxScale = Convert.ToInt32(featureLayer.MinimumScale > 1 ? featureLayer.MinimumScale : 0)
                        };

                        var legends = new List<Rest.Json.Legend.Legend>();
                        foreach(var tocLegendItem in tocLegendItems.Items)
                        {
                            if (tocLegendItem.Image == null)
                                continue;

                            MemoryStream ms = new MemoryStream();
                            tocLegendItem.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                            legends.Add(new Rest.Json.Legend.Legend()
                            {
                                Label = String.Empty,
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

        #endregion

        #region FeatureService Query

        public void FeatureServiceQuery(IServiceRequestContext context)
        {

        }

        #endregion

        #endregion

        #region Helper

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
                var spatialReference = JsonConvert.DeserializeObject<Rest.Json.JsonService.SpatialReference>(sref);

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
