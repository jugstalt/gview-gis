using Proj4Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.Geometry
{
    public sealed class GeometricTransformerProj4Managed : gView.Framework.Geometry.IGeometricTransformer, IDisposable
    {
        private CoordinateReferenceSystem _fromSrs = null, _toSrs = null;
        private bool _toProjective = true, _fromProjective = true;

        private const double RAD2DEG = (180.0 / Math.PI);
        //static private object lockThis = new object();

        private CoordinateReferenceSystemFactory _factory = new CoordinateReferenceSystemFactory();
        private ISpatialReference _fromSRef = null, _toSRef = null;

        CoordinateReferenceSystem[] _projectionPipeline = null;
        BasicCoordinateTransform[] _basicCoordinateTransformations = null;
        BasicCoordinateTransform[] _basicCoordinateTransformationsInverse = null;

        #region IGeometricTransformer Member

        private ISpatialReference FromSpatialReference
        {
            get
            {
                return _fromSRef;
            }
            set
            {
                _fromSRef = value;
                _fromSrs = _factory.CreateFromParameters("from", allParameters(value));
                _fromProjective = (value.SpatialParameters.IsGeographic == false);
            }
        }

        private ISpatialReference ToSpatialReference
        {
            get
            {
                return _toSRef;
            }
            set
            {
                _toSRef = value;
                _toSrs = _factory.CreateFromParameters("to", allParameters(value));
                _toProjective = (value.SpatialParameters.IsGeographic == false);
            }
        }

        public void SetSpatialReferences(ISpatialReference from, ISpatialReference to)
        {
            if (from == null)
            {
                this.FromSpatialReference = from;
            }

            if (to == null)
            {
                this.ToSpatialReference = to;
            }

            if ((from != null && from.Datum == null) && (to != null && to.Datum != null))
            {
                ISpatialReference toSRef = (ISpatialReference)to.Clone();
                toSRef.Datum = null;
                this.ToSpatialReference = toSRef;
                this.FromSpatialReference = from;
            }
            else if ((from != null && from.Datum != null) && (to != null && to.Datum == null))
            {
                this.ToSpatialReference = to;
                ISpatialReference fromSRef = (ISpatialReference)from.Clone();
                fromSRef.Datum = null;
                this.FromSpatialReference = fromSRef;
            }
            else
            {
                this.ToSpatialReference = to;
                this.FromSpatialReference = from;
            }

            _projectionPipeline = ProjectionPipeline(_fromSrs, _toSrs);
            _basicCoordinateTransformations = new BasicCoordinateTransform[_projectionPipeline.Length - 1];
            _basicCoordinateTransformationsInverse = new BasicCoordinateTransform[_projectionPipeline.Length - 1];

            for (int p = 0, p_to = _projectionPipeline.Length; p < p_to - 1; p++)
            {
                BasicCoordinateTransform t = new BasicCoordinateTransform(_projectionPipeline[p], _projectionPipeline[p + 1]);
                _basicCoordinateTransformations[p] = t;
            }
            for (int p = _projectionPipeline.Length; p > 1; p--)
            {
                BasicCoordinateTransform t = new BasicCoordinateTransform(_projectionPipeline[p - 1], _projectionPipeline[p - 2]);
                _basicCoordinateTransformationsInverse[_projectionPipeline.Length - p] = t;
            }
        }

        private string[] allParameters(ISpatialReference sRef)
        {
            if (sRef == null)
            {
                return "".Split();
            }

            if (sRef.Datum == null)
            {
                return sRef.Parameters;
            }

            string parameters = "";
            foreach (string param in sRef.Parameters)
            {
                parameters += param + " ";
            }
            parameters += sRef.Datum.Parameter;

            return parameters.Split(' ');
        }
        private string allParametersString(ISpatialReference sRef)
        {
            if (sRef == null)
            {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string param in sRef.Parameters)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(param.Trim());
            }
            if (sRef.Datum != null && !String.IsNullOrEmpty(sRef.Datum.Parameter))
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(sRef.Datum.Parameter);
            }
            return sb.ToString();
        }
        private bool ParametersContain(ISpatialReference sRef, string p)
        {
            foreach (string param in sRef.Parameters)
            {
                if (param == p)
                {
                    return true;
                }
            }
            return false;
        }

        public object Transform2D(object geometry)
        {
            return Transform2D_(geometry, false);
        }
        public object InvTransform2D(object geometry)
        {
            return Transform2D_(geometry, true);
        }

        private BasicCoordinateTransform[] BasicTransformations(bool inverse)
        {
            if (inverse)
            {
                return _basicCoordinateTransformationsInverse;
            }
            else
            {
                return _basicCoordinateTransformations;
            }
        }

        private object Transform2D_(object geometry, bool inverse)
        {
            if (_projectionPipeline == null)
            {
                return geometry;
            }

            CoordinateReferenceSystem from = _fromSrs, to = _toSrs;
            bool fromProjective = _fromProjective, toProjektive = _toProjective;

            if (geometry == null)
            {
                return null;
            }

            if (from == null || to == null)
            {
                return geometry;
            }

            if (geometry is PointCollection)
            {
                IPointCollection pColl = (IPointCollection)geometry;
                int pointCount = pColl.PointCount;
                if (pointCount == 0)
                {
                    return geometry;
                }

                IPointCollection target = null;

                var basicTransformations = BasicTransformations(inverse);
                for (int b = 0, b_to = basicTransformations.Length; b < b_to; b++)
                {
                    BasicCoordinateTransform t = basicTransformations[b];

                    if (pColl is IRing)
                    {
                        target = new Ring();
                    }
                    else if (pColl is IPath)
                    {
                        target = new Path();
                    }
                    else if (pColl is IMultiPoint)
                    {
                        target = new MultiPoint();
                    }
                    else
                    {
                        target = new PointCollection();
                    }

                    for (int i = 0; i < pointCount; i++)
                    {
                        ProjCoordinate cFrom = new ProjCoordinate(pColl[i].X, pColl[i].Y);
                        var cTo = new ProjCoordinate();
                        t.Transform(cFrom, cTo);
                        target.AddPoint(new Point(cTo.X, cTo.Y));
                    }

                    pColl = target;
                }
                return target;
            }
            if (geometry is IPoint)
            {
                IPoint target = null;

                var basicTransformations = BasicTransformations(inverse);
                for (int b = 0, b_to = basicTransformations.Length; b < b_to; b++)
                {
                    BasicCoordinateTransform t = basicTransformations[b];

                    ProjCoordinate cFrom = new ProjCoordinate(((IPoint)geometry).X, ((IPoint)geometry).Y), cTo = new ProjCoordinate();
                    t.Transform(cFrom, cTo);
                    target = new Point(cTo.X, cTo.Y);

                    geometry = target;
                }
                return target;
            }
            if (geometry is IEnvelope)
            {
                return Transform2D_(((IEnvelope)geometry).ToPolygon(10), inverse);
            }
            if (geometry is IPolyline)
            {
                int count = ((IPolyline)geometry).PathCount;
                IPolyline polyline = new Polyline();
                for (int i = 0; i < count; i++)
                {
                    polyline.AddPath((IPath)Transform2D_(((IPolyline)geometry)[i], inverse));
                }
                return polyline;
            }
            if (geometry is IPolygon)
            {
                int count = ((IPolygon)geometry).RingCount;
                IPolygon polygon = new Polygon();
                for (int i = 0; i < count; i++)
                {
                    polygon.AddRing((IRing)Transform2D_(((IPolygon)geometry)[i], inverse));
                }
                return polygon;
            }

            if (geometry is IAggregateGeometry)
            {
                int count = ((IAggregateGeometry)geometry).GeometryCount;
                IAggregateGeometry aGeom = new AggregateGeometry();
                for (int i = 0; i < count; i++)
                {
                    aGeom.AddGeometry((IGeometry)Transform2D_(((IAggregateGeometry)geometry)[i], inverse));
                }
                return aGeom;
            }

            return null;
        }

        private CoordinateReferenceSystem[] ProjectionPipeline(CoordinateReferenceSystem from, CoordinateReferenceSystem to)
        {
            //
            //  Proj4net berücksichtigt nadgrids=@null nicht, wodurch bei Koordinatensystem mit diesem Parametern ein Fehler im Hochwert entsteht!
            //  Workaround: Zuerst nach WGS84 projezieren und dann weiter...
            //
            if (from.Parameters.Contains("+nadgrids=@null") || to.Parameters.Contains("+nadgrids=@null"))
            {
                var wgs84 = _factory.CreateFromParameters("epsg:4326", "+proj=longlat +ellps=WGS84 +datum=WGS84 +towgs84=0,0,0,0,0,0,0");

                if (!IsEqual(wgs84, from) && !IsEqual(wgs84, to))
                {
                    return new CoordinateReferenceSystem[]
                    {
                        from,
                        wgs84,
                        to
                    };
                }
            }

            return new CoordinateReferenceSystem[] { from, to };
        }

        private bool IsEqual(CoordinateReferenceSystem c1, CoordinateReferenceSystem c2)
        {
            if (c1.Parameters.Length != c2.Parameters.Length)
            {
                return false;
            }

            foreach (var p in c1.Parameters)
            {
                if (!c2.Parameters.Contains(p))
                {
                    return false;
                }
            }

            return true;
        }

        private void ToDeg(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                return;
            }

            for (int i = 0; i < x.Length; i++)
            {
                x[i] *= RAD2DEG;
                y[i] *= RAD2DEG;
            }

        }
        private void ToDeg(IntPtr buffer, int count)
        {
            unsafe
            {
                double* b = (double*)buffer;
                for (int i = 0; i < count; i++)
                {
                    b[i] = b[i] * RAD2DEG;
                }
            }
        }

        //private void ToRad(double[] x, double[] y)
        //{
        //    if (x.Length != y.Length) return;

        //    for (int i = 0; i < x.Length; i++)
        //    {
        //        x[i] = Math.Min(180, Math.Max(-180, x[i]));
        //        y[i] = Math.Min(90, Math.Max(-90, y[i]));

        //        x[i] /= RAD2DEG;
        //        y[i] /= RAD2DEG;
        //    }
        //}
        //private void ToRad(IntPtr buffer, int count)
        //{
        //    unsafe
        //    {
        //        double* b = (double*)buffer;
        //        for (int i = 0; i < count; i++)
        //        {
        //            b[i] = Math.Min(180, Math.Max(-180, b[i]));
        //            b[i] /= RAD2DEG;
        //        }
        //    }
        //}

        public void Release()
        {

        }

        #endregion

        static public IGeometry Transform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to)
        {
            if (geometry == null)
            {
                return null;
            }

            if (from == null || to == null || from.Equals(to))
            {
                return geometry;
            }

            using (IGeometricTransformer transformer = GeometricTransformerFactory.Create())
            {
                //transformer.FromSpatialReference = from;
                //transformer.ToSpatialReference = to;
                transformer.SetSpatialReferences(from, to);
                IGeometry transformed = transformer.Transform2D(geometry) as IGeometry;
                transformer.Release();

                return transformed;
            }
        }

        static public IGeometry InvTransform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to)
        {
            return Transform2D(geometry, to, from);
        }

        #region IDisposable Member

        public void Dispose()
        {
            this.Release();
        }

        #endregion
    }
}
