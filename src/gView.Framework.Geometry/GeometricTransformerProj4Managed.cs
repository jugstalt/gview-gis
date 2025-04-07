using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Extensions;
using Proj4Net.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.Framework.Geometry
{
    public sealed class GeometricTransformerProj4Managed : IGeometricTransformer,
                                                           IDatumGridShiftProvider,
                                                           IDisposable
    {
        private CoordinateReferenceSystem _fromSrs = null, _toSrs = null;
        private bool _toProjective = true, _fromProjective = true;
        private readonly IDatumTransformations _datumTransformations = null;

        private const double RAD2DEG = (180.0 / Math.PI);
        //static private object lockThis = new object();

        private CoordinateReferenceSystemFactory _factory = new CoordinateReferenceSystemFactory();
        private ISpatialReference _fromSRef = null, _toSRef = null;

        CoordinateReferenceSystem[] _projectionPipeline = null;
        BasicCoordinateTransform[] _basicCoordinateTransformations = null;
        BasicCoordinateTransform[] _basicCoordinateTransformationsInverse = null;

        static GeometricTransformerProj4Managed()
        {
            Proj4Net.Core.IO.Paths.PROJ_LIB = GeometricTransformerFactory.PROJ_LIB;
        }

        public GeometricTransformerProj4Managed(IDatumTransformations datumTransformations)
        {
            _datumTransformations = datumTransformations;
        }

        #region IGeometricTransformer Member

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

        public object Transform2D(object geometry)
        {
            return PerformTransform2D(geometry, false);
        }
        public object InvTransform2D(object geometry)
        {
            return PerformTransform2D(geometry, true);
        }

        public void Release()
        {

        }

        #endregion

        #region IDatumGridShiftProvider Member

        public string[] GridShiftNames()
        {
            string projLibPath = Proj4Net.Core.IO.Paths.PROJ_LIB;

            if (String.IsNullOrEmpty(projLibPath) ||
                !Directory.Exists(projLibPath))
            {
                return [];
            }

            List<string> result = new();

            foreach (var file in Directory.GetFiles(projLibPath))
            {
                switch (System.IO.Path.GetExtension(file).ToLower())
                {
                    case ".gsb":
                        result.Add(System.IO.Path.GetFileName(file));
                        break;
                }
            }

            return result.ToArray();
        }

        #endregion

        #region Static Members

        static public IGeometry Transform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to, IDatumTransformations datumTransformations)
        {
            if (geometry == null)
            {
                return null;
            }

            if (from == null || to == null || from.Equals(to))
            {
                return geometry;
            }

            using (IGeometricTransformer transformer = GeometricTransformerFactory.Create(datumTransformations))
            {
                //transformer.FromSpatialReference = from;
                //transformer.ToSpatialReference = to;
                transformer.SetSpatialReferences(from, to);
                IGeometry transformed = transformer.Transform2D(geometry) as IGeometry;
                transformer.Release();

                return transformed;
            }
        }

        static public IGeometry InvTransform2D(IGeometry geometry, ISpatialReference from, ISpatialReference to, IDatumTransformations datumTransformations)
        {
            return Transform2D(geometry, to, from, datumTransformations);
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            this.Release();
        }

        #endregion

        #region Private Members

        private ISpatialReference FromSpatialReference
        {
            get
            {
                return _fromSRef;
            }
            set
            {
                _fromSRef = value;
                _fromSrs = _factory.CreateFromParameters("from", AllParameters(value)?.ToArray() ?? []);
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
                _toSrs = _factory.CreateFromParameters("to", AllParameters(value)?.ToArray() ?? []);
                _toProjective = (value.SpatialParameters.IsGeographic == false);
            }
        }

        private IEnumerable<string> AllParameters(ISpatialReference sRef)
        {
            foreach (string param in sRef?.Parameters ?? [])
            {
                yield return param;
            }

            var datum = _datumTransformations.GetTransformationFor(sRef.Datum);
            var datumParameter = datum?.Parameter;

            if (!string.IsNullOrEmpty(datumParameter))
            {
                yield return datumParameter;
            }
        }
        private string AllParametersString(ISpatialReference sRef)
        {
            if (sRef == null)
            {
                return String.Empty;
            }

            var parameters = new StringBuilder();
            foreach (string param in sRef.Parameters)
            {
                if (parameters.Length > 0)
                {
                    parameters.Append(" ");
                }

                parameters.Append(param.Trim());
            }

            var datum = _datumTransformations.GetTransformationFor(sRef.Datum);
            var datumParameter = datum?.Parameter;

            if (!String.IsNullOrEmpty(datumParameter))
            {
                if (parameters.Length > 0)
                {
                    parameters.Append(" ");
                }

                parameters.Append(datumParameter);
            }

            return parameters.ToString();
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

        private object PerformTransform2D(object geometry, bool inverse)
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
                return PerformTransform2D(((IEnvelope)geometry).ToPolygon(10), inverse);
            }
            if (geometry is IPolyline)
            {
                int count = ((IPolyline)geometry).PathCount;
                IPolyline polyline = new Polyline();
                for (int i = 0; i < count; i++)
                {
                    polyline.AddPath((IPath)PerformTransform2D(((IPolyline)geometry)[i], inverse));
                }
                return polyline;
            }
            if (geometry is IPolygon)
            {
                int count = ((IPolygon)geometry).RingCount;
                IPolygon polygon = new Polygon();
                for (int i = 0; i < count; i++)
                {
                    polygon.AddRing((IRing)PerformTransform2D(((IPolygon)geometry)[i], inverse));
                }
                return polygon;
            }

            if (geometry is IAggregateGeometry)
            {
                int count = ((IAggregateGeometry)geometry).GeometryCount;
                IAggregateGeometry aGeom = new AggregateGeometry();
                for (int i = 0; i < count; i++)
                {
                    aGeom.AddGeometry((IGeometry)PerformTransform2D(((IAggregateGeometry)geometry)[i], inverse));
                }
                return aGeom;
            }

            return null;
        }

        private CoordinateReferenceSystem[] ProjectionPipeline(CoordinateReferenceSystem from, CoordinateReferenceSystem to)
        {
            #region Problem solved with Proj4Net.Core, thx Jürgen

            ////
            ////  Proj4net berücksichtigt nadgrids=@null nicht, wodurch bei Koordinatensystem mit diesem Parametern ein Fehler im Hochwert entsteht!
            ////  Workaround: Zuerst nach WGS84 projezieren und dann weiter...
            ////
            //if (from.Parameters.Contains("+nadgrids=@null") || to.Parameters.Contains("+nadgrids=@null"))
            //{
            //    var wgs84 = _factory.CreateFromParameters("epsg:4326", "+proj=longlat +ellps=WGS84 +datum=WGS84 +towgs84=0,0,0,0,0,0,0");

            //    if (!IsEqual(wgs84, from) && !IsEqual(wgs84, to))
            //    {
            //        return new CoordinateReferenceSystem[]
            //        {
            //            from,
            //            wgs84,
            //            to
            //        };
            //    }
            //}

            #endregion

            return new CoordinateReferenceSystem[] { from, to };
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

        #endregion
    }
}
