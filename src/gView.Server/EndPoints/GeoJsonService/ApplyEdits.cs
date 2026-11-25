#nullable enable

using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.Framework.Editor.Core;
using gView.GeoJsonService.DTOs;
using gView.Interoperability.Extensions.Editing;
using gView.Interoperability.OgcApi;
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
using System.Text.Json;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.GeoJsonService;

/// <summary>
/// .../applyEdits: adds, updates and deletes of several layers in one transaction (all or nothing), so all layers must
/// be in one database. Updates and deletes with a version are only applied if the feature still has it (412).
/// The version is the entity tag of OGC API - Features (the same value for both APIs).
/// </summary>
public class ApplyEdits : BaseApiEndpoint
{
    public ApplyEdits()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.ApplyEdits}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.ApplyEdits}"
                ],
                Handler,
                HttpMethod.Post
            )
    {
    }

    private sealed record LayerEdit(
        int LayerId,
        FeatureEditService Service,
        List<IFeature> Adds,
        List<IFeature> Updates,
        List<string?> UpdateVersions,
        List<(int Oid, string? Version)> Deletes);

    private static Delegate Handler => (
            HttpContext httpContext,
            [FromServices] LoginManager loginManagerService,
            [FromServices] MapServiceManager mapServerService,
            [FromServices] ILogger<ApplyEdits> logger,
            string folder = "",
            string service = "") => HandleSecureAsync<ApplyEditsRequest>(
                    httpContext, mapServerService, loginManagerService, logger,
                    folder, service,
                    async (serviceRequestContext, mapService, identity, request) =>
     {
         using var serviceMap = (await mapServerService.Instance.GetServiceMapAsync(mapService)).ThrowIfNull(service);

         var requestEdits = request.Edits?.ToList() ?? [];
         if (requestEdits.Count == 0)
         {
             throw new MapServerException("applyEdits: no edits");
         }

         var crs = request.CRS.ToSpatialReferenceOrDefault();
         var edits = new List<LayerEdit>();

         #region Check and prepare everything before the first edit

         foreach (var requestEdit in requestEdits)
         {
             int layerId = int.TryParse(requestEdit.LayerId, out var id)
                 ? id
                 : throw new MapServerException($"applyEdits: invalid layerId '{requestEdit.LayerId}'");
             if (edits.Any(e => e.LayerId == layerId))
             {
                 throw new MapServerException($"applyEdits: layer {layerId} more than once, put all its edits in one entry");
             }

             var editService = FeatureEditService.Create(serviceMap, layerId);
             var updateDtos = requestEdit.Updates?.ToList() ?? [];
             var edit = new LayerEdit(
                 layerId,
                 editService,
                 (requestEdit.Adds ?? []).GetFeatrues(editService.FeatureClass, crs, serviceMap.Display?.DatumTransformations).ToList(),
                 updateDtos.GetFeatrues(editService.FeatureClass, crs, serviceMap.Display?.DatumTransformations).ToList(),
                 updateDtos.Select(f => f.Version).ToList(),
                 (requestEdit.Deletes ?? []).Select(DeleteOf).ToList());

             if (edit.Adds.Count > 0)
             {
                 editService.CheckStatement(EditStatements.INSERT);
                 editService.PrepareInserts(edit.Adds);
             }
             if (edit.Updates.Count > 0)
             {
                 editService.CheckStatement(EditStatements.UPDATE);
                 editService.PrepareUpdates(edit.Updates);
                 await editService.CheckObjectsInLayer(edit.Updates.Select(f => f.OID));
             }
             if (edit.Deletes.Count > 0)
             {
                 editService.CheckStatement(EditStatements.DELETE);
                 await editService.CheckObjectsInLayer(edit.Deletes.Select(d => d.Oid));
             }

             edits.Add(edit);
         }

         if (edits.Select(e => e.Service.Database).Distinct().Count() > 1)
         {
             throw new MapServerException("applyEdits: all layers must be in one database (all or nothing); send one request per database");
         }

         #endregion

         #region Versions: optimistic locking

         var ogcApi = new OgcApiFeatureService(serviceMap, String.Empty);
         foreach (var edit in edits)
         {
             var expected = edit.Updates.Select((f, i) => (f.OID, Version: edit.UpdateVersions[i]))
                 .Concat(edit.Deletes.Select(d => (OID: d.Oid, d.Version)))
                 .Where(v => !String.IsNullOrEmpty(v.Version));

             foreach (var (oid, version) in expected)
             {
                 if (await ogcApi.FeatureETagAsync(edit.LayerId, oid) != Quoted(version!))
                 {
                     throw new MapServerException($"applyEdits: the feature {oid} of layer {edit.LayerId} has been changed in the meantime (version)", 412);
                 }
             }
         }

         #endregion

         #region Execute: one session for all layers

         var session = await edits[0].Service.BeginEditSession();
         if (session is null && edits.Count > 1)
         {
             throw new MapServerException("applyEdits: the database can't run edits of several layers in one transaction; send one request per layer");
         }

         async Task<bool> Run(IFeatureEditSession? editSession)
         {
             foreach (var edit in edits)
             {
                 if (edit.Adds.Count > 0 && !await edit.Service.Insert(edit.Adds, editSession)) return false;
                 if (edit.Updates.Count > 0 && !await edit.Service.Update(edit.Updates, editSession)) return false;
                 foreach (var delete in edit.Deletes)
                 {
                     if (!await edit.Service.Delete(delete.Oid, editSession)) return false;
                 }
             }
             return true;
         }

         if (session is null)
         {
             if (!await Run(null))
             {
                 throw new MapServerException(edits[0].Service.LastErrorMessage() ?? "applyEdits failed");
             }
         }
         else
         {
             await using (session)
             {
                 if (!await Run(session) || !await session.Commit())
                 {
                     throw new MapServerException(session.LastErrorMessage ?? "applyEdits failed: nothing was changed");
                 }
             }
         }

         #endregion

         var results = new List<LayerEditsResult>();
         foreach (var edit in edits)
         {
             async Task<EditedFeature[]> Edited(IEnumerable<IFeature> features)
             {
                 var edited = new List<EditedFeature>();
                 foreach (var feature in features)
                 {
                     edited.Add(new EditedFeature()
                     {
                         Oid = feature.OID,
                         Version = feature.OID > 0 ? await ogcApi.FeatureETagAsync(edit.LayerId, feature.OID) : null
                     });
                 }
                 return edited.ToArray();
             }

             results.Add(new LayerEditsResult()
             {
                 LayerId = edit.LayerId.ToString(),
                 Adds = await Edited(edit.Adds),
                 Updates = await Edited(edit.Updates),
                 Deletes = edit.Deletes.Select(d => d.Oid).ToArray()
             });
         }

         return new ApplyEditsResponse() { Succeeded = true, Results = results };
     });

    /// <summary>An id, or { "oid": 1, "version": "..." }</summary>
    private static (int Oid, string? Version) DeleteOf(object delete)
        => delete is JsonElement { ValueKind: JsonValueKind.Object } json
            ? (json.TryGetProperty("oid", out var oid) ? ((object)oid).ToObjectId() : throw new MapServerException("applyEdits: a delete needs oid"),
               json.TryGetProperty("version", out var version) && version.ValueKind == JsonValueKind.String ? version.GetString() : null)
            : (delete.ToObjectId(), null);

    /// <summary>A version with or without the quotes of the entity tag</summary>
    private static string Quoted(string version) => version.StartsWith('"') ? version : $"\"{version}\"";
}
