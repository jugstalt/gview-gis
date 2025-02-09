﻿using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
using gView.Server.EndPoints.GeoJsonService.Extensions;
using gView.Server.Services.MapServer;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Linq;

namespace gView.Server.EndPoints.GeoJsonService;

public class FeaturesInsert : BaseApiEndpoint
{
    public FeaturesInsert()
         : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.EditFeatures}/{{id}}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.EditFeatures}/{{id}}"
                ],
                Handler,
                HttpMethod.Post
            )
    {

    }

    static private Delegate Handler => (
                HttpContext httpContext,
                [FromServices] LoginManager loginManagerService,
                [FromServices] MapServiceManager mapServerService,
                int id,
                string folder = "",
                string service = ""
            ) => HandleSecureAsync<EditFeaturesRequest>(httpContext, mapServerService, loginManagerService, folder, service,
                 async (mapService, identity, editRequest) =>
                 {
                     using var serviceMap = await mapServerService.Instance.GetServiceMapAsync(mapService);

                     var featureClass = serviceMap.CheckEditableStatement(id, Framework.Editor.Core.EditStatements.INSERT).GetFeatureClass(id);

                     var dataset = featureClass.Dataset;
                     var database = dataset?.Database as IFeatureUpdater;
                     if (database == null)
                     {
                         throw new MapServerException("Featureclass is not editable");
                     }

                     string sRefName = editRequest.CRS.ToSpatialReferenceName();
                     if (string.IsNullOrEmpty(sRefName))
                     {
                         throw new MapServerException("Invalid CRS: only name=epsg:... is allowed");
                     }

                     var geoJsonFeatureSref = SpatialReference.FromID(sRefName);

                     var features = editRequest.Features.GetFeatrues(featureClass, geoJsonFeatureSref);
                     if (features.Count() == 0)
                     {
                         throw new MapServerException("No features to add");
                     }

                     if (features.Count(f => f.OID > 0) > 0)
                     {
                         throw new MapServerException("Can't insert features with existing ObjectId");
                     }

                     if (features.Count(f => f.Shape is null) > 0)
                     {
                         throw new MapServerException("Insert features without geometry are not allowed");
                     }
                     features.GeometryMakeValid(serviceMap, featureClass);

                     if (!await database.Insert(featureClass, features.ToList()))
                     {
                         throw new MapServerException(database.LastErrorMessage);
                     }

                     return new EditFeaturesResponse()
                     {
                         Succeeded = true,
                         Statement = Framework.Editor.Core.EditStatements.INSERT.ToString(),
                         Count = features.Count()
                     };
                 });
}
