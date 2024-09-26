using gView.Blazor.Core.Extensions;
using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;
using gView.GraphicsEngine;
using gView.Razor.Dialogs.Models;
using gView.Razor.Leaflet;
using SkiaSharp;
using static System.Formats.Asn1.AsnWriter;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("7A020F6D-9744-4355-AC18-8B31723064A4")]
internal class CopyLayer : ICartoButton
{
    public string ToolTip => "";

    public string Icon => "basic:copy";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 98;

    public string Name => "Copy Layer";

    public bool IsEnabled(ICartoApplicationScopeService scope)
        => scope.SelectedTocTreeNode is not null;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var map = scope.Document.Map as Map;
        var originalLayer = scope.SelectedTocTreeNode?
                                 .TocElement?
                                 .Layers?.FirstOrDefault() as Layer;

        if (map is null || originalLayer is null)
            return false;

        var model = await scope.ShowModalDialog(
               typeof(CopyLayerDialog),
               title: $"Copy Layer",
               model: new CopyLayerDialogModel
               {
                   Layer = originalLayer,
                   Map = map
               }
           );

        if (model is null) return false;

        return model.CopyMode switch
        {
            CopyLayerMode.SplitByFilter => await SplitByFilter(scope, model),
            _ => await Copy(scope, model)
        };
    }

    #region Helper

    async private Task<bool> Copy(ICartoApplicationScopeService scope, CopyLayerDialogModel model)
    {
        var map = model.Map!;
        var originalLayer = model.Layer;
        var newLayers = originalLayer.CreateCopy(map, newName: model.NewNamePattern);

        foreach (var newLayer in newLayers)
        {
            if (newLayers.IndexOf(newLayer) == 0)  // first shoud be positioned ater original
            {
                int pos = -1;
                if (map.TOC?.Elements != null)
                {
                    var originalTocElement = scope.Document.Map.TOC.Elements.FirstOrDefault(e => e.Layers != null && e.Layers.Contains(originalLayer));
                    pos = scope.Document.Map.TOC.Elements.IndexOf(originalTocElement) + 1;
                }
                map.AddLayer(newLayer, pos);
            }
            else
            {
                map.AddLayer(newLayer);
            }
        }

        await scope.EventBus.FireMapSettingsChangedAsync();

        return true;
    }

    async private Task<bool> SplitByFilter(ICartoApplicationScopeService scope, CopyLayerDialogModel model)
    {
        var map = model.Map!;
        var originalLayer = model.Layer as IFeatureLayer;

        if (originalLayer?.FeatureClass is null) return false;

        int pos = 0;
        if (map.TOC?.Elements != null)
        {
            var originalTocElement = scope.Document.Map.TOC.Elements.FirstOrDefault(e => e.Layers != null && e.Layers.Contains(originalLayer));
            pos = scope.Document.Map.TOC.Elements.IndexOf(originalTocElement) + 1;
        }

        var field = originalLayer.FeatureClass.FindField(model.FilterField);
        if (field is null) return false;

        var fieldFormatString = field.FieldValueFormatString();
        var distinctFilter = new DistinctFilter(model.FilterField) { OrderBy = model.FilterField };

        using (IFeatureCursor cursor = (IFeatureCursor)await originalLayer.FeatureClass.Search(distinctFilter))
        {
            int counter = 0;
            IFeature feature;
            while ((feature = await cursor.NextFeature()) is not null)
            {
                var fieldValue = feature.Fields[0].Value?.ToString() ?? "";

                var newName = (model.NewNamePattern ?? originalLayer.TocNameOrLayerTitle(map)).Replace($"[{model.FilterField}]", fieldValue);

                var newLayer = originalLayer.CreateCopy(map, newName: newName).FirstOrDefault() as IFeatureLayer;
                if (newLayer is null) continue;

                newLayer.FilterQuery = new QueryFilter() { WhereClause = $"{model.FilterField}={String.Format(fieldFormatString, fieldValue)}" };

                map.AddLayer(newLayer, pos + counter);

                counter++;
                if (counter >= 1000)
                    break;
            }
        }

        await scope.EventBus.FireMapSettingsChangedAsync();

        return true;
    }

    #endregion
}