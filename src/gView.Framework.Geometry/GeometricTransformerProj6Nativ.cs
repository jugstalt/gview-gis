using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

// slow!!
namespace gView.Framework.Geometry
{
    public enum PJ_DIRECTION
    {
        PJ_IDENT = 0,
        PJ_FWD = 1,
        PJ_INV = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PJ_COORD
    {
        public double x;
        public double y;
        public double z;
        public double t;
    }


    public static class Proj6Wrapper
    {
        // proj_create_crs_to_crs:
        //   Creates a transformation between two coordinate systems 
        //   (e.g., "EPSG:4326" -> "EPSG:3857").
        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr proj_create_crs_to_crs(
            IntPtr ctx,        // proj_context, can be null if no custom context is used
            string srcCrs,     // Source definition (PROJ string or EPSG code)
            string dstCrs,     // Destination definition (PROJ string or EPSG code)
            IntPtr areaOfUse   // Optional area restriction, usually null
        );

        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void proj_context_set_search_paths(
            IntPtr ctx,
            int count,
            string[] paths
        );

        // proj_destroy:
        //   Frees the pointer created with proj_create_*
        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void proj_destroy(IntPtr projPtr);

        // proj_errno:
        //   Returns the last error code for a PJ object
        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int proj_errno(IntPtr projPtr);

        // proj_trans:
        //   Transforms a coordinate set (PJ_COORD) with the given PJ object
        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern PJ_COORD proj_trans(
            IntPtr projPtr,
            PJ_DIRECTION direction,
            PJ_COORD coord
        );

        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void proj_trans_array(
            IntPtr projPtr,
            PJ_DIRECTION direction,
            ulong n,                          // Number of coordinates
            [In, Out] PJ_COORD[] coordArray  // Array of coordinates
        );

        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void proj_trans_array(
            IntPtr projPtr,
            PJ_DIRECTION direction,
            ulong n,               // Number of coordinates
            [In, Out] IntPtr data  // Array of coordinates
        );

        // Optional: If you want to create/delete a custom PROJ_CONTEXT
        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr proj_context_create();

        [DllImport("proj6.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void proj_context_destroy(IntPtr ctx);
    }

    public sealed class GeometricTransformerProj6Nativ : IGeometricTransformer, IDisposable
    {
        static private string[] PROJ_LIB = new string[]
        {
            GeometricTransformerFactory.PROJ_LIB
        };

        //static private object LockThis1 = new object();
        static IntPtr _ctx;

        static GeometricTransformerProj6Nativ()
        {
            if (_ctx == IntPtr.Zero)
            {
                _ctx = Proj6Wrapper.proj_context_create();

                Proj6Wrapper.proj_context_set_search_paths(_ctx, PROJ_LIB.Length, PROJ_LIB);
            }
        }
        
        static Dictionary<string, IntPtr> _projCache = new Dictionary<string, IntPtr>();

        IntPtr _pj, _pjInv;
        private ISpatialReference _fromSRef = null, _toSRef = null;
        private readonly IDatumTransformations _datumTransformations = null;

        private bool _toProjective = true, _fromProjective = true;
        private const double RAD2DEG = (180.0 / Math.PI);

        public GeometricTransformerProj6Nativ(IDatumTransformations datumTransformations)
        {
            _datumTransformations = datumTransformations;
        }

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
                _toProjective = (value.SpatialParameters.IsGeographic == false);
            }
        }

        public void SetSpatialReferences(ISpatialReference from, ISpatialReference to)
        {
            this.ToSpatialReference = to;
            this.FromSpatialReference = from;
        }

