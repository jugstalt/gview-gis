using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode.Extensions;
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

public class GetLegend : BaseApiEndpoint
{
    public GetLegend()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.GetLegend}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.GetLegend}"
                ],
                Handler,
                HttpMethod.Get
            )
    {
    }

    private static Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] MapServiceManager mapServiceManager,
                [FromServices] ILogger<GetLegend> logger,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<GetLegendRequest>(httpContext, mapServiceManager, loginManagerService, logger, folder, service,
                async (serviceRequestContext, mapService, identity, legendRequest) =>
                {
                    if (legendRequest.Width <= 0 || legendRequest.Height <= 0)
                    {
                        throw new MapServerException($"Invalid legend size ({legendRequest.Width},{legendRequest.Height})");
                    }

                    var legendLayers = new List<LegendLayer>();
                    using var serviceMap = (await mapServiceManager.Instance.GetServiceMapAsync(mapService)).ThrowIfNull();

                    if (legendRequest.Dpi.HasValue && legendRequest.Dpi.Value > 0)
                    {
                        serviceMap.Display.Dpi = legendRequest.Dpi.Value;
                    }

                    foreach (var layer in serviceMap.MapElements.Where(l => l is ILayer).Select(l => (ILayer)l))
                    {
                        var tocElement = serviceMap.TOC.GetTOCElement(layer);
                        if (tocElement == null)
                        {
                            continue;
                        }

                        using (var tocLegendItems = await serviceMap.TOC.LegendSymbol(tocElement, legendRequest.Width, legendRequest.Height))
                        {
                            if (tocLegendItems.Items == null || tocLegendItems.Items.Count() == 0)
                            {
                                continue;
                            }

                            var legendLayer = new LegendLayer()
                            {
                                Id = layer.ID.ToString(),
                                Name = tocElement.Name,
                                LayerType = "",  // todo
                                MinScaleDenominator = layer.MinimumScale,
                                MaxScaleDenominator = layer.MaximumScale
                            };

                            var legendItems = new List<LegendItem>();

                            foreach (var tocLegendItem in tocLegendItems.Items)
                            {
                                if (tocLegendItem.Image == null)
                                {
                                    continue;
                                }

                                MemoryStream ms = new MemoryStream();
                                tocLegendItem.Image.Save(ms, GraphicsEngine.ImageFormat.Png);

                                legendItems.Add(new LegendItem()
                                {
                                    Label = tocLegendItem.Label,
                                    ImageBase64 = Convert.ToBase64String(ms.ToArray()),
                                    ImageContentType = "image/png",
                                    Width = tocLegendItem.Image.Width,
                                    Height = tocLegendItem.Image.Height,
                                });
                            }

                            legendLayer.Items = legendItems.ToArray();
                            legendLayers.Add(legendLayer);
                        }
                    }

                    return new GetLegendResponse()
                    {
                        Layers = legendLayers,
                    };
                });
}
