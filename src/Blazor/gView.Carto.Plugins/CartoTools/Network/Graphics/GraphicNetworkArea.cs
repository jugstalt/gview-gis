using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

/// <summary>The outline of the area a network search is restricted to (dashed, not filled).</summary>
class GraphicNetworkArea : IGraphicElement
{
    private readonly SimpleLineSymbol _symbol;
    private readonly IPolyline _outline;

    public GraphicNetworkArea(IPolygon polygon)
    {
        _symbol = new SimpleLineSymbol
        {
            Color = ArgbColor.FromArgb(220, 120, 40, 200),
            Width = 3,
            DashStyle = LineDashStyle.Dash
        };
        // the rings as lines: an outline that does not hide the map
        _outline = new gView.Framework.Geometry.Polyline();
        for (int i = 0; i < polygon.RingCount; i++)
        {
            var ring = polygon[i];
            var path = new gView.Framework.Geometry.Path();
            for (int j = 0; j < ring.PointCount; j++)
            {
                path.AddPoint(ring[j]);
            }
            if (ring.PointCount > 0)
            {
                path.AddPoint(ring[0]);
            }
            _outline.AddPath(path);
        }
    }

    public void Draw(IDisplay display) => display.Draw(_symbol, _outline);
}
