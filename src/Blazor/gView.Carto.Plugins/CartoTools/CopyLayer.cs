using gView.Blazor.Core.Extensions;
using gView.Carto.Core;
using gView.Carto.Core.Reflection;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Razor.Components.Dialogs;
using gView.Carto.Razor.Components.Dialogs.Models;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Cartography;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Cursors;
using gView.Framework.Data;
using gView.Framework.Data.Extensions;
using gView.Framework.Data.Filters;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("7A020F6D-9744-4355-AC18-8B31723064A4")]
[RestorePointAction(RestoreAction.SetRestorePointOnClick)]
internal class CopyLayer : ICartoButton
{
    public string ToolTip => "";

    public string Icon => "basic:copy";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 98;

    public string Name => "Copy Layer";

    public bool IsVisible(ICartoApplicationScopeService scope)
        => scope.SelectedTocTreeNode is not null;

    public bool IsDisabled(ICartoApplicationScopeService scope) => false;

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
        var copyElements = originalLayer.CreateCopy(map, newName: model.NewNamePattern);

        foreach (var copyElement in copyElements)
        {
            if (copyElements.IndexOf(copyElement) == 0)  // first shoud be positioned after original
            {
                int pos = -1;
                if (map.TOC?.Elements != null)
                {
                    var originalTocElement = scope.Document.Map.TOC.Elements.FirstOrDefault(e => e.Layers != null && e.Layers.Contains(originalLayer));
                    pos = scope.Document.Map.TOC.Elements.IndexOf(originalTocElement) + 1;
                }
                map.AddLayer(copyElement.layer, pos);
            }
            else
            {
                map.AddLayer(copyElement.layer);
            }

            var tocElement = map.TOC?.GetTOCElement(copyElement.layer);
            if(tocElement is not null)
            {
                tocElement.Name = copyElement.tocName;
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

                var copyElement = originalLayer.CreateCopy(map, newName: newName).FirstOrDefault();
                var copyFeatureLayer = copyElement.layer as IFeatureLayer;
                if (copyFeatureLayer is null) continue;

                copyFeatureLayer.FilterQuery = new QueryFilter() { WhereClause = $"{model.FilterField}={String.Format(fieldFormatString, fieldValue)}" };

                map.AddLayer(copyFeatureLayer, pos + counter);
                var tocElement = map.TOC?.GetTOCElement(copyFeatureLayer);
                if (tocElement is not null)
                {
                    tocElement.Name = copyElement.tocName;
                }

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