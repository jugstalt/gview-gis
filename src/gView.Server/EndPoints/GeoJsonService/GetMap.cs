using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.GeoJsonService.DTOs;
using gView.GraphicsEngine;
using gView.Interoperability.Extensions;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetMap : BaseApiEndpoint
{
    public GetMap()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.GetMap}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.GetMap}"
                ],
                Handler,
                HttpMethod.Get | HttpMethod.Post
            )
    {
    }

    private static Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<GetMap> logger,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<GetMapRequest>(httpContext, mapServiceManager, loginManagerService, logger, folder, service,
                async (serviceRequestContext, mapService, identity, mapRequest) =>
            {
                using var serviceMap = await mapServiceManager.Instance.GetServiceMapAsync(mapService);

                #region Display (ImageSize, DPI, Rotaion, Extent)

                if (mapRequest.Width <= 0 || mapRequest.Height <= 0)
                {
                    throw new MapServerException($"Invalid image size ({mapRequest.Width},{mapRequest.Height})");
                }

                if (mapRequest.Dpi.HasValue)
                {
                    serviceMap.Display.Dpi = mapRequest.Dpi.Value;
                }

                serviceMap.Display.ImageWidth = mapRequest.Width;
                serviceMap.Display.ImageHeight = mapRequest.Height;

                serviceMap.ResizeImageSizeToMapServiceLimits();

                serviceMap.Display.SpatialReference = mapRequest.CRS.ToSpatialReferenceOrDefault();
                
                serviceMap.Display.ZoomTo(mapRequest.BBox.ToEnvelope());

                if (mapRequest.Rotation.HasValue && mapRequest.Rotation.Value != 0.0)
                {
                    serviceMap.Display.DisplayTransformation.DisplayRotation = mapRequest.Rotation.Value;
                }

                #endregion

                #region ImageFormat / Transparency

                if (!Enum.TryParse(mapRequest.Format.Split('/').Last(), true, out ImageFormat imageFormat))
                {
                    throw new MapServerException($"Unsuported image format: {mapRequest.Format}");
                }

                if (mapRequest.Transparent)
                {
                    serviceMap.Display.MakeTransparent = true;
                    serviceMap.Display.TransparentColor = ArgbColor.White;
                }
                else
                {
                    serviceMap.Display.MakeTransparent = false;
                }

                if (serviceMap.Display.MakeTransparent && imageFormat == ImageFormat.Png)
                {
                    // Beim Png sollt dann beim zeichnen keine Hintergrund Rectangle gemacht werden
                    // Darum Farbe mit A=0
                    // Sonst schaut das Bild beim PNG32 und Antialiasing immer zerrupft aus...
                    serviceMap.Display.BackgroundColor = ArgbColor.Transparent;
                }

                #endregion

                #region Layers

                serviceMap.BeforeRenderLayers += (
                        IServiceMap sender,
                        IServiceRequestContext context,
                        List<ILayer> layers) =>
                {
                    var layerIds = mapRequest.Layers?.ToArray();

                    #region Layer Visibility

                    var visibilityPattern = layerIds.GetVisibilityPattern();

                    if (visibilityPattern != MapLayerVisibilityPattern.Defaults)  // defaults: dont change visibilty, use map defautls
                    {


                        foreach (var layer in layers)
                        {
                            var tocElement = sender.TOC?.GetTocElementByLayerId(layer.ID);
                            var layerVisibility = tocElement != null
                                ? layerIds.LayerOrParentIsInArray(tocElement) // if group is shown -> all layers in group are shown...
                                : layerIds.ContainsId(layer.ID);

                            layer.Visible = (visibilityPattern, layerVisibility) switch
                            {
                                (_, MapLayerVisibility.Visible) => true,
                                (_, MapLayerVisibility.Include) => true,
                                (_, MapLayerVisibility.Exclude) => false,
                                (MapLayerVisibilityPattern.Normal, MapLayerVisibility.Invisible) => false,
                                _ => layer.Visible
                            };
                        }


                    }

                    #endregion

                    #region ToDo: QueryDefs

                    #endregion

                    #region ToDo: DynamicLayers

                    #endregion
                };

                #endregion

                await serviceMap.Render();

                string contentType = imageFormat.ToContentType();

                if (mapRequest.ResponseFormat == MapReponseFormat.Image)
                {
                    using MemoryStream ms = new();
                    await serviceMap.SaveImage(ms, imageFormat);
                    return Results.File(ms.ToArray(), contentType);
                }
                else if (mapRequest.ResponseFormat == MapReponseFormat.Base64)
                {
                    using MemoryStream ms = new();
                    await serviceMap.SaveImage(ms, imageFormat);

                    return new GetMapResponse()
                    {
                        ImageBase64 = Convert.ToBase64String(ms.ToArray()),
                        BBox = serviceMap.Display.Envelope.ToBBox(),
                        CRS = serviceMap.Display.SpatialReference is not null
                                ? CoordinateReferenceSystem.CreateByName(serviceMap.Display.SpatialReference.Name)
                                : null,
                        Width = serviceMap.Display.ImageWidth,
                        Height = serviceMap.Display.ImageHeight,
                        ScaleDenominator = serviceMap.Display.MapScale,
                        ContentType = contentType
                    };
                }
                else
                {
                    string serviceMapName = serviceMap.Name.Replace("/", "_").Replace(",", "_");
                    string fileName =
                       $"{serviceMapName}_{System.Guid.NewGuid():N}.{imageFormat.ToString().ToLower()}";

                    string path = ($"{mapServiceManager.Instance.OutputPath}/{fileName}").ToPlatformPath();
                    await serviceMap.SaveImage(path, imageFormat);

                    return new GetMapResponse()
                    {
                        ImageUrl = $"{serviceRequestContext.ServiceRequest.OutputUrl}/{fileName}",
                        BBox = serviceMap.Display.Envelope.ToBBox(),
                        CRS = serviceMap.Display.SpatialReference is not null
                                ? CoordinateReferenceSystem.CreateByName(serviceMap.Display.SpatialReference.Name)
                                : null,
                        Width = serviceMap.Display.ImageWidth,
                        Height = serviceMap.Display.ImageHeight,
                        ScaleDenominator = serviceMap.Display.MapScale,
                        ContentType = contentType
                    };
                }
            });
}