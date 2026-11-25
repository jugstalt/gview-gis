#nullable enable

using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Core.UI;
using gView.Framework.Data.Extensions;
using gView.Framework.GeoJsonService.Extensions;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
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
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

/// <summary>
/// .../identify: the features of several layers at a point (with a tolerance in pixels) or a line/polygon.
/// visible/top respect the visibility (like GetMap) and the scale of the map the client shows. Needs the right query.
/// </summary>
public class Identify : BaseApiEndpoint
{
    public Identify()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.Identify}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.Identify}"
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
            [FromServices] ILogger<Identify> logger,
            string folder = "",
            string service = "") => HandleSecureAsync<IdentifyRequest>(
                    httpContext, mapServerService, loginManagerService, logger,
                    folder, service,
                    async (serviceRequestContext, mapService, identity, request) =>
     {
         if (!(await mapService.GetAccessTypes(identity)).HasFlag(AccessTypes.Query))
         {
             throw new MapServerException("Forbidden: identify needs the right query", 403);
         }

         string option = (request.LayerOption ?? "visible").ToLowerInvariant();
         if (option is not ("visible" or "top" or "all"))
         {
             throw new MapServerException($"Invalid layerOption '{request.LayerOption}': visible, top or all");
         }

         using var map = (await mapServerService.Instance.GetServiceMapAsync(mapService)).ThrowIfNull(service);

         var sRef = request.CRS.ToSpatialReferenceOrDefault();
         var geometry = request.Geometry?.ToGeometry() ?? throw new MapServerException("geometry is required");
         var outSRef = request.OutCRS is not null ? request.OutCRS.ToSpatialReferenceOrDefault() : sRef;

         #region The map of the client: scale and tolerance

         double tolerance = 0.0;
         if (request.MapExtent is not null && request.ImageWidth > 0 && request.ImageHeight > 0)
         {
             map.Display.SpatialReference = sRef;
             map.Display.Dpi = Math.Clamp(request.Dpi ?? ImageSizeLimits.DefaultDpi, 1.0, ImageSizeLimits.EffectiveMaxDpi(map.MapServiceProperties?.MaxDpi ?? 0));
             map.Display.ImageWidth = Math.Min(request.ImageWidth, ImageSizeLimits.AbsoluteMaxImageSize);
             map.Display.ImageHeight = Math.Min(request.ImageHeight, ImageSizeLimits.AbsoluteMaxImageSize);
             map.Display.ZoomTo(request.MapExtent.ToEnvelope());

             tolerance = Math.Max(0, request.Tolerance) * map.Display.Envelope.Width / map.Display.ImageWidth;
         }
         else if (option != "all")
         {
             throw new MapServerException($"layerOption {option} needs mapExtent, imageWidth and imageHeight (the map the client shows)");
         }

         IGeometry filterGeometry = geometry is IPoint point && tolerance > 0
             ? new Envelope(point.X - tolerance, point.Y - tolerance, point.X + tolerance, point.Y + tolerance)
             : geometry;

         #endregion

         #region Layers: top first, visible like GetMap and in scale

         var layers = map.MapElements
             .OfType<IFeatureLayer>()
             .Where(layer => layer.FeatureClass is not null)
             .Select(layer => (layer, toc: map.TOC.GetTOCElement(layer)))
             .Where(l => l.toc is not null && !l.toc.IsHidden())
             .OrderBy(l => map.TOC.GetTOCElementOrder(l.toc!))
             .ToList();

         if (option != "all")
         {
             // only read: the layers of the service map are shared between requests
             layers = layers
                 .Where(l => GetMap.IsVisible(map, l.layer, request.Layers) && l.layer.RenderInScale(map.Display))
                 .ToList();
         }

         #endregion

         int maxRecordCount = map.MapServiceProperties?.MaxRecordCount > 0 ? map.MapServiceProperties.MaxRecordCount : 1000;
         int limit = request.Limit is > 0 ? Math.Min(request.Limit.Value, maxRecordCount) : maxRecordCount;

         var results = new List<IdentifyResult>();
         foreach (var (layer, toc) in layers)
         {
             var featureClass = layer.FeatureClass;
             var filter = new Framework.Data.Filters.SpatialFilter()
             {
                 Geometry = filterGeometry,
                 FilterSpatialReference = sRef,
                 SpatialRelation = spatialRelation.SpatialRelationIntersects,
                 SubFields = "*",
                 WhereClause = layer.FilterQuery?.WhereClause ?? "",  // only what the layer shows
                 Limit = limit + 1  // one more: exceededLimit
             };
             filter.SetFeatureSpatialReference(outSRef, map.Display?.DatumTransformations);

             var features = new List<gView.GeoJsonService.DTOs.Feature>();
             using (var cursor = await featureClass.Search(filter) as IFeatureCursor)
             {
                 IFeature feature;
                 while (cursor is not null && (feature = await cursor.NextFeature()) is not null)
                 {
                     features.Add(feature.ToGeoJsonFeature(featureClass.IDFieldName, request.ReturnGeometry));
                 }
             }

             if (features.Count == 0)
             {
                 continue;
             }

             bool exceededLimit = features.Count > limit;
             results.Add(new IdentifyResult()
             {
                 LayerId = layer.ID.ToString(),
                 LayerName = toc!.Name,
                 FeatureCollection = new FeatureCollection()
                 {
                     CRS = outSRef is not null ? CoordinateReferenceSystem.CreateByName(outSRef.Name) : null,
                     Features = exceededLimit ? features.Take(limit).ToList() : features,
                     ExceededLimit = exceededLimit
                 }
             });

             if (option == "top")
             {
                 break;
             }
         }

         return new IdentifyResponse() { Results = results };
     });
}
