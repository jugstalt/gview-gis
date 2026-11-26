#nullable enable

using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Data;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.MapServer;
using gView.Framework.Geometry;
using gView.GeoJsonService.DTOs;
using gView.Framework.Core.UI;
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
using System.Threading.Tasks;

namespace gView.Server.EndPoints.GeoJsonService;

/// <summary>
/// .../layers and .../layers/{layerId}: the layers with extent, description, edit operations, the properties with
/// their domains and the renderer. Properties need the right query or edit, extent and renderer the right map.
/// </summary>
public class GetLayers : BaseApiEndpoint
{
    public GetLayers()
        : base(
                [
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.GetLayers}/{{layerId:int}}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.GetLayers}/{{layerId:int}}",
                    $"{Routes.Base}/{Routes.GetServices}/{{folder}}/{{service}}/{Routes.GetLayers}",
                    $"{Routes.Base}/{Routes.GetServices}/{{service}}/{Routes.GetLayers}"
                ],
                Handler
            )
    {
    }

    private static Delegate Handler => (
            HttpContext httpContext,
            [FromServices] LoginManager loginManagerService,
            [FromServices] MapServiceManager mapServerService,
            [FromServices] ILogger<GetLayers> logger,
            int? layerId,
            string folder = "",
            string service = "") => HandleSecureAsync<GetLayersRequest>(
                    httpContext, mapServerService, loginManagerService, logger,
                    folder, service,
                    async (serviceRequestContext, mapService, identity, layersRequest) =>
     {
         using var map = (await mapServerService.Instance.GetServiceMapAsync(mapService)).ThrowIfNull(service);
         var accessTypes = await mapService.GetAccessTypes(identity);
         if (accessTypes == AccessTypes.None)
         {
             throw new MapServerException("Forbidden", 403);
         }

         var crs = layersRequest.CRS.ToSpatialReferenceOrDefault();
         var editLayers = map.GetModule<Plugins.Modules.EditorModule>();

         var layers = map.MapElements
             .OfType<ILayer>()
             .Where(layer => map.TOC.GetTOCElement(layer) is ITocElement element && !element.IsHidden())
             .Where(layer => layerId is null || layer.ID == layerId)
             .ToList();

         if (layerId is not null && layers.Count == 0)
         {
             throw new MapServerException($"Unknown layer {layerId}", 404);
         }

         var details = new List<LayerDetail>();
         foreach (var layer in layers)
         {
             details.Add(await LayerDetailAsync(map, layer, crs, accessTypes, editLayers));
         }

         return new GetLayersResponse() { Layers = details };
     });

    private static async Task<LayerDetail> LayerDetailAsync(IServiceMap map, ILayer layer, ISpatialReference crs, AccessTypes accessTypes,
                                                            Plugins.Modules.EditorModule? editModule)
    {
        var tocElement = map.TOC.GetTOCElement(layer);
        bool mapAccess = accessTypes.HasFlag(AccessTypes.Map);

        var detail = new LayerDetail()
        {
            Id = layer.ID.ToString(),
            ParentId = layer.GroupLayer?.ID.ToString(),
            Name = tocElement?.Name ?? layer.Title,
            LayerType = layer.LayerTypeName(),
            DefaultVisibility = mapAccess ? tocElement?.LayerVisible : null,
            MinScaleDenominator = mapAccess ? layer.MinimumScale : null,
            MaxScaleDenominator = mapAccess ? layer.MaximumScale : null,
            Description = map.GetLayerDescription(layer.ID),
            Copyright = map.GetLayerCopyrightText(layer.ID)
        };

        if (layer is not IFeatureLayer featureLayer)
        {
            return detail;
        }

        detail.GeometryType = featureLayer.LayerGeometryType.ToString();

        var editStatements = accessTypes.HasFlag(AccessTypes.Edit)
            ? editModule?.GetEditLayer(layer.ID)?.Statements ?? Framework.Editor.Core.EditStatements.NONE
            : Framework.Editor.Core.EditStatements.NONE;
        detail.EditOperations = new[]
            {
                (Framework.Editor.Core.EditStatements.INSERT, "insert"),
                (Framework.Editor.Core.EditStatements.UPDATE, "update"),
                (Framework.Editor.Core.EditStatements.DELETE, "delete")
            }
            .Where(s => editStatements.HasFlag(s.Item1))
            .Select(s => s.Item2)
            .ToArray();

        if (accessTypes.HasFlag(AccessTypes.Query) || accessTypes.HasFlag(AccessTypes.Edit))
        {
            bool editable = editStatements.HasFlag(Framework.Editor.Core.EditStatements.INSERT)
                            || editStatements.HasFlag(Framework.Editor.Core.EditStatements.UPDATE);
            var properties = new List<LayerProperty>();
            foreach (var field in featureLayer.Fields?.ToEnumerable().Where(f => f.type != FieldType.Shape) ?? [])
            {
                properties.Add(await field.ToLayerPropertyAsync(editable));
            }
            detail.Properties = properties;
        }

        if (mapAccess)
        {
            detail.Renderer = featureLayer.FeatureRenderer.ToLayerRenderer(map as IMap);

            if (featureLayer.FeatureClass?.Envelope is IEnvelope envelope)
            {
                var fromSRef = featureLayer.FeatureClass.SpatialReference ?? map.LayerDefaultSpatialReference;
                using var transformer = GeometricTransformerFactory.Create(map.Display?.DatumTransformations);
                if (fromSRef is not null && crs is not null)
                {
                    transformer.SetSpatialReferences(fromSRef, crs);
                    envelope = (transformer.Transform2D(envelope) as IGeometry)?.Envelope ?? envelope;
                }
                detail.Extent = envelope.ToBBox();
                detail.CRS = crs is not null ? CoordinateReferenceSystem.CreateByName(crs.Name) : null;
            }
        }

        return detail;
    }
}
