using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Geometry
{
    class GeomTriangle
    {
        private Vector2D _a, _b, _c;
        
        public GeomTriangle(Vector2D a, Vector2D b, Vector2D c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        public GeomTriangle(double x1, double y1, double x2, double y2, double x3, double y3)
            : this(new Vector2D(x1, y1), new Vector2D(x2, y2), new Vector2D(x3, y3))
        {
        }

        public Vector2D a
        {
            get { return _a; }
        }
        public Vector2D b
        {
            get { return _b; }
        }
        public Vector2D c
        {
            get { return _c; }
        }

        public List<Vector2D> PerpendicularVectors(double len)
        {
            List<Vector2D> vecs = new List<Vector2D>();
            Vector2D v1 = _b - _a;
            Vector2D v2 = _c - _a;
            Vector2D v3 = _c - _b;

            v1.Length = len;
            v2.Length = len;
            v3.Length = len;

            double a1 = v1.Angle;
            double a2 = v2.Angle;
            double r = (a1 < a2) ? -Math.PI / 2.0 : Math.PI / 2.0;

            v1.Rotate(r);  
            v2.Rotate(-r); 
            v2.Rotate(-r);

            vecs.Add(new Vector2D(v1));
            vecs.Add(new Vector2D(v2));
            vecs.Add(new Vector2D(v3));

            return vecs;
        }
    }
}
