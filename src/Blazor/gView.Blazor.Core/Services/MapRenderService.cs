using gView.Blazor.Core.Exceptions;
using gView.Blazor.Core.Extensions;
using gView.Framework.Cartography;
using gView.Framework.Common;
using gView.Framework.Common.Extensions;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.GraphicsEngine.Abstraction;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace gView.Blazor.Core.Services;

public class MapRenderService : IDisposable
{
    private Map? _map;
    private readonly ConcurrentDictionary<int, IBitmap> _iBitmaps = new();
    private readonly ConcurrentDictionary<int, ICancelTracker> _cancelTrackers = new();

    private ISpatialReference? _mapControlSRef;
    private IEnvelope? _mapControlBounds;

    private readonly string _scopeId;
    private readonly GeoTransformerService _geoTransformer;

    public MapRenderService(GeoTransformerService geoTransformer)
    {
        _scopeId = Guid.NewGuid().ToString();
        _geoTransformer = geoTransformer;
    }

    #region IDispose

    public void Dispose()
    {
        CancelRender();
        DisposeMap();
        DisposeBitmaps();
    }

    #endregion

    public Map CreateMap(ISpatialReference mapSRef)
    {
        var map = new Map();
        map.Display.SpatialReference = mapSRef;
        map.MakeTransparent = true;

        return map;
    }

    public void InitMap(Map map, ISpatialReference? mapControlSRef = null)
    {
        CancelRender();

        _mapControlSRef = mapControlSRef ?? map.SpatialReference;

        #region Remember current extent and size

        var bounds = _map?.Envelope;
        var iWidth = _map?.ImageWidth;
        var iHeight = _map?.ImageHeight;
        var sRef = _map?.SpatialReference;

        #endregion

        if (_map != null)
        {
            _map.Dispose();
        }

        _map = map;

        #region Recover current extend and size

        if (bounds != null && iWidth.HasValue && iHeight.HasValue && sRef != null)
        {
            bounds = _geoTransformer.Transform(bounds, sRef, _map.SpatialReference).Envelope;
            _map.Envelope = bounds;
            _map.ImageWidth = iWidth.Value;
            _map.ImageHeight = iHeight.Value;
        }

        #endregion
    }

