using gView.Framework.Core.Geometry;
using gView.Framework.Core.Common;
using System.Threading.Tasks;

namespace gView.Framework.Core.Carto
{
    public interface IMapRenderer
    {
        event NewBitmapEvent NewBitmap;
        event DoRefreshMapViewEvent DoRefreshMapView;
        event StartDrawingLayerEvent StartDrawingLayer;
        event FinishedDrawingLayerEvent FinishedDrawingLayer;
        event StartRefreshMapEvent StartRefreshMap;

        DrawPhase DrawPhase { get; }

        Task<bool> RefreshMap(DrawPhase drawPhase, ICancelTracker cancelTracker);

        void HighlightGeometry(IGeometry geometry, int milliseconds);

        void FireRefreshMapView(DrawPhase drawPhase, double suppressPeriode = 500/* ms */);
    }
}