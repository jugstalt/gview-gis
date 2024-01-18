using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Models.Tree;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Data;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("14A1B7E6-6D73-4062-A7BF-5CB7600A6DB3")]
internal class AddDataToGroupLayer : ICartoTool
{
    public string Name => "Add Data";

    public string ToolTip => "Add spatial data to group";

    public ToolType ToolType => ToolType.Command;

    public string Icon => "basic:database-plus";

    public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

    public int SortOrder => 0;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        return scope.SelectedTocTreeNode is TocParentNode;
    }

    async public Task<bool> OnEvent(ICartoApplicationScopeService scope)
    {
        var groupLayer = (scope.SelectedTocTreeNode as TocParentNode)?.TocElement?.Layers?.FirstOrDefault() as IGroupLayer;

        if (groupLayer is null)
        {
            return false;
        }

        var model = await scope.ShowKnownDialog(
                                            KnownDialogs.ExplorerDialog,
                                            title: "Add Data",
                                            model: new ExplorerDialogModel()
                                            {
                                                Filters = new List<ExplorerDialogFilter> {
                                                    new OpenDataFilter()
                                                },
                                                Mode = ExploerDialogMode.Open
                                            });

        if (model == null)
        {
            return false;
        }

        var map = scope.Document.Map;
        bool firstLayer = map.MapElements!.Any() != true;
        var layersResult = await model.GetLayers(scope.GeoTransformer, map.Display.SpatialReference);

        foreach (var layer in layersResult.layers.OrderLayersByGeometryType().Where(l => l is Layer).Select(l => (Layer)l))
        {

            layer.GroupLayer = groupLayer;

            int pos = 1;
            if (map.TOC?.Elements != null)
            {
                var nextLayer = groupLayer.ChildLayer.FirstOrHigherIndexOfGeometryTypeOrder(layer);
                var nextTocElement = scope.Document.Map.TOC.Elements.FirstOrDefault(e => e.Layers != null && e.Layers.Contains(nextLayer));
                pos = scope.Document.Map.TOC.Elements.IndexOf(nextTocElement);
            }

            scope.Document.Map.AddLayer(layer, pos);
        }

        if (layersResult.layersExtent is not null)
        {
            if (firstLayer || layersResult.layersExtent.Intersects(map.Display.Envelope) == false)
            {
                await scope.EventBus.FireMapZoomToAsync(layersResult.layersExtent);
            }
        }
        await scope.EventBus.FireMapSettingsChangedAsync();

        return true;
    }
}
