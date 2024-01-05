using gView.Carto.Core.Abstractions;
using gView.Carto.Core.Extensions;
using gView.Carto.Core.Models.Tree;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
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

    public event Func<IMapRenderer, Task>? OnStartRenderMap;
    public Task FireStartRenderMap(IMapRenderer mapRenderer)
        => OnStartRenderMap?.FireAsync(mapRenderer) ?? Task.CompletedTask;

    public event Func<IMapRenderer, Task>? OnFinishedRenderMap;
    public Task FireFinishedRenderMap(IMapRenderer mapRenderer)
        => OnFinishedRenderMap?.FireAsync(mapRenderer) ?? Task.CompletedTask;

    public event Func<IMapRenderer, string, Task>? OnStartRendererLayer;
    public Task FireStartRendererLayer(IMapRenderer mapRenderer, string layerName)
        => OnStartRendererLayer?.FireAsync(mapRenderer, layerName) ?? Task.CompletedTask;

    public event Func<IMapRenderer, string, ITimeEvent?, Task>? OnFinishedRendererLayer;
    public Task FireFinishedRendererLayer(IMapRenderer mapRenderer, string layerName, ITimeEvent? timeEvent)
        => OnFinishedRendererLayer?.FireAsync(mapRenderer, layerName, timeEvent) ?? Task.CompletedTask;

    public event Func<IPoint, IPoint, Task>? OnMapMouseMove;
    public Task FireMapMouseMove(IPoint latLong, IPoint layerPoint)
        => OnMapMouseMove?.FireAsync(latLong, layerPoint) ?? Task.CompletedTask;

    public event Func<ILayer, Task>? OnShowDataTable;
    public Task FireShowDataTable(ILayer layer)
        => OnShowDataTable?.FireAsync(layer) ?? Task.CompletedTask;

    public event Func<ILayer, ILayer?, Task>? OnRefreshDataTable;
    public Task FireRefreshDataTable(ILayer layer, ILayer? oldLayer)
        => OnRefreshDataTable?.FireAsync(layer, oldLayer) ?? Task.CompletedTask;
}
