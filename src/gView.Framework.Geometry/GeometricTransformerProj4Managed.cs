using gView.Framework.Core.Geometry;
using gView.Framework.Geometry.Extensions;
using Proj4Net.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace gView.Framework.Geometry;

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
    BasicCoordinateTransform _basicCoordinateTransformation = null;
    BasicCoordinateTransform _basicCoordinateTransformationInverse = null;

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
    }

    public object Transform2D(object geometry)
    {
        _basicCoordinateTransformation = new BasicCoordinateTransform(_fromSrs, _toSrs);

        return PerformTransform2D(geometry, false);
    }
    public object InvTransform2D(object geometry)
    {
        _basicCoordinateTransformationInverse = new BasicCoordinateTransform(_toSrs, _fromSrs);

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

        var datum = _datumTransformations.GetTransformationDatumFor(sRef.Datum);
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

    private BasicCoordinateTransform BasicTransformation(bool inverse)
    {
        if (inverse)
        {
            return _basicCoordinateTransformationInverse;
        }
        else
        {
            return _basicCoordinateTransformation;
        }
    }

    private object PerformTransform2D(object geometry, bool inverse)
    {
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

        if (geometry is PointCollection pColl)
        {
            int pointCount = pColl.PointCount;
            if (pointCount == 0)
            {
                return geometry;
            }

            IPointCollection target = null;

            var basicTransformation = BasicTransformation(inverse);

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

            target.AddPoints(Enumerable.Range(0, pointCount).Select(i => new Point()).ToArray());

            var cFrom = new Coordinate();

            for (int i = 0; i < pointCount; i++)
            {
                cFrom.X = pColl[i].X;
                cFrom.Y = pColl[i].Y;

                var transformedCoordinate = basicTransformation.Transform(cFrom);

                var collectionPoint = target[i];
                collectionPoint.X = transformedCoordinate.X;
                collectionPoint.Y = transformedCoordinate.Y;
            }

            return target;
        }
        if (geometry is IPoint point)
        {
            IPoint target = null;

            var basicTransformation = BasicTransformation(inverse);

            Coordinate cFrom = new(point.X, point.Y);
            var cTo = basicTransformation.Transform(cFrom);
            target = new Point(cTo.X, cTo.Y);

            return target;
        }
        if (geometry is IEnvelope envelope)
        {
            return PerformTransform2D(envelope.ToPolygon(10), inverse);
        }
        if (geometry is IPolyline polyline)
        {
            int count = polyline.PathCount;
            IPolyline newPolyline = new Polyline();
            for (int i = 0; i < count; i++)
            {
                newPolyline.AddPath((IPath)PerformTransform2D(polyline[i], inverse));
            }
            return newPolyline;
        }
        if (geometry is IPolygon polygon)
        {
            int count = polygon.RingCount;
            IPolygon newPolygon = new Polygon();
            for (int i = 0; i < count; i++)
            {
                newPolygon.AddRing((IRing)PerformTransform2D(polygon[i], inverse));
            }
            return newPolygon;
        }

        if (geometry is IAggregateGeometry aggrGeom)
        {
            int count = aggrGeom.GeometryCount;
            IAggregateGeometry newAggrGeom = new AggregateGeometry();
            for (int i = 0; i < count; i++)
            {
                newAggrGeom.AddGeometry((IGeometry)PerformTransform2D(aggrGeom[i], inverse));
            }
            return newAggrGeom;
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

    #endregion
}
