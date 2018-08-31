using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.LinAlg
{
    public class Vector3d
    {
        private double[] _v = new double[3];

        public Vector3d() { }
        public Vector3d(double[] v)
        {
            if (v == null) return;
            for (int i = 0; i < Math.Min(v.Length, 3); i++)
            {
                _v[i] = v[i];
            }
        }
        public Vector3d(double v0, double v1, double v2)
        {
            _v[0] = v0;
            _v[1] = v1;
            _v[2] = v2;
        }

        public double VectorNorm
        {
            get
            {
                double nn = _v[0] * _v[0] + _v[1] * _v[1] + _v[2] * _v[2];
                if (nn == 0.0) return 0.0;

                return Math.Sqrt(nn);
            }
        }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index > 2) return 0.0;
                return _v[index];
            }
            set
            {
                if (index < 0 || index > 2) return;
                _v[index] = value;
            }
        }

        public void Normalize()
        {
            double n = this.VectorNorm;
            if (n == 0.0) return;

            _v[0] /= n;
            _v[1] /= n;
            _v[2] /= n;
        }

        public Vector3d Product(Vector3d vec)
        {
            if (vec == null) return null;

            Vector3d x = new Vector3d();

            x._v[0] = _v[1] * vec._v[2] - _v[2] * vec._v[1];
            x._v[1] = _v[2] * vec._v[0] - _v[0] * vec._v[2];
            x._v[2] = _v[0] * vec._v[1] - _v[1] * vec._v[0];
            return x;
        }

        public static double operator *(Vector3d v1, Vector3d v2)
        {
            if (v1 == null || v2 == null) return 0.0;

            return v1._v[0] * v2._v[0] +
                   v1._v[1] * v2._v[1] +
                   v1._v[2] * v2._v[2];
        }

        public static Vector3d operator *(double f, Vector3d v1)
        {
            if (v1 == null) return null;

            return new Vector3d(v1._v[0] * f, v1._v[1] * f, v1._v[2] * f);
        }

        public static Vector3d operator +(Vector3d v1, Vector3d v2)
        {
            if (v1 == null) return v2;
            if (v2 == null) return v1;

            return new Vector3d(
                v1._v[0] + v2._v[0],
                v1._v[1] + v2._v[1],
                v1._v[2] + v2._v[2]);
        }

        public static Vector3d operator -(Vector3d v1, Vector3d v2)
        {
            if (v1 == null) return v2;
            if (v2 == null) return v1;

            return new Vector3d(
                v1._v[0] - v2._v[0],
                v1._v[1] - v2._v[1],
                v1._v[2] - v2._v[2]);
        }
        public static Vector3d operator %(Vector3d v1, Vector3d v2)
        {
            if (v1 == null) return null;
            return v1.Product(v2);
        }

        public double[] ToArray()
        {
            return new double[] { _v[0], _v[1], _v[2] };
        }
    }

    public class Vector2d
    {
        private double[] _v = new double[2];

        public Vector2d() { }
        public Vector2d(double[] v)
        {
            if (v == null) return;
            for (int i = 0; i < Math.Min(v.Length, 2); i++)
            {
                _v[i] = v[i];
            }
        }
        public Vector2d(double v0, double v1)
        {
            _v[0] = v0;
            _v[1] = v1;
        }

        public double VectorNorm
        {
            get
            {
                double nn = _v[0] * _v[0] + _v[1] * _v[1];
                if (nn == 0.0) return 0.0;

                return Math.Sqrt(nn);
            }
        }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index > 1) return 0.0;
                return _v[index];
            }
            set
            {
                if (index < 0 || index > 1) return;
                _v[index] = value;
            }
        }

        public void Normalize()
        {
            double n = this.VectorNorm;
            if (n == 0.0) return;

            _v[0] /= n;
            _v[1] /= n;
        }

        public double Angle
        {
            get
            {
                double angle = Math.Atan2(_v[1], _v[0]);
                if (angle < 0.0) angle += 2.0 * Math.PI;

                return angle;
            }
        }

        public static Vector2d operator *(double f, Vector2d v1)
        {
            if (v1 == null) return null;

            return new Vector2d(v1._v[0] * f, v1._v[1] * f);
        }

        // Inneres Produkt
        public static double operator *(Vector2d v1, Vector2d v2)
        {
            return v1._v[0] * v2._v[0] +
                   v1._v[1] * v2._v[1];
        }

        public static double operator %(Vector2d v1, Vector2d v2)
        {
            return v1[0] * v2[1] - v1[1] * v2[0];
        }

        public static Vector2d operator +(Vector2d v1, Vector2d v2)
        {
            if (v1 == null) return v2;
            if (v2 == null) return v1;

            return new Vector2d(
                v1._v[0] + v2._v[0],
                v1._v[1] + v2._v[1]);
        }

        public static Vector2d operator -(Vector2d v1, Vector2d v2)
        {
            if (v1 == null) return v2;
            if (v2 == null) return v1;

            return new Vector2d(
                v1._v[0] - v2._v[0],
                v1._v[1] - v2._v[1]);
        }
    }
}
