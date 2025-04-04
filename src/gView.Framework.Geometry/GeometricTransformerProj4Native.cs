using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    public struct UV
    {
        public double u, v;
    }


    public class Proj4Wrapper
    {
        [DllImport("proj.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr pj_init(int argc, string[] args);

        [DllImport("proj.dll", CharSet = CharSet.Ansi)]
        public static extern IntPtr pj_init_plus(string args);

        [DllImport("proj.dll")]
        public static extern void pj_free(IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern UV pj_fwd(UV uv, IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern UV pj_inv(UV uv, IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern IntPtr pj_get_errno_ref();

        [DllImport("proj.dll")]
        public static extern IntPtr pj_strerrno(int errno);

        [DllImport("proj.dll")]
        public static extern int pj_is_latlong(IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern IntPtr pj_get_release();

        [DllImport("proj.dll")]
        public static extern int pj_transform(IntPtr src, IntPtr dst, int point_count, int point_offset, IntPtr x, IntPtr y, IntPtr z);

        [DllImport("proj.dll")]
        public static extern int pj_is_geocent(IntPtr projPJ);

        [DllImport("proj.dll")]
        public static extern void pj_set_searchpath(int count, IntPtr path);
    }

    public sealed class GeometricTransformerProj4Nativ : IGeometricTransformer, 
                                                         IDatumGridShiftProvider,
                                                         IDisposable
    {
        private IntPtr _fromID = IntPtr.Zero, _toID = IntPtr.Zero;
        //private int _preToID = -1, _preFromID = -1;
        //private IntPtr _fromStr = (IntPtr)0, _toStr = (IntPtr)0;
        static private object LockThis1 = new object();

        private ISpatialReference _fromSRef = null, _toSRef = null;

        private bool _toProjective = true, _fromProjective = true;
        private const double RAD2DEG = (180.0 / Math.PI);

        #region IGeometricTransformer Member

        private ISpatialReference FromSpatialReference
        {
            get
            {
                return _fromSRef;
            }
            set
            {
                lock (LockThis1)  // pj_free und pj_init nicht ThreadSave!!!
                {
                    try
                    {
                        //Console.Write("FromID: " + this.GetHashCode()+"\t"+ System.Threading.Thread.CurrentThread.ManagedThreadId + "\t");

                        if (_fromSRef != null && value != null &&
                            _fromSRef.Equals(value))
                        {
                            return;
                        }

                        _fromSRef = value;

                        if (_fromID != IntPtr.Zero)
                        {
                            Proj4Wrapper.pj_free(_fromID);
                        }

                        if (value == null)
                        {
                            return;
                        }

                        if (value.Parameters == null)
                        {
                            return;
                        }

                        string[] parms = allParameters(value);
                        _fromID = Proj4Wrapper.pj_init(parms.Length, parms);

                        //if (ParametersContain(value, "+nadgrids=@null"))
                        //    _preFromID = Proj4Wrapper.pj_init(5, new string[] { "+proj=longlat", "+ellps=WGS84", "+datum=WGS84", "+towgs84=0,0,0,0,0,0,0", "+no_defs" });

                        #region Variante mit pj_init_plus
                        //_fromStr = Marshal.StringToHGlobalAnsi(allParametersString(value));
                        //_fromID = Proj4Wrapper.pj_init_plus(_fromStr);
                        //Marshal.FreeHGlobal(_fromStr);
                        //_fromID = Proj4Wrapper.pj_init_plus(allParametersString(value));
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine("\tERROR:" + _fromID.ToString());
                        throw new Exception(value.ToString() + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                    //Console.WriteLine();
                    //if (value.SpatialParameters.IsGeographic)
                    //{
                    //    _fromProjective = false;
                    //}
                    _fromProjective = (value.SpatialParameters.IsGeographic == false);
                }
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
                lock (LockThis1)  // pj_free und pj_init nicht ThreadSave!!!
                {
                    try
                    {
                        if (_toSRef != null && value != null &&
                            _toSRef.Equals(value))
                        {
                            return;
                        }

                        _toSRef = value;

                        if (_toID != IntPtr.Zero)
                        {
                            Proj4Wrapper.pj_free(_toID);
                        }

                        if (value == null)
                        {
                            return;
                        }

                        if (value.Parameters == null)
                        {
                            return;
                        }

                        string[] parms = allParameters(value);
                        _toID = Proj4Wrapper.pj_init(parms.Length, parms);

                        //if (ParametersContain(value, "+nadgrids=@null"))
                        //    _preToID = Proj4Wrapper.pj_init(5, new string[] { "+proj=longlat", "+ellps=WGS84", "+datum=WGS84", "+towgs84=0,0,0,0,0,0,0", "+no_defs" });

                        #region Variante pj_init_plus
                        //_toStr = Marshal.StringToHGlobalAnsi(allParametersString(value));
                        //_toID = Proj4Wrapper.pj_init_plus(_toStr);
                        //Marshal.FreeHGlobal(_toStr);
                        //Console.WriteLine("ToID:" + _toID);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.Message);
                        throw new Exception(value.ToString() + "\r\n" + _toID + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                }
                //if (value.SpatialParameters.IsGeographic)
                //{
                //    _toProjective = false;
                //}
                _toProjective = (value.SpatialParameters.IsGeographic == false);
            }
        }

        public void SetSpatialReferences(ISpatialReference from, ISpatialReference to)
        {
            if (from == null)
            {
                lock (LockThis1)
                {
                    try
                    {
                        if (_fromID != IntPtr.Zero)
                        {
                            Proj4Wrapper.pj_free(_fromID);
                        }
                    }
                    catch { }
                    _fromID = IntPtr.Zero;
                    _fromSRef = null;
                }
            }

            if (to == null)
            {
                lock (LockThis1)
                {
                    try
                    {
                        if (_toID != IntPtr.Zero)
                        {
                            Proj4Wrapper.pj_free(_toID);
                        }
                    }
                    catch { }
                    _toID = IntPtr.Zero;
                    _toSRef = null;
                }
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
            return Transform2D_(geometry, _fromID, _toID, _fromProjective, _toProjective);
        }
        public object InvTransform2D(object geometry)
        {
            return Transform2D_(geometry, _toID, _fromID, _toProjective, _fromProjective);
        }

        private object Transform2D_(object geometry, IntPtr from, IntPtr to, bool fromProjective, bool toProjektive)
        {
            if (geometry == null)
            {
                return null;
            }

            if (from == IntPtr.Zero || to == IntPtr.Zero)
            {
                return geometry;
            }

            if (geometry is PointCollection)
            {
                PointCollection pColl = (PointCollection)geometry;
                int pointCount = pColl.PointCount;
                if (pointCount == 0)
                {
                    return geometry;
                }

                IntPtr buffer = Marshal.AllocHGlobal(pointCount * 2 * sizeof(double));

                lock (LockThis1)
                {
                    try
                    {
                        IntPtr xPtr = IntPtr.Zero, yPtr = IntPtr.Zero;
                        unsafe
                        {
                            double* b = (double*)buffer;

                            for (int i = 0; i < pointCount; i++)
                            {
                                b[i] = pColl[i].X;
                                b[pointCount + i] = pColl[i].Y;
                            }

                            if (!fromProjective)
                            {
                                ToRad(buffer, pointCount * 2);
                            }

                            xPtr = (IntPtr)(&b[0]);
                            yPtr = (IntPtr)(&b[pointCount]);
                        }
                        if (from != IntPtr.Zero && to != IntPtr.Zero)
                        {
                            //if (preTo > 0)
                            //{
                            //    Proj4Wrapper.pj_transform(from, preTo, pointCount, 0, xPtr, yPtr, (IntPtr)0);
                            //    Proj4Wrapper.pj_transform(preTo, to, pointCount, 0, xPtr, yPtr, (IntPtr)0);
                            //}
                            //else
                            //{
                            Proj4Wrapper.pj_transform(from, to, pointCount, 0, xPtr, yPtr, IntPtr.Zero);
                            //}
                        }
                        if (!toProjektive)
                        {
                            ToDeg(buffer, pointCount * 2);
                        }

                        IPointCollection target = null;
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

                        unsafe
                        {
                            double* b = (double*)buffer;
                            for (int i = 0; i < pointCount; i++)
                            {
                                target.AddPoint(new Point(b[i], b[pointCount + i]));
                            }

                            return target;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
                //double [] x;
                //double [] y;
                //((PointCollection)geometry).getXY(out x,out y);
                //if (!fromProjective) ToRad(x, y);

                //if(from>0 && to>0)
                //    Proj4Wrapper.pj_transform(from,to,x.Length,0,x,y,null);
                //if (!toProjektive) ToDeg(x, y);

                //if(geometry is Ring) 
                //{
                //    Ring ring=new Ring();
                //    ring.setXYZ(x,y,null);
                //    return ring;
                //}
                //if(geometry is Path) 
                //{
                //    Path path=new Path();
                //    path.setXYZ(x,y,null);
                //    return path;
                //}
                //if (geometry is MultiPoint)
                //{
                //    MultiPoint multiPoint = new MultiPoint();
                //    multiPoint.setXYZ(x, y, null);
                //    return multiPoint;
                //}
                //return new PointCollection(x,y,null);
            }
            if (geometry is IPoint)
            {
                double[] x = { ((IPoint)geometry).X };
                double[] y = { ((IPoint)geometry).Y };
                if (!fromProjective)
                {
                    ToRad(x, y);
                }

                lock (LockThis1)
                {
                    if (from != IntPtr.Zero && to != IntPtr.Zero)
                    {
                        unsafe
                        {
                            fixed (double* xx = x)
                            fixed (double* yy = y)
                            {
                                IntPtr xPtr = (IntPtr)(xx);
                                IntPtr yPtr = (IntPtr)(yy);
                                //if (preTo > 0)
                                //{
                                //    Proj4Wrapper.pj_transform(from, preTo, x.Length, 0, xPtr, yPtr, (IntPtr)0);
                                //    Proj4Wrapper.pj_transform(preTo, to, x.Length, 0, xPtr, yPtr, (IntPtr)0);
                                //}
                                //else
                                //{
                                Proj4Wrapper.pj_transform(from, to, x.Length, 0, xPtr, yPtr, IntPtr.Zero);
                                //}
                            }
                        }
                    }
                }
                if (!toProjektive)
                {
                    ToDeg(x, y);
                }

                return new Point(x[0], y[0]);
            }
            if (geometry is IEnvelope)
            {
                return Transform2D_(((IEnvelope)geometry).ToPolygon(10), from, to, fromProjective, toProjektive);
            }
            if (geometry is IPolyline)
            {
                int count = ((IPolyline)geometry).PathCount;
                IPolyline polyline = new Polyline();
                for (int i = 0; i < count; i++)
                {
                    polyline.AddPath((IPath)Transform2D_(((IPolyline)geometry)[i], from, to, fromProjective, toProjektive));
                }
                return polyline;
            }
            if (geometry is IPolygon)
            {
                int count = ((IPolygon)geometry).RingCount;
                IPolygon polygon = new Polygon();
                for (int i = 0; i < count; i++)
                {
                    polygon.AddRing((IRing)Transform2D_(((IPolygon)geometry)[i], from, to, fromProjective, toProjektive));
                }
                return polygon;
            }

            if (geometry is IAggregateGeometry)
            {
                int count = ((IAggregateGeometry)geometry).GeometryCount;
                IAggregateGeometry aGeom = new AggregateGeometry();
                for (int i = 0; i < count; i++)
                {
                    aGeom.AddGeometry((IGeometry)Transform2D_(((IAggregateGeometry)geometry)[i], from, to, fromProjective, toProjektive));
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
            lock (LockThis1)
            {
                //try
                //{
                //    if (_preToID > 0) Proj4Wrapper.pj_free(_preToID);
                //}
                //catch { }
                //try
                //{
                //    if (_preFromID > 0) Proj4Wrapper.pj_free(_preFromID);
                //}
                //catch { }
                try
                {
                    if (_fromID != IntPtr.Zero)
                    {
                        Proj4Wrapper.pj_free(_fromID);
                    }
                }
                catch { }
                try
                {
                    if (_toID != IntPtr.Zero)
                    {
                        Proj4Wrapper.pj_free(_toID);
                    }
                }
                catch { }
                _fromSRef = _toSRef = null;
                _fromID = _toID = IntPtr.Zero;
                //_preFrom = _preTo = -1;
            }
        }

        #endregion

        #region IDatumGridShiftProvider Member

        public string[] GridShiftNames()
        {
            string projLibPath = Environment.GetEnvironmentVariable("PROJ_LIB");

            if (String.IsNullOrEmpty(projLibPath) ||
                Directory.Exists(projLibPath))
            {
                return [];
            }

            List<string> result = new();

            foreach (var file in Directory.GetFiles(projLibPath))
            {
                switch (System.IO.Path.GetExtension(file).ToLower())
                {
                    case ".gsd":
                        result.Add(System.IO.Path.GetFileName(file));
                        break;
                }
            }

            return result.ToArray();
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
