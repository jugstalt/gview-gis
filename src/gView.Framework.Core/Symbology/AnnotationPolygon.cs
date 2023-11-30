using System;

namespace gView.Framework.Core.Symbology
{
    public class AnnotationPolygon : IAnnotationPolygonCollision
    {
        private float _x1, _y1, _width, _height;
        private double _angle = 0.0;
        private double cos_a = 1.0, sin_a = 0.0;
        private GraphicsEngine.CanvasPointF[] _points = null;

        public AnnotationPolygon(float x1, float y1, float width, float height)
        {
            _x1 = x1;
            _y1 = y1;
            _width = width;
            _height = height;
        }

        public void Rotate(float x0, float y0, double angle)
        {
            _angle = angle;

            if (_angle != 0.0)
            {
                cos_a = Math.Cos(_angle * Math.PI / 180.0);
                sin_a = Math.Sin(_angle * Math.PI / 180.0);

                _x1 -= x0;
                _y1 -= y0;

                float x = (float)(_x1 * cos_a - _y1 * sin_a);
                float y = (float)(_x1 * sin_a + _y1 * cos_a);

                _x1 = x + x0;
                _y1 = y + y0;
            }
            else
            {
                cos_a = 1.0;
                sin_a = 0.0;
            }
        }

        public AnnotationPolygonEnvelope Envelope
        {
            get
            {
                float x2 = _x1 + (float)(cos_a * _width - sin_a * _height);
                float y2 = _y1 + (float)(sin_a * _width + cos_a * _height);

                AnnotationPolygonEnvelope poly = new AnnotationPolygonEnvelope(_x1, _y1, x2, y2);

                x2 = _x1 + (float)(cos_a * _width);
                y2 = _y1 + (float)(sin_a * _width);
                poly.Append(x2, y2);

                x2 = _x1 + (float)(-sin_a * _height);
                y2 = _y1 + (float)(cos_a * _height);
                poly.Append(x2, y2);

                return poly;
            }
        }

        public bool Contains(float x, float y)
        {
            float lx = x - _x1;
            float ly = y - _y1;

            float wx = (float)(cos_a * lx + sin_a * ly);
            float wy = (float)(-sin_a * lx + cos_a * ly);

            if (wx >= 0 && wx <= _width &&
                wy >= 0 && wy <= _height)
            {
                return true;
            }

            return false;
        }

        public GraphicsEngine.CanvasPointF[] ToCoords()
        {
            GraphicsEngine.CanvasPointF[] points = new GraphicsEngine.CanvasPointF[4];
            points[0] = new GraphicsEngine.CanvasPointF(_x1, _y1);
            points[1] = new GraphicsEngine.CanvasPointF(points[0].X + (float)(cos_a * _width), points[0].Y + (float)(sin_a * _width));
            points[2] = new GraphicsEngine.CanvasPointF(points[1].X + (float)(-sin_a * _height), points[1].Y + (float)(cos_a * _height));
            points[3] = new GraphicsEngine.CanvasPointF(points[0].X + (float)(-sin_a * _height), points[0].Y + (float)(cos_a * _height));

            return points;
        }

        public bool CheckCollision(IAnnotationPolygonCollision cand)
        {
            if (cand is AnnotationPolygon)
            {
                AnnotationPolygon lp = (AnnotationPolygon)cand;

                if (_points == null)
                {
                    _points = ToCoords();
                }

                if (lp._points == null)
                {
                    lp._points = lp.ToCoords();
                }

                if (HasSeperateLine(this, lp))
                {
                    return false;
                }

                if (HasSeperateLine(lp, this))
                {
                    return false;
                }

                return true;
            }
            else if (cand is AnnotationPolygonCollection)
            {
                foreach (IAnnotationPolygonCollision child in (AnnotationPolygonCollection)cand)
                {
                    if (CheckCollision(child))
                    {
                        return true;
                    }
                }
            }
            return false;

        }

        private static bool HasSeperateLine(AnnotationPolygon tester, AnnotationPolygon cand)
        {
            for (int i = 1; i <= tester._points.Length; i++)
            {
                GraphicsEngine.CanvasPointF p1 = tester[i];
                Vector2dF ortho = new Vector2dF(p1, tester._points[i - 1]);
                ortho.ToOrtho();
                ortho.Normalize();

                float t_min = 0f, t_max = 0f, c_min = 0f, c_max = 0f;
                MinMaxAreaForOrhtoSepLine(p1, ortho, tester, ref t_min, ref t_max);
                MinMaxAreaForOrhtoSepLine(p1, ortho, cand, ref c_min, ref c_max);

                if (t_min <= c_max && t_max <= c_min ||
                    c_min <= t_max && c_max <= t_min)
                {
                    return true;
                }
            }

            return false;
        }

        private static void MinMaxAreaForOrhtoSepLine(GraphicsEngine.CanvasPointF p1, Vector2dF ortho, AnnotationPolygon lp, ref float min, ref float max)
        {
            for (int j = 0; j < lp._points.Length; j++)
            {
                Vector2dF rc = new Vector2dF(lp[j], p1);
                float prod = ortho.DotProduct(rc);
                if (j == 0)
                {
                    min = max = prod;
                }
                else
                {
                    min = Math.Min(min, prod);
                    max = Math.Max(max, prod);
                }
            }
        }

        public GraphicsEngine.CanvasPointF this[int index]
        {
            get
            {
                if (_points == null)
                {
                    _points = ToCoords();
                }

                if (index < 0 || index >= _points.Length)
                {
                    return _points[0];
                }

                return _points[index];
            }
        }

        public float X1 { get { return _x1; } set { _x1 = value; } }
        public float Y1 { get { return _y1; } set { _y1 = value; } }
        public double Angle { get { return _angle; } set { _angle = value; } }

        public GraphicsEngine.CanvasPointF CenterPoint
        {
            get
            {
                float cx = 0f, cy = 0f;
                foreach (GraphicsEngine.CanvasPointF point in ToCoords())
                {
                    cx += point.X / 4f;
                    cy += point.Y / 4f;
                }
                return new GraphicsEngine.CanvasPointF(cx, cy);
            }
        }

        #region Vector Helper Classes
        private class Vector2dF
        {
            float _x, _y;

            public Vector2dF(GraphicsEngine.CanvasPointF p1, GraphicsEngine.CanvasPointF p0)
            {
                _x = p1.X - p0.X;
                _y = p1.Y - p0.Y;
            }

            public void ToOrtho()
            {
                float x = _x;
                _x = -_y;
                _y = x;
            }

            public void Normalize()
            {
                float l = (float)Math.Sqrt(_x * _x + _y * _y);
                _x /= l;
                _y /= l;
            }

            public float DotProduct(Vector2dF v)
            {
                return _x * v._x + _y * v._y;
            }
        }
        #endregion
    }
}
