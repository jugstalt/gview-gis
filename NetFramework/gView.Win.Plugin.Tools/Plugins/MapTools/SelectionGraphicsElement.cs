using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using System.Collections.Generic;

namespace gView.Plugins.MapTools
{
    internal class SelectionGraphicsElement : IGraphicElement
    {
        private gView.Plugins.MapTools.Controls.selectionMothode _methode;
        private List<IPoint> _points;
        private SimpleLineSymbol _lineSymbol;
        private SimpleFillSymbol _fillSymbol;
        private SimplePointSymbol _pointSymbol;

        public SelectionGraphicsElement()
        {
            _points = new List<IPoint>();

            _lineSymbol = new SimpleLineSymbol();
            _lineSymbol.Color = GraphicsEngine.ArgbColor.Blue;
            _lineSymbol.Width = 2;
            _pointSymbol = new SimplePointSymbol();
            _pointSymbol.Color = GraphicsEngine.ArgbColor.Blue;
            _pointSymbol.Size = 4;
            _fillSymbol = new SimpleFillSymbol();
            _fillSymbol.Color = GraphicsEngine.ArgbColor.FromArgb(100, 255, 255, 100);
            _fillSymbol.OutlineSymbol = _lineSymbol;
        }

        public gView.Plugins.MapTools.Controls.selectionMothode Methode
        {
            get { return _methode; }
            set { _methode = value; }
        }

        public void AddPoint(IPoint point)
        {
            _points.Add(point);
        }
        public void ClearPoints()
        {
            _points.Clear();
        }

        public IGeometry Geometry
        {
            get
            {
                switch (_methode)
                {
                    case gView.Plugins.MapTools.Controls.selectionMothode.Multipoint:
                        if (_points.Count == 0)
                        {
                            return null;
                        }

                        return new MultiPoint(_points);
                    case gView.Plugins.MapTools.Controls.selectionMothode.Polyline:
                        if (_points.Count < 2)
                        {
                            return null;
                        }

                        Polyline line = new Polyline();
                        line.AddPath(new gView.Framework.Geometry.Path(_points));
                        return line;
                    case gView.Plugins.MapTools.Controls.selectionMothode.Polygon:
                        if (_points.Count < 3)
                        {
                            return null;
                        }

                        Polygon poly = new Polygon();
                        poly.AddRing(new Ring(_points));
                        return poly;
                }
                return null;
            }
        }
        #region IGraphicElement Member

        public void Draw(IDisplay display)
        {
            if (_points.Count == 0)
            {
                return;
            }

            IGeometry geom = Geometry;

            if (_methode == gView.Plugins.MapTools.Controls.selectionMothode.Multipoint)
            {
                if (geom != null)
                {
                    display.Draw(_pointSymbol, geom);
                }
            }
            else if (_methode == gView.Plugins.MapTools.Controls.selectionMothode.Polyline)
            {
                if (geom != null)
                {
                    display.Draw(_lineSymbol, geom);
                }

                display.Draw(_pointSymbol, new MultiPoint(_points));
            }
            else if (_methode == gView.Plugins.MapTools.Controls.selectionMothode.Polygon)
            {
                if (geom != null)
                {
                    display.Draw(_fillSymbol, geom);
                }

                display.Draw(_pointSymbol, new MultiPoint(_points));
            }
        }

        #endregion
    }
}
