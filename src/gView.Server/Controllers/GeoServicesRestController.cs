﻿using gView.Framework.Cartography;
using gView.Framework.Common;
using gView.Framework.Common.Extensions;
using gView.Framework.Common.Json;
using gView.Framework.Common.Reflection;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Data;
using gView.Interoperability.GeoServices.Request;
using gView.Interoperability.GeoServices.Rest.DTOs;
using gView.Interoperability.GeoServices.Rest.DTOs.Features;
using gView.Interoperability.GeoServices.Rest.DTOs.FeatureServer;
using gView.Interoperability.GeoServices.Rest.DTOs.Renderers.SimpleRenderers;
using gView.Interoperability.GeoServices.Rest.DTOs.Request;
using gView.Interoperability.GeoServices.Rest.Reflection;
using gView.Interoperability.OGC;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Services.Hosting;
using gView.Server.Services.Logging;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static gView.Interoperability.GeoServices.Rest.DTOs.JsonServicesDTO;

namespace gView.Server.Controllers;

public class GeoServicesRestController : BaseController
{
    private readonly MapServiceManager _mapServerService;
    private readonly UrlHelperService _urlHelperService;
    private readonly LoginManager _loginManagerService;
    private readonly EncryptionCertificateService _encryptionCertificate;
    private readonly PerformanceLoggerService _performanceLogger;
    private readonly MapServerManagerOptions _mapServerManagerOptions;

    public GeoServicesRestController(
        MapServiceManager mapServerService,
        UrlHelperService urlHelperService,
        LoginManager loginManagerService,
        EncryptionCertificateService encryptionCertificate,
        PerformanceLoggerService performanceLogger,
        IOptions<MapServerManagerOptions> mapServerManagerOptions)
        : base(mapServerService, loginManagerService, encryptionCertificate)
    {
        _mapServerService = mapServerService;
        _urlHelperService = urlHelperService;
        _loginManagerService = loginManagerService;
        _encryptionCertificate = encryptionCertificate;
        _performanceLogger = performanceLogger;
        _mapServerManagerOptions = mapServerManagerOptions.Value;
    }

    public const double Version = 10.61;
    public const string FullVersion = "10.6.1";
    //private const string DefaultFolder = "default";

    public int JsonExportResponse { get; private set; }

    public IActionResult Index()
    {
        return RedirectToAction("Services");
    }

    public IActionResult RestInfo()
    {
        return Result(new RestInfoResponseDTO()
        {
            CurrentVersion = Version,
            FullVersion = FullVersion,
            AuthInfoInstance = new RestInfoResponseDTO.AuthInfo()
            {
                IsTokenBasedSecurity = true,
                ShortLivedTokenValidity = 60,
                TokenServicesUrl = $"{_urlHelperService.AppRootUrl(this.Request)}/geoservices/tokens"
            }
        });
    }

    public Task<IActionResult> Services()
    {
        return Folder(String.Empty);
    }

