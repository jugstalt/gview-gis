using gView.Blazor.Core.Exceptions;
using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using gView.GraphicsEngine.Abstraction;
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

    public MapRenderService() { }

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

        if (_map != null)
        {
            _map.Dispose();
        }

        _map = new Map();
        _map.Display.SpatialReference = mapSRef;
        _map.MakeTransparent = true;

        _map.NewBitmap += NewBitmapCreated;
        _map.DoRefreshMapView += MakeMapViewRefresh;
    }

    public void SetBoundsAndSize(IEnvelope bounds, int imageWidth, int imageHeight)
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);

        _map!.Display.iWidth = imageWidth;
        _map!.Display.iHeight=imageHeight;
        _map!.Display.ZoomTo(bounds);
    }

    public void AddFeatureClass(IFeatureClass featureClass) 
    {
        MapRendererNotIntializedException.ThrowIfNull(_map);

        _map!.AddLayer(LayerFactory.Create(featureClass));
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
        if(_cancelTracker != null)
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
        if (_map != null )
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
        => OnRefreshMapImage?.Invoke(data); 

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
