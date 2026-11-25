#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.GeoJsonService.DTOs;
using gView.GraphicsEngine;
using gView.Interoperability.Extensions;
using gView.Interoperability.Extensions.Rendering;
using gView.Server.AppCode.Extensions;
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
                using var serviceMap = (await mapServiceManager.Instance.GetServiceMapAsync(mapService)).ThrowIfNull();

                ImageSizeLimits.ThrowIfInvalidImageSize(mapRequest.Width, mapRequest.Height);

                var imageFormatString = (mapRequest.Format ?? "png").Split('/').Last().ToLowerInvariant();
                if (!Enum.TryParse(imageFormatString switch
                    {
                        "jpg" => "jpeg",
                        _ => imageFormatString
                    },
                    true,
                    out ImageFormat imageFormat))
                {
                    throw new MapServerException($"Unsuported image format: {mapRequest.Format}");
                }

                if (mapRequest.BBox is null)
                {
                    throw new MapServerException("bbox is required");
                }

                // rendered like GeoServices export: the extent follows the aspect ratio of the image, the extent
                // actually drawn is in the response; image size and dpi are limited by the service
                var imageBytes = await serviceMap.RenderImageAsync(new MapImageRequest()
                {
                    SpatialReference = mapRequest.CRS.ToSpatialReferenceOrDefault(),
                    BBox = mapRequest.BBox.ToEnvelope(),
                    Width = mapRequest.Width,
                    Height = mapRequest.Height,
                    Dpi = mapRequest.Dpi is > 0 ? mapRequest.Dpi.Value : ImageSizeLimits.DefaultDpi,
                    Transparent = mapRequest.Transparent,
                    AlphaBackground = imageFormat == ImageFormat.Png,
                    BackgroundColor = null,  // opaque images keep the background of the map
                    Format = imageFormat,
                    Rotation = mapRequest.Rotation ?? 0.0,
                    FitToBBox = false,
                    BeforeRenderLayers = (sender, context, layers) => SetLayerVisibility(sender, layers, mapRequest.Layers?.ToArray())
                }) ?? throw new MapServerException("No map image rendered");

                string contentType = imageFormat.ToContentType();

                if (mapRequest.ResponseFormat == MapReponseFormat.Image)
                {
                    return Results.File(imageBytes, contentType);
                }

                var response = new GetMapResponse()
                {
                    BBox = serviceMap.Display.Envelope.ToBBox(),
                    CRS = serviceMap.Display.SpatialReference is not null
                            ? CoordinateReferenceSystem.CreateByName(serviceMap.Display.SpatialReference.Name)
                            : null,
                    Width = serviceMap.Display.ImageWidth,
                    Height = serviceMap.Display.ImageHeight,
                    ScaleDenominator = serviceMap.Display.MapScale,
                    Rotation = mapRequest.Rotation is double rotation && rotation != 0.0 ? rotation : null,
                    ContentType = contentType
                };

                if (mapRequest.ResponseFormat == MapReponseFormat.Base64)
                {
                    response.ImageBase64 = Convert.ToBase64String(imageBytes);
                }
                else
                {
                    string serviceMapName = serviceMap.Name.Replace("/", "_").Replace(",", "_");
                    string fileName = $"{serviceMapName}_{System.Guid.NewGuid():N}.{imageFormatString}";

                    await File.WriteAllBytesAsync(($"{mapServiceManager.Instance.OutputPath}/{fileName}").ToPlatformPath(), imageBytes);
                    response.ImageUrl = $"{serviceRequestContext.ServiceRequest.OutputUrl}/{fileName}";
                }

                return response;
            });

    /// <summary>
    /// The visibility of the layers from the request: null = the defaults of the service, ids = exactly these
    /// (a group shows all its layers), +id/-id = add to/remove from the defaults
    /// </summary>
    private static void SetLayerVisibility(IServiceMap sender, List<ILayer> layers, string[]? layerIds)
    {
        var visibilityPattern = layerIds!.GetVisibilityPattern();  // null = defaults
        if (visibilityPattern == MapLayerVisibilityPattern.Defaults)
        {
            return;
        }

        foreach (var layer in layers)
        {
            var tocElement = sender.TOC?.GetTocElementByLayerId(layer.ID);
            var layerVisibility = tocElement != null
                ? layerIds!.LayerOrParentIsInArray(tocElement) // if group is shown -> all layers in group are shown...
                : layerIds!.ContainsId(layer.ID);

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
}
