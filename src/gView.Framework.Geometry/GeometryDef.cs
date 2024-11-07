using System;
using gView.Framework.Core.Geometry;

namespace gView.Framework.Geometry
{
    public class GeometryDef : IGeometryDef
    {
        private bool _hasZ = false, _hasM = false;
        private ISpatialReference _sRef = null;
        private GeometryType _geomType = GeometryType.Unknown;
        //private gView.Framework.Data.GeometryFieldType _fieldType = gView.Framework.Data.GeometryFieldType.Default;

        public GeometryDef()
        {
        }
        public GeometryDef(GeometryType geomType)
        {
            _geomType = geomType;
        }
        public GeometryDef(GeometryType geomType, ISpatialReference sRef)
            : this(geomType)
        {
            _sRef = sRef;
        }
        public GeometryDef(GeometryType geomType, ISpatialReference sRef, bool hasZ)
            : this(geomType, sRef)
        {
            _hasZ = hasZ;
        }
        public GeometryDef(IGeometryDef geomDef)
        {
            if (geomDef != null)
            {
                _hasZ = geomDef.HasZ;
                _hasM = geomDef.HasM;
                _sRef = ((geomDef.SpatialReference != null) ? (ISpatialReference)geomDef.SpatialReference.Clone() : null);
                _geomType = geomDef.GeometryType;
                //_fieldType = geomDef.GeometryFieldType;
            }
        }
        #region IGeometryDef Member

        public bool HasZ
        {
            get { return _hasZ; }
            set { _hasM = value; }
        }

        public bool HasM
        {
            get { return _hasM; }
            set { _hasM = value; }
        }
        public ISpatialReference SpatialReference
        {
            get { return _sRef; }
            set { _sRef = value; }
        }

        public GeometryType GeometryType
        {
            get { return _geomType; }
            set { _geomType = value; }
        }

        //public gView.Framework.Data.GeometryFieldType GeometryFieldType
        //{
        //    get
        //    {
        //        return _fieldType;
        //    }
        //    set
        //    {
        //        _fieldType = value;
        //    }
        //}
        #endregion

        static public void VerifyGeometryType(IGeometry geometry, IGeometryDef geomDef)
        {
            if (geomDef == null)
            {
                throw new ArgumentException("VerifyGeometryType - IGeometryDef Argument is null!");
            }

            if (geometry == null)
            {
                throw new ArgumentException("VerifyGeometryType - IGeometry Argument is null!");
            }

            switch (geomDef.GeometryType)
            {
                case GeometryType.Envelope:
                    if (geometry is IEnvelope)
                    {
                        return;
                    }

                    break;
                case GeometryType.Point:
                    if (geometry is IPoint)
                    {
                        return;
                    }

                    break;
                case GeometryType.Multipoint:
                    if (geometry is IMultiPoint)
                    {
                        return;
                    }

                    break;
                case GeometryType.Polyline:
                    if (geometry is IPolyline)
                    {
                        return;
                    }

                    break;
                case GeometryType.Polygon:
                    if (geometry is IPolygon)
                    {
                        return;
                    }

                    break;
                case GeometryType.Aggregate:
                    if (geometry is IAggregateGeometry)
                    {
                        return;
                    }

                    break;
            }

            throw new ArgumentException("Wrong Geometry for geometry type "
                + geomDef.GeometryType.ToString() + ": "
                + geometry.GetType().ToString());
        }
    }
}
