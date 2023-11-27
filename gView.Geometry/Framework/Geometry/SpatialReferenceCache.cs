using System.Collections.Generic;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    internal class SpatialReferenceCache
    {
        private static List<ISpatialReference> _sRefs = new List<ISpatialReference>();
        private static Dictionary<string, string> _esriWTKs = new Dictionary<string, string>();
        private static object lockThis = new object();

        public SpatialReferenceCache()
        {
        }

        public static void Clear()
        {
            _sRefs.Clear();
        }
        public static ISpatialReference FromID(string id)
        {
            lock (lockThis)
            {
                foreach (ISpatialReference sRef in _sRefs)
                {
                    if (sRef.Name == id)
                    {
                        return sRef;
                    }
                }

                ISpatialReference sr = SpatialReference.FromID_(id);
                if (sr != null)
                {
                    _sRefs.Add(sr);
                }

                return sr;
            }
        }
        public static string ToESRIWKT(SpatialReference sRef)
        {
            lock (lockThis)
            {
                string sParam = sRef.ToString("P");

                foreach (string key in _esriWTKs.Keys)
                {
                    if (key.Equals(sParam))
                    {
                        return _esriWTKs[key];
                    }
                }

                string wkt = SpatialReference.ToESRIWKT_(sRef);
                _esriWTKs.Add(sParam, wkt);
                return wkt;
            }
        }
    }

}
