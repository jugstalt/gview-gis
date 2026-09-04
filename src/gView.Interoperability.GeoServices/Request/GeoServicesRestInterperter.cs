using gView.Framework.Common;
using gView.Framework.Common.Diagnostics;
using gView.Framework.Common.Json;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Core.UI;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.Framework.Editor.Core;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.OGC.GeoJson;
using gView.Interoperability.Extensions;
using gView.Interoperability.GeoServices.Exceptions;
using gView.Interoperability.GeoServices.Extensions;
using gView.Interoperability.GeoServices.Request.Extensions;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Interoperability.GeoServices.Rest.DTOs.Features;
using gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer;
using gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;
using gView.Interoperability.GeoServices.Rest.DTOs.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Interoperability.GeoServices.Request;

[RegisterPlugIn("6702B376-8848-435B-8611-6516A9726D3F")]
public class GeoServicesRestInterperter : IServiceRequestInterpreter
{
    private IMapServer _mapServer;
    private JsonExportMapDTO _exportMap = null;

    private bool _useTOC = false;

    #region IServiceRequestInterpreter

    public string IdentityName => "geoservices";
    public string IdentityLongName => "OGC/ESRI GeoServices (REST)";

    public int Priority => 100;

    public InterpreterCapabilities Capabilities =>
        new InterpreterCapabilities(new InterpreterCapabilities.Capability[]
        {
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
            case "identify":
                await Identify(context);
                break;
            case "legend":
                await Legend(context);
                break;
            case "querylegends":
                await QueryLegends(context);
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
            case "featureserver_applyedits":
                await ApplyEdits(context);
                break;
            case "featureserver_applyedits_service":
                await ApplyEditsService(context);
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
            case "identify":
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
            case "featureserver_applyedits":
            case "featureserver_applyedits_service":
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
            _exportMap = JSerializer.Deserialize<JsonExportMapDTO>(context.ServiceRequest.Request);

            //Console.WriteLine(_exportMap.BBox);
            //Console.WriteLine(_exportMap.Size);

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                #region SpatialReference

                if (!String.IsNullOrWhiteSpace(_exportMap.BBoxSRef))
                {
                    serviceMap.Display.SpatialReference = SRef(_exportMap.BBoxSRef);
                }

                #endregion

                #region Display

                serviceMap.Display.Dpi = _exportMap.Dpi;
                //serviceMap.ScaleSymbolFactor = (float)_exportMap.Dpi / 96f;

                var size = _exportMap.Size.ToSize();
                serviceMap.Display.ImageWidth = size[0];
                serviceMap.Display.ImageHeight = size[1];

                serviceMap.ResizeImageSizeToMapServiceLimits();

                if (_exportMap.Rotation != 0.0)
                {
                    serviceMap.Display.DisplayTransformation.DisplayRotation = _exportMap.Rotation;
                }

                var bbox = _exportMap.BBox.ToBBox();

                serviceMap.Display.ZoomTo(new Envelope(bbox[0], bbox[1], bbox[2], bbox[3]));

                #endregion

                #region ImageFormat / Transparency

                if (!Enum.TryParse<ImageFormat>(_exportMap.ImageFormat, true, out ImageFormat imageFormat))
                {
                    throw new MapServerException("Unsuported image format: " + _exportMap.ImageFormat);
                }

                var iFormat = imageFormat.ToGraphicsEngineImageFormat();

                if (_exportMap.Transparent)
                {
                    serviceMap.Display.MakeTransparent = true;
                    serviceMap.Display.TransparentColor = GraphicsEngine.ArgbColor.White;
                }
                else
                {
                    serviceMap.Display.MakeTransparent = false;
                }

                if (serviceMap.Display.MakeTransparent && iFormat == GraphicsEngine.ImageFormat.Png)
                {
                    // Beim Png sollt dann beim zeichnen keine Hintergrund Rectangle gemacht werden
                    // Darum Farbe mit A=0
                    // Sonst schaut das Bild beim PNG32 und Antialiasing immer zerrupft aus...
                    serviceMap.Display.BackgroundColor = GraphicsEngine.ArgbColor.Transparent;
                }

                #endregion

                serviceMap.BeforeRenderLayers += ServiceMap_BeforeRenderLayers;
                await serviceMap.Render();

                if (serviceMap.MapImage != null)
                {
                    if (_exportMap.OutputFormat?.ToLower() == "image")
                    {
                        context.ServiceRequest.Succeeded = true;

                        MemoryStream ms = new MemoryStream();
                        serviceMap.MapImage.Save(ms, iFormat);

                        context.ServiceRequest.ResponseContentType = _exportMap.GetContentType();
                        context.ServiceRequest.Response = ms.ToArray();

                        // debug
                        //string fileName = serviceMap.Name
                        //    .Replace("/", "_")
                        //    .Replace(",", "_") + "_" + System.Guid.NewGuid().ToString("N") + "." + iFormat.ToString().ToLower();

                        //string path = (_mapServer.OutputPath + @"/" + fileName).ToPlatformPath();
                        //await serviceMap.SaveImage(path, iFormat);
                    }
                    else
                    {
                        string serviceMapName = serviceMap.Name.Replace("/", "_").Replace(",", "_");
                        string fileName =
                           $"{serviceMapName}_{System.Guid.NewGuid():N}.{iFormat.ToString().ToLower()}";

                        string path = ($"{_mapServer.OutputPath}/{fileName}").ToPlatformPath();
                        await serviceMap.SaveImage(path, iFormat);

                        context.ServiceRequest.Succeeded = true;
                        context.ServiceRequest.Response = new JsonExportResponseDTO()
                        {
                            Href = $"{context.ServiceRequest.OutputUrl}/{fileName}",
                            Width = serviceMap.Display.ImageWidth,
                            Height = serviceMap.Display.ImageHeight,
                            ContentType = $"image/{iFormat.ToString().ToLower()}",
                            Scale = serviceMap.Display.MapScale,
                            Dpi = (int)serviceMap.Display.Dpi,
                            Extent = new JsonExtentDTO()
                            {
                                Xmin = serviceMap.Display.Envelope.MinX,
                                Ymin = serviceMap.Display.Envelope.MinY,
                                Xmax = serviceMap.Display.Envelope.MaxX,
                                Ymax = serviceMap.Display.Envelope.MaxY
                                // ToDo: SpatialReference
                            },
                            IdleMilliseconds = ContextVariables.UseMetrics ? serviceMap.Metrics : null,
                            Error = serviceMap is IMap && ((IMap)serviceMap).HasRequestExceptions ?
                                new JsonErrorDTO.ErrorDef()
                                {
                                    Code = 999,
                                    Message = "MapExport exeption occured",
                                    Details = ((IMap)serviceMap).RequestExceptions.Select(ex => ex.Message)
                                } : null
                        };
                    }
                }
                else
                {
                    context.ServiceRequest.Succeeded = false;
                    context.ServiceRequest.Response = new JsonErrorDTO()
                    {
                        Error = new JsonErrorDTO.ErrorDef()
                        {
                            Code = -1,
                            Message = "No image data"
                        }
                    };
                }
            }
        }
        catch (Exception ex)
        {
            await context.HandleMapServerException(ex);
        }
    }

    private void ServiceMap_BeforeRenderLayers(
        IServiceMap sender,
        IServiceRequestContext context,
        List<ILayer> layers)
    {
        var mapLayersString = _exportMap?.Layers.Trim();

        if (!String.IsNullOrWhiteSpace(mapLayersString) &&
            mapLayersString.Contains(":") &&
            mapLayersString.IndexOf(":") < mapLayersString.Length - 1)  // show: ...
        {
            #region Apply Visibility

            string option = _exportMap.Layers.Substring(0, mapLayersString.IndexOf(":")).ToLower();
            int[] layerIds = _exportMap.Layers.Substring(mapLayersString.IndexOf(":") + 1)
                                    .Split(',').Select(l => int.Parse(l)).ToArray();


            foreach (var layer in layers)
            {
                var tocElement = sender.TOC?.GetTocElementByLayerId(layer.ID);
                bool layerIdContains = tocElement != null ?
                    LayerOrParentIsInArray(sender, tocElement, layerIds) :    // this is how AGS works: if group is shown -> all layers in group are shown...
                    layerIds.Contains(layer.ID);

                switch (option)
                {
                    case "show":
                        layer.Visible = layerIdContains;
                        break;
                    case "hide":
                        layer.Visible = !layerIdContains;
                        break;
                    case "include":
                        if (layerIdContains)
                        {
                            layer.Visible = true;
                        }
                        break;
                    case "exclude":
                        if (layerIdContains)
                        {
                            layer.Visible = false;
                        }

                        break;
                }
            }

            #endregion
        }

        if (!String.IsNullOrEmpty(_exportMap?.LayerDefs))
        {
            #region Apply Layer Definitions

            Dictionary<string, string> layerDefs = null;

            try
            {
                layerDefs = JSerializer.Deserialize<Dictionary<string, string>>(_exportMap.LayerDefs);

                foreach (var layerId in layerDefs.Keys)
                {
                    var lID = int.Parse(layerId);
                    var layer = layers.Where(l => l.ID == lID).FirstOrDefault();
                    if (layer is IFeatureLayer)
                    {
                        ((IFeatureLayer)layer).FilterQuery = ((IFeatureLayer)layer).FilterQuery.AppendWhereClause(layerDefs[layerId]);

                        //File.WriteAllText("c:\\temp\\filter.txt", ((IFeatureLayer)layer).FilterQuery.WhereClause);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MapServerException($"Can't parse layer definitions: {ex.Message}");
            }

            #endregion
        }

        if (!String.IsNullOrWhiteSpace(_exportMap.DynamicLayers))
        {
            #region Apply Dynamic Layers

            var jsonDynamicLayers = JSerializer.Deserialize<JsonDynamicLayerDTO[]>(_exportMap.DynamicLayers);
            foreach (var jsonDynamicLayer in jsonDynamicLayers)
            {
                if (jsonDynamicLayer.Source != null)
                {
                    var featureLayers = MapServerHelper.FindMapLayers(sender, _useTOC, jsonDynamicLayer.Source.MapLayerId.ToString());

                    foreach (var featureLayer in featureLayers)
                    {
                        if (!(featureLayer.Class is IFeatureClass))
                        {
                            continue;
                        }

                        IFeatureClass fc = (IFeatureClass)featureLayer.Class;
                        var dynLayer = LayerFactory.Create(featureLayer.Class, featureLayer) as IFeatureLayer;
                        if (dynLayer != null)
                        {
                            if (jsonDynamicLayer.DrawingInfo?.Renderer != null)
                            {
                                dynLayer.FeatureRenderer = JsonRendererDTO.FromJsonRenderer(jsonDynamicLayer.DrawingInfo.Renderer);
                            }
                            else
                            {
                                dynLayer.FeatureRenderer = null;
                            }

                            if (jsonDynamicLayer.DrawingInfo?.LabelingInfo != null && jsonDynamicLayer.DrawingInfo.LabelingInfo.Length == 1)
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

            #endregion
        }
    }

    private bool LayerOrParentIsInArray(IServiceMap map, ITocElement tocElement, int[] layerIds)
    {
        while (tocElement != null)
        {
            if (tocElement.Layers != null)
            {
                foreach (var layer in tocElement.Layers)
                {
                    if (layerIds.Contains(layer.ID))
                    {
                        return true;
                    }
                }
            }
            tocElement = tocElement.ParentGroup;
        }

        return false;
    }

    #endregion

    #region Query

    async private Task Query(IServiceRequestContext context, bool isFeatureServer = false)
    {
        try
        {
            var query = JSerializer.Deserialize<JsonQueryLayerDTO>(context.ServiceRequest.Request);

            int featureCount = 0;
            List<JsonFeatureDTO> jsonFeatures = new List<JsonFeatureDTO>();
            List<JsonFeatureResponseDTO.Field> jsonFields = new List<JsonFeatureResponseDTO.Field>();
            double idleMilliseconds = 0D;
            string objectIdFieldName = String.Empty;
            EsriGeometryType esriGeometryType = EsriGeometryType.esriGeometryAny;
            JsonSpatialReferenceDTO featureSref = null;

            #region GeoJson

            bool returnGeoJson = "geojson".Equals(query.OutputFormat, StringComparison.InvariantCultureIgnoreCase) &&
                     query.ReturnCountOnly == false &&
                     query.ReturnIdsOnly == false;
            List<IFeature> returnGeoJsonFeatures = new List<IFeature>();

            if (returnGeoJson && String.IsNullOrEmpty(query.OutSRef))
            {
                query.OutSRef = "4326";
            }

            Envelope extent = null;

            int maxRecordCount;

            #endregion

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                maxRecordCount = query.ReturnIdsOnly == true ?   // return all Ids!
                                int.MaxValue :
                                serviceMap.MapServiceProperties.MaxRecordCount;
                string filterQuery;

                var tableClasses = FindTableClass(serviceMap, query.LayerId.ToString(), out filterQuery);
                if (isFeatureServer == true && tableClasses.Count > 1)
                {
                    throw new MapServerException("FeatureService can't be used with aggregated feature classes");
                }

                if (!String.IsNullOrWhiteSpace(query.Where))
                {
                    query.Where.CheckWhereClauseForSqlInjection();
                }

                foreach (var tableClass in tableClasses)
                {
                    objectIdFieldName = tableClass.IDFieldName;
                    if (tableClass is IFeatureClass)
                    {
                        var geometryType = ((IFeatureClass)tableClass).GeometryType;
                        if (geometryType == GeometryType.Unknown)
                        {
                            var featureLayer = serviceMap.MapElements.Where(l => l.ID == query.LayerId).FirstOrDefault() as IFeatureLayer;
                            if (featureLayer != null)
                            {
                                geometryType = featureLayer.LayerGeometryType;
                            }
                        }

                        esriGeometryType = JsonLayerDTO.ToGeometryType(geometryType);
                    }

                    IQueryFilter filter;

                    if (!String.IsNullOrWhiteSpace(query.Geometry))
                    {
                        filter = new SpatialFilter();
                        var jsonGeometry = query.Geometry.ToJsonGeometry();
                        var filterGeometry = jsonGeometry.ToGeometry();

                        ((SpatialFilter)filter).Geometry = filterGeometry;
                        ((SpatialFilter)filter).FilterSpatialReference =
                            SRef(query.InSRef) ??
                            (filterGeometry.Srs > 0 ? SpatialReference.FromID($"epsg:{filterGeometry.Srs}") : null);
                    }
                    else if (query.ReturnDistinctValues)
                    {
                        filter = new DistinctFilter(query.OutFields);
                    }
                    else if (!String.IsNullOrWhiteSpace(query.ObjectIds))
                    {
                        filter = new RowIDFilter(tableClass.IDFieldName,
                            query.ObjectIds.Split(',').Select(id => int.Parse(id)).ToList());
                    }
                    else
                    {
                        filter = new QueryFilter();
                    }

                    #region Prepare Where Clause

                    filter.WhereClause = query.Where;

                    if (!String.IsNullOrWhiteSpace(filterQuery))
                    {
                        filter.WhereClause = (!String.IsNullOrWhiteSpace(filter.WhereClause)) ?
                            "(" + filter.WhereClause + ") AND (" + filterQuery + ")" :
                            filterQuery;
                    }

                    #endregion

                    if (query.ReturnCountOnly == true)
                    {
                        if (tableClass is ITableClass2 && !String.IsNullOrWhiteSpace(tableClass.IDFieldName))
                        {
                            #region Fast Count

                            featureCount += await ((ITableClass2)tableClass).ExecuteCount(filter);
                            continue;

                            #endregion
                        }
                        else
                        {
                            filter.SubFields = !String.IsNullOrWhiteSpace(tableClass.IDFieldName) ? tableClass.IDFieldName : "*";
                        }
                    }
                    else if (query.ReturnIdsOnly)
                    {
                        if (String.IsNullOrEmpty(tableClass.IDFieldName))
                        {
                            throw new MapServerException("Can't query IdsOnly. Table has no ID-Field.");
                        }

                        filter.SubFields = tableClass.IDFieldName;
                    }
                    else if (query.ReturnExtentOnly)
                    {
                        if (tableClass is IFeatureClass && !filter.HasField(((IFeatureClass)tableClass).ShapeFieldName))
                        {
                            filter.SubFields = ((IFeatureClass)tableClass).ShapeFieldName;
                        }
                        else
                        {
                            throw new MapServerException("ReturnExtentOnly can only applied on tables with shape/geometry field");
                        }
                    }
                    else if (query.ReturnDistinctValues)
                    {
                        if (query.ReturnGeometry)
                        {
                            throw new MapServerException("Geometry is not supported with DISTINCT.");
                        }
                    }
                    else
                    {
                        var outFields = query.OutFields.FieldsNames().CheckAllowedFunctions(tableClass, isFeatureServer);

                        filter.SubFields = String.Join(",", outFields);
                        if (query.ReturnGeometry)
                        {
                            if (tableClass is IFeatureClass && !filter.HasField(((IFeatureClass)tableClass).ShapeFieldName))
                            {
                                filter.AddField(((IFeatureClass)tableClass).ShapeFieldName);
                            }
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(query.OutSRef))
                    {
                        filter.SetFeatureSpatialReference(SRef(query.OutSRef), serviceMap.Display?.DatumTransformations);
                    }
                    else if (tableClass is IFeatureClass)
                    {
                        filter.SetFeatureSpatialReference(
                            ((IFeatureClass)tableClass).SpatialReference,
                            serviceMap.Display?.DatumTransformations);
                    }
                    if (filter.FeatureSpatialReference != null)
                    {
                        try
                        {
                            featureSref = new JsonSpatialReferenceDTO()
                            {
                                Wkid = int.Parse(filter.FeatureSpatialReference.Name.Split(':')[1])
                            };
                        }
                        catch { }
                    }

                    #region Limit/Begin/Order

                    filter.Limit = query.ReturnCountOnly ?
                        int.MaxValue :                                 // count queries always consider all matching features
                        query.ResultRecordCount > 0 ?
                            Math.Min(query.ResultRecordCount, maxRecordCount) :
                            maxRecordCount;

                    filter.BeginRecord = query.ReturnCountOnly ?
                        1 :
                        query.ResultOffset + 1;  // Start is 1 by IQueryFilter definition

                    filter.OrderBy = query.OrderByFields;

                    #endregion

                    bool transform = false;
                    bool hasZ = query.ReturnZ && (tableClass as IFeatureClass)?.HasZ == true;
                    bool hasM = query.ReturnM && (tableClass as IFeatureClass)?.HasM == true;

                    using (var geoTransfromer = GeometricTransformerFactory.Create(serviceMap.Display?.DatumTransformations))
                    {
                        if (tableClass is IFeatureClass fc && 
                            fc.SpatialReference == null && 
                            filter.FeatureSpatialReference != null && 
                            serviceMap.LayerDefaultSpatialReference != null)
                        {
                            geoTransfromer.SetSpatialReferences(serviceMap.LayerDefaultSpatialReference, filter.FeatureSpatialReference);
                            transform = true;

                            #region Transform Filter Geometry

                            if (filter is ISpatialFilter &&
                                ((ISpatialFilter)filter).FilterSpatialReference != null &&
                                !serviceMap.LayerDefaultSpatialReference.EpsgCode.Equals(((ISpatialFilter)filter).FilterSpatialReference?.EpsgCode))
                            {
                                using (var filterTransformer = GeometricTransformerFactory.Create(serviceMap.Display?.DatumTransformations))
                                {
                                    filterTransformer.SetSpatialReferences(((SpatialFilter)filter).FilterSpatialReference, serviceMap.LayerDefaultSpatialReference);
                                    ((SpatialFilter)filter).Geometry = filterTransformer.Transform2D(((SpatialFilter)filter).Geometry) as IGeometry;
                                }
                            }

                            #endregion
                        }

                        using (var cursor = await tableClass.Search(filter))
                        {
                            if (cursor == null)
                            {
                                throw new ExecuteQueryException();
                            }

                            bool firstFeature = true;
                            if (cursor is IFeatureCursor)
                            {
                                IFeature feature;
                                IFeatureCursor featureCursor = (IFeatureCursor)cursor;

                                while ((feature = await featureCursor.NextFeature()) != null)
                                {
                                    featureCount++;

                                    if (query.ReturnCountOnly)
                                    {
                                        continue;  // slow-count fallback: only the number of features matters
                                    }

                                    if (query.ReturnGeometry == false && query.ReturnExtentOnly == false)
                                    {
                                        feature.Shape = null;  // do not geometry
                                    }
                                    else if (transform)
                                    {
                                        feature.Shape = geoTransfromer.Transform2D(feature.Shape) as IGeometry;
                                    }

                                    if (returnGeoJson)
                                    {
                                        returnGeoJsonFeatures.Add(feature);
                                    }
                                    else if (query.ReturnExtentOnly)
                                    {
                                        #region Calculate Envelope

                                        if (feature.Shape != null)
                                        {
                                            if (extent == null)
                                            {
                                                extent = new Envelope(feature.Shape.Envelope);
                                            }
                                            else
                                            {
                                                extent.Union(feature.Shape.Envelope);
                                            }
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        var jsonFeature = new JsonFeatureDTO();
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
                                                        jsonFields.Add(new JsonFeatureResponseDTO.Field()
                                                        {
                                                            Name = tableField.name,
                                                            Alias = tableField.aliasname,
                                                            Length = tableField.size > 0 ? tableField.size : null,
                                                            Type = JsonFieldDTO.ToType(tableField.type).ToString()
                                                        });
                                                    }
                                                }
                                            }
                                        }

                                        jsonFeature.Geometry = feature.Shape?.ToJsonGeometry(hasZ, hasM);

                                        jsonFeatures.Add(jsonFeature);
                                        firstFeature = false;
                                    }
                                }
                            }

                            if (cursor is IDiagnostics)
                            {
                                idleMilliseconds = ((IDiagnostics)cursor).DiagnosticParameters?.IdleMilliseconds ?? 0D;
                            }
                        }
                    }
                }
            }

            context.ServiceRequest.Succeeded = true;

            if (query.ReturnCountOnly == true)
            {
                context.ServiceRequest.Response = new JsonFeatureCountResponseDTO()
                {
                    Count = featureCount  //jsonFeatures.Count()
                };
            }
            else if (query.ReturnIdsOnly)
            {
                var objectIds = jsonFeatures
                    .Select(f => Convert.ToInt32(((IDictionary<string, object>)f.Attributes)[objectIdFieldName]))
                    .ToArray();

                context.ServiceRequest.Response = new JsonObjectIdResponseDTO()
                {
                    ObjectIdFieldName = objectIdFieldName,
                    // No resultRecordCount => all ids are returned (exceededTransferLimit stays false).
                    // With resultRecordCount the result is a page (resultOffset is honored by filter.BeginRecord);
                    // if the page is full there may be more ids beyond it.
                    ExceededTransferLimit = query.ResultRecordCount > 0 && objectIds.Length >= query.ResultRecordCount,
                    ObjectIds = objectIds
                };
            }
            else if (query.ReturnExtentOnly)
            {
                context.ServiceRequest.Response = new JsonExtentResponseDTO()
                {
                    Extend = new JsonExtentDTO()
                    {
                        Xmin = extent != null ? extent.MinX : double.NaN,
                        Ymin = extent != null ? extent.MinY : double.NaN,
                        Xmax = extent != null ? extent.MaxX : double.NaN,
                        Ymax = extent != null ? extent.MaxY : double.NaN,
                        SpatialReference = featureSref
                    }
                };
            }
            else if (returnGeoJson)
            {
                context.ServiceRequest.Response = Encoding.UTF8.GetBytes(GeoJsonHelper.ToGeoJson(returnGeoJsonFeatures));
                context.ServiceRequest.ResponseContentType = Constants.JsonContentType;
            }
            else if (isFeatureServer)
            {
                context.ServiceRequest.Response = new JsonFeatureServiceQueryResponseDTO()
                {
                    ObjectIdFieldName = objectIdFieldName,
                    GeometryType = esriGeometryType.ToString(),
                    SpatialReference = featureSref,
                    Fields = jsonFields.ToArray(),
                    Features = jsonFeatures.ToArray(),
                    ExceededTransferLimit = jsonFeatures.Count() >= maxRecordCount
                };
            }
            else
            {
                context.ServiceRequest.Response = new JsonFeatureResponseDTO()
                {
                    GeometryType = esriGeometryType.ToString(),
                    SpatialReference = featureSref,
                    Fields = jsonFields.ToArray(),
                    Features = jsonFeatures.ToArray(),
                    ExceededTransferLimit = jsonFeatures.Count() >= maxRecordCount,
                    IdleMilliseconds = SystemVariables.UseDiagnostic ?
                            new Dictionary<string, double> { ["0"] = idleMilliseconds } :
                            null
                };
            }
        }
        catch (Exception ex)
        {
            await context.HandleMapServerException(ex);
        }
    }

    #endregion

    #region Identify

    async private Task Identify(IServiceRequestContext context)
    {
        try
        {
            // https://developers.arcgis.com/rest/services-reference/identify-map-service-.htm

            var identify = JSerializer.Deserialize<JsonIdentifyDTO>(context.ServiceRequest.Request);
            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                #region Parameters

                IGeometry geometry = null;
                switch (identify.geometryType?.ToLower())
                {
                    case "esrigeometrypoint":
                    case "esrigeometryenvelope":
                        geometry = identify.Geometry.ToJsonGeometry().ToGeometry();
                        break;
                    default:
                        geometry = JSerializer.Deserialize<Rest.DTOs.Features.Geometry.JsonGeometryDTO>(identify.Geometry)?.ToGeometry();
                        break;
                }
                if (geometry == null)
                {
                    throw new MapServerException("Invalid identify geometry");
                }

                var bbox = identify.MapExtent?.ToBBox();
                if (bbox == null || bbox.Length != 4)
                {
                    throw new Exception("Invalid identify map extent. Use <xmin>, <ymin>, <xmax>, <ymax>");
                }
                var mapExtent = new Envelope(bbox[0], bbox[1], bbox[2], bbox[3]);

                var imageDisplay = identify.ImageDisplay?.Split(',').Select(f => NumberExtensions.ToFloat(f)).ToArray();
                if (imageDisplay == null || imageDisplay.Length != 3)
                {
                    throw new MapServerException("Invalid identify image display. Use <width>, <height>, <dpi>");
                }

                ISpatialReference sRef = SpatialReference.FromID("epsg:" + identify.SRef);

                #endregion

                #region Initialize Display

                serviceMap.Display.SpatialReference = sRef;
                serviceMap.Display.ImageWidth = (int)imageDisplay[0];
                serviceMap.Display.ImageHeight = (int)imageDisplay[1];
                serviceMap.Display.Dpi = imageDisplay[2];

                serviceMap.Display.Limit = mapExtent;
                serviceMap.Display.ZoomTo(mapExtent);

                //serviceMap.Display.Image2World(geometry);  // the geometry is in world coordinate system!

                if (geometry is IPoint && mapExtent.Width > 0 && mapExtent.Height > 0)
                {
                    double tol = identify.PixelTolerance * serviceMap.Display.MapScale / (96 / 0.0254);  // [m]
                    if (sRef != null &&
                        sRef.SpatialParameters.IsGeographic)
                    {
                        tol = (180.0 * tol / Math.PI) / 6370000.0;
                    }

                    geometry = new Envelope(((IPoint)geometry).X - tol, ((IPoint)geometry).Y - tol, ((IPoint)geometry).X + tol, ((IPoint)geometry).Y + tol);
                }

                #endregion

                #region Collect Layers

                var layers = serviceMap.MapElements?
                                       .Where(l => l is ILayer)
                                       .Select(l => (ILayer)l)
                                       .ToArray() ?? new ILayer[0];

                if (!String.IsNullOrWhiteSpace(identify?.Layers) && identify.Layers.Contains(":"))
                {
                    #region Apply Visibility

                    string option = identify.Layers.Substring(0, identify.Layers.IndexOf(":")).ToLower();
                    int[] layerIds = identify.Layers.Substring(identify.Layers.IndexOf(":") + 1)
                                             .Split(',').Select(l => int.Parse(l)).ToArray();


                    foreach (var layer in layers)
                    {
                        var tocElement = serviceMap.TOC.GetTocElementByLayerId(layer.ID);
                        bool layerIdContains = tocElement != null ?
                            LayerOrParentIsInArray(serviceMap, tocElement, layerIds) :    // this is how AGS works: if group is shown -> all layers in group are shown...
                            layerIds.Contains(layer.ID);

                        switch (option)
                        {
                            case "show":
                                layer.Visible = layerIdContains;
                                break;
                            case "hide":
                                layer.Visible = !layerIdContains;
                                break;
                            case "include":
                                if (layerIdContains)
                                {
                                    layer.Visible = true;
                                }

                                break;
                            case "exclude":
                                if (layerIdContains)
                                {
                                    layer.Visible = false;
                                }

                                break;
                        }
                    }

                    #endregion
                }

                if (!String.IsNullOrEmpty(identify?.LayerDefs))
                {
                    #region Apply Layer Definitions

                    Dictionary<string, string> layerDefs = null;

                    try
                    {
                        layerDefs = JSerializer.Deserialize<Dictionary<string, string>>(identify.LayerDefs);

                        foreach (var layerId in layerDefs.Keys)
                        {
                            var lID = int.Parse(layerId);
                            var layer = layers.Where(l => l.ID == lID).FirstOrDefault();
                            if (layer is IFeatureLayer)
                            {
                                ((IFeatureLayer)layer).FilterQuery = ((IFeatureLayer)layer).FilterQuery.AppendWhereClause(layerDefs[layerId]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new MapServerException($"Can't parse layer definitions: {ex.Message}");
                    }

                    #endregion
                }

                #endregion

                #region Sptial Reference

                var spatialFilter = new SpatialFilter();

                spatialFilter.FilterSpatialReference = sRef;
                spatialFilter.SetFeatureSpatialReference(sRef, serviceMap.Display?.DatumTransformations);
                spatialFilter.Geometry = geometry;
                spatialFilter.SubFields = "*";

                #endregion

                var results = new List<JsonIdentifyResponseDTO.Result>();

                #region Query Layers

                foreach (var layer in layers.Where(l => l.Visible))
                {
                    ICursor cursor = null;

                    if (layer.Class is IFeatureClass)
                    {
                        cursor = await ((IFeatureClass)layer.Class).GetFeatures(spatialFilter);
                    }
                    else if (layer.Class is IPointIdentify)
                    {
                        using (var pointIdentifyContext = ((IPointIdentify)layer.Class).CreatePointIdentifyContext())
                        {
                            cursor = await ((IPointIdentify)layer.Class).PointQuery(serviceMap.Display, geometry.Envelope.Center, sRef, null, pointIdentifyContext);
                        }
                    }

                    if (cursor is IFeatureCursor)
                    {
                        IFeature feature;

                        while ((feature = await ((IFeatureCursor)cursor).NextFeature()) != null)
                        {
                            var result = new JsonIdentifyResponseDTO.Result() { LayerId = layer.ID, LayerName = layer.Title };

                            result.ResultAttributes = feature.AttributesDictionary();

                            if (identify.ReturnGeometry)
                            {
                                // ToDo: Project?
                                result.Geometry = feature.Shape?.ToJsonGeometry();
                            }

                            results.Add(result);
                        }
                    }
                    else if (cursor is IRowCursor)
                    {
                        IRow row;
                        while ((row = await ((IRowCursor)cursor).NextRow()) != null)
                        {
                            var result = new JsonIdentifyResponseDTO.Result() { LayerId = layer.ID, LayerName = layer.Title };

                            result.ResultAttributes = row.AttributesDictionary()
                                                         .AppendRasterAttributes();
                            if (identify.ReturnGeometry)
                            {
                                result.Geometry = geometry?.Envelope?.Center?.ToJsonGeometry();
                            }

                            results.Add(result);
                        }
                    }
                }

                #endregion

                context.ServiceRequest.Response = new JsonIdentifyResponseDTO()
                {
                    Results = results.ToArray()
                };
            }
        }
        catch (Exception ex)
        {
            await context.HandleMapServerException(ex);
        }
    }

    #endregion

    #region Legend

    async private Task Legend(IServiceRequestContext context)
    {
        try
        {
            var legendLayers = new List<Rest.DTOs.Legend.LayerDTO>();

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                foreach (var layer in serviceMap.MapElements.Where(l => l is ILayer).Select(l => (ILayer)l))
                {
                    var tocElement = serviceMap.TOC.GetTOCElement(layer);
                    if (tocElement == null)
                    {
                        continue;
                    }

                    using (var tocLegendItems = await serviceMap.TOC.LegendSymbol(tocElement))
                    {
                        if (tocLegendItems.Items == null || tocLegendItems.Items.Count() == 0)
                        {
                            continue;
                        }

                        var legendLayer = new Rest.DTOs.Legend.LayerDTO()
                        {
                            LayerId = layer.ID,
                            LayerName = tocElement.Name,
                            LayerType = "Feature-Layer",
                            MinScale = Convert.ToInt32(layer.MaximumScale > 1 ? layer.MaximumScale : 0),
                            MaxScale = Convert.ToInt32(layer.MinimumScale > 1 ? layer.MinimumScale : 0)
                        };

                        var legends = new List<Rest.DTOs.Legend.LegendDTO>();

                        foreach (var tocLegendItem in tocLegendItems.Items)
                        {
                            if (tocLegendItem.Image == null)
                            {
                                continue;
                            }

                            MemoryStream ms = new MemoryStream();
                            tocLegendItem.Image.Save(ms, GraphicsEngine.ImageFormat.Png);

                            legends.Add(new Rest.DTOs.Legend.LegendDTO()
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
            context.ServiceRequest.Response = new Rest.DTOs.Legend.LegendResponseDTO()
            {
                Layers = legendLayers.ToArray()
            };
        }
        catch (Exception ex)
        {
            await context.HandleMapServerException(ex);
        }
    }

    #endregion

    #region QueryLegends

    async private Task QueryLegends(IServiceRequestContext context)
    {
        try
        {
            var queryLegendsRequest = JSerializer.Deserialize<JsonQueryLegendsDTO>(context.ServiceRequest.Request);
            var legendLayers = new List<Rest.DTOs.Legend.LayerDTO>();

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                #region SpatialReference

                if (!String.IsNullOrWhiteSpace(queryLegendsRequest.BBoxSRef))
                {
                    serviceMap.Display.SpatialReference = SRef(queryLegendsRequest.BBoxSRef);
                }

                #endregion

                #region Display

                serviceMap.Display.Dpi = queryLegendsRequest.Dpi;
                //serviceMap.ScaleSymbolFactor = (float)_exportMap.Dpi / 96f;

                var size = queryLegendsRequest.Size.ToSize();
                serviceMap.Display.ImageWidth = size[0];
                serviceMap.Display.ImageHeight = size[1];

                serviceMap.ResizeImageSizeToMapServiceLimits();

                if (queryLegendsRequest.Rotation != 0.0)
                {
                    serviceMap.Display.DisplayTransformation.DisplayRotation = queryLegendsRequest.Rotation;
                }

                var bbox = queryLegendsRequest.BBox.ToBBox();

                serviceMap.Display.ZoomTo(new Envelope(bbox[0], bbox[1], bbox[2], bbox[3]));

                var filter = new SpatialFilter();
                filter.Geometry = serviceMap.Display.Envelope;
                filter.FilterSpatialReference = serviceMap.Display.SpatialReference;

                // todo: quick and dirty - use show:1,2,3, .... 
                var layerIds = String.IsNullOrEmpty(queryLegendsRequest.Layers) 
                    ? serviceMap.MapElements.Where(l=> l is IFeatureLayer).Select(l => l.ID).ToArray()
                    : queryLegendsRequest.Layers.Split(":")[1].Split(",")
                                                   .Select(id => int.Parse(id)).ToArray();

                #endregion

                foreach (var layer in serviceMap.MapElements
                                                .Where(l => l is IFeatureLayer)
                                                .Select(l => (IFeatureLayer)l)
                                                .Where(l => layerIds.Contains(l.ID)))
                {
                    if(!layer.RenderInScale(serviceMap.Display))
                    {
                        continue; 
                    }

                    var tocElement = serviceMap.TOC.GetTOCElement(layer);
                    if (tocElement == null)
                    {
                        continue;
                    }

                    filter.SubFields = layer.FeatureClass.IDFieldName ?? "*";
                    filter.WhereClause = layer.FilterQuery?.WhereClause;

                    var hasLegendItemsResult = await layer.HasLegendItems(filter);
                    if(!hasLegendItemsResult.hasItems)
                    {
                        continue;
                    }

                    using (var tocLegendItems = await serviceMap.TOC.LegendSymbol(tocElement, symbolKeys: hasLegendItemsResult.itemKeys))
                    {
                        if (tocLegendItems.Items == null || tocLegendItems.Items.Count() == 0)
                        {
                            continue;
                        }

                        var legendLayer = new Rest.DTOs.Legend.LayerDTO()
                        {
                            LayerId = layer.ID,
                            LayerName = tocElement.Name,
                            LayerType = "Feature-Layer",
                            MinScale = Convert.ToInt32(layer.MaximumScale > 1 ? layer.MaximumScale : 0),
                            MaxScale = Convert.ToInt32(layer.MinimumScale > 1 ? layer.MinimumScale : 0)
                        };

                        var legends = new List<Rest.DTOs.Legend.LegendDTO>();

                        foreach (var tocLegendItem in tocLegendItems.Items)
                        {
                            if (tocLegendItem.Image == null)
                            {
                                continue;
                            }

                            MemoryStream ms = new MemoryStream();
                            tocLegendItem.Image.Save(ms, GraphicsEngine.ImageFormat.Png);

                            legends.Add(new Rest.DTOs.Legend.LegendDTO()
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
            context.ServiceRequest.Response = new Rest.DTOs.Legend.LegendResponseDTO()
            {
                Layers = legendLayers.ToArray()
            };
        }
        catch (Exception ex)
        {
            await context.HandleMapServerException(ex);
        }
    }

    #endregion

    #region FeatureService

    async private Task AddFeatures(IServiceRequestContext context)
    {
        try
        {
            var editRequest = JSerializer.Deserialize<JsonFeatureServerUpdateRequestDTO>(context.ServiceRequest.Request);

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                CheckEditableStatement(serviceMap, editRequest, EditStatements.INSERT);

                var featureClass = GetFeatureClass(serviceMap, editRequest);
                var dataset = featureClass.Dataset;
                var database = dataset?.Database as IFeatureUpdater;
                if (database == null)
                {
                    throw new MapServerException("Featureclass is not editable");
                }

                List<IFeature> features = GetFeatures(featureClass, editRequest, true, serviceMap.Display?.DatumTransformations);
                if (features.Count == 0)
                {
                    throw new MapServerException("No features to add");
                }

                if (features.Count(f => f.OID > 0) > 0)
                {
                    throw new MapServerException("Can't insert features with existing ObjectId");
                }

                if (features.Count(f => f.Shape is null) > 0)
                {
                    throw new MapServerException("Insert features without geometry are not allowed");
                }

                features.GeometryMakeValid(serviceMap, featureClass);

                if (!await database.Insert(featureClass, features))
                {
                    throw new Exception(database.LastErrorMessage);
                }

                context.ServiceRequest.Succeeded = true;
                context.ServiceRequest.Response = JSerializer.Serialize(
                    new JsonFeatureServerResponseDTO()
                    {
                        AddResults = new JsonFeatureServerResponseDTO.JsonResponse[]
                        {
                            new JsonFeatureServerResponseDTO.JsonResponse()
                            {
                                Success = true
                            }
                        }
                    });
            }
        }
        catch (Exception ex)
        {
            await context.HandleFeatureServerException(ex);
        }
    }

    async private Task UpdateFeatures(IServiceRequestContext context)
    {
        try
        {
            var editRequest = JSerializer.Deserialize<JsonFeatureServerUpdateRequestDTO>(context.ServiceRequest.Request);

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                CheckEditableStatement(serviceMap, editRequest, EditStatements.UPDATE);

                var featureClass = GetFeatureClass(serviceMap, editRequest);
                var dataset = featureClass.Dataset;
                var database = dataset?.Database as IFeatureUpdater;
                if (database == null)
                {
                    throw new MapServerException("Featureclass is not editable");
                }

                List<IFeature> features = GetFeatures(featureClass, editRequest, true, serviceMap.Display?.DatumTransformations);
                if (features.Count == 0)
                {
                    throw new MapServerException("No features to add");
                }

                if (features.Where(f => f.OID <= 0).Count() > 0)
                {
                    throw new MapServerException("Can't update features without existing ObjectId");
                }

                features.GeometryMakeValid(serviceMap, featureClass);

                if (!await database.Update(featureClass, features))
                {
                    throw new Exception(database.LastErrorMessage);
                }

                context.ServiceRequest.Succeeded = true;
                context.ServiceRequest.Response = JSerializer.Serialize(
                    new JsonFeatureServerResponseDTO()
                    {
                        UpdateResults = features.Select(f => f.OID).ToEditJsonResponse(true).ToArray()
                    });
            }
        }
        catch (Exception ex)
        {
            await context.HandleFeatureServerException(ex);
        }
    }

    async private Task DeleteFeatures(IServiceRequestContext context)
    {
        try
        {
            var editRequest = JSerializer.Deserialize<JsonFeatureServerDeleteRequestDTO>(context.ServiceRequest.Request);

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                CheckEditableStatement(serviceMap, editRequest, EditStatements.DELETE);

                var featureClass = GetFeatureClass(serviceMap, editRequest);
                var dataset = featureClass.Dataset;
                var database = dataset?.Database as IFeatureUpdater;
                if (database == null)
                {
                    throw new MapServerException("Featureclass is not editable");
                }

                var objectIds = editRequest.ObjectIds.Split(',').Select(s => int.Parse(s));
                foreach (int objectId in objectIds)
                {
                    if (!await database.Delete(featureClass, objectId))
                    {
                        throw new Exception(database.LastErrorMessage);
                    }
                }

                context.ServiceRequest.Succeeded = true;
                context.ServiceRequest.Response = JSerializer.Serialize(
                    new JsonFeatureServerResponseDTO()
                    {
                        DeleteResults = objectIds.ToEditJsonResponse(true).ToArray()
                    });
            }
        }
        catch (Exception ex)
        {
            await context.HandleFeatureServerException(ex);
        }
    }

    /// <summary>
    /// Layer level applyEdits (.../FeatureServer/{layerId}/applyEdits).
    /// This is the operation used by QGIS to commit adds, updates and deletes in one request.
    /// </summary>
    async private Task ApplyEdits(IServiceRequestContext context)
    {
        try
        {
            var editRequest = JSerializer.Deserialize<JsonFeatureServerApplyEditsRequestDTO>(context.ServiceRequest.Request);

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                var response = await ApplyEditsToLayer(
                    serviceMap,
                    editRequest.LayerId,
                    editRequest.Adds,
                    editRequest.Updates,
                    ParseObjectIds(editRequest.Deletes),
                    editRequest.RollbackOnFailure);

                context.ServiceRequest.Succeeded = true;
                context.ServiceRequest.Response = JSerializer.Serialize(response);
            }
        }
        catch (Exception ex)
        {
            await context.HandleFeatureServerException(ex);
        }
    }

    /// <summary>
    /// Service level applyEdits (.../FeatureServer/applyEdits) with an "edits" array
    /// containing per layer adds/updates/deletes. This is the variant used by ArcGIS Pro.
    /// </summary>
    async private Task ApplyEditsService(IServiceRequestContext context)
    {
        try
        {
            var editRequest = JSerializer.Deserialize<JsonFeatureServerApplyEditsServiceRequestDTO>(context.ServiceRequest.Request);

            if (editRequest?.Edits == null || editRequest.Edits.Length == 0)
            {
                throw new MapServerException("applyEdits: no 'edits' in request");
            }

            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                var results = new List<JsonFeatureServerApplyEditsServiceResultDTO>();

                foreach (var layerEdits in editRequest.Edits)
                {
                    try
                    {
                        var layerResponse = await ApplyEditsToLayer(
                            serviceMap,
                            layerEdits.Id,
                            layerEdits.Adds,
                            layerEdits.Updates,
                            ParseObjectIds(layerEdits.Deletes),
                            editRequest.RollbackOnFailure);

                        results.Add(new JsonFeatureServerApplyEditsServiceResultDTO()
                        {
                            Id = layerEdits.Id,
                            AddResults = layerResponse.AddResults,
                            UpdateResults = layerResponse.UpdateResults,
                            DeleteResults = layerResponse.DeleteResults
                        });
                    }
                    catch (Exception ex) when (!editRequest.RollbackOnFailure)
                    {
                        results.Add(new JsonFeatureServerApplyEditsServiceResultDTO()
                        {
                            Id = layerEdits.Id,
                            AddResults = new[] { ApplyEditsErrorResponse(ex.Message) }
                        });
                    }
                }

                context.ServiceRequest.Succeeded = true;
                context.ServiceRequest.Response = JSerializer.Serialize(results.ToArray());
            }
        }
        catch (Exception ex)
        {
            await context.HandleFeatureServerException(ex);

            // service level applyEdits returns a top level error object on a full failure
            context.ServiceRequest.Response = JSerializer.Serialize(new JsonErrorDTO()
            {
                Error = new JsonErrorDTO.ErrorDef()
                {
                    Code = 500,
                    Message = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Shared implementation for the layer level and service level applyEdits operations.
    /// Adds, updates and deletes are applied in this order. Each phase (insert/update) is
    /// executed as a single database call (transactional per phase); a true rollback across
    /// all three phases is not supported, so with rollbackOnFailure=true the first failing
    /// phase aborts the request and phases that already succeeded are kept.
    /// </summary>
    async private Task<JsonFeatureServerResponseDTO> ApplyEditsToLayer(
        IServiceMap serviceMap,
        int layerId,
        JsonFeatureDTO[] adds,
        JsonFeatureDTO[] updates,
        IReadOnlyList<int> deleteObjectIds,
        bool rollbackOnFailure)
    {
        var featureClass = GetFeatureClass(serviceMap, layerId);
        var database = featureClass?.Dataset?.Database as IFeatureUpdater;
        if (database == null)
        {
            throw new MapServerException("Featureclass is not editable");
        }

        var datumTransformations = serviceMap.Display?.DatumTransformations;

        bool hasAdds = adds is { Length: > 0 };
        bool hasUpdates = updates is { Length: > 0 };
        bool hasDeletes = deleteObjectIds is { Count: > 0 };

        if (!hasAdds && !hasUpdates && !hasDeletes)
        {
            throw new MapServerException("applyEdits: request contains no adds, updates or deletes");
        }

        var response = new JsonFeatureServerResponseDTO();

        #region Adds

        if (hasAdds)
        {
            CheckEditableStatement(serviceMap, layerId, EditStatements.INSERT);

            var addFeatures = GetFeatures(featureClass, adds, true, datumTransformations);

            if (addFeatures.Any(f => f.OID > 0))
            {
                throw new MapServerException("Can't insert features with existing ObjectId");
            }
            if (addFeatures.Any(f => f.Shape is null))
            {
                throw new MapServerException("Insert features without geometry are not allowed");
            }

            addFeatures.GeometryMakeValid(serviceMap, featureClass);

            if (!await database.Insert(featureClass, addFeatures))
            {
                if (rollbackOnFailure)
                {
                    throw new MapServerException($"applyEdits (adds) failed: {database.LastErrorMessage}");
                }

                response.AddResults = addFeatures
                    .Select(_ => ApplyEditsErrorResponse(database.LastErrorMessage))
                    .ToArray();
            }
            else
            {
                // NOTE: the IFeatureUpdater.Insert implementations do not report back the
                // generated ObjectIds, so addResults are returned without "objectId".
                // Clients (QGIS) pick up the real ids on the next layer refresh.
                response.AddResults = addFeatures
                    .Select(_ => new JsonFeatureServerResponseDTO.JsonResponse() { Success = true })
                    .ToArray();
            }
        }

        #endregion

        #region Updates

        if (hasUpdates)
        {
            CheckEditableStatement(serviceMap, layerId, EditStatements.UPDATE);

            var updateFeatures = GetFeatures(featureClass, updates, true, datumTransformations);

            if (updateFeatures.Any(f => f.OID <= 0))
            {
                throw new MapServerException("Can't update features without existing ObjectId");
            }

            updateFeatures.GeometryMakeValid(serviceMap, featureClass);

            if (!await database.Update(featureClass, updateFeatures))
            {
                if (rollbackOnFailure)
                {
                    throw new MapServerException($"applyEdits (updates) failed: {database.LastErrorMessage}");
                }

                response.UpdateResults = updateFeatures
                    .Select(f => ApplyEditsErrorResponse(database.LastErrorMessage, f.OID))
                    .ToArray();
            }
            else
            {
                response.UpdateResults = updateFeatures
                    .Select(f => f.OID)
                    .ToEditJsonResponse(true)
                    .ToArray();
            }
        }

        #endregion

        #region Deletes

        if (hasDeletes)
        {
            CheckEditableStatement(serviceMap, layerId, EditStatements.DELETE);

            var deleteResults = new List<JsonFeatureServerResponseDTO.JsonResponse>(deleteObjectIds.Count);

            foreach (var objectId in deleteObjectIds)
            {
                if (await database.Delete(featureClass, objectId))
                {
                    deleteResults.Add(new JsonFeatureServerResponseDTO.JsonResponse()
                    {
                        Success = true,
                        ObjectId = objectId
                    });
                }
                else
                {
                    if (rollbackOnFailure)
                    {
                        throw new MapServerException($"applyEdits (deletes) failed for objectId={objectId}: {database.LastErrorMessage}");
                    }

                    deleteResults.Add(ApplyEditsErrorResponse(database.LastErrorMessage, objectId));
                }
            }

            response.DeleteResults = deleteResults.ToArray();
        }

        #endregion

        return response;
    }

    private static JsonFeatureServerResponseDTO.JsonResponse ApplyEditsErrorResponse(string message, int? objectId = null)
        => new JsonFeatureServerResponseDTO.JsonResponse()
        {
            Success = false,
            ObjectId = objectId,
            Error = new JsonFeatureServerResponseDTO.JsonError()
            {
                Code = 999,
                Description = String.IsNullOrWhiteSpace(message) ? "unknown error" : message
            }
        };

    /// <summary>
    /// Parses the "deletes" parameter of the layer level applyEdits operation.
    /// Accepts "1,2,3", "[1,2,3]" and "[{ \"objectId\": 1 }, ...]".
    /// </summary>
    private static IReadOnlyList<int> ParseObjectIds(string deletes)
    {
        var result = new List<int>();

        if (String.IsNullOrWhiteSpace(deletes))
        {
            return result;
        }

        var text = deletes.Trim();

        if (text.StartsWith("[") || text.StartsWith("{"))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(text);
                AppendObjectIds(doc.RootElement, result);
                return result;
            }
            catch
            {
                text = text.Trim('[', ']', ' ');
            }
        }

        foreach (var part in text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var oid))
            {
                result.Add(oid);
            }
        }

        return result;
    }

    private static IReadOnlyList<int> ParseObjectIds(System.Text.Json.JsonElement? deletes)
    {
        var result = new List<int>();

        if (deletes.HasValue)
        {
            AppendObjectIds(deletes.Value, result);
        }

        return result;
    }

    private static void AppendObjectIds(System.Text.Json.JsonElement element, List<int> target)
    {
        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    AppendObjectIds(item, target);
                }
                break;
            case System.Text.Json.JsonValueKind.Number:
                if (element.TryGetInt32(out var number))
                {
                    target.Add(number);
                }
                break;
            case System.Text.Json.JsonValueKind.String:
                if (int.TryParse(element.GetString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var s))
                {
                    target.Add(s);
                }
                break;
            case System.Text.Json.JsonValueKind.Object:
                if (element.TryGetProperty("objectId", out var objectIdProperty)
                    && objectIdProperty.ValueKind == System.Text.Json.JsonValueKind.Number
                    && objectIdProperty.TryGetInt32(out var objectId))
                {
                    target.Add(objectId);
                }
                break;
        }
    }

    #region Helper

    private IFeatureLayer GetFeatureLayer(IServiceMap serviceMap, JsonFeatureServerEditRequesDTO editRequest)
    {
        return serviceMap.MapElements.Where(e => e.ID == editRequest.LayerId).FirstOrDefault() as IFeatureLayer;
    }

    private IFeatureClass GetFeatureClass(IServiceMap serviceMap, JsonFeatureServerEditRequesDTO editRequest)
        => GetFeatureClass(serviceMap, editRequest.LayerId);

    private IFeatureClass GetFeatureClass(IServiceMap serviceMap, int layerId)
    {
        string filterQuery;

        var tableClasses = FindTableClass(serviceMap, layerId.ToString(), out filterQuery);
        if (tableClasses.Count > 1)
        {
            throw new MapServerException("FeatureService can't be used with aggregated feature classes");
        }
        if (tableClasses.Count == 0 || !(tableClasses[0] is IFeatureClass))
        {
            throw new MapServerException("FeatureService can only used with feature classes");
        }

        var featureClass = (IFeatureClass)tableClasses[0];

        return featureClass;
    }

    private List<IFeature> GetFeatures(IFeatureClass featureClass, JsonFeatureServerUpdateRequestDTO editRequest, bool projetToFeatureClassSpatialReference, IDatumTransformations datumTransformations)
        => GetFeatures(featureClass, editRequest.Features, projetToFeatureClassSpatialReference, datumTransformations);

    private List<IFeature> GetFeatures(IFeatureClass featureClass, IEnumerable<JsonFeatureDTO> jsonFeatures, bool projetToFeatureClassSpatialReference, IDatumTransformations datumTransformations)
    {
        int? fcSrs = featureClass?.SpatialReference?.EpsgCode;

        List<IFeature> features = new List<IFeature>();
        foreach (var jsonFeature in jsonFeatures ?? Enumerable.Empty<JsonFeatureDTO>())
        {
            var feature = ToFeature(featureClass, jsonFeature);

            if (projetToFeatureClassSpatialReference == true)
            {
                if (feature.Shape != null && feature.Shape.Srs.HasValue && !feature.Shape.Srs.Equals(fcSrs))
                {
                    using (var transformer = GeometricTransformerFactory.Create(datumTransformations))
                    {
                        var fromSrs = SpatialReference.FromID("epsg:" + feature.Shape.Srs.ToString());
                        var toSrs = SpatialReference.FromID("epsg:" + fcSrs.ToString());

                        transformer.SetSpatialReferences(fromSrs, toSrs);

                        var transformedShape = transformer.Transform2D(feature.Shape) as IGeometry;
                        transformedShape.Srs = fcSrs;

                        feature.Shape = transformedShape;
                    }
                }
            }

            features.Add(feature);
        }

        return features;
    }

    private void CheckEditableStatement(IServiceMap serviceMap, JsonFeatureServerEditRequesDTO editRequest, EditStatements statement)
        => CheckEditableStatement(serviceMap, editRequest.LayerId, statement);

    private void CheckEditableStatement(IServiceMap serviceMap, int layerId, EditStatements statement)
    {
        var editModule = serviceMap.GetModule<gView.Plugins.Modules.EditorModule>();
        if (editModule == null)
        {
            throw new MapServerException("No editor module available for service");
        }

        var editLayer = editModule.GetEditLayer(layerId);
        if (editLayer == null)
        {
            throw new MapServerException($"No editable layer found with id={layerId}");
        }

        if (!editLayer.Statements.HasFlag(statement))
        {
            throw new MapServerException($"Editoperation {statement} not allowed for layer with id={layerId}");
        }
    }

    #endregion

    #endregion

    #endregion

    #region Helper

    private Feature ToFeature(IFeatureClass fc, JsonFeatureDTO jsonFeature)
    {
        var feature = new Feature();

        feature.Shape = jsonFeature.Geometry.ToGeometry();
        //var attributes = jsonFeature.Attributes.ToDictionaryWithNativeTypes();
        var attributes = (IDictionary<string, object>)jsonFeature.Attributes;

        if (attributes == null)
        {
            throw new MapServerException("No features attributes!");
        }

        for (int f = 0, fieldCount = fc.Fields.Count; f < fieldCount; f++)
        {
            var field = fc.Fields[f];

            if (attributes.ContainsKey(field.name))
            {
                switch (field.type)
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
        if (map == null)
        {
            return null;
        }

        List<ITableClass> classes = new List<ITableClass>();
        foreach (ILayer element in MapServerHelper.FindMapLayers(map, _useTOC, id))
        {
            if (element.Class is ITableClass)
            {
                classes.Add(element.Class as ITableClass);
            }

            if (element is IFeatureLayer)
            {
                if (((IFeatureLayer)element).FilterQuery != null)
                {
                    string fquery = ((IFeatureLayer)element).FilterQuery.WhereClause;
                    if (String.IsNullOrWhiteSpace(filterQuery))
                    {
                        filterQuery = fquery;
                    }
                    else if (filterQuery != fquery)
                    {
                        filterQuery = "(" + filterQuery + ") AND (" + fquery + ")";
                    }
                }
            }
        }
        return classes;
    }

    private ISpatialReference SRef(string sref)
    {
        if (String.IsNullOrWhiteSpace(sref))
        {
            return null;
        }

        sref = sref.Trim();
        if (sref.StartsWith("{") && sref.EndsWith("}"))
        {
            var spatialReference = JSerializer.Deserialize<Rest.DTOs.JsonMapServiceDTO.SpatialReference>(sref);

            return SpatialReference.FromID("epsg:" + spatialReference.Wkid) ??
                   SpatialReference.FromID("epsg:" + spatialReference.LatestWkid);
        }

        return SpatialReference.FromID("epsg:" + sref);
    }

    private GraphicsEngine.ArgbColor ToColor(int[] col)
    {
        if (col == null)
        {
            return GraphicsEngine.ArgbColor.Transparent;
        }

        if (col.Length == 3)
        {
            return GraphicsEngine.ArgbColor.FromArgb(col[0], col[1], col[2]);
        }

        if (col.Length == 4)
        {
            return GraphicsEngine.ArgbColor.FromArgb(col[3], col[0], col[1], col[2]);
        }

        throw new MapServerException($"Invalid symbol color: [{String.Join(",", col)}]");
    }

    #endregion
}
