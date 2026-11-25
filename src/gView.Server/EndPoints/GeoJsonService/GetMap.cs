#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.Framework.Data.Extensions;
using gView.Framework.Data;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology;
using gView.Framework.Cartography.Rendering;
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

                // layer definitions and highlights are checked before rendering: an invalid filter is a 400, not a broken image
                var layerDefinitions = LayerDefinitions(serviceMap, mapRequest.LayerDefs);
                var highlights = Highlights(serviceMap, mapRequest.Highlight);

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
                    BeforeRenderLayers = (sender, context, layers) =>
                    {
                        SetLayerVisibility(sender, layers, mapRequest.Layers?.ToArray());
                        ApplyLayerDefinitions(layers, layerDefinitions);
                        AddHighlightLayers(layers, highlights);
                    }
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
    /// The visibility of the layers from the request, set on the copies of the layers the renderer passes to
    /// BeforeRenderLayers (never on the layers of the service map: they are shared between requests)
    /// </summary>
    private static void SetLayerVisibility(IServiceMap sender, List<ILayer> layers, string[]? layerIds)
    {
        if (layerIds!.GetVisibilityPattern() == MapLayerVisibilityPattern.Defaults)  // null = defaults
        {
            return;
        }

        foreach (var layer in layers)
        {
            layer.Visible = IsVisible(sender, layer, layerIds);
        }
    }

    /// <summary>
    /// Whether a layer is visible with the layer ids of a request, without changing it: null = the defaults of the
    /// service, ids = exactly these (a group shows all its layers), +id/-id = add to/remove from the defaults
    /// </summary>
    internal static bool IsVisible(IServiceMap map, ILayer layer, string[]? layerIds)
    {
        var visibilityPattern = layerIds!.GetVisibilityPattern();  // null = defaults
        if (visibilityPattern == MapLayerVisibilityPattern.Defaults)
        {
            return layer.Visible;
        }

        var layerVisibility = layerIds!.LayerOrParentIsInArray(layer, map.TOC?.GetTOCElement(layer));  // if group is shown -> all layers in group are shown...

        return (visibilityPattern, layerVisibility) switch
        {
            (_, MapLayerVisibility.Visible) => true,
            (_, MapLayerVisibility.Include) => true,
            (_, MapLayerVisibility.Exclude) => false,
            (MapLayerVisibilityPattern.Normal, MapLayerVisibility.Invisible) => false,
            _ => layer.Visible
        };
    }

    /// <summary>The where clause per layer id: SQL (checked against SQL injection) and/or CQL2 (translated)</summary>
    private static Dictionary<int, string> LayerDefinitions(IServiceMap map, IDictionary<string, AttributeFilter>? layerDefs)
    {
        var result = new Dictionary<int, string>();
        foreach (var (key, filter) in layerDefs ?? new Dictionary<string, AttributeFilter>())
        {
            var featureClass = FeatureClassOf(map, key, "layerDefs");
            var whereClause = filter?.WhereClauseWithResolveParameters();
            whereClause?.CheckWhereClauseForSqlInjection(featureClass);
            var cql2 = filter?.Cql2Predicate();

            var where = whereClause.AppendWhereClause(cql2 is not null ? featureClass.Cql2ToWhereClause(cql2) : null);
            if (!String.IsNullOrWhiteSpace(where))
            {
                result[int.Parse(key)] = where;
            }
        }
        return result;
    }

    /// <summary>The object ids to highlight per layer id</summary>
    private static Dictionary<int, (IFeatureClass featureClass, int[] objectIds)> Highlights(IServiceMap map, IDictionary<string, int[]>? highlight)
    {
        var result = new Dictionary<int, (IFeatureClass, int[])>();
        foreach (var (key, objectIds) in highlight ?? new Dictionary<string, int[]>())
        {
            var featureClass = FeatureClassOf(map, key, "highlight");
            if (String.IsNullOrEmpty(featureClass.IDFieldName))
            {
                throw new MapServerException($"highlight: layer {key} has no object id field");
            }
            var ids = (objectIds ?? []).Where(id => id > 0).Distinct().ToArray();
            if (ids.Length > 0)
            {
                result[int.Parse(key)] = (featureClass, ids);
            }
        }
        return result;
    }

    private static IFeatureClass FeatureClassOf(IServiceMap map, string layerId, string parameter)
        => int.TryParse(layerId, out var id) && map.FindTableClass(id, out _) is { Count: 1 } tableClasses && tableClasses[0] is IFeatureClass featureClass
            ? featureClass
            : throw new MapServerException($"{parameter}: '{layerId}' is no feature layer");

    /// <summary>On the copies of the layers the renderer passes: never on the shared layers of the service map</summary>
    private static void ApplyLayerDefinitions(List<ILayer> layers, Dictionary<int, string> layerDefinitions)
    {
        foreach (var layer in layers.OfType<IFeatureLayer>())
        {
            if (layerDefinitions.TryGetValue(layer.ID, out var where))
            {
                layer.FilterQuery = layer.FilterQuery.AppendWhereClause(where);
            }
        }
    }

    /// <summary>
    /// A copy of each highlighted layer on top: only the given objects (inside the layer's filter), with a highlight
    /// symbol, without labels
    /// </summary>
    private static void AddHighlightLayers(List<ILayer> layers, Dictionary<int, (IFeatureClass featureClass, int[] objectIds)> highlights)
    {
        foreach (var (layerId, (featureClass, objectIds)) in highlights)
        {
            var layer = layers.OfType<IFeatureLayer>().FirstOrDefault(l => l.ID == layerId);
            if (layer is null || LayerFactory.Create(layer.Class, layer) is not IFeatureLayer highlightLayer)
            {
                continue;  // not drawn (e.g. out of scale)
            }

            highlightLayer.FeatureRenderer = new SimpleRenderer() { Symbol = HighlightSymbol(layer.LayerGeometryType) };
            highlightLayer.LabelRenderer = null;
            highlightLayer.FilterQuery = layer.FilterQuery.AppendWhereClause(
                $"{featureClass.IDFieldName} IN ({String.Join(",", objectIds)})");
            highlightLayer.Visible = true;
            layers.Add(highlightLayer);
        }
    }

    private static ISymbol HighlightSymbol(gView.Framework.Core.Geometry.GeometryType geometryType)
        => geometryType switch
        {
            gView.Framework.Core.Geometry.GeometryType.Polygon or gView.Framework.Core.Geometry.GeometryType.Envelope => new SimpleFillSymbol()
            {
                Color = ArgbColor.FromArgb(70, 255, 255, 0),
                OutlineColor = ArgbColor.FromArgb(255, 0, 255, 255),
                OutlineWidth = 3
            },
            gView.Framework.Core.Geometry.GeometryType.Polyline => new SimpleLineSymbol() { Color = ArgbColor.FromArgb(200, 0, 255, 255), Width = 5 },
            _ => new SimplePointSymbol()
            {
                Color = ArgbColor.FromArgb(255, 255, 255, 0),
                Size = 12,
                OutlineColor = ArgbColor.FromArgb(255, 0, 0, 0),
                OutlineWidth = 1
            }
        };
}
