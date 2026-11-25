using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.Extensions.Editing;
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
                     using var serviceMap = (await mapServerService.Instance.GetServiceMapAsync(mapService)).ThrowIfNull();

                     var editService = FeatureEditService.Create(serviceMap, id);
                     editService.CheckStatement(Framework.Editor.Core.EditStatements.UPDATE);

                     // materialized once: preparing (projection, make valid) and updating work on the same features
                     var features = (editRequest.Features ?? [])
                         .GetFeatrues(editService.FeatureClass, editRequest.CRS.ToSpatialReferenceOrDefault(), serviceMap.Display?.DatumTransformations)
                         .ToList();
                     if (features.Count == 0)
                     {
                         throw new MapServerException("No features to update");
                     }

                     editService.PrepareUpdates(features);
                     await editService.CheckObjectsInLayer(features.Select(f => f.OID));  // only objects the layer shows
                     await editService.ExecuteAsync(session => editService.Update(features, session));

                     return new EditFeaturesResponse()
                     {
                         Succeeded = true,
                         Statement = Framework.Editor.Core.EditStatements.UPDATE.ToString(),
                         Count = features.Count,
                         ObjectIds = features.Select(f => f.OID).ToArray()
                     };
                 });
}
