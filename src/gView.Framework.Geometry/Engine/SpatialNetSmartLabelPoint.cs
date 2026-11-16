using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using SpatialNET;
using System.Collections.Generic;
using System.Linq;

namespace gView.Framework.Geometry.Engine
{
    /// <summary>
    /// A label point of a large polygon with lazy alternative points from SpatialNET
    /// (<c>FastAlternativePolygonLabelsLazy</c>): nothing is computed until the label engine
    /// rejects this point, and it stops at the first alternative that fits.
    /// </summary>
    internal sealed class SpatialNetSmartLabelPoint : Point, ISmartLabelPoint
    {
        private readonly Coordinate _seed;
        private readonly IPolygon2D _polygon;
        private readonly double _pixelSize;
        private readonly IEnvelope _visible;

        public SpatialNetSmartLabelPoint(Coordinate seed, IPolygon2D polygon, double pixelSize, IEnvelope visible = null)
            : base(seed.X, seed.Y)
        {
            _seed = seed;
            _polygon = polygon;
            _pixelSize = pixelSize;
            _visible = visible;
        }

        public IMultiPoint AlernativeLabelPoints(IDisplay display)
        {
            var points = new MultiPoint();
            foreach (var point in AlternativeLabelPointsLazy(display))
            {
                points.AddPoint(point);
            }

            return points;
        }

        public IEnumerable<IPoint> AlternativeLabelPointsLazy(IDisplay display)
        {
            double pixel = _pixelSize > 0 ? _pixelSize : display.MapScale / display.Dpm;
            var options = new AlternativeLabelOptions
            {
                Separation = GeometryEngineOptions.LabelAlternativeSeparationPixels * pixel,
                MaxCount = GeometryEngineOptions.LabelMaxAlternatives,
            };

            foreach (var coordinate in Shapes.FastAlternativePolygonLabelsLazy(_polygon, _seed, options))
            {
                var point = new Point(coordinate.X, coordinate.Y) { Srs = Srs };
                if (_visible == null || _visible.Contains(point))
                {
                    yield return point;
                }
            }
        }
    }
}
