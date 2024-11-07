using gView.Framework.Core.Geometry;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

public class GraphicTargetPoint : GraphicHotspotPoint
{
    public GraphicTargetPoint(IPoint point)
        : base(point, "Target")
    {
    }
}
