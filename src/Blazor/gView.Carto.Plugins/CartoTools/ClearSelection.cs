using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;

namespace gView.Carto.Plugins.CartoTools;

/// <summary>Clears the selection of all layers of the map</summary>
[RegisterPlugIn("C5E1B7A2-3F4D-4C8E-9A61-2D7B0E5F8A13")]
internal class ClearSelection : ICartoButton
{
    public string Name => "Clear Selection";

    public string ToolTip => "Clear the selection of all layers";

    public string Icon => "basic:trashcan-x";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 80;

    public bool IsVisible(ICartoApplicationScopeService scope) => true;

    public bool IsDisabled(ICartoApplicationScopeService scope)
        => !ClearSelectionHelper.HasSelection(scope.Document?.Map?.MapElements ?? []);

    public Task<bool> OnClick(ICartoApplicationScopeService scope)
        => ClearSelectionHelper.ClearAsync(scope, scope.Document?.Map?.MapElements ?? []);
}

/// <summary>Clears the selection of the layer(s) selected in the TOC</summary>
[RegisterPlugIn("8D2F4A6B-1C3E-4B7D-A5F9-6E0C2B8D4F71")]
internal class ClearLayerSelection : ICartoButton
{
    public string Name => "Clear Selection";

    public string ToolTip => "Clear the selection of this layer";

    public string Icon => "basic:trashcan-x";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 95;

    public bool IsVisible(ICartoApplicationScopeService scope)
        => ClearSelectionHelper.FeatureSelections(scope.SelectedTocTreeNode?.Value?.Layers ?? []).Any();

    public bool IsDisabled(ICartoApplicationScopeService scope)
        => !ClearSelectionHelper.HasSelection(scope.SelectedTocTreeNode?.Value?.Layers ?? []);

    public Task<bool> OnClick(ICartoApplicationScopeService scope)
        => ClearSelectionHelper.ClearAsync(scope, scope.SelectedTocTreeNode?.Value?.Layers ?? []);
}

internal static class ClearSelectionHelper
{
    public static IEnumerable<IFeatureSelection> FeatureSelections(IEnumerable<IDatasetElement> elements)
    {
        foreach (var element in elements)
        {
            if (element is IFeatureSelection selection)
            {
                yield return selection;
            }
            if (element is IGroupLayer groupLayer)
            {
                foreach (var child in FeatureSelections(groupLayer.ChildLayers))
                {
                    yield return child;
                }
            }
        }
    }

    public static bool HasSelection(IEnumerable<IDatasetElement> elements)
        => FeatureSelections(elements).Any(s => s.SelectionSet?.Count > 0);

    public static async Task<bool> ClearAsync(ICartoApplicationScopeService scope, IEnumerable<IDatasetElement> elements)
    {
        var cleared = 0;
        foreach (var selection in FeatureSelections(elements).Distinct().ToArray())
        {
            if (selection.SelectionSet?.Count > 0)
            {
                selection.ClearSelection();
                selection.FireSelectionChangedEvent();
                cleared++;
            }
        }

        if (cleared > 0)
        {
            await scope.EventBus.FireRefreshMapAsync(DrawPhase.Selection);
        }

        return cleared > 0;
    }
}
