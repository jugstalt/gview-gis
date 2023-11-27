using System;

namespace gView.Framework.Core.Symbology
{
    public class AnnotationPolygonEnvelope
    {
        private float _minx, _miny, _maxx, _maxy;

        internal AnnotationPolygonEnvelope(float minx, float miny, float maxx, float maxy)
        {
            _minx = Math.Min(minx, maxx);
            _miny = Math.Min(miny, maxy);
            _maxx = Math.Max(minx, maxx);
            _maxy = Math.Max(miny, maxy);
        }
        internal AnnotationPolygonEnvelope(AnnotationPolygonEnvelope env)
        {
            _minx = env._minx;
            _miny = env._miny;
            _maxx = env._maxx;
            _maxy = env._maxy;
        }
        public float MinX { get { return _minx; } }
        public float MinY { get { return _miny; } }
        public float MaxX { get { return _maxx; } }
        public float MaxY { get { return _maxy; } }

        internal void Append(float x, float y)
        {
            _minx = Math.Min(_minx, x);
            _miny = Math.Min(_miny, y);
            _maxx = Math.Max(_maxx, x);
            _maxy = Math.Max(_maxy, y);
        }

        internal void Append(AnnotationPolygonEnvelope env)
        {
            _minx = Math.Min(_minx, env._minx);
            _miny = Math.Min(_miny, env._miny);
            _maxx = Math.Max(_maxx, env._maxx);
            _maxy = Math.Max(_maxy, env._maxy);
        }
    }
}
