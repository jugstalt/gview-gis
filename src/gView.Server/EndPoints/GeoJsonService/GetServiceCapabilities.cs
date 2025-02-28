using gView.Framework.Cartography;
using gView.Framework.Common.Extensions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode.Extensions;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetServiceCapabilities : BaseApiEndpoint
{
    public GetServiceCapabilities()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.GetServiceCapabilities}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.GetServiceCapabilities}"
                ],
                Handler
            )
    {
    }

    private static Delegate Handler => (
            HttpContext httpContext,
            [FromServices] LoginManager loginManagerService,
            [FromServices] MapServiceManager mapServerService,
            [FromServices] ILogger<GetServiceCapabilities> logger,
            string folder = "",
            string service = "") => HandleSecureAsync<GetServiceCapabilitiesRequest>(
                    httpContext, mapServerService, loginManagerService, logger,
                    folder, service, 
                    async (serviceRequestContext, mapService, identity, capabilitiesRequest) =>
     {
         if (String.IsNullOrEmpty(service))
         {
             (folder, service) = ("", folder);
         }

         string path = String.IsNullOrEmpty(folder)
                        ? service
                        : $"{folder}/{service}";
         
         string onlineResource =
               serviceRequestContext.ServiceRequest.OnlineResource.OrTake(
                   mapServerService.Options.OnlineResource
                );

         string url = $"{onlineResource}/{Routes.Base}/{Routes.GetServices}/{path}";
         var map = await mapServerService.Instance.GetServiceMapAsync(service, folder);
         var accessTypes = await mapService.GetAccessTypes(identity);

         var originalMapSRef = map.Display.SpatialReference;
         var fullExtent = map.FullExtent();
         var intialExtent = fullExtent; // map.Display.Envelope;
         map.Display.SpatialReference = capabilitiesRequest.CRS.ToSpatialReferenceOrDefault();

         using (var geoTransfromer = GeometricTransformerFactory.Create())
         {
             geoTransfromer.SetSpatialReferences(originalMapSRef, map.Display.SpatialReference);
             fullExtent = (geoTransfromer.Transform2D(fullExtent) as IGeometry)?.Envelope;
             intialExtent = (geoTransfromer.Transform2D(intialExtent) as IGeometry)?.Envelope;
         }

         return new GetServiceCapabilitiesResponse()
         {
             MapTitle = String.IsNullOrWhiteSpace(map.Title)
                    ? (map.Name.Contains("/") ? map.Name.Substring(map.Name.LastIndexOf("/") + 1) : map.Name)
                    : map.Title,
             Copyright = map.GetLayerCopyrightText(Map.MapCopyrightTextId),
             Description = map.GetLayerDescription(Map.MapDescriptionId),

             SupportedRequests = SupportedServiceRequests(map, url, accessTypes),

             CRS = accessTypes.HasFlag(AccessTypes.Map)
                ? map.Display.SpatialReference is not null
                    ? CoordinateReferenceSystem.CreateByName(map.Display.SpatialReference.Name)
                    : null
                : null,
             FullExtent = accessTypes.HasFlag(AccessTypes.Map)
                            ? fullExtent.ToBBox()
                            : null,
             InitialExtent = accessTypes.HasFlag(AccessTypes.Map)
                            ? intialExtent.ToBBox()
                            : null,
             Units = accessTypes.HasFlag(AccessTypes.Map)
                            ? map.Display.MapUnits.ToString()
                            : null,

             Layers = accessTypes != AccessTypes.None
                ? map.MapElements
                    .Where(e =>
                    {
                        var tocElement = map.TOC.GetTOCElement(e as ILayer);
                        return tocElement == null ? false : tocElement.IsHidden() == false;
                    })
                    .Select(l =>
            {
                var layer = (ILayer)l;
                var tocElement = map.TOC.GetTOCElement(layer);

                int? parentLayerId =
                       layer.GroupLayer != null
                       ? layer.GroupLayer.ID
                       : null;

                var layerInfo = new LayerInfo()
                {
                    Id = layer.ID.ToString(),
                    ParentId = parentLayerId?.ToString(),
                    Name = tocElement.Name,
                    DefaultVisibility = accessTypes.HasFlag(AccessTypes.Map) ? tocElement.LayerVisible : null,
                    MaxScaleDenominator = accessTypes.HasFlag(AccessTypes.Map) ? layer.MaximumScale : null,
                    MinScaleDenominator = accessTypes.HasFlag(AccessTypes.Map) ? layer.MinimumScale : null,
                };

                if (layer is IFeatureLayer featureLayer)
                {
                    layerInfo.GeometryType = featureLayer.LayerGeometryType.ToString();
                    layerInfo.SuportedOperations = SupportedLayerOperations(map, featureLayer, accessTypes);
                    layerInfo.Properties = accessTypes.HasFlag(AccessTypes.Query) || accessTypes.HasFlag(AccessTypes.Edit)
                        ? featureLayer.Fields?.ToEnumerable()
                            .Where(f => f.type != FieldType.Shape)
                            .Select(f =>
                            {
                                var layerProperty = new LayerProperty()
                                {
                                    Name = f.name,
                                    Aliasname = String.IsNullOrEmpty(f.aliasname) ? f.name : f.aliasname,
                                };


                                if (f.type == FieldType.ID)
                                {
                                    layerProperty.Type = PropertyType.Integer;
                                    layerProperty.IsPrimaryKey = true;
                                }
                                else if (Enum.TryParse(f.type.ToString(), true, out PropertyType propertyType))
                                {
                                    layerProperty.Type = propertyType;
                                }

                                return layerProperty;
                            })
                        : null;
                }

                return layerInfo;
            })
                : null
         };
     });

    private static IEnumerable<SupportedRequest> SupportedServiceRequests(IServiceMap map, string url, AccessTypes accessTypes)
    {
        if (accessTypes.HasFlag(AccessTypes.Map))
        {
            yield return new GetMapRequestProperties()
            {
                Url = $"{url}/map",
                HttpMethods = ["GET", "POST"],
                MaxImageHeight = map.MapServiceProperties.MaxImageWidth,
                MaxImageWidth = map.MapServiceProperties.MaxImageHeight,
                SupportedFormats = ["png", "jpg"]
            };
            yield return new GetLegendRequestProperties()
            {
                Url = $"{url}/legend",
                HttpMethods = ["GET"]
            };
        }
        if (accessTypes.HasFlag(AccessTypes.Query))
        {
            yield return new GetFeaturesRequestProperties()
            {
                Url = $"{url}/query/{{layerId}}",
                HttpMethods = ["GET", "POST"],
                MaxFeaturesLimit = map.MapServiceProperties.MaxRecordCount
            };
        }
        if (accessTypes.HasFlag(AccessTypes.Edit))
        {
            yield return new EditFeaturesRequestProperties()
            {
                Url = $"{url}/features/{{layerId}}",
                HttpMethods = ["POST", "PUT", "DELETE"],
            };
        }
    }

    private static IEnumerable<string> SupportedLayerOperations(IServiceMap map, IFeatureLayer layer, AccessTypes accessTypes)
    {
        if (accessTypes.HasFlag(AccessTypes.Query))
        {
            yield return RequestProperties.QueryFeatures;
        }

        if (accessTypes.HasFlag(AccessTypes.Edit))
        {
            var editModule = map.GetModule<Plugins.Modules.EditorModule>();
            var editLayer = editModule?.GetEditLayer(layer.ID);

            if (editLayer is not null)
            {
                if (editLayer.Statements.HasFlag(Framework.Editor.Core.EditStatements.INSERT)) yield return $"{RequestProperties.EditFeatures}.post";
                if (editLayer.Statements.HasFlag(Framework.Editor.Core.EditStatements.UPDATE)) yield return $"{RequestProperties.EditFeatures}.put";
                if (editLayer.Statements.HasFlag(Framework.Editor.Core.EditStatements.DELETE)) yield return $"{RequestProperties.EditFeatures}.delete";
            }
        }
    }
}
