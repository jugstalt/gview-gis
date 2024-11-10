using gView.Blazor.Core.Extensions;
using gView.Carto.Core;
using gView.Carto.Core.Reflection;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Blazor.Models;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Data;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("1DAFAC8A-EA0F-4E3B-8542-26660F1E9CB3")]
[RestorePointAction(RestoreAction.SetRestorePointOnClick)]
internal class LayerJoins : ICartoButton
{
    public string Name => "Joins";

    public string ToolTip => "";

    public string Icon => "basic:merge";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 95;

    public void Dispose()
    {

    }

    public bool IsDisabled(ICartoApplicationScopeService scope) => false;

    public bool IsVisible(ICartoApplicationScopeService scope)
        => scope.SelectedTocTreeNode?
                 .Value?
                 .Layers?.Count == 1
        && scope.SelectedTocTreeNode
                .Value
                .Layers.First()
                .Class is IFeatureClass;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        await scope.EventBus.FireCloseTocInlineEditorsAsync();

        //var originalMap = scope.Document.Map as Map;
        //var clonedMap = originalMap?.Clone() as Map;

        //if (originalMap is null || clonedMap is null)
        //{
        //    return false;
        //}

        var originalLayer = scope.SelectedTocTreeNode?.Value?.Layers?.FirstOrDefault() as FeatureLayer;
        var clonedLayer = originalLayer?.Clone(scope.Document.Map) as FeatureLayer;

        if (originalLayer is null || clonedLayer is null)
        {
            return false;
        }

        clonedLayer.Class = originalLayer.Class;
        var tocElement = scope.Document.Map.TOC.GetTocElementByLayerId(originalLayer.ID);

        var model = await scope.ShowModalDialog(
            typeof(Razor.Components.Dialogs.LayerJoinsDialog),
            $"Joins: {tocElement?.Name}",
            new Razor.Components.Dialogs.Models.LayerJoinsModel()
            {
                Map = scope.Document.Map,
                FeatureLayer = clonedLayer
            },
            new ModalDialogOptions()
            {
                Width = ModalDialogWidth.ExtraExtraLarge,
                FullWidth = false
            });

        if (model is null) return false;

        originalLayer.Joins = clonedLayer.Joins;

        await scope.EventBus.FireRefreshMapAsync();

        return true;
    }
}
