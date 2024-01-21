using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Models.Tree;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Data.Filters;
using gView.Framework.Core.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.Carto.Core.Services;
public class CartoEventBusService
{
    public event Func<bool, string, Task>? OnBusyStatusChangedAsync;
    public Task FireBusyStatusChangedAsync(bool isBusy, string statusText)
        => OnBusyStatusChangedAsync?.FireAsync(isBusy, statusText) ?? Task.CompletedTask;

    public event Func<TocTreeNode?, Task>? OnSelectedTocTreeNodeChangedAsync;
    public Task FireSelectedTocTreeNodeChangedAsync(TocTreeNode? selectedTocTreeNode)
        => OnSelectedTocTreeNodeChangedAsync?.FireAsync(selectedTocTreeNode) ?? Task.CompletedTask;

    public event Func<Task>? OnCloseTocInlineEditorsAsync;
    public Task FireCloseTocInlineEditorsAsync()
        => OnCloseTocInlineEditorsAsync?.FireAsync() ?? Task.CompletedTask;

    public event Func<ICartoDocument, Task>? OnCartoDocumentLoadedAsync;
    public Task FireCartoDocumentLoadedAsync(ICartoDocument cartoDocument)
        => OnCartoDocumentLoadedAsync?.FireAsync(cartoDocument) ?? Task.CompletedTask;

    public event Func<Task>? OnMapSettingsChangedAsync;
    public Task FireMapSettingsChangedAsync()
        => OnMapSettingsChangedAsync?.FireAsync() ?? Task.CompletedTask;

    public event Func<ILayer, ILayer, Task>? OnLayerSettingsChangedAsync;
    public Task FireLayerSettingsChangedAsync(ILayer oldLayer, ILayer newLayer)
        => OnLayerSettingsChangedAsync?.FireAsync(oldLayer, newLayer) ?? Task.CompletedTask;

    public event Func<Task>? OnRefreshContentTreeAsync;
    public Task FireRefreshContentTreeAsync()
        => OnRefreshContentTreeAsync?.FireAsync() ?? Task.CompletedTask;

    public event Func<DrawPhase, int, Task>? OnRefreshMapAsync;
    public Task FireRefreshMapAsync(DrawPhase drawPhase = DrawPhase.All, int delay = 0)
        => OnRefreshMapAsync?.FireAsync(drawPhase, delay) ?? Task.CompletedTask;

    public event Func<IEnvelope, Task>? OnMapZoomToAsync;
    public Task FireMapZoomToAsync(IEnvelope envelope)
        => OnMapZoomToAsync?.FireAsync(envelope) ?? Task.CompletedTask;

    public event Func<IMapRenderer, Task>? OnStartRenderMapAsync;
    public Task FireStartRenderMapAsync(IMapRenderer mapRenderer)
        => OnStartRenderMapAsync?.FireAsync(mapRenderer) ?? Task.CompletedTask;

    public event Func<IMapRenderer, Task>? OnFinishedRenderMapAsync;
    public Task FireFinishedRenderMapAsync(IMapRenderer mapRenderer)
        => OnFinishedRenderMapAsync?.FireAsync(mapRenderer) ?? Task.CompletedTask;

    public event Func<IMapRenderer, string, Task>? OnStartRendererLayerAsync;
    public Task FireStartRendererLayerAsync(IMapRenderer mapRenderer, string layerName)
        => OnStartRendererLayerAsync?.FireAsync(mapRenderer, layerName) ?? Task.CompletedTask;

    public event Func<IMapRenderer, string, ITimeEvent?, Task>? OnFinishedRendererLayerAsync;
    public Task FireFinishedRendererLayerAsync(IMapRenderer mapRenderer, string layerName, ITimeEvent? timeEvent)
        => OnFinishedRendererLayerAsync?.FireAsync(mapRenderer, layerName, timeEvent) ?? Task.CompletedTask;

    public event Func<ILayer, IQueryFilter?, Task>? OnHighlightFeaturesAsync;
    public Task FireHightlightFeaturesAsync(ILayer layer, IQueryFilter? filter)
        => OnHighlightFeaturesAsync?.FireAsync(layer, filter) ?? Task.CompletedTask;

    public event Func<IPoint, IPoint, Task>? OnMapMouseMoveAsync;
    public Task FireMapMouseMoveAsync(IPoint latLng, IPoint layerPoint)
        => OnMapMouseMoveAsync?.FireAsync(latLng, layerPoint) ?? Task.CompletedTask;

    public event Func<IPoint, Task>? OnMapClickAsync;
    public Task FireMapClickAsync(IPoint point)
        => OnMapClickAsync?.FireAsync(point) ?? Task.CompletedTask;

    public event Func<IEnvelope, Task>? OnMapBBoxAsync;
    public Task FireMapBBoxAsync(IEnvelope bbox)
        => OnMapBBoxAsync?.FireAsync(bbox) ?? Task.CompletedTask;

    public event Func<ILayer, Task>? OnShowDataTableAsync;
    public Task FireShowDataTableAsync(ILayer layer)
        => OnShowDataTableAsync?.FireAsync(layer) ?? Task.CompletedTask;

    public event Func<ILayer, ILayer?, Task>? OnRefreshDataTableAsync;
    public Task FireRefreshDataTableAsync(ILayer layer, ILayer? oldLayer)
        => OnRefreshDataTableAsync?.FireAsync(layer, oldLayer) ?? Task.CompletedTask;
}
