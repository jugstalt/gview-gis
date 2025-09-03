using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.GeoJsonService.DTOs;
using gView.Server.AppCode.Extensions;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class FeaturesDelete : BaseApiEndpoint
{
    public FeaturesDelete()
         : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.EditFeatures}/{{id}}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.EditFeatures}/{{id}}"
                ],
                Handler,
                HttpMethod.Delete
            )
    {

    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] MapServiceManager mapServerService,
                [FromServices] ILogger<FeaturesDelete> logger,
                int id,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<EditFeaturesRequest>(httpContext, mapServerService, loginManagerService, logger, folder, service,
                 async (serviceRequestContext, mapService, identity, editRequest) =>
                 {
                     using var serviceMap = (await mapServerService.Instance.GetServiceMapAsync(mapService)).ThrowIfNull();

                     var featureClass = serviceMap.CheckEditableStatement(id, Framework.Editor.Core.EditStatements.DELETE).GetFeatureClass(id);

                     var dataset = featureClass.Dataset;
                     var database = dataset?.Database as IFeatureUpdater;
                     if (database == null)
                     {
                         throw new MapServerException("Featureclass is not editable");
                     }

                     var objectIds = editRequest.ObjectIds?.Select(o => Convert.ToInt32(o));
                     foreach (int objectId in objectIds)
                     {
                         if (!await database.Delete(featureClass, objectId))
                         {
                             throw new Exception(database.LastErrorMessage);
                         }
                     }

                     return new EditFeaturesResponse()
                     {
                         Succeeded = true,
                         Statement = Framework.Editor.Core.EditStatements.DELETE.ToString(),
                         Count = objectIds.Count()
                     };
                 });
}
