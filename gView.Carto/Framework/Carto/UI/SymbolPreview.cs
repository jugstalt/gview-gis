using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using gView.Framework.system;

namespace gView.Framework.Carto.UI
{
    public class SymbolPreview
    {
        public SymbolPreview()
        {
        }

        private static IGeometry GeometryFromSymbol(ISymbol symbol, IEnvelope env)
        {
            IGeometry geometry = null;
            
            if (symbol is ISymbolCollection)
            {
                if (((ISymbolCollection)symbol).Symbols.Count > 0)
                {
                    geometry = GeometryFromSymbol(((ISymbolCollection)symbol).Symbols[0].Symbol, env);
                }
            }
            else if (symbol is IPointSymbol || symbol is ITextSymbol)
            {
                geometry = new gView.Framework.Geometry.Point((env.minx + env.maxx) / 2, Math.Abs(env.miny - env.maxy) / 2);
            }
            else if (symbol is ILineSymbol)
            {
                IPoint p1 = new gView.Framework.Geometry.Point(env.minx + 5, Math.Abs(env.miny - env.maxy) / 2);
                IPoint p2 = new gView.Framework.Geometry.Point(env.maxx - 5, Math.Abs(env.miny - env.maxy) / 2);
                IPath path = new Path();
                path.AddPoint(p1);
                path.AddPoint(p2);
                IPolyline line = new Polyline();
                line.AddPath(path);
                geometry = line;
            }
            else if (symbol is IFillSymbol)
            {
                IPoint p1 = new gView.Framework.Geometry.Point(env.minx + 5, 5);
                IPoint p2 = new gView.Framework.Geometry.Point(env.maxx - 5, 5);
                IPoint p3 = new gView.Framework.Geometry.Point(env.maxx - 5, Math.Abs(env.miny - env.maxy) - 5);
                IPoint p4 = new gView.Framework.Geometry.Point(env.minx + 5, Math.Abs(env.miny - env.maxy) - 5);
                IRing ring = new Ring();
                ring.AddPoint(p1);
                ring.AddPoint(p2);
                ring.AddPoint(p3);
                ring.AddPoint(p4);
                IPolygon polygon = new Polygon();
                polygon.AddRing(ring);
                geometry = polygon;
            }

            return geometry;
        }

        public static void Draw(System.Drawing.Graphics graphics, System.Drawing.Rectangle rectangle, ISymbol symbol)
        {
            Draw(graphics, rectangle, symbol, true);
        }
        public static void Draw(System.Drawing.Graphics graphics, System.Drawing.Rectangle rectangle, ISymbol symbol, bool cls)
        {
            if (symbol == null) return;

            Display display = new gView.Framework.Carto.Display();
            display.dpi = graphics.DpiX;

            IEnvelope env = display.Limit = new Envelope(0, rectangle.Top + rectangle.Height, rectangle.Left + rectangle.Width, 0);
            display.iWidth = (int)env.maxx;
            display.iHeight = (int)env.maxy;
            display.ZoomTo(env.minx, env.miny, env.maxx, env.maxy);

            IGeometry geometry = GeometryFromSymbol(symbol, new Envelope(rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top));
            if (geometry == null) return;

            if (PlugInManager.PlugInID(symbol).Equals(KnownObjects.Symbology_PolygonMask))
                return;

            display.GraphicsContext = graphics;

            if (cls)
            {
                using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
                {
                    graphics.FillRectangle(brush, rectangle);
                }
            }

            ISymbol sym = symbol.Clone(display) as ISymbol;
            if (sym != null)
            {
                if (sym is ITextSymbol)
                    ((ITextSymbol)sym).Text = "Text";
                sym.Draw(display, geometry);
                sym.Release();
            }
            else
            {
                symbol.Draw(display, geometry);
            }
        }
    }
}
