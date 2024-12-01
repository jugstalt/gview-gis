#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Exceptions;
using gView.Framework.Data.Filters;
using gView.GeoJsonService.DTOs;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class GetFeatures : BaseApiEndpoint
{
    public GetFeatures()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.GetFeatures}/{{id}}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.GetFeatures}/{{id}}"
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
                int id,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<GetFeaturesRequest>(httpContext, loginManagerService,
            async (identity, queryRequest) =>
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

                int maxRecordCount = queryRequest.IdsOnly == true  // return all Ids!
                                ? int.MaxValue
                                : serviceMap.MapServiceProperties.MaxRecordCount;

                var tableClass = serviceMap.MapElements
                                           .FirstOrDefault(e => e.ID == id)?.Class as ITableClass;

                if (tableClass is null)
                {
                    throw new MapServerException($"Layer with id {id} is not queryable");
                }

                IQueryFilter? filter;

                if (queryRequest.SpatialFilter?.Geometry is not null)
                {
                    filter = new Framework.Data.Filters.SpatialFilter();
                    //var jsonGeometry = query.Geometry.ToJsonGeometry();
                    //var filterGeometry = jsonGeometry.ToGeometry();

                    //((SpatialFilter)filter).Geometry = filterGeometry;
                    //((SpatialFilter)filter).FilterSpatialReference =
                    //    SRef(query.InSRef) ??
                    //    (filterGeometry.Srs > 0 ? SpatialReference.FromID($"epsg:{filterGeometry.Srs}") : null);
                }
                else if (queryRequest.Distinct == true)
                {
                    filter = new DistinctFilter(queryRequest.OutFields.First());
                }
                else if (queryRequest.ObjectIds?.Any() == true)
                {
                    filter = new RowIDFilter(tableClass.IDFieldName,
                        queryRequest.ObjectIds.Select(id => int.Parse(id)).ToList());
                }
                else
                {
                    filter = new QueryFilter();
                }

                filter.WhereClause = queryRequest.Filter?.WhereClause ?? "1=1";

                return new { success = true };
            });
}