    async public Task<IActionResult> Folder(string id)
    {
        return await SecureMethodHandler(async (identity) =>
        {
            _mapServerService.ReloadServices(id, true);

            if (!String.IsNullOrWhiteSpace(id))
            {
                var folderService = _mapServerService.MapServices
                    .Where(f => f.Type == MapServiceType.Folder && String.IsNullOrWhiteSpace(f.Folder) && id.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                    .FirstOrDefault();

                if (folderService == null || !await folderService.HasAnyAccess(identity))
                {
                    throw new Exception("Unknown folder or forbidden");
                }
            }

            List<string> folders = new List<string>();
            foreach (var f in _mapServerService.MapServices.Where(s => s.Type == MapServiceType.Folder && s.Folder == id))
            {
                if (await f.HasAnyAccess(identity))
                {
                    folders.Add(f.Name);
                }
            }

            List<AgsService> services = new List<AgsService>();
            foreach (var s in _mapServerService.MapServices)
            {
                if (s.Type != MapServiceType.Folder &&
                   s.Folder == id &&
                   (await s.GetSettingsAsync()).IsRunningOrIdle() &&
                    await s.HasAnyAccess(identity))
                {
                    var agsServices = await AgsServices(identity, s);
                    if (agsServices?.Any() == true)
                    {
                        services.AddRange(agsServices);
                    }
                    else if (await s.HasPublishAccess(identity))
                    {
                        // if there is only a publish rule:
                        //    add with type unknown, 
                        //    so the services are listed inside then
                        //    "Publish Map" tool, for eg "carto-publish" client...
                        services.Add(new AgsService()
                        {
                            Name = (String.IsNullOrWhiteSpace(s.Folder) ? "" : s.Folder + "/") + s.Name,
                            Type = ""
                        });
                    }
                }
            }
            var jsonService = String.IsNullOrEmpty(id) ?
                new JsonServicesRootDTO() : new JsonServicesDTO();

            jsonService.CurrentVersion = Version;
            jsonService.Folders = folders.ToArray();
            jsonService.Services = services.ToArray();

            return Result(jsonService);
        });
    }

    async public Task<IActionResult> Service(string id, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var mapService = _mapServerService.Instance.GetMapService(id, folder);
            if (mapService == null)
            {
                throw new Exception($"Unknown service: {id}");
            }

            await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

            using (var map = (await _mapServerService.Instance.GetServiceMapAsync(id, folder)).ThrowIfNull(id))
            {
                IEnvelope fullExtent = map.FullExtent();
                var spatialReference = map.Display.SpatialReference;
                int epsgCode = spatialReference != null ? spatialReference.EpsgCode : 0;

                return Result(new JsonMapServiceDTO()
                {
                    CurrentVersion = 10.61,
                    MapName = String.IsNullOrWhiteSpace(map.Title) ?
                        (map.Name.Contains("/") ? map.Name.Substring(map.Name.LastIndexOf("/") + 1) : map.Name) :
                        map.Title,
                    CopyrightText = map.GetLayerCopyrightText(Map.MapCopyrightTextId),
                    ServiceDescription = map.GetLayerDescription(Map.MapDescriptionId),
                    Layers = map.MapElements
                        .Where(e =>
                        {
                            var tocElement = map.TOC.GetTOCElement(e as ILayer);

                            return tocElement == null ? false : tocElement.IsHidden() == false;
                        })
                        .Select(e =>
                        {
                            var tocElement = map.TOC.GetTOCElement(e as ILayer);

                            int parentLayerId =
                                (e is IFeatureLayer && ((IFeatureLayer)e).GroupLayer != null) ?
                                ((IFeatureLayer)e).GroupLayer.ID :
                                -1;

                            return new JsonMapServiceDTO.Layer()
                            {
                                Id = e.ID,
                                ParentLayerId = parentLayerId,
                                Name = tocElement != null ? tocElement.Name : e.Title,
                                DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                                MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0,
                                MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0,
                            };
                        })
                        .ToArray(),
                    SpatialReferenceInstance = epsgCode > 0 ? new JsonMapServiceDTO.SpatialReference(epsgCode) : null,
                    InitialExtend = map.Display.Envelope == null ? null : new JsonMapServiceDTO.Extent()
                    {
                        XMin = fullExtent != null ? fullExtent.MinX : 0D,
                        YMin = fullExtent != null ? fullExtent.MinY : 0D,
                        XMax = fullExtent != null ? fullExtent.MaxX : 0D,
                        YMax = fullExtent != null ? fullExtent.MaxY : 0D,
                        SpatialReference = new JsonMapServiceDTO.SpatialReference(epsgCode)
                    },
                    FullExtent = new JsonMapServiceDTO.Extent()
                    {
                        XMin = fullExtent != null ? fullExtent.MinX : 0D,
                        YMin = fullExtent != null ? fullExtent.MinY : 0D,
                        XMax = fullExtent != null ? fullExtent.MaxX : 0D,
                        YMax = fullExtent != null ? fullExtent.MaxY : 0D,
                        SpatialReference = new JsonMapServiceDTO.SpatialReference(epsgCode)
                    },
                    MaxImageWidth = map.MapServiceProperties.MaxImageWidth,
                    MaxImageHeight = map.MapServiceProperties.MaxImageHeight,
                    MaxRecordCount = map.MapServiceProperties.MaxRecordCount
                });
            }
        });
    }

    async public Task<IActionResult> ServiceLayers(string id, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var mapService = _mapServerService.Instance.GetMapService(id, folder);
            if (mapService == null)
            {
                throw new Exception("Unknown service: " + id);
            }

            await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

            using (var map = (await _mapServerService.Instance.GetServiceMapAsync(id, folder)).ThrowIfNull(id))
            {
                var jsonLayers = new JsonLayersDTO();
                jsonLayers.Layers = map.MapElements
                    .Where(e =>
                    {   // Just show layer in Toc (and not hidden)
                        var tocElement = map.TOC.GetTOCElement(e as ILayer);

                        return tocElement == null ? false : tocElement.IsHidden() == false;
                    })
                    .Select(e => JsonLayer(map, e.ID))
                    .ToArray();

                return Result(jsonLayers);
            }
        });
    }

    async public Task<IActionResult> ServiceLayer(string id, int layerId, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var mapService = _mapServerService.Instance.GetMapService(id, folder);
            if (mapService == null)
            {
                throw new Exception("Unknown service: " + id);
            }

            await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

            var map = (await _mapServerService.Instance.GetServiceMapAsync(id, folder)).ThrowIfNull(id);

            var jsonLayers = new JsonLayersDTO();
            return Result(JsonLayer(map, layerId));
        });
    }

    #region MapServer

    public Task<IActionResult> ExportMap(string id, string folder = "")
        => SecureMethodHandler(async (identity) =>
    {
        var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

        #region Request

        JsonExportMapDTO exportMap = Deserialize<JsonExportMapDTO>(
            Request.HasFormContentType ?
            Request.Form :
            Request.Query);

        ServiceRequest serviceRequest = new ServiceRequest(id, folder, JSerializer.Serialize(exportMap))
        {
            OnlineResource = _mapServerService.Options.OnlineResource,
            OutputUrl = _mapServerService.Options.OutputUrl,
            Method = "export",
            Identity = identity
        };

        #endregion

        #region Queue & Wait

        IServiceRequestContext context = await ServiceRequestContext.TryCreate(
            _mapServerService.Instance,
            interpreter,
            serviceRequest);

        string format = ResultFormat();
        if (String.IsNullOrWhiteSpace(format))
        {
            using (var serviceMap = await context.CreateServiceMapInstance())
            {
                exportMap.InitForm(serviceMap);
                return FormResult(exportMap);
            }
        }

        await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

        #endregion

        if (serviceRequest.Succeeded)
        {
            if (ResultFormat() == "image" && serviceRequest.Response is byte[])
            {
                return Result((byte[])serviceRequest.Response, serviceRequest.ResponseContentType);
            }
            else
            {
                return Result(serviceRequest.Response, folder, id, "ExportMap");
                //return Result(JsonConvert.DeserializeObject<JsonExportResponse>(serviceRequest.ResponseAsString), folder, id, "ExportMap");
            }
        }
        else
        {
            return Result(serviceRequest.Response);
        }
    });

    async public Task<IActionResult> Query(string id, int layerId, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

            #region Request

            JsonQueryLayerDTO queryLayer = Deserialize<JsonQueryLayerDTO>(
                Request.HasFormContentType ?
                Request.Form :
                Request.Query);
            queryLayer.LayerId = layerId;

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, JSerializer.Serialize(queryLayer))
            {
                OnlineResource = _mapServerService.Options.OnlineResource,
                OutputUrl = _mapServerService.Options.OutputUrl,
                Method = "query",
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServerService.Instance,
                interpreter,
                serviceRequest);

            string format = ResultFormat();
            if (String.IsNullOrWhiteSpace(format))
            {
                return FormResult(queryLayer);
            }

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            if (serviceRequest.Succeeded)
            {
                return Result(serviceRequest.Response, folder, id, "Query", contentType: serviceRequest.ResponseContentType);
            }
            else
            {
                return Result(serviceRequest.Response);
            }
        });
    }

    async public Task<IActionResult> Legend(string id, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

            #region Request

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, String.Empty)
            {
                OnlineResource = _mapServerService.Options.OnlineResource,
                OutputUrl = _mapServerService.Options.OutputUrl,
                Method = "legend",
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServerService.Instance,
                interpreter,
                serviceRequest);

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            return Result(serviceRequest.Response, folder, id, "Legend");
        });
    }

    async public Task<IActionResult> Identify(string id, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

            #region Request

            JsonIdentifyDTO identify = Deserialize<JsonIdentifyDTO>(
                Request.HasFormContentType ?
                Request.Form :
                Request.Query);

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, JSerializer.Serialize(identify))
            {
                OnlineResource = _mapServerService.Options.OnlineResource,
                OutputUrl = _mapServerService.Options.OutputUrl,
                Method = "identify",
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServerService.Instance,
                interpreter,
                serviceRequest);

            string format = ResultFormat();
            if (String.IsNullOrWhiteSpace(format))
            {
                using (var serviceMap = await context.CreateServiceMapInstance())
                {
                    identify.InitForm(serviceMap);
                    return FormResult(identify);
                }
            }

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            if (serviceRequest.Succeeded)
            {
                return Result(serviceRequest.Response, folder, id, "Identify");
            }
            else
            {
                return Result(serviceRequest.Response);
            }

        });
    }

    #endregion

    #region WmsServer 

    public Task<IActionResult> WmsServer(string id, string folder = "")
        => SecureMethodHandler(async (identity) =>
    {
        try
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(WMSRequest));

            #region Request

            string requestString = Request.QueryString.ToString();
            if (Request.Method.ToLower() == "post" && Request.Body.CanRead)
            {
                string body = await Request.GetBody();

                if (!String.IsNullOrWhiteSpace(body))
                {
                    requestString = body;
                }
            }

            while (requestString.StartsWith("?"))
            {
                requestString = requestString.Substring(1);
            }

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, requestString)
            {
                OnlineResource = _mapServerService.Options.OnlineResource.AppendWmsServerPath(this.Request, id, folder),
                OutputUrl = _mapServerService.Options.OutputUrl,
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                   _mapServerService.Instance,
                   interpreter,
                   serviceRequest);


            context.SetContextMetadata(WMSRequest.LayerNameFormatMetadataKey, "{0}");

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            var response = serviceRequest.Response;

            if (response is byte[])
            {
                return WmsResult((byte[])response, serviceRequest.ResponseContentType);
            }

            return WmsResult(response?.ToString() ?? String.Empty, serviceRequest.ResponseContentType);
        }
        catch (Exception ex)
        {
            // ToDo: OgcXmlExcpetion
            return WmsResult(ex.Message, "text/plain");
        }
    });

    #endregion

    #region FeatureServer

    async public Task<IActionResult> FeatureServerService(string id, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var mapService = _mapServerService.Instance.GetMapService(id, folder);
            if (mapService == null)
            {
                throw new Exception("Unknown service: " + id);
            }

            await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

            var map = (await _mapServerService.Instance.GetServiceMapAsync(id, folder)).ThrowIfNull(id);

            gView.Framework.Geometry.Envelope fullExtent = null;
            var spatialReference = map.Display.SpatialReference;
            int epsg = spatialReference != null ? spatialReference.EpsgCode : 0;

            return Result(new JsonFeatureServiceDTO()
            {
                CurrentVersion = 10.61,
                Layers = map.MapElements
                .Where(e =>
                {
                    var tocElement = map.TOC.GetTOCElement(e as ILayer);

                    return tocElement == null ? false : tocElement.IsHidden() == false;
                })
                .Select(e =>
                {
                    var tocElement = map.TOC.GetTOCElement(e as ILayer);

                    int parentLayerId =
                                (e is IFeatureLayer && ((IFeatureLayer)e).GroupLayer != null) ?
                                ((IFeatureLayer)e).GroupLayer.ID :
                                -1;

                    IFeatureClass fc = (IFeatureClass)e.Class;

                    if (fc?.Envelope != null)
                    {
                        if (fullExtent == null)
                        {
                            fullExtent = new Framework.Geometry.Envelope(fc.Envelope);
                        }
                        else
                        {
                            fullExtent.Union(fc.Envelope);
                        }
                    }

                    if (epsg == 0 && fc?.SpatialReference != null && fc.SpatialReference.Name.ToLower().StartsWith("epsg:"))
                    {
                        int.TryParse(fc.SpatialReference.Name.Substring(5), out epsg);
                    }

                    var geometryType = e.Class is IFeatureClass ?
                        ((IFeatureClass)e.Class).GeometryType :
                        GeometryType.Unknown;

                    if (geometryType == GeometryType.Unknown && e is IFeatureLayer)   // if layer is SQL Spatial with undefined geometrytype...
                    {
                        geometryType = ((IFeatureLayer)e).LayerGeometryType;                             // take the settings from layer-properties
                    }

                    if (fc != null || tocElement.Layers?.FirstOrDefault() is IGroupLayer)
                    {
                        return new JsonMapServiceDTO.Layer()
                        {
                            Id = e.ID,
                            ParentLayerId = parentLayerId,
                            Name = tocElement != null ? tocElement.Name : e.Title,
                            DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true,
                            MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0,
                            MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0,
                            GeometryType = e.Class is IFeatureClass ?
                                Interoperability.GeoServices.Rest.DTOs.JsonLayerDTO.ToGeometryType(geometryType).ToString() :
                                null,
                            LayerType = fc != null ? "Feature Layer" : "Group Layer"
                        };
                    }

                    return null;
                })
                .Where(e => e != null)
                .ToArray(),
                SpatialReferenceInstance = epsg > 0 ? new JsonMapServiceDTO.SpatialReference(epsg) : null,
                InitialExtend = new JsonMapServiceDTO.Extent()
                {
                    XMin = fullExtent != null ? fullExtent.MinX : 0D,
                    YMin = fullExtent != null ? fullExtent.MinY : 0D,
                    XMax = fullExtent != null ? fullExtent.MaxX : 0D,
                    YMax = fullExtent != null ? fullExtent.MaxY : 0D,
                    SpatialReference = new JsonMapServiceDTO.SpatialReference(epsg)
                },
                FullExtent = new JsonMapServiceDTO.Extent()
                {
                    XMin = fullExtent != null ? fullExtent.MinX : 0D,
                    YMin = fullExtent != null ? fullExtent.MinY : 0D,
                    XMax = fullExtent != null ? fullExtent.MaxX : 0D,
                    YMax = fullExtent != null ? fullExtent.MaxY : 0D,
                    SpatialReference = new JsonMapServiceDTO.SpatialReference(epsg)
                }
            });
        });
    }

    async public Task<IActionResult> FeatureServerQuery(string id, int layerId, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

            #region Request

            JsonQueryLayerDTO queryLayer = Deserialize<JsonQueryLayerDTO>(
                Request.HasFormContentType ?
                Request.Form :
                Request.Query);
            queryLayer.LayerId = layerId;

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, JSerializer.Serialize(queryLayer))
            {
                OnlineResource = _mapServerService.Options.OnlineResource,
                OutputUrl = _mapServerService.Options.OutputUrl,
                Method = "featureserver_query",
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServerService.Instance,
                interpreter,
                serviceRequest);

            string format = ResultFormat();
            if (String.IsNullOrWhiteSpace(format))
            {
                return FormResult(queryLayer);
            }

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            if (serviceRequest.Succeeded)
            {
                return Result(serviceRequest.Response, folder, id, "Query", contentType: serviceRequest.ResponseContentType);
            }
            else
            {
                return Result(serviceRequest.Response);
            }
        });
    }

    async public Task<IActionResult> FeatureServerAddFeatures(string id, int layerId, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

            #region Request

            JsonFeatureServerUpdateRequestDTO editRequest = Deserialize<JsonFeatureServerUpdateRequestDTO>(
                Request.HasFormContentType ?
                Request.Form :
                Request.Query);
            editRequest.LayerId = layerId;

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, JSerializer.Serialize(editRequest))
            {
                OnlineResource = _mapServerService.Options.OnlineResource,
                OutputUrl = _mapServerService.Options.OutputUrl,
                Method = "featureserver_addfeatures",
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServerService.Instance,
                interpreter,
                serviceRequest);

            string format = ResultFormat();
            if (String.IsNullOrWhiteSpace(format))
            {
                return FormResult(editRequest);
            }

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            return Result(JSerializer.Deserialize<JsonFeatureServerResponseDTO>(serviceRequest.ResponseAsString));
        },
        onException: (ex) =>
        {
            return Result(new JsonFeatureServerResponseDTO()
            {
                AddResults = new JsonFeatureServerResponseDTO.JsonResponse[]
                {
                    new JsonFeatureServerResponseDTO.JsonResponse()
                    {
                        Success=false,
                        Error=new JsonFeatureServerResponseDTO.JsonError()
                        {
                            Code=999,
                            Description=ex.Message
                        }
                    }
                }
            });
        });
    }

    async public Task<IActionResult> FeatureServerUpdateFeatures(string id, int layerId, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

            #region Request

            JsonFeatureServerUpdateRequestDTO editRequest = Deserialize<JsonFeatureServerUpdateRequestDTO>(
                Request.HasFormContentType ?
                Request.Form :
                Request.Query);
            editRequest.LayerId = layerId;

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, JSerializer.Serialize(editRequest))
            {
                OnlineResource = _mapServerService.Options.OnlineResource,
                OutputUrl = _mapServerService.Options.OutputUrl,
                Method = "featureserver_updatefeatures",
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServerService.Instance,
                interpreter,
                serviceRequest);

            string format = ResultFormat();
            if (String.IsNullOrWhiteSpace(format))
            {
                return FormResult(editRequest);
            }

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            return Result(JSerializer.Deserialize<JsonFeatureServerResponseDTO>(serviceRequest.ResponseAsString));
        },
        onException: (ex) =>
        {
            return Result(new JsonFeatureServerResponseDTO()
            {
                UpdateResults = new JsonFeatureServerResponseDTO.JsonResponse[]
                {
                    new JsonFeatureServerResponseDTO.JsonResponse()
                    {
                        Success=false,
                        Error=new JsonFeatureServerResponseDTO.JsonError()
                        {
                            Code=999,
                            Description=ex.Message
                        }
                    }
                }
            });
        });
    }

    async public Task<IActionResult> FeatureServerDeleteFeatures(string id, int layerId, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var interpreter = _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter));

            #region Request

            JsonFeatureServerDeleteRequestDTO editRequest = Deserialize<JsonFeatureServerDeleteRequestDTO>(
                Request.HasFormContentType ?
                Request.Form :
                Request.Query);
            editRequest.LayerId = layerId;

            ServiceRequest serviceRequest = new ServiceRequest(id, folder, JSerializer.Serialize(editRequest))
            {
                OnlineResource = _mapServerService.Options.OnlineResource,
                OutputUrl = _mapServerService.Options.OutputUrl,
                Method = "featureserver_deletefeatures",
                Identity = identity
            };

            #endregion

            #region Queue & Wait

            IServiceRequestContext context = await ServiceRequestContext.TryCreate(
                _mapServerService.Instance,
                interpreter,
                serviceRequest);

            string format = ResultFormat();
            if (String.IsNullOrWhiteSpace(format))
            {
                return FormResult(editRequest);
            }

            await _mapServerService.TaskQueue.AwaitRequest(interpreter.Request, context);

            #endregion

            return Result(JSerializer.Deserialize<JsonFeatureServerResponseDTO>(serviceRequest.ResponseAsString));
        },
        onException: (ex) =>
        {
            return Result(new JsonFeatureServerResponseDTO()
            {
                DeleteResults = new JsonFeatureServerResponseDTO.JsonResponse[]
                                {
                    new JsonFeatureServerResponseDTO.JsonResponse()
                    {
                        Success=false,
                        Error=new JsonFeatureServerResponseDTO.JsonError()
                        {
                            Code=999,
                            Description=ex.Message
                        }
                    }
                }
            });
        });
    }

    async public Task<IActionResult> FeatureServerLayer(string id, int layerId, string folder = "")
    {
        return await SecureMethodHandler(async (identity) =>
        {
            var mapService = _mapServerService.Instance.GetMapService(id, folder);
            if (mapService == null)
            {
                throw new Exception("Unknown service: " + id);
            }

            await mapService.CheckAccess(identity, _mapServerService.GetInterpreter(typeof(GeoServicesRestInterperter)));

            var map = (await _mapServerService.Instance.GetServiceMapAsync(id, folder)).ThrowIfNull(id);

            var jsonLayers = new JsonLayersDTO();
            return Result(JsonFeatureServerLayer(map, layerId));
        });
    }

    #endregion

    #region Security

    public Task<IActionResult> GenerateToken(string request, string username, string password, int expiration = 60)
    {
        try
        {
            // https://developers.arcgis.com/rest/users-groups-and-items/generate-token.htm

            string format = ResultFormat();
            if (String.IsNullOrWhiteSpace(format))
            {
                return Task.FromResult(FormResult(new JsonGenerateTokenDTO()));
            }

            if (request?.ToLower() == "gettoken")
            {
                if (String.IsNullOrWhiteSpace(username))
                {
                    throw new Exception("username required");
                }

                if (String.IsNullOrWhiteSpace(password))
                {
                    throw new Exception("password required");
                }

                var authToken = _loginManagerService.GetAuthToken(username, password, expireMinutes: expiration);
                if (authToken == null)
                {
                    throw new Exception("unknown username or password");
                }

                return Task.FromResult(Result(new JsonSecurityTokenDTO()
                {
                    Token = _encryptionCertificate.ToToken(authToken),
                    Expires = Convert.ToInt64((new DateTime(authToken.Expire, DateTimeKind.Utc) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds)
                }));
            }
            else
            {
                throw new Exception("unkown request: " + request);
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result(new JsonErrorDTO()
            {
                Error = new JsonErrorDTO.ErrorDef() { Code = 400, Message = ex.Message }
            }));
        }
    }

    #endregion

    #region Helper

    #region Json

    private JsonLayerDTO JsonLayer(IServiceMap map, int layerId, JsonLayerDTO result = null)
    {
        var datasetElement = map.MapElements.Where(e => e.ID == layerId).FirstOrDefault();
        if (datasetElement == null)
        {
            throw new Exception("Unknown layer: " + layerId);
        }

        var tocElement = map.TOC.GetTOCElement(datasetElement as ILayer);
        bool isJsonFeatureServiceLayer = result is JsonFeatureServerLayerDTO;

        JsonLayerLinkDTO parentLayer = null;
        if (datasetElement is ILayer && ((ILayer)datasetElement).GroupLayer != null)
        {
            parentLayer = new JsonLayerLinkDTO()
            {
                Id = ((ILayer)datasetElement).GroupLayer.ID,
                Name = ((ILayer)datasetElement).GroupLayer.Title
            };
        }

        if (datasetElement is GroupLayer && datasetElement.Class == null)  // GroupLayer
        {
            var groupLayer = (GroupLayer)datasetElement;
            string type = "Group Layer";
            var childLayers = groupLayer.ChildLayers != null ?
                    groupLayer.ChildLayers.Where(l => map.TOC.GetTOCElement(l) != null).Select(l =>
                    {
                        var childTocElement = map.TOC.GetTOCElement(l);

                        return new JsonLayerLinkDTO()
                        {
                            Id = l.ID,
                            Name = childTocElement.Name
                        };
                    }).ToArray() :
                    new JsonLayerLinkDTO[0];

            if (groupLayer.MapServerStyle == MapServerGrouplayerStyle.Checkbox)
            {
                type = "Feature Layer";
                childLayers = null;
            }


            var jsonGroupLayer = new JsonLayerDTO()
            {
                CurrentVersion = Version,
                Id = groupLayer.ID,
                Name = groupLayer.Title,
                DefaultVisibility = groupLayer.Visible,
                MaxScale = Math.Max(groupLayer.MinimumScale > 1 ? groupLayer.MinimumScale : 0, 0),
                MinScale = Math.Max(groupLayer.MaximumScale > 1 ? groupLayer.MaximumScale : 0, 0),
                Type = type,
                ParentLayer = parentLayer,
                SubLayers = childLayers
            };

            if (jsonGroupLayer.SubLayers != null)
            {
                JsonSpatialReferenceDTO spatialReference = new JsonSpatialReferenceDTO(map.Display.SpatialReference != null ? map.Display.SpatialReference.EpsgCode : 0);
                JsonExtentDTO extent = null;
                foreach (var subLayer in jsonGroupLayer.SubLayers)
                {
                    var featureClass = map.MapElements.Where(e => e.ID == subLayer.Id && e.Class is IFeatureClass).Select(l => (IFeatureClass)l.Class).FirstOrDefault();

                    if (featureClass != null)
                    {
                        int epsgCode = featureClass.SpatialReference != null ? featureClass.SpatialReference.EpsgCode : 0;
                        if (epsgCode == spatialReference.Wkid || epsgCode == 0)
                        {
                            var envelope = featureClass.Envelope;
                            if (envelope != null)
                            {
                                if (extent == null)
                                {
                                    extent = new JsonExtentDTO()
                                    {
                                        Xmin = featureClass.Envelope.MinX,
                                        Ymin = featureClass.Envelope.MinY,
                                        Xmax = featureClass.Envelope.MaxX,
                                        Ymax = featureClass.Envelope.MaxY,
                                        SpatialReference = spatialReference
                                    };
                                }
                                else
                                {
                                    extent.Xmin = Math.Min(extent.Xmin, featureClass.Envelope.MinX);
                                    extent.Ymin = Math.Min(extent.Ymin, featureClass.Envelope.MinY);
                                    extent.Xmax = Math.Min(extent.Xmax, featureClass.Envelope.MaxX);
                                    extent.Ymax = Math.Min(extent.Ymax, featureClass.Envelope.MaxY);
                                }
                            }
                        }
                    }
                }
                jsonGroupLayer.Extent = extent;

                jsonGroupLayer.Description = map.GetLayerDescription(layerId);
                jsonGroupLayer.CopyrightText = map.GetLayerCopyrightText(layerId);
            }
            return jsonGroupLayer;
        }
        else // Featurelayer, Rasterlayer
        {
            JsonFieldDTO[] fields = new JsonFieldDTO[0];
            if (datasetElement.Class is ITableClass)
            {
                fields = ((ITableClass)datasetElement.Class).Fields.ToEnumerable()
                    .Where(f => isJsonFeatureServiceLayer && f.name.EndsWith("()") ? false : true)  // FeatureServer => don't show functions line STArea, STLength, ... only supported with MapServer
                    .Select(f =>
                    {
                        if (isJsonFeatureServiceLayer)
                        {
                            return new JsonFeatureLayerFieldDTO()
                            {
                                Name = f.name,
                                Alias = f.aliasname,
                                Type = JsonFieldDTO.ToType(f.type).ToString(),
                                Editable = f.type != FieldType.ID,
                                Nullable = f.type != FieldType.ID,
                                Length = f.size
                            };
                        }
                        else
                        {
                            return new JsonFieldDTO()
                            {
                                Name = f.name,
                                Alias = f.aliasname,
                                Type = JsonFieldDTO.ToType(f.type).ToString()
                            };
                        }
                    })
                    .ToArray();
            }

            JsonExtentDTO extent = null;
            var spatialReference = (datasetElement.Class is IFeatureClass ? ((IFeatureClass)datasetElement.Class).SpatialReference : null) ?? (map.LayerDefaultSpatialReference ?? map.Display.SpatialReference);
            int epsgCode = spatialReference != null ? spatialReference.EpsgCode : 0;

            if (datasetElement.Class is IFeatureClass && ((IFeatureClass)datasetElement.Class).Envelope != null)
            {
                extent = new JsonExtentDTO()
                {
                    // DoTo: SpatialReference
                    Xmin = ((IFeatureClass)datasetElement.Class).Envelope.MinX,
                    Ymin = ((IFeatureClass)datasetElement.Class).Envelope.MinY,
                    Xmax = ((IFeatureClass)datasetElement.Class).Envelope.MaxX,
                    Ymax = ((IFeatureClass)datasetElement.Class).Envelope.MaxY,
                    SpatialReference = new JsonSpatialReferenceDTO(epsgCode)
                };
            }

            string type = "Feature Layer";
            if (datasetElement.Class is IRasterClass &&
                !(datasetElement.Class is IRasterCatalogClass)) // RasterCatalogClass is like a Featureclass (Features a rendert as Image, but you can query/filter them as Polygons with attributes...)
            {
                type = "Raster Layer";
            }

            JsonDrawingInfoDTO drawingInfo = null;
            if (datasetElement is IFeatureLayer)
            {
                var featureLayer = (IFeatureLayer)datasetElement;

                drawingInfo = new JsonDrawingInfoDTO()
                {
                    Renderer = JsonRendererDTO.FromFeatureRenderer(featureLayer.FeatureRenderer)
                };
            }

            result = result ?? new JsonLayerDTO();

            var geometryType = datasetElement.Class is IFeatureClass ?
                ((IFeatureClass)datasetElement.Class).GeometryType :
                GeometryType.Unknown;

            if (geometryType == GeometryType.Unknown && datasetElement is IFeatureLayer)   // if layer is SQL Spatial with undefined geometrytype...
            {
                geometryType = ((IFeatureLayer)datasetElement).LayerGeometryType;                             // take the settings from layer-properties
            }

            result.CurrentVersion = Version;
            result.Id = datasetElement.ID;
            result.Name = tocElement != null ? tocElement.Name : datasetElement.Title;
            result.DefaultVisibility = tocElement != null ? tocElement.LayerVisible : true;
            result.MaxScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MinimumScale > 1 ? tocElement.Layers[0].MinimumScale : 0, 0) : 0;
            result.MinScale = tocElement != null && tocElement.Layers.Count() > 0 ? Math.Max(tocElement.Layers[0].MaximumScale > 1 ? tocElement.Layers[0].MaximumScale : 0, 0) : 0;
            result.Fields = fields;
            result.Extent = extent;
            result.Type = type;
            result.ParentLayer = parentLayer;
            result.DrawingInfo = drawingInfo;
            result.GeometryType = datasetElement.Class is IFeatureClass ?
                Interoperability.GeoServices.Rest.DTOs.JsonLayerDTO.ToGeometryType(geometryType).ToString() :
                EsriGeometryType.esriGeometryNull.ToString();

            result.Description = map.GetLayerDescription(layerId);
            result.CopyrightText = map.GetLayerCopyrightText(layerId);

            if (result is JsonFeatureServerLayerDTO)
            {
                var editorModule = map.GetModule<gView.Plugins.Modules.EditorModule>();
                if (editorModule != null)
                {
                    var editLayer = editorModule.GetEditLayer(result.Id);
                    if (editLayer != null)
                    {
                        List<string> editOperations = new List<string>();
                        foreach (Framework.Editor.Core.EditStatements statement in Enum.GetValues(typeof(Framework.Editor.Core.EditStatements)))
                        {
                            if (statement != Framework.Editor.Core.EditStatements.NONE && editLayer.Statements.HasFlag(statement))
                            {
                                editOperations.Add(statement.ToString());
                            }
                        }

                        if (editOperations.Count > 0)
                        {
                            ((JsonFeatureServerLayerDTO)result).IsEditable = true;
                            ((JsonFeatureServerLayerDTO)result).EditOperations = editOperations.ToArray();
                        }
                    }
                }
            }

            return result;
        }
    }

    private JsonLayerDTO JsonFeatureServerLayer(IServiceMap map, int layerId)
    {
        return JsonLayer(map, layerId, new JsonFeatureServerLayerDTO());
    }

    #endregion

    private IActionResult Result(object obj, string folder = null, string id = null, string method = null, string contentType = "text/plain")
    {
        if (base.ActionStartTime.HasValue && obj is JsonStopWatchDTO)
        {
            ((JsonStopWatchDTO)obj).DurationMilliseconds = (DateTime.UtcNow - base.ActionStartTime.Value).TotalMilliseconds;

            ((JsonStopWatchDTO)obj).AddPerformanceLoggerItem(_performanceLogger,
                                                          folder, id, method);
        }

        string format = ResultFormat();

        if (format == "json")
        {
            return Json(obj);
        }
        else if (format == "pjson")
        {
            return Json(obj, new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            }.AddServerDefaults());
        }
        else if (IsRawResultFormat(format))
        {
            if (obj is byte[])
            {
                return File((byte[])obj, contentType);
            }
            else if (obj is string)
            {
                return File(Encoding.UTF8.GetBytes(obj.ToString()), contentType);
            }
            else
            {
                return Json(obj);
            }
        }

        #region ToHtml

        AddNavigation();
        ViewData["htmlbody"] = obj.GeoServicesObjectToHtml(Request, _urlHelperService, _mapServerService);
        return View("_htmlbody");

        #endregion
    }

    private IActionResult Result(byte[] data, string contentType)
    {
        return File(data, contentType);
    }

    public IActionResult FormResult(object obj)
    {
        AddNavigation();
        ViewData["htmlBody"] = obj.GeoServicesObjectToHtmlForm();
        return View("_htmlbody");
    }

    private IActionResult WmsResult(string response, string contentType)
    {
        byte[] data = null;
        if (response.StartsWith("base64:"))
        {
            response = response.Substring("base64:".Length);
            data = Convert.FromBase64String(response);
        }
        else
        {
            data = Encoding.UTF8.GetBytes(response);
        }
        return WmsResult(data, contentType);
    }

    private IActionResult WmsResult(byte[] data, string contentType)
    {
        if (String.IsNullOrEmpty(contentType))
        {
            contentType = "text/plain";
        }

        return File(data, contentType);
    }

    private void AddNavigation()
    {
        var requestPath = this.Request.Path.ToString();
        while (requestPath.StartsWith("/"))
        {
            requestPath = requestPath.Substring(1);
        }

        string path = _urlHelperService.AppRootUrl(this.Request);
        string[] requestPathItems = requestPath.Split('/');
        string[] serverTypes = new string[] { "mapserver", "featureserver" };

        Dictionary<string, string> menuItems = new Dictionary<string, string>();
        bool usePath = false;
        for (int i = 0, to = requestPathItems.Length; i < to; i++)
        {
            var item = requestPathItems[i];
            path += "/" + item;

            if (i < to - 1 && serverTypes.Contains(requestPathItems[i + 1].ToLower()))
            {
                continue;
            }
            else if (i > 0 && serverTypes.Contains(item.ToLower()))
            {
                item = requestPathItems[i - 1] + " (" + item + ")";
            }

            if (usePath)
            {
                menuItems.Add(item, path);
            }

            if (item.ToLower() == "rest")
            {
                usePath = true;
            }
        }

        ViewData["mainMenuItems"] = "_mainMenuGeoServicesPartial";
        ViewData["menuItems"] = menuItems;
    }

    private string ResultFormat()
    {
        if (!String.IsNullOrWhiteSpace(Request.Query["f"]))
        {
            return Request.Query["f"].ToString().ToLower();
        }
        if (Request.HasFormContentType && !String.IsNullOrWhiteSpace(Request.Form["f"].ToString().ToLower()))
        {
            return Request.Form["f"];
        }

        return String.Empty;
    }

    private bool IsRawResultFormat(string resultFormat = null)
    {
        if (String.IsNullOrEmpty(resultFormat))
        {
            resultFormat = ResultFormat();
        }

        switch (resultFormat?.ToLower())
        {
            case "geojson":
                return true;
        }

        return false;
    }

    async private Task<IEnumerable<string>> ServiceTypes(IIdentity identity, IMapService mapService)
    {
        var accessTypes = await mapService.GetAccessTypes(identity);

        List<string> serviceTypes = new List<string>(2);
        if (accessTypes.HasFlag(AccessTypes.Map))
        {
            serviceTypes.Add("MapServer");
        }

        if (accessTypes.HasFlag(AccessTypes.Edit) /* || accessTypes.HasFlag(AccessTypes.Query)*/)
        {
            serviceTypes.Add("FeatureServer");
        }

        return serviceTypes;
    }

    async private Task<IEnumerable<AgsService>> AgsServices(Identity identity, IMapService mapService)
    {
        return (await ServiceTypes(identity, mapService))
                    .Select(s => new AgsService()
                    {
                        Name = (String.IsNullOrWhiteSpace(mapService.Folder) ? "" : mapService.Folder + "/") + mapService.Name,
                        Type = s
                    });
    }

    private T Deserialize<T>(IEnumerable<KeyValuePair<string, StringValues>> nv)
    {
        var instance = (T)Activator.CreateInstance(typeof(T));

        foreach (var propertyInfo in typeof(T).GetProperties())
        {
            if (propertyInfo.SetMethod == null)
            {
                continue;
            }

            var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonPropertyAttribute == null)
            {
                continue;
            }

            string key = jsonPropertyAttribute.Name ?? propertyInfo.Name;
            var keyValuePair = nv.Where(k => key.Equals(k.Key, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (keyValuePair.Key == null)
            {
                key = "&" + key;
                keyValuePair = nv.Where(k => k.Key == key).FirstOrDefault();   // Sometimes the keyvalue-key starts with an & ??
            }

            if (key.Equals(keyValuePair.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                var val = keyValuePair.Value.ToString();

                if (propertyInfo.PropertyType == typeof(double))
                {
                    if (!String.IsNullOrWhiteSpace(val))
                    {
                        propertyInfo.SetValue(instance, NumberExtensions.ToDouble(keyValuePair.Value.ToString()));
                    }
                }
                else if (propertyInfo.PropertyType == typeof(float))
                {
                    if (!String.IsNullOrWhiteSpace(val))
                    {
                        propertyInfo.SetValue(instance, NumberExtensions.ToFloat(keyValuePair.Value.ToString()));
                    }
                }
                else if (propertyInfo.PropertyType == typeof(System.Int16))
                {
                    if (!String.IsNullOrWhiteSpace(val))
                    {
                        propertyInfo.SetValue(instance, Convert.ToInt16(val));
                    }
                }
                else if (propertyInfo.PropertyType == typeof(System.Int32))
                {
                    if (!String.IsNullOrWhiteSpace(val))
                    {
                        propertyInfo.SetValue(instance, Convert.ToInt32(val));
                    }
                }
                else if (propertyInfo.PropertyType == typeof(System.Int64))
                {
                    if (!String.IsNullOrWhiteSpace(val))
                    {
                        propertyInfo.SetValue(instance, Convert.ToInt64(val));
                    }
                }
                else if (propertyInfo.PropertyType == typeof(bool))
                {
                    if (!String.IsNullOrWhiteSpace(val))
                    {
                        propertyInfo.SetValue(instance, Convert.ToBoolean(val));
                    }
                }
                else if (propertyInfo.PropertyType == typeof(System.String))
                {
                    propertyInfo.SetValue(instance, val);
                }
                else
                {
                    if ((val.Trim().StartsWith("{") && val.Trim().EndsWith("}")) ||
                        (val.Trim().StartsWith("[") && val.Trim().EndsWith("]")))
                    {
                        propertyInfo.SetValue(instance, JSerializer.Deserialize(val, propertyInfo.PropertyType));
                    }
                    else
                    {
                        propertyInfo.SetValue(instance, Convert.ChangeType(keyValuePair.Value.ToString(), propertyInfo.PropertyType));
                    }
                }
            }
        }

        return instance;
    }

    async override protected Task<IActionResult> SecureMethodHandler(Func<Identity, Task<IActionResult>> func, Func<Exception, IActionResult> onException = null)
    {
        if (onException == null)
        {
            onException = (e) =>
            {
                try
                {
                    throw e;
                }
                catch (MapServerAuthException)
                {
                    throw; // Handled in AuthenticationExceptionMiddleware
                }
                catch (MapServerException mse)
                {
                    return Result(new JsonErrorDTO()
                    {
                        Error = new JsonErrorDTO.ErrorDef() { Code = 999, Message = mse.Message }
                    });
                }
                catch (Exception)
                {
                    return Result(new JsonErrorDTO()
                    {
                        Error = new JsonErrorDTO.ErrorDef() { Code = 999, Message = "unknown error" }
                    });

                }
            };
        }

        return await base.SecureMethodHandler(func, onException: onException);
    }

    #endregion
}