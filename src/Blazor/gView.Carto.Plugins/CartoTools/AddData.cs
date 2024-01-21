using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.DataExplorer.Razor.Components.Dialogs.Filters;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("7C9CA04B-7DFC-4028-8CEF-25D2A02272ED")]
internal class AddData : ICartoInitialButton
{
    public string Name => "Add Data";

    public string ToolTip => "Add spatial data to map";

    public string Icon => "basic:database-plus";

    public CartoToolTarget Target => CartoToolTarget.Map;

    public int SortOrder => 1;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope) => true;

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowKnownDialog(KnownDialogs.ExplorerDialog,
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

        foreach (var layer in layersResult.layers.OrderLayersByGeometryType())
        {
            scope.Document.Map.AddLayer(layer);
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
