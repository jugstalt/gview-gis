using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.IO;
using gView.Framework.Geometry;

namespace gView.Framework.Geometry
{
    public class PersistableGeometry : IPersistable
    {
        IGeometry _geometry = null;

        public PersistableGeometry()
        {
        }
        public PersistableGeometry(IGeometry geometry)
        {
            _geometry = geometry;
        }

        public IGeometry Geometry
        {
            get { return _geometry; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            PersistablePoint ppoint = stream.Load("Point", null, new PersistablePoint(new Point())) as PersistablePoint;
            if (ppoint != null && ppoint.Point != null)
            {
                _geometry = ppoint.Point;
                return;
            }

            PersistablePointCollection pmultipoint = stream.Load("Multipoint", null, new PersistablePointCollection(new MultiPoint())) as PersistablePointCollection;
            if (pmultipoint != null && pmultipoint.PointCollection is IMultiPoint)
            {
                _geometry = pmultipoint.PointCollection as IMultiPoint;
                return;
            }

            PersistablePolyline ppolyline = stream.Load("Polyline", null, new PersistablePolyline(new Polyline())) as PersistablePolyline;
            if (ppolyline != null && ppolyline.Polyline != null)
            {
                _geometry = ppolyline.Polyline;
                return;
            }

            PersistablePolygon ppolygon = stream.Load("Polygon", null, new PersistablePolygon(new Polygon())) as PersistablePolygon;
            if (ppolygon != null && ppolygon.Polygon != null)
            {
                _geometry = ppolygon.Polygon;
                return;
            }

            PersistableEnvelope penvelope = stream.Load("Envelope", null, new PersistableEnvelope(new Envelope())) as PersistableEnvelope;
            if (penvelope != null && penvelope.Envelope != null)
            {
                _geometry = penvelope.Envelope;
                return;
            }

            PersistableAggregateGeometry pageometry = stream.Load("AggregateGeometry", null, new PersistableAggregateGeometry(new AggregateGeometry())) as PersistableAggregateGeometry;
            if (pageometry != null && pageometry.AggregateGeometry != null)
            {
                _geometry = pageometry.AggregateGeometry;
                return;
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_geometry == null || stream == null) return;

            if (_geometry is IPoint)
                stream.Save("Point", new PersistablePoint(_geometry as IPoint));
            else if (_geometry is IMultiPoint)
                stream.Save("Multipoint", new PersistablePointCollection(_geometry as IMultiPoint));
            else if (_geometry is IPolyline)
                stream.Save("Polyline", new PersistablePolyline(_geometry as IPolyline));
            else if (_geometry is Polygon)
                stream.Save("Polygon", new PersistablePolygon(_geometry as IPolygon));
            else if (_geometry is IEnvelope)
                stream.Save("Envelope", new PersistableEnvelope(_geometry as IEnvelope));
            else if (_geometry is IAggregateGeometry)
                stream.Save("AggregateGeometry", new PersistableAggregateGeometry(_geometry as IAggregateGeometry));
        }

        #endregion
    }

    internal class PersistablePoint : IPersistable
    {
        IPoint _point = null;

        public PersistablePoint(IPoint point)
        {
            _point = point;
        }

        public IPoint Point
        {
            get { return _point; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_point==null || stream == null) return;
            
            _point.X = (double)stream.Load("x", 0.0);
            _point.Y = (double)stream.Load("y", 0.0);
        }

        public void Save(IPersistStream stream)
        {
            if (_point == null) return;
            stream.Save("x", _point.X);
            stream.Save("y", _point.Y);
        }

        #endregion
    }

    internal class PersistablePointCollection : IPersistable
    {
        IPointCollection _pColl;

        public PersistablePointCollection(IPointCollection pcoll)
        {
            _pColl = pcoll;
        }

