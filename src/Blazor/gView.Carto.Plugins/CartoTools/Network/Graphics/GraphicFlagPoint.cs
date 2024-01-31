using gView.Framework.Core.Geometry;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

public class GraphicFlagPoint : GraphicHotspotPoint
{
    public GraphicFlagPoint(IPoint point, string text)
        : base(point, text)
    {
    }
}
