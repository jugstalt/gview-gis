using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

/// <summary>A node or a rule violation of a network search result.</summary>
class GraphicNetworkResultNode : IGraphicElement
{
    private readonly SimplePointSymbol _symbol;
    private readonly IPoint _point;

    public GraphicNetworkResultNode(IPoint point, ArgbColor color, float size = 8)
    {
        _point = point;
        _symbol = new SimplePointSymbol
        {
            Color = color,
            OutlineColor = ArgbColor.White,
            Size = size,
            Marker = SimplePointSymbol.MarkerType.Circle
        };
    }

    public void Draw(IDisplay display)
    {
        if (display is not null && _point is not null)
        {
            display.Draw(_symbol, _point);
        }
    }
}

/// <summary>An edge of a network search result: the part of its line feature.</summary>
class GraphicNetworkResultEdge : IGraphicElement
{
    private readonly SimpleLineSymbol _symbol;
    private readonly IPolyline _polyline;

    public GraphicNetworkResultEdge(IPolyline polyline, ArgbColor color)
    {
        _polyline = polyline;
        _symbol = new SimpleLineSymbol { Color = color, Width = 5 };
    }

    public void Draw(IDisplay display)
    {
        if (display is not null && _polyline is not null)
        {
            display.Draw(_symbol, _polyline);
        }
    }
}