        public IPointCollection PointCollection
        {
            get { return _pColl; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_pColl==null || stream == null) return;

            PersistablePoint p;
            while ((p = stream.Load("v", null, new PersistablePoint(new Point())) as PersistablePoint) != null)
            {
                _pColl.AddPoint(p.Point);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_pColl == null) return;

            for (int i = 0; i < _pColl.PointCount; i++)
                stream.Save("v", new PersistablePoint(_pColl[i]));
        }

        #endregion
    }

    internal class PersistablePolyline : IPersistable
    {
        private IPolyline _polyline;

        public PersistablePolyline(IPolyline polyline)
        {
            _polyline = polyline;
        }

        public IPolyline Polyline
        {
            get { return _polyline; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_polyline == null || stream == null) return;
            
            PersistablePointCollection p;
            while ((p = stream.Load("Path", null, new PersistablePointCollection(new Path())) as PersistablePointCollection) != null)
            {
                _polyline.AddPath(p.PointCollection as IPath);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_polyline == null || stream == null) return;
            for (int i = 0; i < _polyline.PathCount; i++)
                stream.Save("Path", new PersistablePointCollection(_polyline[i]));
        }

        #endregion
    }

    internal class PersistablePolygon : IPersistable
    {
        private IPolygon _polygon;

        public PersistablePolygon(IPolygon polygon)
        {
            _polygon = polygon;
        }

        public IPolygon Polygon
        {
            get { return _polygon; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_polygon == null || stream == null) return;
            PersistablePointCollection p;
            while ((p = stream.Load("Ring", null, new PersistablePointCollection(new Ring())) as PersistablePointCollection) != null)
            {
                _polygon.AddRing(p.PointCollection as IRing);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_polygon == null || stream == null) return;
            for (int i = 0; i < _polygon.RingCount; i++)
                stream.Save("Ring", new PersistablePointCollection(_polygon[i]));
        }

        #endregion
    }

    internal class PersistableAggregateGeometry : IPersistable
    {
        private IAggregateGeometry _ageometry;

        public PersistableAggregateGeometry(IAggregateGeometry ageometry)
        {
            _ageometry = ageometry;
        }

        public IAggregateGeometry AggregateGeometry
        {
            get { return _ageometry; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_ageometry == null || stream == null) return;
            PersistableGeometry p;
            while ((p = stream.Load("Geometry", null, new PersistableGeometry()) as PersistableGeometry) != null)
            {
                _ageometry.AddGeometry(p.Geometry);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_ageometry == null || stream == null) return;
            for (int i = 0; i < _ageometry.GeometryCount; i++)
                stream.Save("Geometry", new PersistableGeometry(_ageometry[i]));
        }

        #endregion
    }

    internal class PersistableEnvelope : IPersistable
    {
        private IEnvelope _envelope;

        public PersistableEnvelope(IEnvelope envelope)
        {
            _envelope = envelope;
        }

        public IEnvelope Envelope
        {
            get { return _envelope; }
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (stream == null || _envelope == null) return;

            PersistablePoint lowerleft = (PersistablePoint)stream.Load("lowerleft", new PersistablePoint(new Point()), new PersistablePoint(new Point()));
            PersistablePoint upperright = (PersistablePoint)stream.Load("upperright", new PersistablePoint(new Point()), new PersistablePoint(new Point()));

            _envelope.minx = Math.Min(lowerleft.Point.X, upperright.Point.X);
            _envelope.miny = Math.Min(lowerleft.Point.Y, upperright.Point.Y);
            _envelope.maxx = Math.Max(lowerleft.Point.X, upperright.Point.X);
            _envelope.maxy = Math.Max(lowerleft.Point.Y, upperright.Point.Y);
        }

        public void Save(IPersistStream stream)
        {
            if (stream == null || _envelope == null) return;

            stream.Save("lowerleft", new PersistablePoint(_envelope.LowerLeft));
            stream.Save("upperright", new PersistablePoint(_envelope.UpperRight));
        }

        #endregion
    }
}
