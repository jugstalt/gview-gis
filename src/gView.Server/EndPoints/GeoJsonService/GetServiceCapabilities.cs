using gView.Framework.Cartography;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode.Extensions;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
            string folder = "",
            string service = "") => HandleSecureAsync(httpContext, loginManagerService, async (identity) =>
     {
         if (String.IsNullOrEmpty(service))
         {
             (folder, service) = ("", folder);
         }

         var mapService = mapServerService.Instance.GetMapService(service, folder);

         if (mapService == null)
         {
             throw new MapServerException("Unknown service");
         }
         if (await mapService.HasAnyAccess(identity) == false)
         {
             throw new MapServerAuthException("service forbidden");
         }

         string path = String.IsNullOrEmpty(folder)
                        ? service
                        : $"{folder}/{service}";
         // todo: onlineresouce for folder...?
         string url = $"{mapServerService.Options.OnlineResource}/{Routes.Base}/{Routes.GetServices}/{path}";
         var map = await mapServerService.Instance.GetServiceMapAsync(service, folder);

         return new GetServiceCapabilitiesResponse()
         {
             MapTitle = String.IsNullOrWhiteSpace(map.Title)
                        ? (map.Name.Contains("/") ? map.Name.Substring(map.Name.LastIndexOf("/") + 1) : map.Name)
                        : map.Title,
             Copyright = map.GetLayerCopyrightText(Map.MapCopyrightTextId),
             Description = map.GetLayerDescription(Map.MapDescriptionId),

             SupportedRequests =
             [
                 new GetMapRequestProperties ()
                 {
                     Url = $"{url}/map",
                     HttpMethods = [ "GET", "POST" ],
                     MaxImageHeight = map.MapServiceProperties.MaxImageWidth,
                     MaxImageWidth = map.MapServiceProperties.MaxImageHeight,
                     SupportedFormats=[ "png", "jpg" ]
                 },
                 new GetLegendRequestProperties()
                 {
                     Url = $"{url}/legend",
                     HttpMethods = [ "GET" ]
                 },
                 new GetFeaturesRequestProperties ()
                 {
                     Url = $"{url}/features",
                     HttpMethods = [ "GET", "POST" ],
                     MaxFeaturesLimit = map.MapServiceProperties.MaxRecordCount
                 }
             ],

             CRS = map.Display.SpatialReference is not null
                    ? CoordinateReferenceSystem.CreateByName(map.Display.SpatialReference.Name)
                    : null,
             FullExtent = map.FullExtent().ToBBox(),
             InitialExtent = map.FullExtent().ToBBox(),
             Units = map.Display.MapUnits.ToString(),

             Layers = map.MapElements
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
                        DefaultVisibility = tocElement.LayerVisible,
                        MaxScaleDenominator = layer.MaximumScale,
                        MinScaleDenominator = layer.MinimumScale,
                    };

                    if (layer is IFeatureLayer featureLayer)
                    {
                        layerInfo.GeometryType = featureLayer.LayerGeometryType.ToString();
                        layerInfo.SuportedOperations = ["QUERY"];
                        layerInfo.Properties = featureLayer.Fields?.ToEnumerable()
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
                            });

                    }

                    return layerInfo;
                })
         };
     });
}
