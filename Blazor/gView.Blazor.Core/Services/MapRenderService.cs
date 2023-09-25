using gView.Blazor.Core.Exceptions;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.GraphicsEngine.Abstraction;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using System;
using System.IO;
using System.Threading.Tasks;

namespace gView.Blazor.Core.Services;

public class MapRenderService : IDisposable
{
    private Map? _map;
    private IBitmap? _iBitmap;
    private MapRenderInstance2? _renderer;
    private ICancelTracker? _cancelTracker;

    private readonly GeoTransformerService _geoTransformer;

    public MapRenderService(GeoTransformerService geoTransformer)
    {
        _geoTransformer = geoTransformer;
    }

    #region IDispose

    public void Dispose()
    {
        ReleaseCurrentMapRendererInstance();
        DisposeMap();
        DisposeBitmap();
    }

    #endregion

    public void InitMap(ISpatialReference mapSRef)
    {
        ReleaseCurrentMapRendererInstance();

        #region Remember current extent and size

        var bounds = _map?.Envelope;
        var iWidth = _map?.iWidth;
        var iHeight = _map?.iHeight;
        var sRef = _map?.SpatialReference;

        #endregion

        if (_map != null)
        {
            _map.Dispose();
            _map.NewBitmap -= NewBitmapCreated;
            _map.DoRefreshMapView -= MakeMapViewRefresh;
        }

        _map = new Map();
        _map.Display.SpatialReference = mapSRef;
        _map.MakeTransparent = true;

        #region Recover current extend and size

        if (bounds != null && iWidth.HasValue && iHeight.HasValue && sRef != null)
        {
            bounds = _geoTransformer.Transform(bounds, sRef, mapSRef).Envelope;
            _map.Envelope = bounds;
            _map.iWidth = iWidth.Value;
            _map.iHeight = iHeight.Value;
        }

        #endregion

        _map.NewBitmap += NewBitmapCreated;
        _map.DoRefreshMapView += MakeMapViewRefresh;
    }

    public void SetBoundsAndSize(IEnvelope bounds, int imageWidth, int imageHeight)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);

        _map.Display.iWidth = imageWidth;
        _map.Display.iHeight = imageHeight;
        _map.Display.ZoomTo(bounds);
    }

    #region Properties

    public IEnvelope Bounds => _map?.Envelope ?? Envelope.Null();
    public int ImageWidth => _map?.iWidth ?? 0;
    public int ImageHeight=> _map?.iHeight ?? 0;
    public bool BoundsIntialized() => _map != null &&
        _map?.Envelope != null &&
        !Envelope.IsNull(_map?.Envelope) &&
        _map.Envelope.Width > 0 &&
        _map.Envelope.Height > 0 &&
        ImageWidth > 0 && ImageHeight > 0;

    public ISpatialReference SpatialReference => _map?.SpatialReference ?? new SpatialReference("wgs:3857");

    #endregion

    public bool AddWebFeatureClass(IWebFeatureClass webFeatureClass,IWebServiceClass webServiceClass)
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

    public void BeginRender()
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);

        Task.Run(async () =>
        {
            var mapRenderInstance = await CreateMapRendererInstance();
            await mapRenderInstance.RefreshMap(DrawPhase.All, _cancelTracker = new CancelTracker());
        });
    }

    public void CancelRender()
    {
        _cancelTracker?.Cancel();
    }

    #region Helper

    async private Task<MapRenderInstance2> CreateMapRendererInstance()
    {
        ReleaseCurrentMapRendererInstance();

        var mapRendererInstance = await MapRenderInstance2.CreateAsync(_map);

        mapRendererInstance.NewBitmap += NewBitmapCreated;
        mapRendererInstance.DoRefreshMapView += MakeMapViewRefresh;

        return mapRendererInstance;
    }

    private void ReleaseCurrentMapRendererInstance()
    {
        if (_cancelTracker != null)
        {
            _cancelTracker.Cancel();
        }

        if (_renderer != null)
        {
            _renderer.NewBitmap -= NewBitmapCreated;
            _renderer.DoRefreshMapView -= MakeMapViewRefresh;

            _renderer = null;
        }
    }

    private byte[]? CreateBitmapSnapshot()
    {
        if (_iBitmap != null)
        {
            try
            {
                var ms = new MemoryStream();

                _iBitmap.Save(ms, GraphicsEngine.ImageFormat.Png);
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

    private void DisposeBitmap()
    {
        if (_iBitmap != null)
        {
            _iBitmap.Dispose();
            _iBitmap = null;
        }
    }

    #endregion

    #region Events

    public delegate Task RefreshMapImage(byte[] data);
    public event RefreshMapImage? OnRefreshMapImage;

    public void FireRefreshMapImage(byte[]? data)
        => OnRefreshMapImage?.Invoke(data ?? Array.Empty<byte>());

    #endregion

    #region EventHandlers

    private void NewBitmapCreated(IBitmap bitmap)
    {
        _iBitmap = bitmap;

        FireRefreshMapImage(CreateBitmapSnapshot());
    }

    private void MakeMapViewRefresh()
    {
        FireRefreshMapImage(CreateBitmapSnapshot());
    }

    #endregion
}
