using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.GeoJsonService.DTOs;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
            [FromServices] MapServiceManager mapServerService,
            string folder = "",
            string service = "") => HandleSecureAsync<GetMapRequest>(httpContext, loginManagerService, 
                async (identity, mapRequest) =>
            {
                var mapService = mapServerService.Instance.GetMapService(service, folder);
                if (mapService == null)
                {
                    throw new MapServerException("Unknown service");
                }
                if (await mapService.HasAnyAccess(identity) == false)
                {
                    throw new MapServerAuthException("service forbidden");
                }

                using var serviceMap = await mapServerService.Instance.GetServiceMapAsync(mapService);

                if(mapRequest.Dpi.HasValue)
                {
                    serviceMap.Display.Dpi = mapRequest.Dpi.Value;
                }
                serviceMap.Display.ImageWidth = mapRequest.Width;
                serviceMap.Display.ImageHeight = mapRequest.Height;

                serviceMap.Display.ZoomTo(mapRequest.BBox.ToEnvelope());

                if (mapRequest.Rotation.HasValue && mapRequest.Rotation.Value != 0.0)
                {
                    serviceMap.Display.DisplayTransformation.DisplayRotation = mapRequest.Rotation.Value;
                }

                await serviceMap.Render();

                var imageFormat = GraphicsEngine.ImageFormat.Png;

                if (mapRequest.ResponseFormat == "image")
                {
                    MemoryStream ms = new();
                    await serviceMap.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
                else
                {
                    string serviceMapName = serviceMap.Name.Replace("/", "_").Replace(",", "_");
                    string fileName =
                       $"{serviceMapName}_{System.Guid.NewGuid():N}.{imageFormat.ToString().ToLower()}";

                    string path = ($"{mapServerService.Instance.OutputPath}/{fileName}").ToPlatformPath();
                    await serviceMap.SaveImage(path, imageFormat);

                    return new GetMapResponse()
                    {
                        ImageUrl = $"{mapServerService.Instance.OutputUrl}/{fileName}",
                        BBox = serviceMap.Display.Envelope.ToBBox(),
                        Width = serviceMap.Display.ImageWidth,
                        Height = serviceMap.Display.ImageHeight,
                        ScaleDenominator = serviceMap.Display.MapScale
                    };
                }
            });
}