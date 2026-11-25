#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.MapServer;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.Extensions.Cql2;
using gView.Interoperability.Extensions.Extensions;
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
/// .../find: a text search over the text fields of several layers, case-insensitive (contains or equals). The search
/// is a CQL2 filter (CASEI(field) LIKE CASEI('%text%')), translated for each data source. Needs the right query.
/// </summary>
public class Find : BaseApiEndpoint
{
    public Find()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.Find}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.Find}"
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
            [FromServices] ILogger<Find> logger,
            string folder = "",
            string service = "") => HandleSecureAsync<FindRequest>(
                    httpContext, mapServerService, loginManagerService, logger,
                    folder, service,
                    async (serviceRequestContext, mapService, identity, request) =>
     {
         if (!(await mapService.GetAccessTypes(identity)).HasFlag(AccessTypes.Query))
         {
             throw new MapServerException("Forbidden: find needs the right query", 403);
         }

         var text = request.SearchText?.Trim();
         if (String.IsNullOrEmpty(text))
         {
             throw new MapServerException("searchText is required");
         }

         using var map = (await mapServerService.Instance.GetServiceMapAsync(mapService)).ThrowIfNull(service);

         var outSRef = request.OutCRS.ToSpatialReferenceOrDefault();
         int maxRecordCount = map.MapServiceProperties?.MaxRecordCount > 0 ? map.MapServiceProperties.MaxRecordCount : 1000;
         int limit = request.Limit is > 0 ? Math.Min(request.Limit.Value, maxRecordCount) : maxRecordCount;
         var layerIds = request.Layers?.Select(id => int.TryParse(id, out var value) ? value : throw new MapServerException($"Invalid layer id '{id}'")).ToHashSet();
         var pattern = new CqlCaseInsensitive(new CqlString(request.Contains ? $"%{text}%" : text));

         var layers = map.MapElements
             .OfType<IFeatureLayer>()
             .Where(layer => layer.FeatureClass is not null && (layerIds is null || layerIds.Contains(layer.ID)))
             .Select(layer => (layer, toc: map.TOC.GetTOCElement(layer)))
             .Where(l => l.toc is not null && !l.toc.IsHidden())
             .OrderBy(l => map.TOC.GetTOCElementOrder(l.toc!))
             .ToList();

         var results = new List<IdentifyResult>();
         foreach (var (layer, toc) in layers)
         {
             var featureClass = layer.FeatureClass;
             var dialect = (featureClass as ISupportsSqlDialect)?.SqlDialect ?? SqlDialect.Unknown;
             var fields = featureClass.Fields.ToEnumerable()
                 .Where(f => f.IsStringType())
                 .Where(f => request.SearchFields is null
                             || request.SearchFields.Any(name => name.Equals(f.name, StringComparison.OrdinalIgnoreCase)))
                 .ToList();
             if (fields.Count == 0)
             {
                 continue;  // nothing to search in this layer
             }

             CqlPredicate predicate = fields
                 .Select(f => (CqlPredicate)new CqlLike(new CqlCaseInsensitive(new CqlProperty(f.name)), pattern, false))
                 .Aggregate((left, right) => new CqlOr(left, right));

             var filter = new QueryFilter()
             {
                 SubFields = "*",
                 WhereClause = featureClass.Cql2ToWhereClause(predicate).AppendWhereClause(layer.FilterQuery?.WhereClause),
                 Limit = limit + 1  // one more: exceededLimit
             };
             filter.SetFeatureSpatialReference(outSRef, map.Display?.DatumTransformations);

             var features = new List<gView.GeoJsonService.DTOs.Feature>();
             using (var cursor = await featureClass.Search(filter) as IFeatureCursor)
             {
                 IFeature feature;
                 while (cursor is not null && (feature = await cursor.NextFeature()) is not null)
                 {
                     features.Add(feature.ToGeoJsonFeature(featureClass.IDFieldName, request.ReturnGeometry,
                         new GeometryOptions(request.MaxAllowableOffset, request.GeometryPrecision)));
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
         }

         return new FindResponse() { Results = results };
     });
}