        private IEnumerable<string> AllParameters(ISpatialReference sRef)
        {
            foreach (string param in sRef?.Parameters ?? [])
            {
                yield return param;
            }

            var datum = _datumTransformations.GetTransformationDatumFor(sRef.Datum);
            var datumParameter = datum?.Parameter;

            if (!string.IsNullOrEmpty(datumParameter))
            {
                yield return datumParameter;
            }
        }
        private string AllParametersString(ISpatialReference sRef)
        {
            if(sRef == null)
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

            var datum = _datumTransformations.GetTransformationDatumFor(sRef.Datum);
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

        public object Transform2D(object geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            string key = $"{AllParametersString(_fromSRef)}=>{AllParametersString(_toSRef)}";
            _pj = _projCache.ContainsKey(key) ? _projCache[key] : IntPtr.Zero;

            if (_pj == IntPtr.Zero)
            {
                _pj = Proj6Wrapper.proj_create_crs_to_crs(_ctx,
                    AllParametersString(_fromSRef),
                    AllParametersString(_toSRef),
                    IntPtr.Zero);
                
                _projCache[key] = _pj;
                Console.WriteLine($"Allocated transformation: {key}");
            }

            return Transform2D_(geometry, _pj, _fromProjective, _toProjective);
        }
        public object InvTransform2D(object geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            string key = $"{AllParametersString(_toSRef)}=>{AllParametersString(_fromSRef)}";
            _pjInv = _projCache.ContainsKey(key) ? _projCache[key] : IntPtr.Zero;

            if (_pjInv == IntPtr.Zero)
            {
                _pjInv = Proj6Wrapper.proj_create_crs_to_crs(_ctx,
                    AllParametersString(_toSRef),
                    AllParametersString(_fromSRef),
                    IntPtr.Zero);

                _projCache[key] = _pjInv;
                Console.WriteLine($"Allocated transformation: {key}");
            }

            return Transform2D_(geometry, _pjInv, _toProjective, _fromProjective);
        }

        //private object Transform2D_(object geometry, ISpatialReference from, ISpatialReference to, bool fromProjective, bool toProjektive)
        //{
        //    if (geometry == null)
        //    {
        //        return null;
        //    }

        //    if (_pj == IntPtr.Zero)
        //    {
        //        throw new Exception("GeometryTransformer is not initialized");
        //    }

        //    return Transform2D_(geometry, _pj, fromProjective, toProjektive);
        //}

        private object Transform2D_(object geometry, IntPtr pj, bool fromProjective, bool toProjektive)
        {
            if (geometry is IPointCollection)
            {
                PointCollection pColl = (PointCollection)geometry;
                int pointCount = pColl.PointCount;
                if (pointCount == 0)
                {
                    return geometry;
                }

                var coordArray = new PJ_COORD[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    coordArray[i] = new PJ_COORD
                    {
                        x = pColl[i].X,
                        y = pColl[i].Y,
                        z = pColl[i].Z
                    };
                }
                Proj6Wrapper.proj_trans_array(pj, PJ_DIRECTION.PJ_FWD, (ulong)coordArray.Length, coordArray);

                IPointCollection target = geometry switch
                {
                    IRing => new Ring(),
                    IPath => new Path(),
                    IMultiPoint => new MultiPoint(),
                    _ => new PointCollection()
                };
                for (int i = 0; i < pointCount; i++)
                {
                    target.AddPoint(new Point(
                        coordArray[i].x,
                        coordArray[i].y,
                        coordArray[i].z
                    ));
                }
                return target;

                //IntPtr buffer = Marshal.AllocHGlobal(pointCount * 4 * sizeof(double));
                //try
                //{
                //    unsafe
                //    {
                //        double* b = (double*)buffer;

                //        for (int i = 0; i < pointCount; i++)
                //        {
                //            b[i * 4] = pColl[i].X;
                //            b[i * 4 + 1] = pColl[i].Y;
                //            b[i * 4 + 2] = pColl[i].Z;
                //            b[i * 4 + 3] = 0;
                //        }
                //    }

                //    Proj6Wrapper.proj_trans_array(_pj, PJ_DIRECTION.PJ_FWD, (ulong)pointCount, buffer);
                    
                //    IPointCollection target = geometry switch
                //    {
                //        IRing => new Ring(),
                //        IPath => new Path(),
                //        IMultiPoint => new MultiPoint(),
                //        _ => new PointCollection()
                //    };

                //    unsafe
                //    {
                //        double* b = (double*)buffer;
                //        for (int i = 0; i < pointCount; i++)
                //        {
                //            target.AddPoint(new Point(b[i * 4], b[i * 4 + 1]));
                //        }

                //        return target;
                //    }
                //}
                //finally
                //{
                //    Marshal.FreeHGlobal(buffer);
                //}

                /*
                var coordArray = new PJ_COORD[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    coordArray[i] = new PJ_COORD
                    {
                        x = pColl[i].X,
                        y = pColl[i].Y,
                        z = pColl[i].Z,
                        //t = point.M
                    };
                }

                Proj6Wrapper.proj_trans_array(_pj, PJ_DIRECTION.PJ_FWD, (ulong)coordArray.Length, coordArray);

                int errorNo = Proj6Wrapper.proj_errno(_pj);
                if (errorNo != 0)
                {
                    throw new Exception($"proj_trans() Fehlercode: {errorNo}");
                }

                IPointCollection result = geometry switch
                {
                    IRing => new Ring(),
                    IPath => new Path(),
                    _ => new PointCollection()
                };
                for (int i = 0; i < pointCount; i++)
                {
                    result.AddPoint(new Point(
                        coordArray[i].x,
                        coordArray[i].y,
                        coordArray[i].z
                    //pColl[i].M = coordArray[i].t;
                    ));
                }
                return result;
                */
            }
            if (geometry is IPoint)
            {
                double[] x = { ((IPoint)geometry).X };
                double[] y = { ((IPoint)geometry).Y };

                var coord = new PJ_COORD()
                {
                    x = ((IPoint)geometry).X,
                    y = ((IPoint)geometry).Y,
                    z = ((IPoint)geometry).Z
                };

                var outCoord = Proj6Wrapper.proj_trans(_pj, PJ_DIRECTION.PJ_FWD, coord);

                int errorNo = Proj6Wrapper.proj_errno(_pj);
                if (errorNo != 0)
                {
                    throw new Exception($"proj_trans() Fehlercode: {errorNo}");
                }

                return new Point(outCoord.x, outCoord.y);
            }
            if (geometry is IEnvelope)
            {
                return Transform2D_(((IEnvelope)geometry).ToPolygon(10), _pj, fromProjective, toProjektive);
            }
            if (geometry is IPolyline)
            {
                int count = ((IPolyline)geometry).PathCount;
                IPolyline polyline = new Polyline();
                for (int i = 0; i < count; i++)
                {
                    polyline.AddPath((IPath)Transform2D_(((IPolyline)geometry)[i], _pj, fromProjective, toProjektive));
                }
                return polyline;
            }
            if (geometry is IPolygon)
            {
                int count = ((IPolygon)geometry).RingCount;
                IPolygon polygon = new Polygon();
                for (int i = 0; i < count; i++)
                {
                    polygon.AddRing((IRing)Transform2D_(((IPolygon)geometry)[i], _pj, fromProjective, toProjektive));
                }
                return polygon;
            }

            if (geometry is IAggregateGeometry)
            {
                int count = ((IAggregateGeometry)geometry).GeometryCount;
                IAggregateGeometry aGeom = new AggregateGeometry();
                for (int i = 0; i < count; i++)
                {
                    aGeom.AddGeometry((IGeometry)Transform2D_(((IAggregateGeometry)geometry)[i], _pj, fromProjective, toProjektive));
                }
                return aGeom;
            }

            return null;
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

        private void ToRad(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                return;
            }

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = Math.Min(180, Math.Max(-180, x[i]));
                y[i] = Math.Min(90, Math.Max(-90, y[i]));

                x[i] /= RAD2DEG;
                y[i] /= RAD2DEG;
            }
        }
        private void ToRad(IntPtr buffer, int count)
        {
            unsafe
            {
                double* b = (double*)buffer;
                for (int i = 0; i < count; i++)
                {
                    b[i] = Math.Min(180, Math.Max(-180, b[i]));
                    b[i] /= RAD2DEG;
                }
            }
        }

        public void Release()
        {
            //if (_pj != IntPtr.Zero)
            //{
            //    Proj6Wrapper.proj_destroy(_pj);
            //    _pj = IntPtr.Zero;
            //}

            //if (_pjInv != IntPtr.Zero)
            //{
            //    Proj6Wrapper.proj_destroy(_pjInv);
            //    _pjInv = IntPtr.Zero;
            //}

            //if (_ctx != IntPtr.Zero)
            //{
            //    Proj6Wrapper.proj_context_destroy(_ctx);
            //    _ctx = IntPtr.Zero;
            //}
        }

        #endregion

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

        #region IDisposable Member

        public void Dispose()
        {
            this.Release();
        }

        #endregion
    }
}
