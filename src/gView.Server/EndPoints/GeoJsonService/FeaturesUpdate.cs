using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.GeoJsonService.DTOs;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class FeaturesUpdate : BaseApiEndpoint
{
    public FeaturesUpdate()
         : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.EditFeatures}/{{id}}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.EditFeatures}/{{id}}"
                ],
                Handler,
                HttpMethod.Put
            )
    {

    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] MapServiceManager mapServerService,
                [FromServices] ILogger<FeaturesUpdate> logger,
                int id,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<EditFeaturesRequest>(httpContext, mapServerService, loginManagerService, logger, folder, service,
                 async (serviceRequestContext, mapService, identity, editRequest) =>
                 {
                     using var serviceMap = await mapServerService.Instance.GetServiceMapAsync(mapService);

                     var featureClass = serviceMap.CheckEditableStatement(id, Framework.Editor.Core.EditStatements.UPDATE).GetFeatureClass(id);

                     var dataset = featureClass.Dataset;
                     var database = dataset?.Database as IFeatureUpdater;
                     if (database == null)
                     {
                         throw new MapServerException("Featureclass is not editable");
                     }

                     var geoJsonFeatureSref = editRequest.CRS.ToSpatialReferenceOrDefault();

                     var features = editRequest.Features.GetFeatrues(featureClass, geoJsonFeatureSref, serviceMap.Display?.DatumTransformations);
                     if (features.Count() == 0)
                     {
                         throw new MapServerException("No features to update");
                     }

                     if (features.Count(f => f.OID <= 0) > 0)
                     {
                         throw new MapServerException("Can't update features without existing ObjectId");
                     }

                     features.GeometryMakeValid(serviceMap, featureClass);

                     if (!await database.Update(featureClass, features.ToList()))
                     {
                         throw new MapServerException(database.LastErrorMessage);
                     }

                     return new EditFeaturesResponse()
                     {
                         Succeeded = true,
                         Statement = Framework.Editor.Core.EditStatements.UPDATE.ToString(),
                         Count = features.Count()
                     };
                 });
}
