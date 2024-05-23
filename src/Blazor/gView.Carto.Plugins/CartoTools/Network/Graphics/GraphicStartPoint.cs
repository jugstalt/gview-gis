using gView.Framework.Core.Geometry;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

public class GraphicStartPoint : GraphicHotspotPoint
{
    public GraphicStartPoint(IPoint point)
        : base(point, "Start")
    {
    }
}
