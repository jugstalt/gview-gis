using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.Symbology;
using System;

namespace gView.Plugins.MapTools
{
    internal class MeasureGraphicsElement : IGraphicElement
    {
        private IPolygon _polygon;
        private SimpleLineSymbol _lineSymbol;
        private HatchSymbol _fillSymbol;
        private SimplePointSymbol _pointSymbol;
        private object lockThis = new object();
        private bool _stopped = true, _showArea = true, _dynamic = false;

        public MeasureGraphicsElement()
        {
            BeginNew();

            _lineSymbol = new SimpleLineSymbol();
            _lineSymbol.Color = GraphicsEngine.ArgbColor.Blue;
            _lineSymbol.Width = 2;
            _pointSymbol = new SimplePointSymbol();
            _pointSymbol.Color = GraphicsEngine.ArgbColor.Blue;
            _pointSymbol.Size = 4;
            _fillSymbol = new HatchSymbol();
        }

        public void BeginNew()
        {
            _polygon = new Polygon();
            _polygon.AddRing(new Ring());
            _stopped = false;
        }

        public void Stop()
        {
            _stopped = true;

            if (_moveAble != null) // MoveAble löschen
            {
                _moveAble = null;
                _polygon[0].RemovePoint(_polygon[0].PointCount - 1);
            }
        }

        public void Close()
        {
            if (_moveAble == null)
            {
                if (_polygon[0].PointCount > 2)
                {
                    _polygon[0].AddPoint(new Point(_polygon[0][0].X, _polygon[0][0].Y));
                }
            }
            else
            {
                if (_polygon[0].PointCount > 3)
                {
                    _moveAble.X = _polygon[0][0].X;
                    _moveAble.Y = _polygon[0][0].Y;
                    _moveAble = null;
                }
            }
        }

        public bool Stopped { get { return _stopped; } }

        public bool ShowArea
        {
            get { return _showArea; }
            set { _showArea = value; }
        }

        private IPoint _moveAble = null;
        public void AddPoint(double X, double Y)
        {
            if (_stopped)
            {
                BeginNew();
            }

            lock (lockThis)
            {
                if (_moveAble != null)
                {
                    _moveAble.X = X;
                    _moveAble.Y = Y;
                    _moveAble = null;
                }
                else
                {
                    _polygon[0].AddPoint(new Point(X, Y));
                }
            }
        }

        public IPoint MoveAble
        {
            get
            {
                if (_stopped || _polygon[0].PointCount == 0)
                {
                    return null;
                }

                lock (lockThis)
                {
                    if (_moveAble == null)
                    {
                        _polygon[0].AddPoint(new Point(0, 0));
                    }

                    return _moveAble = _polygon[0][_polygon[0].PointCount - 1];
                }
            }
        }

        public bool Dynamic
        {
            get { return _dynamic; }
            set { _dynamic = value; }
        }
        public double Length
        {
            get
            {
                //return _polygon[0].Length;

                gView.Framework.Geometry.Path path = new gView.Framework.Geometry.Path();
                for (int i = 0; i < _polygon[0].PointCount; i++)
                {
                    if (!_dynamic && _polygon[0][i] == _moveAble)
                    {
                        continue;
                    }

                    path.AddPoint(_polygon[0][i]);
                }
                return path.Length;
            }
        }
        public double SegmentLength
        {
            get
            {
                if (_moveAble == null || _polygon[0].PointCount < 2)
                {
                    return 0.0;
                }

                double dx = _moveAble.X - _polygon[0][_polygon[0].PointCount - 2].X;
                double dy = _moveAble.Y - _polygon[0][_polygon[0].PointCount - 2].Y;

                return Math.Sqrt(dx * dx + dy * dy);
            }
        }

        public double SegmentAngle
        {
            get
            {
                if (_moveAble == null || _polygon[0].PointCount < 2)
                {
                    return 0.0;
                }

                double dx = _moveAble.X - _polygon[0][_polygon[0].PointCount - 2].X;
                double dy = _moveAble.Y - _polygon[0][_polygon[0].PointCount - 2].Y;

                return Math.Atan2(dy, dx) * 180.0 / Math.PI;
            }
        }

        public double Area
        {
            get
            {
                //return _polygon[0].Area;

                Ring ring = new Ring();
                for (int i = 0; i < _polygon[0].PointCount; i++)
                {
                    if (!_dynamic && _polygon[0][i] == _moveAble)
                    {
                        continue;
                    }

                    ring.AddPoint(_polygon[0][i]);
                }
                return ring.Area;
            }
        }

        public int PointCount
        {
            get
            {
                return (_moveAble != null) ? _polygon[0].PointCount - 1 : _polygon[0].PointCount;
            }
        }

        #region IGraphicElement Member

        public void Draw(IDisplay display)
        {
            if (_lineSymbol != null && _polygon[0].PointCount > 1)
            {
                Polyline line = new Polyline();
                line.AddPath(_polygon[0]);
                display.Draw(_lineSymbol, line);
            }
            if (_pointSymbol != null)
            {
                for (int i = 0; i < _polygon[0].PointCount - ((_moveAble == null) ? 0 : 1); i++)
                {
                    display.Draw(_pointSymbol, _polygon[0][i]);
                }
            }
            if (_fillSymbol != null && _polygon[0].PointCount > 2 && ShowArea)
            {
                display.Draw(_fillSymbol, _polygon);
            }
        }

        #endregion
    }
}
