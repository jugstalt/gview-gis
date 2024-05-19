using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.GraphicsEngine;

namespace gView.Carto.Plugins.CartoTools.Network.Graphics;

public class GraphicHotspotPoint : IGraphicElement
{
    private string _text = String.Empty;
    private IPoint _point;
    private SimpleLineSymbol _lineSymbol = new SimpleLineSymbol();
    private GlowingTextSymbol _textSymbol = new GlowingTextSymbol();

    public GraphicHotspotPoint(IPoint point, string text)
    {
        _point = point;
        _text = text;

        _lineSymbol.Color = ArgbColor.Red;
        _lineSymbol.Width = 3;
        _textSymbol.Color = ArgbColor.Black;
        _textSymbol.GlowingColor = ArgbColor.Yellow;
        _textSymbol.Smoothingmode = gView.Framework.Core.Symbology.SymbolSmoothing.AntiAlias;
        _textSymbol.Font = GraphicsEngine.Current.Engine.CreateFont("Arial", 10);
        _textSymbol.TextSymbolAlignment = gView.Framework.Core.Symbology.TextSymbolAlignment.leftAlignOver;
        _textSymbol.Text = _text;
    }

    #region IGraphicElement Member

    public void Draw(IDisplay display)
    {
        if (display == null || _point == null)
        {
            return;
        }

        Point p1 = new Point(_point.X, _point.Y + (20 * display.MapScale / (96 / 0.0254)));
        gView.Framework.Geometry.Path path = new ();
        path.AddPoint(_point);
        path.AddPoint(p1);
        Polyline pLine = new Polyline();
        pLine.AddPath(path);

        display.Draw(_lineSymbol, pLine);
        display.Draw(_textSymbol, p1);
    }

    #endregion
}
