using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

/// <summary>
/// The nodes or violations of one legend row of a network search result with one symbol. Draws only the points in the
/// map and one marker per spot on the screen, so hundreds of thousands (build problems) stay fast to draw.
/// </summary>
class GraphicNetworkResultPoints : IGraphicElement
{
    /// <summary>The size of a spot on the screen in pixels: further points there are left out.</summary>
    private const double Cell = 2;

    private readonly SimplePointSymbol _symbol;
    private readonly IReadOnlyList<IPoint> _points;

    public GraphicNetworkResultPoints(IReadOnlyList<IPoint> points, ArgbColor color, float size = 8)
    {
        _points = points;
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
        if (display?.Envelope is not IEnvelope envelope)
        {
            return;
        }
        // a little beyond the map, so markers at the edge are drawn whole
        double room = Math.Max(envelope.Width, envelope.Height) * 0.02;
        double minX = envelope.MinX - room, minY = envelope.MinY - room, maxX = envelope.MaxX + room, maxY = envelope.MaxY + room;

        var drawn = new HashSet<long>();
        foreach (var point in _points)
        {
            if (point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY)
            {
                continue;
            }
            double x = point.X, y = point.Y;
            display.World2Image(ref x, ref y);
            long cell = ((long)Math.Floor(x / Cell) << 32) ^ (uint)(int)Math.Floor(y / Cell);
            if (drawn.Add(cell))
            {
                display.Draw(_symbol, point);
            }
        }
    }
}
