using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Geometry
{
    class Vector2D
    {
        double _x, _y;

        public Vector2D(double x, double y)
        {
            _x = x;
            _y = y;
        }
        public Vector2D(Vector2D vec)
            : this(vec._x, vec._y)
        {
        }

        public double Length
        {
            get
            {
                return Math.Sqrt((_x * _x + _y * _y));
            }
            set
            {
                Normalize();
                _x *= value;
                _y *= value;
            }
        }

        public void Normalize()
        {
            double len = Length;
            if (len == 0.0 || len == 1.0) return;
            _x /= len;
            _y /= len;
        }

        public void Inv()
        {
            _x = -_x;
            _y = -_y;
        }

        public double Angle
        {
            get
            {
                double angle = Math.Atan2(_y, _x);
                return (angle >= 0.0) ? angle : angle + 2.0 * Math.PI;
            }
        }

        public Vector2D PerpendicularVector
        {
            get
            {
                return new Vector2D(_y, -_x);
            }
        }

        public void Rotate(double angle)
        {
            double cos_a = Math.Cos(angle);
            double sin_a = Math.Sin(angle);

            double x = _x * cos_a - _y * sin_a;
            double y = _x * sin_a + _y * cos_a;

            _x = x;
            _y = y;
        }

        public double X
        {
            get { return _x; }
            set { _x = value; }
        }
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a._x + b._x, a._y + b._y);
        }
        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a._x - b._x, a._y - b._y);
        }
        public static Vector2D operator *(Vector2D a, Vector2D b)
        {
            return new Vector2D(a._x * b._x, a._y * b._y);
        }
        public static Vector2D operator *(Vector2D a, double fac)
        {
            return new Vector2D(a._x * fac, a._y * fac);
        }
        public static bool operator ==(Vector2D a, Vector2D b)
        {
            return (a._x == b._x && a._y == b._y);
        }

        public static bool operator !=(Vector2D a, Vector2D b)
        {
            return !(a == b);
        }
    }
}
