using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.Carto.Core.Services;

public class CartoDisplayService : IDisposable
{
    private readonly CartoEventBusService _eventBus;

    public CartoDisplayService(CartoEventBusService eventBus)
    {
        _eventBus = eventBus;

        _eventBus.OnStartRenderMapAsync += HandleStartRenderMap;
    }

    public double ScaleDominator { get; private set; }
    public int ImageWidth { get; private set; }
    public int ImageHeight { get; private set; }
    public IEnvelope MapBounds { get; private set; } = new Envelope();
    public double Dpi { get; set; }
    public ISpatialReference? SpatialReference { get; private set; }

    #region Event Handlers

    private Task HandleStartRenderMap(IMapRenderer mapRenderer)
    {
        if (mapRenderer.DrawPhase == DrawPhase.Geography
            && mapRenderer is IDisplay display)
        {
            ScaleDominator = display.MapScale;
            ImageWidth = display.ImageWidth;
            ImageHeight = display.ImageHeight;
            MapBounds = new Envelope(display.Envelope);
            Dpi = display.Dpi;
            SpatialReference = display.SpatialReference;
        }

        return Task.CompletedTask;
    }

    #endregion

    public void Dispose()
    {
        _eventBus.OnStartRenderMapAsync -= HandleStartRenderMap;
    }
}