    public void SetBoundsAndSize(IEnvelope bounds, int imageWidth, int imageHeight)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);

        _map.Display.ImageWidth = imageWidth;
        _map.Display.ImageHeight = imageHeight;

        _mapControlBounds = bounds;

        if (_mapControlSRef != null && _map.SpatialReference != null)
        {
            _map.Display.ZoomTo(_geoTransformer.Transform(bounds, _mapControlSRef, _map.SpatialReference).Envelope);
        }
    }

    #region Properties

    public IEnvelope Bounds => _map?.Envelope ?? Envelope.Null();
    public int ImageWidth => _map?.ImageWidth ?? 0;
    public int ImageHeight => _map?.ImageHeight ?? 0;
    public bool BoundsIntialized() => _map != null &&
        _map?.Envelope != null &&
        !Envelope.IsNull(_map?.Envelope) &&
        _map.Envelope.Width > 0 &&
        _map.Envelope.Height > 0 &&
        ImageWidth > 0 && ImageHeight > 0;

    public ISpatialReference SpatialReference => _map?.SpatialReference ?? new SpatialReference("wgs:3857");

    #endregion

    public bool AddWebFeatureClass(IWebFeatureClass webFeatureClass, IWebServiceClass webServiceClass)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);
        _map.AddLayer(LayerFactory.Create(webServiceClass));
        _map.AddLayer(LayerFactory.Create(webFeatureClass, webServiceClass));
        return true;
    }

    public bool AddFeatureClass(IFeatureClass featureClass)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);
        _map.AddLayer(LayerFactory.Create(featureClass));
        return true;
    }

    public bool AddRasterClass(IRasterClass rasterClass)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);
        _map.AddLayer(LayerFactory.Create(rasterClass));
        return true;
    }

    public bool AddWebServiceClass(IWebServiceClass webServiceClass)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);
        _map.AddLayer(LayerFactory.Create(webServiceClass));
        return true;
    }

    async public Task<bool> AddFeatureDataset(IFeatureDataset featureDataset)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);

        foreach (var datasetElement in (await featureDataset.Elements()).OrEmpty())
        {
            if (datasetElement?.Class is IFeatureClass)
            {
                AddFeatureClass((IFeatureClass)datasetElement.Class);
            }
        }

        return true;
    }

    private void BeginRender(DrawPhase drawPhase)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);
        MapRendererNotIntializedException.ThrowIfNull(_mapControlSRef);
        MapRendererNotIntializedException.ThrowIfNull(_mapControlBounds);

        drawPhase.WithNormalized(phase =>
        {
            Task.Run(async () =>
            {
                MapRendererInstance? renderer = null;
                try
                {
                    renderer = await MapRendererInstance.CreateAsync(_map);

                    renderer.StartRefreshMap += HandleStartRefreshMap;
                    renderer.NewBitmap += HandleNewBitmapCreated;
                    renderer.DoRefreshMapView += HandleMakeMapViewRefresh;
                    renderer.StartDrawingLayer += HandleStartDrawingLayer;
                    renderer.FinishedDrawingLayer += HandleFinishedDrawingLayer;

                    var cancelTracker = _cancelTrackers.GetOrAdd(renderer.GetHashCode(), new CancelTracker());

                    renderer.SpatialReference = _mapControlSRef;
                    renderer.Display.ZoomTo(_mapControlBounds);

                    await renderer.RefreshMap(phase, cancelTracker);
                    await FireFinishedRenderMap(renderer);
                }
                finally
                {
                    if (renderer is not null)
                    {
                        renderer.StartRefreshMap -= HandleStartRefreshMap;
                        renderer.NewBitmap -= HandleNewBitmapCreated;
                        renderer.DoRefreshMapView -= HandleMakeMapViewRefresh;
                        renderer.StartDrawingLayer -= HandleStartDrawingLayer;
                        renderer.FinishedDrawingLayer -= HandleFinishedDrawingLayer;

                        _cancelTrackers.RemoveIfExists(renderer.GetHashCode());

                        renderer.Dispose();
                    }
                }
            });
        });
    }

    public void CancelRender()
    {
        _cancelTrackers.ForEach((hashCode, cancelTracker) =>
        {
            cancelTracker.Cancel();
        });
        _cancelTrackers.Clear();
    }

    public async Task Rerender(DrawPhase drawPhase = DrawPhase.All, int delay = 0)
    {
        if (delay <= 0)
        {
            CancelRender();
            BeginRender(drawPhase);
        }

        using (var mutex = await FuzzyMutexAsync.LockAsync(_scopeId))
        {
            if (mutex.WasBlocked == false)
            {
                await Task.Delay(delay);

                CancelRender();
                BeginRender(drawPhase);
            }
        }
    }

    #region Helper

    private byte[]? CreateBitmapSnapshot(int hashCode)
    {
        _iBitmaps.TryGetValue(hashCode, out IBitmap? iBitmap);
        if (iBitmap != null)
        {
            try
            {
                var ms = new MemoryStream();

                iBitmap.Save(ms, GraphicsEngine.ImageFormat.Png);
                return ms.ToArray();
            }
            catch { }
        }

        return null;
    }

    private void DisposeMap()
    {
        if (_map != null)
        {
            _map.Dispose();
            _map = null;
        }
    }

    private void DisposeBitmaps()
    {
        _iBitmaps.ForEach((hashCode, bitmap) =>
        {
            bitmap?.Dispose();
        });
        _iBitmaps.Clear();
    }

    #endregion

    #region Events

    public delegate Task StartRenderMap(IMapRenderer mapRenderer);
    public delegate Task FinishedRenderMap(IMapRenderer mapRenderer);
    public delegate Task RefreshMapImage(IMapRenderer mapRenderer, byte[] data);
    public delegate Task StartDrawingLayer(IMapRenderer mapRenderer, string layerName);
    public delegate Task FinsishedDrawingLayer(IMapRenderer mapRenderer, ITimeEvent timeEvent);

    public event StartRenderMap? OnStartRenderMap;
    public event FinishedRenderMap? OnFinishedRenderMap;
    public event RefreshMapImage? OnRefreshMapImage;
    public event StartDrawingLayer? OnStartDrawingLayer;
    public event FinsishedDrawingLayer? OnFinishedDrawingLayer;


    private Task FireStartRenderMap(IMapRenderer mapRenderer)
        => OnStartRenderMap?.Invoke(mapRenderer)
        ?? Task.CompletedTask;

    private Task FireFinishedRenderMap(IMapRenderer mapRenderer)
        => OnFinishedRenderMap?.Invoke(mapRenderer)
        ?? Task.CompletedTask;

    private Task FireRefreshMapImage(IMapRenderer mapRenderer, byte[]? data)
        => OnRefreshMapImage?.Invoke(mapRenderer, data ?? Array.Empty<byte>())
        ?? Task.CompletedTask;

    private Task FireStartDrawingLayer(IMapRenderer mapRenderer, string layerName)
        => OnStartDrawingLayer?.Invoke(mapRenderer, layerName)
        ?? Task.CompletedTask;

    private Task FireFinsishedDrawingLayer(IMapRenderer mapRenderer, ITimeEvent timeEvent)
        => OnFinishedDrawingLayer?.Invoke(mapRenderer, timeEvent)
        ?? Task.CompletedTask;

    #endregion

    #region EventHandlers

    private void HandleNewBitmapCreated(IMapRenderer renderer, IBitmap? bitmap)
    {
        if (renderer is not null)
        {
            _cancelTrackers.TryGetValue(renderer.GetHashCode(), out ICancelTracker? cancelTracker);

            if (bitmap is null)
            {
                _iBitmaps.RemoveIfExists(renderer.GetHashCode());
            }
            else
            {
                _iBitmaps.TryAdd(renderer.GetHashCode(), bitmap);
                if (cancelTracker?.Continue == true)
                {
                    FireRefreshMapImage(renderer, CreateBitmapSnapshot(renderer.GetHashCode()));
                }
            }
        }
    }

    private void HandleMakeMapViewRefresh(IMapRenderer renderer)
    {
        if (renderer is not null)
        {
            _cancelTrackers.TryGetValue(renderer.GetHashCode(), out ICancelTracker? cancelTracker);

            if (cancelTracker?.Continue == true)
            {
                if (renderer.DrawPhase == DrawPhase.Geography)
                    Console.WriteLine($"{renderer.GetHashCode()} {renderer.DrawPhase}");

                FireRefreshMapImage(renderer, CreateBitmapSnapshot(renderer.GetHashCode()));
            }
        }
    }

    private void HandleStartDrawingLayer(IMapRenderer renderer, string layerName)
    {
        if (renderer is not null)
        {
            FireStartDrawingLayer(renderer, layerName);
        }
    }

    private void HandleFinishedDrawingLayer(IMapRenderer renderer, ITimeEvent timeEvent)
    {
        if (renderer is not null)
        {
            FireFinsishedDrawingLayer(renderer, timeEvent);
        }
    }

    private void HandleStartRefreshMap(IMapRenderer renderer)
    {
        if (renderer is not null)
        {
            FireStartRenderMap(renderer);
        }
    }

    #endregion
}
